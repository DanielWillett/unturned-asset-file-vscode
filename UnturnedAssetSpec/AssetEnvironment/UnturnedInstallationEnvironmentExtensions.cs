using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
public static class UnturnedInstallationEnvironmentExtensions
{
    public static void AddUnturnedSearchableDirectories(this InstallationEnvironment env, GameInstallDir installDir)
    {
        string baseFolder = installDir.BaseFolder;

        env.AddSearchableDirectoryIfExists(Path.Combine(baseFolder, "Bundles"));
        env.AddSearchableDirectoryIfExists(Path.Combine(baseFolder, "Sandbox"));
        string maps = Path.Combine(baseFolder, "Maps");
        if (Directory.Exists(maps))
        {
            foreach (string mapFolder in Directory.EnumerateDirectories(maps, "*", SearchOption.TopDirectoryOnly))
            {
                if (File.Exists(Path.Combine(mapFolder, "Level.dat")))
                    env.AddSearchableDirectoryIfExists(Path.Combine(mapFolder, "Bundles"));
            }
        }

        if (installDir.WorkshopFolder == null || !Directory.Exists(installDir.WorkshopFolder))
        {
            return;
        }

        List<ulong>? disabledMods = null;
        foreach (string modFolder in Directory.EnumerateDirectories(installDir.WorkshopFolder, "*", SearchOption.TopDirectoryOnly))
        {
            if (!UnturnedUgcUtility.TryGetUgcType(modFolder, out UnturnedUgcUtility.UgcType type) || type == UnturnedUgcUtility.UgcType.Localization)
                continue;

            ulong.TryParse(Path.GetFileName(modFolder), NumberStyles.Number, CultureInfo.InvariantCulture, out ulong thisModId);

            if (!UnturnedUgcUtility.IsUgcEnabled(thisModId, baseFolder, ref disabledMods))
            {
                continue;
            }

            env.AddSearchableDirectoryIfExists(type == UnturnedUgcUtility.UgcType.Map
                ? Path.Combine(modFolder, "Bundles")
                : modFolder
            );
        }
    }
}
