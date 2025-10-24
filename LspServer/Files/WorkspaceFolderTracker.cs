using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using FileSystemWatcher = System.IO.FileSystemWatcher;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class WorkspaceFolderTracker : IDisposable
{
    private FileSystemWatcher? _watcher;

    public string Name { get; }
    public string FilePath { get; }
    public DocumentUri Uri { get; }
    public WorkspaceFolder? Folder { get; }
    public bool IsActive { get; }

    internal bool IsWatchedByClient { get; set; }

    public event Action<WorkspaceFolderTracker, string>? FileDeleted;
    public event Action<WorkspaceFolderTracker, string>? FileCreated;
    public event Action<WorkspaceFolderTracker, string>? FileUpdated;
    public event Action<WorkspaceFolderTracker, string, string>? FileRenamed;

    public WorkspaceFolderTracker(DocumentUri uri, WorkspaceFolder? folder, bool isWatchedByClient)
    {
        Folder = folder;
        Uri = uri;
        IsActive = string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase);
        FilePath = IsActive ? Path.GetFullPath(uri.GetFileSystemPath()) : uri.ToUnencodedString();
        Name = folder?.Name ?? Path.GetFileName(FilePath);
        IsWatchedByClient = isWatchedByClient;

        if (isWatchedByClient)
            return;

        _watcher = new FileSystemWatcher(FilePath, "*.*");
        try
        {
            _watcher.NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName
                                   | NotifyFilters.CreationTime;
            _watcher.IncludeSubdirectories = true;
            _watcher.EnableRaisingEvents = true;

            _watcher.Changed += FileWatcherFileUpdated;
            _watcher.Deleted += FileWatcherFileRemoved;
            _watcher.Created += FileWatcherFileCreated;
            _watcher.Renamed += FileWatcherFileRenamed;
        }
        catch
        {
            _watcher.Dispose();
            throw;
        }
    }

    private void FileWatcherFileCreated(object sender, FileSystemEventArgs e)
    {
        FileCreated?.Invoke(this, e.FullPath);
    }

    private void FileWatcherFileRenamed(object sender, RenamedEventArgs e)
    {
        FileRenamed?.Invoke(this, e.OldFullPath, e.FullPath);
    }

    private void FileWatcherFileRemoved(object sender, FileSystemEventArgs e)
    {
        FileDeleted?.Invoke(this, e.FullPath);
    }

    private void FileWatcherFileUpdated(object sender, FileSystemEventArgs e)
    {
        FileUpdated?.Invoke(this, e.FullPath);
    }

    internal void ConsumeChange(string fileName, FileChangeType changeType)
    {
        switch (changeType)
        {
            case FileChangeType.Deleted:
                FileDeleted?.Invoke(this, fileName);
                break;

            case FileChangeType.Changed:
                FileUpdated?.Invoke(this, fileName);
                break;

            case FileChangeType.Created:
                FileCreated?.Invoke(this, fileName);
                break;
        }
    }

    public IEnumerable<string> EnumerateFiles(string pattern)
    {
        return IsActive ? Directory.EnumerateFiles(FilePath, pattern, SearchOption.AllDirectories) : Enumerable.Empty<string>();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Interlocked.Exchange(ref _watcher, null)?.Dispose();
    }
}