using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace UnturnedAssetSpecTests;

public class StatusTests
{
    private AssetSpecDatabase? _runner;
    
    private static void AssertValidDoc(JsonDocument? doc)
    {
        Assert.That(doc, Is.Not.Null);
        doc.RootElement.GetProperty("Game").GetProperty("Major_Version");
        doc.RootElement.GetProperty("Game").GetProperty("Minor_Version");
        doc.RootElement.GetProperty("Game").GetProperty("Patch_Version");
    }

    [Test]
    public async Task TestFromLocal()
    {
        if (!Directory.Exists(InstallDirTests.ExpectedInstallDir))
        {
            Assert.Inconclusive("Game not installed where it's expected.");
        }

        _runner = new AssetSpecDatabase { UseInternet = false, MultiThreaded = false };

        await _runner.InitializeAsync();

        JsonDocument? doc = _runner.StatusInformation;
        AssertValidDoc(doc);
    }

    [Test]
    public async Task TestFromInternet()
    {
        _runner = new AssetSpecDatabase(new InstallDirUtility("NotUnturned", "Not304930")) { UseInternet = true };

        await _runner.InitializeAsync();

        JsonDocument? doc = _runner.StatusInformation;
        AssertValidDoc(doc);
    }

    [Test]
    public async Task TestInitializedSuccessfully()
    {
        _runner = new AssetSpecDatabase { UseInternet = true };

        await _runner.InitializeAsync();

        Assert.That(_runner.CurrentGameVersion, Is.Not.Null);
        Assert.That(_runner.CurrentGameVersion.Major, Is.EqualTo(3));

        Assert.That(_runner.NPCAchievementIds, Is.Not.Null);
        Assert.That(_runner.NPCAchievementIds, Does.Contain("Soulcrystal"));
    }

    [TearDown]
    public void TearDown()
    {
        _runner?.Dispose();
    }
}