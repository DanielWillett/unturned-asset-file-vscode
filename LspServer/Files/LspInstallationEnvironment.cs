using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspInstallationEnvironment : InstallationEnvironment
{
    private readonly ILogger<LspInstallationEnvironment> _logger;
    private readonly InstallDirUtility _installDir;
    private readonly LspWorkspaceEnvironment _workspace;
    private bool _hasEvents;

    public LspInstallationEnvironment(
        InstallDirUtility installDir,
        IAssetSpecDatabase database,
        ILogger<LspInstallationEnvironment> logger,
        LspWorkspaceEnvironment workspace)
        : base(database)
    {
        _installDir = installDir;
        _logger = logger;
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

        using IDisposable? scope = _logger.BeginScope("Source directories");
        _logger.LogInformation("Source directories: ");
        foreach (string dir in SourceDirectories)
        {
            _logger.LogInformation(dir);
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

    protected override void Log(string fileName, string msg)
    {
        _logger.LogWarning("{0} - " + msg, fileName);
    }

    protected override void Log(string msg)
    {
        _logger.LogError(msg);
    }
}
