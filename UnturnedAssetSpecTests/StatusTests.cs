using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Text.Json;

namespace UnturnedAssetSpecTests;

#pragma warning disable VSTHRD200

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

        _runner = AssetSpecDatabase.FromOffline(useInstallDir: true);

        await _runner.InitializeAsync();

        JsonDocument? doc = _runner.StatusInformation;
        AssertValidDoc(doc);
    }

    [Test]
    [Ignore("Makes network requests.")]
    public async Task TestFromInternet()
    {
        _runner = AssetSpecDatabase.FromOffline(useInstallDir: false);

        await _runner.InitializeAsync();

        JsonDocument? doc = _runner.StatusInformation;
        AssertValidDoc(doc);
    }

    [Test]
    [Ignore("Makes network requests.")]
    public async Task TestInitializedSuccessfully()
    {
        _runner = AssetSpecDatabase.FromOnline(useInstallDir: false);

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

#pragma warning restore VSTHRD200