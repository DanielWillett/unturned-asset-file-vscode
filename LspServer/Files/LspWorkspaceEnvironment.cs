using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Handlers;
using DanielWillett.UnturnedDataFileLspServer.Project;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using FileSystemWatcher = OmniSharp.Extensions.LanguageServer.Protocol.Models.FileSystemWatcher;
// ReSharper disable InconsistentlySynchronizedField

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspWorkspaceEnvironment : IWorkspaceEnvironment, IObserver<WorkspaceFolderChange>, IDisposable, IDidChangeWatchedFilesHandler
{
    private readonly OpenedFileTracker _tracker;
    private readonly UnturnedAssetFileSyncHandler? _fileSync;
    private readonly ILogger<LspWorkspaceEnvironment> _logger;
    private readonly Lazy<IParsingServices> _parsingServices;
    private readonly IAssetSpecDatabase _database;
    private readonly ILanguageServerFacade? _languageServer;
    private readonly IDisposable? _workspaceFoldersUnsubscriber;

    private readonly ServerDifficultyCache _difficultyCache;

    private bool _hasInitialized;

    private readonly ConcurrentDictionary<DocumentUri, WorkspaceFolderTracker> _folderTrackers =
        new ConcurrentDictionary<DocumentUri, WorkspaceFolderTracker>();

    private readonly List<DocumentUri> _rootUris;

    public event Action<WorkspaceFolderTracker>? WorkspaceFolderAdded;
    public event Action<WorkspaceFolderTracker>? WorkspaceFolderRemoved;

    public event Action<WorkspaceFolderTracker, string>? FileUpdated;
    public event Action<WorkspaceFolderTracker, string>? FileDeleted;
    public event Action<WorkspaceFolderTracker, string, string>? FileRenamed;
    public event Action<WorkspaceFolderTracker, string>? FileCreated;

    public event Action<WorkspaceFolderTracker, LspProjectFile>? ProjectFileUpdated;
    public event Action<WorkspaceFolderTracker, LspProjectFile>? ProjectFileDeleted;
    public event Action<WorkspaceFolderTracker, string, LspProjectFile>? ProjectFileMoved;
    public event Action<WorkspaceFolderTracker, LspProjectFile>? ProjectFileCreated;

    public IReadOnlyDictionary<DocumentUri, WorkspaceFolderTracker> WorkspaceFolders { get; }

    public LspWorkspaceEnvironment(
        OpenedFileTracker tracker,
        UnturnedAssetFileSyncHandler? fileSync,
        ILogger<LspWorkspaceEnvironment> logger,
        Lazy<IParsingServices> parsingServices,
        IAssetSpecDatabase database,
        ILanguageServerFacade? languageServer,
        ILanguageServerWorkspaceFolderManager? workspaceFolderManager)
    {
        _difficultyCache = ServerDifficultyCache.Create();
        _tracker = tracker;
        _fileSync = fileSync;
        _logger = logger;
        _parsingServices = parsingServices;
        _database = database;
        _languageServer = languageServer;

        WorkspaceFolders = new ReadOnlyDictionary<DocumentUri, WorkspaceFolderTracker>(_folderTrackers);

        if (fileSync != null)
        {
            //fileSync.FileAdded += OnFileOpened;
            fileSync.FileRemoved += OnFileClosed;
        }

        if (workspaceFolderManager == null || languageServer == null)
        {
            _rootUris = new List<DocumentUri>(0);
            return;
        }

        _workspaceFoldersUnsubscriber = workspaceFolderManager.Changed.Subscribe(this);
        _rootUris = new List<DocumentUri>();
    }

    void IObserver<WorkspaceFolderChange>.OnCompleted() { }
    void IObserver<WorkspaceFolderChange>.OnError(Exception error) { }
    void IObserver<WorkspaceFolderChange>.OnNext(WorkspaceFolderChange value)
    {
        if (value.Event == WorkspaceFolderEvent.Remove)
        {
            _logger.LogInformation("Removed workspace folder {0}.", value.Folder.Uri);
            if (_folderTrackers.TryRemove(value.Folder.Uri, out WorkspaceFolderTracker? tracker))
            {
                try
                {
                    WorkspaceFolderRemoved?.Invoke(tracker);
                }
                finally
                {
                    RemoveFolder(tracker);
                }
                lock (_rootUris)
                    _rootUris.Remove(value.Folder.Uri);
            }
        }
        else
        {
            WorkspaceFolderTracker tracker;
            WorkspaceFolderTracker? oldValue = null;
            lock (_folderTrackers)
            {
                _logger.LogInformation("Added workspace folder {0}, watched by client: {1}.", value.Folder.Uri, !_hasInitialized);
                if (_folderTrackers.ContainsKey(value.Folder.Uri))
                {
                    return;
                }

                tracker = new WorkspaceFolderTracker(value.Folder.Uri, value.Folder, !_hasInitialized);
                _folderTrackers.AddOrUpdate(value.Folder.Uri,
                    _ =>
                    {
                        oldValue = null;
                        return tracker;
                    },
                    (_, current) =>
                    {
                        oldValue = current;
                        return tracker;
                    }
                );
                lock (_rootUris)
                {
                    if (!_rootUris.Contains(value.Folder.Uri))
                        _rootUris.Add(value.Folder.Uri);
                }

                RegisterFolder(tracker);
            }

            if (oldValue != null)
                RemoveFolder(oldValue);

            WorkspaceFolderAdded?.Invoke(tracker);
        }
    }

    private void RemoveFolder(WorkspaceFolderTracker folder)
    {
        folder.Dispose();
        folder.FileCreated -= OnFileCreated;
        folder.FileDeleted -= OnFileDeleted;
        folder.FileUpdated -= OnFileUpdated;
        folder.FileRenamed -= OnFileRenamed;
    }
    private void RegisterFolder(WorkspaceFolderTracker folder)
    {
        folder.FileCreated += OnFileCreated;
        folder.FileDeleted += OnFileDeleted;
        folder.FileUpdated += OnFileUpdated;
        folder.FileRenamed += OnFileRenamed;

        if (!_database.IsInitialized)
            return;
        
        lock (folder.ProjectFileLock)
        {
            CreateProjectFiles(folder);
        }
    }

    //private void OnFileOpened(OpenedFile obj)
    //{
    //    
    //}

    private void OnFileClosed(OpenedFile obj)
    {
        _difficultyCache.RemoveCachedFile(obj.File);
    }

    private void OnFileCreated(WorkspaceFolderTracker tracker, string fullPath)
    {
        FileCreated?.Invoke(tracker, fullPath);
        TryUpdateProjectFile(fullPath, tracker);
    }
    private void OnFileDeleted(WorkspaceFolderTracker tracker, string fullPath)
    {
        FileDeleted?.Invoke(tracker, fullPath);
        TryUpdateProjectFile(fullPath, tracker);
    }
    private void OnFileUpdated(WorkspaceFolderTracker tracker, string fullPath)
    {
        FileUpdated?.Invoke(tracker, fullPath);
        TryUpdateProjectFile(fullPath, tracker);
    }
    private void OnFileRenamed(WorkspaceFolderTracker tracker, string oldFullPath, string newFullPath)
    {
        FileRenamed?.Invoke(tracker, oldFullPath, newFullPath);
        TryUpdateProjectFile(newFullPath, tracker, oldFullPath);
    }
    
    private void TryUpdateProjectFile(string fullPath, WorkspaceFolderTracker tracker, string? oldFullPath = null)
    {
        if (!OSPathHelper.IsExtension(fullPath, ".udatproj"))
            return;

        lock (tracker.ProjectFileLock)
        {
            UpdateProjectFile(fullPath, tracker, oldFullPath);
        }
    }

    private DocumentUri? GetBestRootUri(DocumentUri other)
    {
        StringComparison comparisonType = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        int best = 0;
        DocumentUri? bestUri = null;
        
        lock (_rootUris)
        {
            foreach (DocumentUri uri in _rootUris)
            {
                if (!string.Equals(uri.Scheme, other.Scheme, comparisonType))
                    continue;
                if (!string.Equals(uri.Authority, other.Authority, comparisonType))
                    continue;
                if (!other.Path.StartsWith(uri.Path, comparisonType))
                    continue;

                if (best > uri.Path.Length && bestUri is not null)
                    continue;
                
                if (best == uri.Path.Length)
                {
                    if (!other.Query.StartsWith(uri.Query, comparisonType))
                        continue;
                    if (!other.Fragment.StartsWith(uri.Fragment, comparisonType))
                        continue;
                }

                best = uri.Path.Length;
                bestUri = uri;
            }
        }

        return bestUri;
    }

    Task<Unit> IRequestHandler<DidChangeWatchedFilesParams, Unit>.Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
    {
        foreach (FileEvent change in request.Changes)
        {
            DocumentUri? rootUri = GetBestRootUri(change.Uri);
            if (rootUri is null)
            {
                _logger.LogInformation("Client reported change for file \"{0}\" but the best root URI couldn't be determined.", change.Uri);
                continue;
            }

            _logger.LogTrace("Client reported change for file \"{0}\" in root URI \"{1}\".", change.Uri, rootUri);

            if (!_folderTrackers.TryGetValue(rootUri, out WorkspaceFolderTracker? tracker))
                continue;

            string filePath = Path.GetFullPath(change.Uri.GetFileSystemPath());
            tracker.ConsumeChange(filePath, change.Type);
        }

        return Unit.Task;
    }

    DidChangeWatchedFilesRegistrationOptions IRegistration<DidChangeWatchedFilesRegistrationOptions, DidChangeWatchedFilesCapability>.GetRegistrationOptions(
        DidChangeWatchedFilesCapability capability,
        ClientCapabilities clientCapabilities)
    {
        if (_languageServer!.ClientSettings.Capabilities?.Workspace?.DidChangeWatchedFiles.IsSupported is not true)
        {
            _hasInitialized = true;
        }

        if (_languageServer.ClientSettings.WorkspaceFolders is not null)
        {
            foreach (WorkspaceFolder rootFolder in _languageServer.ClientSettings.WorkspaceFolders)
            {
                WorkspaceFolderTracker folder = new WorkspaceFolderTracker(rootFolder.Uri, rootFolder, !_hasInitialized);
                if (!_folderTrackers.TryAdd(rootFolder.Uri, folder))
                    folder.Dispose();
                if (!_rootUris.Contains(rootFolder.Uri))
                    _rootUris.Add(rootFolder.Uri);
                RegisterFolder(folder);
            }
        }

        DocumentUri? rootFolderUri = _languageServer.ClientSettings.RootUri;
        if (rootFolderUri is not null && !_folderTrackers.ContainsKey(rootFolderUri))
        {
            WorkspaceFolderTracker folder = new WorkspaceFolderTracker(rootFolderUri, null, !_hasInitialized);
            if (!_folderTrackers.TryAdd(rootFolderUri, folder))
                folder.Dispose();
            if (!_rootUris.Contains(rootFolderUri))
                _rootUris.Add(rootFolderUri);
            RegisterFolder(folder);
        }

        List<FileSystemWatcher> watchedPatterns = new List<FileSystemWatcher>();

        _hasInitialized = true;

        lock (_folderTrackers)
        {
            foreach (WorkspaceFolderTracker tracker in _folderTrackers.Values)
            {
                if (!tracker.IsWatchedByClient || !tracker.IsActive)
                    continue;

                GlobPattern pattern;
                if (capability.RelativePatternSupport is true)
                {
                    pattern = new GlobPattern(new RelativePattern
                    {
                        BaseUri = tracker.Folder != null ? new WorkspaceFolderOrUri(tracker.Folder) : new WorkspaceFolderOrUri(tracker.Uri),
                        Pattern = UnturnedAssetFileLspServer.FileWatcherGlobPattern
                    });
                    _logger.LogInformation("Client is watching \"{0}\" in \"{1}\".", UnturnedAssetFileLspServer.FileWatcherGlobPattern, tracker.Uri);
                }
                else
                {
                    string fullPath = _languageServer!.ClientSettings.RootPath == null
                        ? tracker.FilePath
                        : Path.GetRelativePath(_languageServer.ClientSettings.RootPath, tracker.FilePath);

                    fullPath = DocumentUri.File(fullPath).ToUnencodedString();
                    if (fullPath.EndsWith('/'))
                        fullPath += UnturnedAssetFileLspServer.FileWatcherGlobPattern;
                    else
                        fullPath += "/" + UnturnedAssetFileLspServer.FileWatcherGlobPattern;

                    pattern = new GlobPattern(fullPath);
                    _logger.LogInformation("Client is watching \"{0}\".", fullPath);
                }

                watchedPatterns.Add(new FileSystemWatcher
                {
                    GlobPattern = pattern,
                    Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
                });
            }
        }

        return new DidChangeWatchedFilesRegistrationOptions
        {
            Watchers = Container.From(watchedPatterns)
        };
    }

    internal void CreateAllProjectFiles()
    {
        foreach (WorkspaceFolderTracker tracker in _folderTrackers.Values)
        {
            lock (tracker.ProjectFileLock)
            {
                CreateProjectFiles(tracker);
            }
        }
    }

    private static int IndexOfProjectFile(string fileName, WorkspaceFolderTracker tracker)
    {
        // assume: lock (tracker.ProjectFileLock) {

        ImmutableArray<LspProjectFile> projectFiles = tracker.ProjectFiles;
        int index = -1;
        for (int i = 0; i < projectFiles.Length; i++)
        {
            LspProjectFile file = projectFiles[i];
            if (!string.Equals(file.FilePath, fileName, OSPathHelper.PathComparison))
                continue;

            index = i;
            break;
        }

        return index;
    }

    internal void UpdateProjectFile(string fileName, WorkspaceFolderTracker tracker, string? oldFullPath = null)
    {
        // assume: lock (tracker.ProjectFileLock) {

        string existingFileName = oldFullPath ?? fileName;

        int index = IndexOfProjectFile(existingFileName, tracker);
        if (oldFullPath != null && index < 0)
        {
            index = IndexOfProjectFile(fileName, tracker);
        }

        ImmutableArray<LspProjectFile> projectFiles = tracker.ProjectFiles;
        LspProjectFile? pjFile = index >= 0 ? projectFiles[index] : null;

        bool deleted = false;

        bool hasLock = false;
        Lock? @lock = null;
        IDisposable? disposable = null;
        bool success = false;
        try
        {
            if (!File.Exists(fileName))
            {
                deleted = true;
                success = false;
            }
            else
            {
                ISourceFile sourceFile;
                if (_tracker.Files.TryGetValue(DocumentUri.File(fileName), out OpenedFile? openedFile))
                {
                    @lock = openedFile.UpdateLock;
                    @lock.Enter();
                    hasLock = true;
                    sourceFile = openedFile.SourceFile;
                }
                else
                {
                    StaticSourceFile wsFile = StaticSourceFile.FromOtherFile(fileName, _parsingServices.Value.Database);
                    disposable = wsFile;
                    sourceFile = wsFile.SourceFile;
                }

                FileEvaluationContext ctx = new FileEvaluationContext(_parsingServices.Value, sourceFile, AssetDatPropertyPosition.Root);
                bool isType = ctx.FileType.Type.Equals(ProjectFileType.TypeId);

                if (isType)
                {
                    pjFile ??= new LspProjectFile(fileName, Path.GetDirectoryName(fileName)!);
                    success = pjFile.TryUpdateFromFile(ref ctx);
                }

                if (success && oldFullPath != null)
                {
                    pjFile!.FilePath = fileName;
                    pjFile.FolderPath = Path.GetDirectoryName(fileName)!;
                }
                
            }
        }
        catch (IOException)
        {
            deleted = true;
            success = false;
        }
        finally
        {
            if (hasLock)
                @lock!.Exit();

            disposable?.Dispose();
        }


        if (success)
        {
            if (index < 0)
            {
                tracker.ProjectFiles = projectFiles.Add(pjFile!);
            }
        }
        else
        {
            if (index >= 0)
            {
                tracker.ProjectFiles = projectFiles.Length == 1
                    ? ImmutableArray<LspProjectFile>.Empty
                    : projectFiles.RemoveAt(index);
            }

            deleted = true;
        }

        if (pjFile == null)
            return;

        if (deleted)
        {
            InvokeProjectFileDeleted(tracker, pjFile);
        }
        else if (index < 0)
        {
            InvokeProjectFileCreated(tracker, pjFile);
        }
        else if (oldFullPath != null)
        {
            InvokeProjectFileMoved(tracker, oldFullPath, pjFile);
        }
        else
        {
            InvokeProjectFileUpdated(tracker, pjFile);
        }
    }

    internal void CreateProjectFiles(WorkspaceFolderTracker tracker)
    {
        // assume: lock (tracker.ProjectFileLock) {
        if (!tracker.ProjectFiles.IsDefault)
        {
            return;
        }

        if (!tracker.IsActive)
        {
            tracker.ProjectFiles = ImmutableArray<LspProjectFile>.Empty;
            return;
        }

        ImmutableArray<LspProjectFile>.Builder files = ImmutableArray.CreateBuilder<LspProjectFile>();
        IParsingServices parsingServices = _parsingServices.Value;
        foreach (string file in tracker.EnumerateFiles("*.udatproj"))
        {
            bool hasLock = false;
            Lock? @lock = null;
            IDisposable? disposable = null;
            try
            {
                ISourceFile sourceFile;
                if (_tracker.Files.TryGetValue(DocumentUri.File(file), out OpenedFile? openedFile))
                {
                    @lock = openedFile.UpdateLock;
                    @lock.Enter();
                    hasLock = true;
                    sourceFile = openedFile.SourceFile;
                }
                else
                {
                    StaticSourceFile wsFile = StaticSourceFile.FromOtherFile(file, parsingServices.Database);
                    disposable = wsFile;
                    sourceFile = wsFile.SourceFile;
                }

                FileEvaluationContext ctx = new FileEvaluationContext(parsingServices, sourceFile, AssetDatPropertyPosition.Root);
                if (!ctx.FileType.Type.Equals(ProjectFileType.TypeId))
                    continue;

                LspProjectFile pjFile = new LspProjectFile(file, Path.GetDirectoryName(file)!);
                if (!pjFile.TryUpdateFromFile(ref ctx))
                    continue;

                files.Add(pjFile);
            }
            finally
            {
                if (hasLock)
                    @lock!.Exit();

                disposable?.Dispose();
            }
        }

        tracker.ProjectFiles = files.MoveToImmutableOrCopy();

        foreach (LspProjectFile file in tracker.ProjectFiles)
        {
            InvokeProjectFileCreated(tracker, file);
        }
    }

    private void InvokeProjectFileUpdated(WorkspaceFolderTracker tracker, LspProjectFile pjFile)
    {
        _logger.LogDebug("Project file updated: {0}.", pjFile.FilePath);
        ProjectFileUpdated?.Invoke(tracker, pjFile);
    }

    private void InvokeProjectFileDeleted(WorkspaceFolderTracker tracker, LspProjectFile pjFile)
    {
        _logger.LogDebug("Project file stopped tracking: {0}.", pjFile.FilePath);
        ProjectFileDeleted?.Invoke(tracker, pjFile);
    }

    private void InvokeProjectFileMoved(WorkspaceFolderTracker tracker, string oldFileName, LspProjectFile pjFile)
    {
        _logger.LogDebug("Project file moved: {0} -> {1}.", oldFileName, pjFile.FilePath);
        ProjectFileMoved?.Invoke(tracker, oldFileName, pjFile);
    }

    private void InvokeProjectFileCreated(WorkspaceFolderTracker tracker, LspProjectFile pjFile)
    {
        _logger.LogDebug("Project file started tracking: {0}.", pjFile.FilePath);
        ProjectFileCreated?.Invoke(tracker, pjFile);
    }

    /// <inheritdoc />
    public DiscoveredBundle? LoadBundleForAsset(ISourceFile file)
    {
        // todo
        return null;
    }

    public IWorkspaceFile? TemporarilyGetOrLoadFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        DocumentUri uri = DocumentUri.File(filePath);
        if (_tracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            return new DontDisposeWorkspaceFile(file);
        }

        try
        {
            return StaticSourceFile.FromAssetFile(filePath, _database, SourceNodeTokenizerOptions.Lazy);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file {0}.", filePath);
            return null;
        }
    }

    /// <inheritdoc />
    public bool TryGetFileDifficulty(string file, out ServerDifficulty difficulty)
    {
        return _difficultyCache.TryGetDifficulty(file, out difficulty);
    }

    public void Dispose()
    {
        if (_fileSync != null)
        {
            //_fileSync.FileAdded -= OnFileOpened;
            _fileSync.FileRemoved -= OnFileClosed;
        }

        _workspaceFoldersUnsubscriber?.Dispose();

        while (_folderTrackers.Count > 0)
        {
            foreach (DocumentUri uri in _folderTrackers.Keys.ToList())
            {
                if (_folderTrackers.TryRemove(uri, out WorkspaceFolderTracker? tracker))
                {
                    tracker.Dispose();
                }
            }
        }
    }

    private class DontDisposeWorkspaceFile(IWorkspaceFile file) : IWorkspaceFile
    {
        /// <inheritdoc />
        public string File => file.File;

        /// <inheritdoc />
        public ISourceFile SourceFile => file.SourceFile;

        /// <inheritdoc />
        public IBundleProxy Bundle => file.Bundle;

        /// <inheritdoc />
        public string GetFullText() => file.GetFullText();

        /// <inheritdoc />
        public event Action<IWorkspaceFile, FileRange>? OnUpdated
        {
            add => file.OnUpdated += value;
            remove => file.OnUpdated -= value;
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}