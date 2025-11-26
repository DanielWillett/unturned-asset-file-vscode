using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

public class AssetSpecValidity
{
    private static bool _hasRanIntoError;

    [Test, NonParallelizable]
    public async Task CheckSpecValidity()
    {
        _hasRanIntoError = false;

        InstallDirUtility util = new InstallDirUtility("NotUnturned", "999999");

        util.TryGetInstallDirectory(out _);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

        TestAssetSpecDatabase db = new TestAssetSpecDatabase(util)
        {
            UseInternet = false,
            MultiThreaded = false
        };

        await db.InitializeAsync();

        Assert.That(_hasRanIntoError, Is.False);

        _hasRanIntoError = false;
    }

    private class TestAssetSpecDatabase : AssetSpecDatabase
    {
        public TestAssetSpecDatabase(InstallDirUtility installDir) : base(installDir)
        {
            
        }

        protected override void Log(string msg)
        {
            if (!(msg.Contains("internet disabled") || msg.StartsWith("InstallDirUtility >>")))
            {
                _hasRanIntoError = true;
            }

            Console.WriteLine(msg);
        }
    }
}
