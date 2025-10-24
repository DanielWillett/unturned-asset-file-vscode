using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Files;
using DanielWillett.UnturnedDataFileLspServer.Handlers;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using System.Collections.Concurrent;
using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Diagnostics;

/// <summary>
/// Handles keeping track of active files and publishing diagnostics.
/// </summary>
internal class DiagnosticsManager : IDisposable
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly UnturnedAssetFileSyncHandler _fileSync;
    private readonly LspWorkspaceEnvironment _workspaceEnvironment;
    private readonly ILanguageServerFacade _languageServer;
    private readonly IAssetSpecDatabase _database;

    private readonly ConcurrentQueue<DiagnosticsWorkItem> _workQueue;
    private readonly Lock _workerThreadLock = new Lock();
    private Thread? _diagnosticProcessorThread;
    private bool _isWorkerThreadRunning;
    private DateTime _lastInlineProcess = DateTime.MinValue;

    private readonly ConcurrentDictionary<string, FileDiagnostics> _diagnostics;

    internal IFilePropertyVirtualizer Virtualizer;
    internal IWorkspaceEnvironment Workspace;
    internal InstallationEnvironment InstallEnvironment;

    public DiagnosticsManager(
        OpenedFileTracker fileTracker,
        UnturnedAssetFileSyncHandler fileSync,
        LspWorkspaceEnvironment workspaceEnvironment,
        ILanguageServerFacade languageServer,
        IAssetSpecDatabase database,
        IFilePropertyVirtualizer virtualizer,
        IWorkspaceEnvironment workspace,
        InstallationEnvironment installEnvironment)
    {
        Virtualizer = virtualizer;
        Workspace = workspace;
        InstallEnvironment = installEnvironment;
        _fileTracker = fileTracker;
        _fileSync = fileSync;
        _workQueue = new ConcurrentQueue<DiagnosticsWorkItem>();
        _diagnostics = new ConcurrentDictionary<string, FileDiagnostics>(OSPathHelper.PathComparer);
        _workspaceEnvironment = workspaceEnvironment;
        _languageServer = languageServer;
        _database = database;
        _workspaceEnvironment.FileCreated += OnFileCreated;
        _workspaceEnvironment.FileDeleted += OnFileDeleted;
        _workspaceEnvironment.FileUpdated += OnFileUpdated;
        _workspaceEnvironment.FileRenamed += OnFileRenamed;
        _workspaceEnvironment.WorkspaceFolderAdded += OnWorkspaceFolderAdded;
        _workspaceEnvironment.WorkspaceFolderRemoved += OnWorkspaceFolderRemoved;
        _fileSync.FileAdded += OnFileOpened;
        _fileSync.FileRemoved += OnFileClosed;
        _fileSync.ContentUpdated += OnContentUpdated;

        foreach (WorkspaceFolderTracker folder in _workspaceEnvironment.WorkspaceFolders)
        {
            OnWorkspaceFolderAdded(folder);
        }
    }

    private void OnFileOpened(OpenedFile obj)
    {
        ReclaculateDiagnostics(obj.File);
        if (_diagnostics.TryGetValue(obj.File, out FileDiagnostics? file))
        {
            file.SetOpenedFile(obj);
        }
    }

    private void OnFileClosed(OpenedFile obj)
    {
        foreach (FileDiagnostics file in _diagnostics.Values)
        {
            file.SetOpenedFile(null, obj);
        }
    }

    public void PushDiagnostics(FileDiagnostics file, Container<Diagnostic> diagnostics)
    {
        _languageServer.SendNotification(new PublishDiagnosticsParams
        {
            Diagnostics = diagnostics,
            Uri = file.Uri,
            Version = null
        });
    }

    public Diagnostic CreateDiagnostic(in DatDiagnosticMessage msg)
    {
        return new Diagnostic
        {
            Code = new DiagnosticCode(msg.Diagnostic.ErrorId),
            Source = UnturnedAssetFileLspServer.DiagnosticSource,
            Message = msg.Message,
            Range = msg.Range.ToRange(),
            Tags = msg.Diagnostic == DatDiagnostics.UNT1018 ? new Container<DiagnosticTag>(DiagnosticTag.Deprecated) : null,
            Severity = (DiagnosticSeverity)msg.Diagnostic.Severity
        };
    }

    public void Dispose()
    {
        _workspaceEnvironment.FileCreated -= OnFileCreated;
        _workspaceEnvironment.FileDeleted -= OnFileDeleted;
        _workspaceEnvironment.FileUpdated -= OnFileUpdated;
        _workspaceEnvironment.FileRenamed -= OnFileRenamed;
        _workspaceEnvironment.WorkspaceFolderAdded -= OnWorkspaceFolderAdded;
        _workspaceEnvironment.WorkspaceFolderRemoved -= OnWorkspaceFolderRemoved;
        _fileSync.FileAdded -= OnFileOpened;
        _fileSync.FileRemoved -= OnFileClosed;
        _fileSync.ContentUpdated -= OnContentUpdated;
    }

    public FileDiagnostics GetOrAddFile(string filePath, DocumentUri? uri)
    {
        if (_diagnostics.TryGetValue(filePath, out FileDiagnostics? d))
            return d;

        FileDiagnostics newDiagnostics = new FileDiagnostics(filePath, uri ?? DocumentUri.File(filePath), this, _database);
        d = _diagnostics.GetOrAdd(filePath, newDiagnostics);
        if (ReferenceEquals(d, newDiagnostics) && _fileTracker.Files.TryGetValue(newDiagnostics.Uri, out OpenedFile? openedFile))
        {
            newDiagnostics.SetOpenedFile(openedFile, null);
            if (!_fileTracker.Files.ContainsKey(newDiagnostics.Uri))
                newDiagnostics.SetOpenedFile(null, openedFile);
        }

        return d;
    }

    private void RemoveDiagnosticsForFiles(string directoryPath)
    {
        StringComparison c = OSPathHelper.PathComparison;
        foreach (string key in _diagnostics.Keys)
        {
            if (!key.StartsWith(directoryPath, c))
                continue;

            RemoveDiagnostics(key);
        }
    }

    private void OnContentUpdated(OpenedFile obj)
    {
        int version = obj.ChangeVersion;
        Task.Run(async () =>
        {
            await Task.Delay(500);
            if (version == obj.ChangeVersion)
            {
                _workQueue.Enqueue(new DiagnosticsWorkItem(obj.File));
                MaybeStartWorkerThread();
            }
        });
    }

    private void ReclaculateDiagnostics(string filePath)
    {
        GetOrAddFile(filePath, null).Recalculate();
    }

    private void TransferDiagnostics(string from, string to)
    {
        if (!_diagnostics.TryRemove(from, out FileDiagnostics? fileDiags))
            return;

        fileDiags.UpdateFileName(to, DocumentUri.File(to));
        _diagnostics[to] = fileDiags;
        fileDiags.Recalculate();
    }

    private void RemoveDiagnostics(string filePath)
    {
        if (_diagnostics.TryRemove(filePath, out FileDiagnostics? file))
        {
            file.Clear();
        }
    }

    private void OnWorkspaceFolderAdded(WorkspaceFolderTracker obj)
    {
        _workQueue.Enqueue(new DiagnosticsWorkItem(obj.FilePath, type: DiagnosticsWorkItemType.DiscoverAll));
        MaybeStartWorkerThread(forceRunOnWorkerThread: true);
    }

    private void OnWorkspaceFolderRemoved(WorkspaceFolderTracker obj)
    {
        _workQueue.Enqueue(new DiagnosticsWorkItem(obj.FilePath, type: DiagnosticsWorkItemType.DeleteAll));
        MaybeStartWorkerThread(forceRunOnWorkerThread: true);
    }

    private void OnFileCreated(WorkspaceFolderTracker tracker, string fullPath)
    {
        _workQueue.Enqueue(new DiagnosticsWorkItem(fullPath));
        MaybeStartWorkerThread();
    }

    private void OnFileDeleted(WorkspaceFolderTracker tracker, string fullPath)
    {
        _workQueue.Enqueue(new DiagnosticsWorkItem(fullPath, type: DiagnosticsWorkItemType.Delete));
    }

    private void OnFileUpdated(WorkspaceFolderTracker tracker, string fullPath)
    {
        _workQueue.Enqueue(new DiagnosticsWorkItem(fullPath));
        MaybeStartWorkerThread();
    }

    private void OnFileRenamed(WorkspaceFolderTracker tracker, string oldFullPath, string newFullPath)
    {
        _workQueue.Enqueue(new DiagnosticsWorkItem(newFullPath, oldFullPath));
        MaybeStartWorkerThread();
    }

    private int _queuedMaybeStart;

    private void MaybeStartWorkerThread(bool forceRunOnWorkerThread = false)
    {
        if (!_database.IsInitialized && Interlocked.Exchange(ref _queuedMaybeStart, 1) == 0)
        {
            _database.OnInitialize(_ =>
            {
                MaybeStartWorkerThread(true);
                return Task.CompletedTask;
            });
            return;
        }

        const int maxInlineProcessCt = 3;
        bool exited = false;
        if (!forceRunOnWorkerThread)
        {
            _workerThreadLock.Enter();
            try
            {
                // try to quickly process a small amout of items without spinning up a new thread
                // only spin up a new thread when there are a lot of items added
                if (_workQueue.Count <= maxInlineProcessCt && (DateTime.UtcNow - _lastInlineProcess).TotalSeconds > 0.5)
                {
                    _lastInlineProcess = DateTime.UtcNow;
                    _workerThreadLock.Exit();
                    exited = true;

                    for (int ct = 0; _workQueue.Count + ct < maxInlineProcessCt && _workQueue.TryDequeue(out DiagnosticsWorkItem workItem); ++ct)
                    {
                        ProcessWorkItem(in workItem, false);
                    }
                }
            }
            finally
            {
                if (!exited)
                    _workerThreadLock.Exit();
            }
        }

        if (_workQueue.Count == 0)
            return;

        _workerThreadLock.Enter();
        try
        {
            if (_isWorkerThreadRunning)
                return;

            _isWorkerThreadRunning = true;
            _diagnosticProcessorThread = new Thread(WorkerThread);
            _diagnosticProcessorThread.Start();
        }
        finally
        {
            _workerThreadLock.Exit();
        }
    }

    private void WorkerThread()
    {
        while (_workQueue.TryDequeue(out DiagnosticsWorkItem workItem))
        {
            ProcessWorkItem(in workItem, true);
        }

        _workerThreadLock.Enter();
        _isWorkerThreadRunning = false;
        _workerThreadLock.Exit();

        if (_workQueue.Count > 0 && _diagnosticProcessorThread == null)
        {
            MaybeStartWorkerThread();
        }
    }

    private void ProcessWorkItem(in DiagnosticsWorkItem workItem, bool isInWorkerThread)
    {
        if (workItem.Type == DiagnosticsWorkItemType.DeleteAll)
        {
            RemoveDiagnosticsForFiles(workItem.FilePath);
            return;
        }

        if (workItem.Type == DiagnosticsWorkItemType.Delete
            || workItem.Type != DiagnosticsWorkItemType.DiscoverAll && !File.Exists(workItem.FilePath))
        {
            RemoveDiagnostics(workItem.FilePath);
            return;
        }

        if (workItem.Type == DiagnosticsWorkItemType.DiscoverAll)
        {
            if (!Directory.Exists(workItem.FilePath))
            {
                RemoveDiagnosticsForFiles(workItem.FilePath);
                return;
            }

            PatternMatchingResult result = UnturnedAssetFileLspServer.FileWatcherMatcher.Execute(
                new DirectoryInfoWrapper(new DirectoryInfo(workItem.FilePath))
            );

            if (!result.HasMatches)
            {
                RemoveDiagnosticsForFiles(workItem.FilePath);
                return;
            }

            foreach (FilePatternMatch file in result.Files)
            {
                _workQueue.Enqueue(new DiagnosticsWorkItem(Path.Combine(workItem.FilePath, file.Path)));
            }

            if (!isInWorkerThread)
                MaybeStartWorkerThread();
            return;
        }

        if (workItem.RenamedFrom != null)
        {
            TransferDiagnostics(workItem.RenamedFrom, workItem.FilePath);
            return;
        }

        ReclaculateDiagnostics(workItem.FilePath);
    }

    private struct DiagnosticsWorkItem
    {
        public DiagnosticsWorkItemType Type { get; }
        public string FilePath { get; }
        public string? RenamedFrom { get; }

        public DiagnosticsWorkItem(string filePath, string? renamedFrom = null, DiagnosticsWorkItemType type = DiagnosticsWorkItemType.Recalculate)
        {
            Type = type;
            FilePath = filePath;
            RenamedFrom = renamedFrom;
        }
    }

    public enum DiagnosticsWorkItemType
    {
        Recalculate,
        DiscoverAll,
        Delete,
        DeleteAll
    }
}
