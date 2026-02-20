using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
#if TEST_LSP
using DanielWillett.UnturnedDataFileLspServer.Files;
#endif

namespace UnturnedAssetSpecTests;

public class AssetSpecValidity
{
    [Test]
    public async Task CheckSpecValidity()
    {
        ILoggerFactory loggerFactory = LoggerFactory.Create(l => l.AddConsole());
#if TEST_LSP
        EnvironmentCache cache = new EnvironmentCache(loggerFactory.CreateLogger<EnvironmentCache>());
#else
        ISpecDatabaseCache? cache = null;
#endif
        AssetSpecDatabase db = AssetSpecDatabase.FromOffline(
            false,
            loggerFactory,
            cache: cache
        );

        await db.InitializeAsync();

        Assert.That(db.AllTypes,                Is.Not.Empty);
        Assert.That(db.FileTypes,               Is.Not.Empty);
        // Assert.That(db.LocalizationFileTypes,   Is.Not.Empty); requires game files
        Assert.That(db.BlueprintSkills,         Is.Not.Empty);
        // Assert.That(db.NPCAchievementIds,       Is.Not.Empty); requires game files if not online
        // Assert.That(db.ValidActionButtons,      Is.Not.Empty); requires game files if not online
    }
}