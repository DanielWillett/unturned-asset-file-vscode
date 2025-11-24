using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using FileSystemWatcher = OmniSharp.Extensions.LanguageServer.Protocol.Models.FileSystemWatcher;
// ReSharper disable InconsistentlySynchronizedField

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspWorkspaceEnvironment : IWorkspaceEnvironment, IObserver<WorkspaceFolderChange>, IDisposable, IDidChangeWatchedFilesHandler
{
    private readonly OpenedFileTracker _tracker;
    private readonly ILogger<LspWorkspaceEnvironment> _logger;
    private readonly IAssetSpecDatabase _database;
    private readonly ILanguageServerFacade? _languageServer;
    private readonly IDisposable? _workspaceFoldersUnsubscriber;
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

    public IEnumerable<WorkspaceFolderTracker> WorkspaceFolders => _folderTrackers.Values;

    public LspWorkspaceEnvironment(
        OpenedFileTracker tracker,
        ILogger<LspWorkspaceEnvironment> logger,
        IAssetSpecDatabase database,
        ILanguageServerFacade? languageServer,
        ILanguageServerWorkspaceFolderManager? workspaceFolderManager)
    {
        _tracker = tracker;
        _logger = logger;
        _database = database;
        _languageServer = languageServer;
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
    }

    private void OnFileCreated(WorkspaceFolderTracker tracker, string fullPath)
    {
        FileCreated?.Invoke(tracker, fullPath);
    }
    private void OnFileDeleted(WorkspaceFolderTracker tracker, string fullPath)
    {
        FileDeleted?.Invoke(tracker, fullPath);
    }
    private void OnFileUpdated(WorkspaceFolderTracker tracker, string fullPath)
    {
        FileUpdated?.Invoke(tracker, fullPath);
    }
    private void OnFileRenamed(WorkspaceFolderTracker tracker, string oldFullPath, string newFullPath)
    {
        FileRenamed?.Invoke(tracker, oldFullPath, newFullPath);
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

            if (_folderTrackers.TryGetValue(rootUri, out WorkspaceFolderTracker? tracker))
            {
                tracker.ConsumeChange(Path.GetFullPath(change.Uri.GetFileSystemPath()), change.Type);
            }
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

    public void Dispose()
    {
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