using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

public class LspInstallationEnvironment : InstallationEnvironment
{
    private readonly ILogger<LspInstallationEnvironment> _logger;

    public LspInstallationEnvironment(AssetSpecDatabase database, ILogger<LspInstallationEnvironment> logger) : base(database)
    {
        _logger = logger;
        if (database.UnturnedInstallDirectory.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            this.AddUnturnedSearchableDirectories(installDir);
        }

        using IDisposable? scope = _logger.BeginScope("Source directories");
        _logger.LogInformation("Source directories: ");
        foreach (string dir in SourceDirectories)
        {
            _logger.LogInformation(dir);
        }
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
