using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

public class LspAssetSpecDatabase : AssetSpecDatabase
{
    private readonly ILogger<LspAssetSpecDatabase> _logger;

    public LspAssetSpecDatabase(ILogger<LspAssetSpecDatabase> logger, JsonSerializerOptions options, ISpecDatabaseCache cache) : base(cache)
    {
        _logger = logger;
        Options = options;
    }

    /// <inheritdoc />
    protected override void Log(string msg)
    {
        _logger.LogError(msg);
    }
}