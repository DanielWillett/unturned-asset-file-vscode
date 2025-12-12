using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

#pragma warning disable VSTHRD200

public class DownloadActionButtonsTest
{
    private AssetSpecDatabase? _runner;
    
    private void AssertValidButtons()
    {
        Assert.That(_runner!.ValidActionButtons.Count, Is.Not.EqualTo(0));
        Assert.That(_runner.ValidActionButtons, Does.Contain("Take"));
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

        AssertValidButtons();
    }

    [Test]
    [Ignore("Makes network requests.")]
    public async Task TestFromInternet()
    {
        _runner = new AssetSpecDatabase(new InstallDirUtility("NotUnturned", "Not304930")) { UseInternet = true };

        await _runner.InitializeAsync();

        AssertValidButtons();
    }

    [Test]
    [Ignore("Makes network requests.")]
    public async Task TestInitializedSuccessfully()
    {
        _runner = new AssetSpecDatabase { UseInternet = true };

        await _runner.InitializeAsync();

        AssertValidButtons();
    }

    [TearDown]
    public void TearDown()
    {
        _runner?.Dispose();
    }
}

#pragma warning restore VSTHRD200