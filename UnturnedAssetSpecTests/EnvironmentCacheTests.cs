#if false && TEST_LSP
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace UnturnedAssetSpecTests;

[NonParallelizable]
internal class EnvironmentCacheTests
{
    [Test]
    [Ignore("Makes network requests.")]
    public async Task SaveToEmptyCache()
    {
        string testFolder = Path.GetFullPath("./test_no_cache");

        if (Directory.Exists(testFolder))
            Directory.Delete(testFolder, true);

        Environment.SetEnvironmentVariable(EnvironmentCache.EnvVarSpecCacheFolder, testFolder);

        IServiceProvider sp = new ServiceCollection()
            .AddLogging(x => x.AddConsole())
            .AddSingleton(new JsonSerializerOptions
            {
                WriteIndented = true
            })
            .AddSingleton<EnvironmentCache>()
            .AddSingleton<ISpecDatabaseCache, EnvironmentCache>(sp => sp.GetRequiredService<EnvironmentCache>())
            .AddSingleton<IAssetSpecDatabase, LspAssetSpecDatabase>()
            .BuildServiceProvider();

        EnvironmentCache cache = sp.GetRequiredService<EnvironmentCache>();
        IAssetSpecDatabase db = sp.GetRequiredService<IAssetSpecDatabase>();

        db.UseInternet = true;

        Assert.That(cache.LatestCommit, Is.Null);

        Assert.That(cache.IsUpToDateCache("abcdef"), Is.False);

        await db.InitializeAsync();

        Assert.That(cache.LatestCommit, Is.Not.Null);
        Assert.That(cache.IsUpToDateCache(cache.LatestCommit), Is.True);


        Assert.That(await cache.GetCachedInformationAsync(), Is.Not.Null);


        foreach (AssetSpecType type in db.Types.Values)
        {
            if (type.Commit == null)
                continue;

            Assert.That(await cache.GetCachedTypeAsync(type.Type), Is.Not.Null, $"For: {type.Type.Normalized.GetTypeName()}");
        }
    }
}
#endif