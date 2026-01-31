using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

public class LspAssetSpecDatabase : AssetSpecDatabase
{
    private readonly ILogger<LspAssetSpecDatabase> _logger;

    public LspAssetSpecDatabase(ILoggerFactory loggerFactory, JsonSerializerOptions options
#if !DEBUG
        , ISpecDatabaseCache cache
#endif
        ) : base(
        new SpecificationFileReader(
            allowInternet: true,
            loggerFactory,
            new Lazy<HttpClient>(() => new HttpClient()),
            options,
            new InstallDirUtility("Unturned", "304930"),
#if !DEBUG
            cache: cache
#else
            cache: null
#endif
        ), loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LspAssetSpecDatabase>();
        Options = options;
    }

    /// <inheritdoc />
    protected override void Log(string msg)
    {
        _logger.LogError(msg);
    }
}