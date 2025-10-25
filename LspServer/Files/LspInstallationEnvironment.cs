using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspInstallationEnvironment : InstallationEnvironment
{
    private readonly ILogger<LspInstallationEnvironment> _logger;
    private readonly LspWorkspaceEnvironment _workspace;

    public LspInstallationEnvironment(IAssetSpecDatabase database, ILogger<LspInstallationEnvironment> logger, LspWorkspaceEnvironment workspace) : base(database)
    {
        _logger = logger;
        _workspace = workspace;

        if (database.UnturnedInstallDirectory.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            this.AddUnturnedSearchableDirectories(installDir);
        }

        _workspace.WorkspaceFolderAdded += OnWorkspaceFolderAdded;
        _workspace.WorkspaceFolderRemoved += OnWorkspaceFolderRemoved;
        foreach (WorkspaceFolderTracker folder in _workspace.WorkspaceFolders)
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
        if (disposing)
        {
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
