using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
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
            // makes debugging easier
            MultiThreaded = false
        };

        await db.InitializeAsync();

        Console.WriteLine(db.Types.Values.SelectMany(x => x.Properties.Where(x => !x.IsOverride)).Count());
        Console.WriteLine(db.Types.Values.SelectMany(x => x.Types.SelectMany(x => x.GetProperties(SpecPropertyContext.Property).Where(x => !x.IsOverride))).Count());

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
