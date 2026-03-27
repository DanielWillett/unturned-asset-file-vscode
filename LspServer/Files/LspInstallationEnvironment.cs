using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspInstallationEnvironment : InstallationEnvironment
{
    private readonly InstallDirUtility _installDir;
    private readonly LspWorkspaceEnvironment _workspace;
    private bool _hasEvents;

    public LspInstallationEnvironment(
        InstallDirUtility installDir,
        IAssetSpecDatabase database,
        ILoggerFactory loggerFactory,
        LspWorkspaceEnvironment workspace)
        : base(database, loggerFactory)
    {
        _installDir = installDir;
        _workspace = workspace;
    }

    internal void Init()
    {
        if (_installDir.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            this.AddUnturnedSearchableDirectories(installDir);
        }

        _hasEvents = true;
        _workspace.WorkspaceFolderAdded += OnWorkspaceFolderAdded;
        _workspace.WorkspaceFolderRemoved += OnWorkspaceFolderRemoved;

        foreach (WorkspaceFolderTracker folder in _workspace.WorkspaceFolders.Values)
        {
            OnWorkspaceFolderAdded(folder);
        }

        using IDisposable? scope = Logger.BeginScope("Source directories");
        Logger.LogInformation("Source directories: ");
        foreach (string dir in SourceDirectories)
        {
            Logger.LogInformation(dir);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _hasEvents)
        {
            _hasEvents = false;
            _workspace.WorkspaceFolderAdded -= OnWorkspaceFolderAdded;
            _workspace.WorkspaceFolderRemoved -= OnWorkspaceFolderRemoved;
        }

        base.Dispose(disposing);
    }


    private void OnWorkspaceFolderAdded(WorkspaceFolderTracker obj)
    {
        AddSearchableDirectoryIfExists(obj.FilePath);
    }

    private void OnWorkspaceFolderRemoved(WorkspaceFolderTracker obj)
    {
        RemoveSearchableDirectory(obj.FilePath);
    }
}
