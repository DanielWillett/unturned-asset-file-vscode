using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

public static class BundleExtensions
{

    extension(IBundleProxy bundle)
    {
        /// <summary>
        /// Creates a <see cref="UnityObject"/> in a <see cref="IBundleProxy"/> from an asset name.
        /// </summary>
        /// <param name="assetName">The asset name, such as <c>Item</c>.</param>
        /// <param name="type">The type of asset to get.</param>
        /// <param name="parsingServices">Workspace services.</param>
        /// <returns>An object representing the given unity asset, or <see langword="null"/> if it's not present.</returns>
        public UnityObject? GetCorrespondingAsset(string assetName, IPropertyType type, ref FileEvaluationContext ctx)
        {
            if (!type.TryEvaluateType(out IType? actualType, ref ctx) || actualType is not IBundleAssetType bundleAssetType)
            {
                return null;
            }

            bool hasLock = false;
            TfmLock? @lock = null;

            try
            {
                DiscoveredBundle? bndl;
                while (true)
                {
                    bndl = bundle.Bundle;
                    if (bndl == null)
                        break;

                    @lock = bndl.GetLock(ctx.Services);
                    PlatformLockHelper.EnterLock(@lock, ref hasLock);
                    if (bndl != bundle.Bundle)
                    {
                        PlatformLockHelper.ExitLock(@lock);
                        hasLock = false;
                        continue;
                    }

                    break;
                }

                if (bndl == null)
                {
                    return null;
                }

                DiscoveredBundle.BundleData data = bndl.GetOrOpenNoLock(ctx.Services);
                if (data.AssetBundle == null)
                {
                    return null;
                }

                if (bundle.Path == null || bndl.IsLegacyBundle)
                {
                    if (bndl.TryLoadAssetFileInfo(
                        in data,
                        assetName,
                        out AssetFileInfo? fileInfo,
                        out string? assetPath))
                    {
                        return Create(bundle, bundleAssetType, data.AssetBundle!, fileInfo, ctx.Services, assetPath);
                    }
                }
                else
                {
                    string path = bundle.Path;
                    path += "/" + assetName;
                    AssetInformation assetInfo = ctx.Services.Database.Information;
                    if (assetInfo.BundleValidFileExtensions.TryGetValue(bundleAssetType.TypeName, out string[]? values)
                        && values != null)
                    {
                        foreach (string str in values)
                        {
                            string p = path + str;
                            if (bndl.TryLoadAssetFileInfo(
                                in data,
                                p,
                                out AssetFileInfo? fileInfo,
                                out string? assetPath))
                            {
                                return Create(bundle, bundleAssetType, data.AssetBundle, fileInfo, ctx.Services, assetPath);
                            }
                        }
                    }
                }

                return null;
            }
            finally
            {
                if (hasLock)
                    PlatformLockHelper.ExitLock(@lock!);
            }
        }
    }

    private static UnityObject Create(IBundleProxy bundle, IBundleAssetType type, AssetsFileInstance file, AssetFileInfo fileInfo, IParsingServices parsingServices, string assetPath)
    {
        return new UnityObject(type, assetPath, bundle, file, fileInfo, parsingServices);
    }
}