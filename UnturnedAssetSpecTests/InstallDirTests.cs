using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

public class InstallDirTests
{
    private InstallDirUtility _runner;

    public const string ExpectedInstallDir = @"C:\Program Files (x86)\Steam\steamapps\common\Unturned";
    public const string ExpectedWorkshopDir = @"C:\Program Files (x86)\Steam\steamapps\workshop\content\304930";

    [SetUp]
    public void Setup()
    {
        _runner = new InstallDirUtility("Unturned", "304930");
    }

    [Test, Platform("win")]
    public void TestWindows()
    {
        if (!Directory.Exists(ExpectedInstallDir) || !Directory.Exists(ExpectedWorkshopDir))
        {
            Assert.Inconclusive("Game not installed where it's expected.");
        }

        GameInstallDir installDir = _runner.InstallDirectory;

        Assert.That(installDir.BaseFolder, Is.EqualTo(ExpectedInstallDir));
        Assert.That(installDir.WorkshopFolder, Is.EqualTo(ExpectedWorkshopDir));
    }
}