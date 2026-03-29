using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

public static class BundleExtensions
{
#if NET9_0_OR_GREATER
    private static void EnterLock(Lock @lock, ref bool hasLock) => @lock.Enter(@lock, ref hasLock);
    private static void ExitLock(Lock @lock) => @lock.Exit(@lock);
#else
    private static void EnterLock(object @lock, ref bool hasLock) => Monitor.Enter(@lock, ref hasLock);
    private static void ExitLock(object @lock) => Monitor.Exit(@lock);
#endif

    extension(IBundleProxy bundle)
    {
        /// <summary>
        /// Creates a <see cref="UnityObject"/> in a <see cref="IBundleProxy"/> from an asset name.
        /// </summary>
        /// <param name="assetName">The asset name, such as <c>Item</c>.</param>
        /// <param name="type">The type of asset to get.</param>
        /// <param name="parsingServices">Workspace services.</param>
        /// <returns>An object representing the given unity asset, or <see langword="null"/> if it's not present.</returns>
        public UnityObject? GetCorrespondingAsset(string assetName, UnityObjectAssetType type, IParsingServices parsingServices)
        {
            bool hasLock = false;
#if NET9_0_OR_GREATER
            Lock? @lock = null;
#else
            object? @lock = null;
#endif

            try
            {
                DiscoveredBundle? bndl;
                while (true)
                {
                    bndl = bundle.Bundle;
                    if (bndl == null)
                        break;

                    @lock = bndl.GetLock(parsingServices);
                    EnterLock(@lock, ref hasLock);
                    if (bndl != bundle.Bundle)
                    {
                        ExitLock(@lock);
                        hasLock = false;
                        continue;
                    }

                    break;
                }

                if (bndl == null)
                {
                    return null;
                }

                DiscoveredBundle.BundleData data = bndl.GetOrOpenNoLock(parsingServices);
                if (bundle.Path == null || bndl.IsLegacyBundle)
                {
                    if (bndl.TryLoadAssetBaseField(
                        in data,
                        assetName,
                        out AssetFileInfo? fileInfo,
                        out AssetTypeValueField? field,
                        out string? assetPath))
                    {
                        return Create(bundle, type, fileInfo, field, assetPath);
                    }
                }
                else
                {
                    string path = bundle.Path;
                    path += "/" + assetName;
                    AssetInformation assetInfo = parsingServices.Database.Information;
                    if (assetInfo.BundleValidFileExtensions.TryGetValue(type.TypeName, out string[]? values)
                        && values != null)
                    {
                        foreach (string str in values)
                        {
                            string p = path + str;
                            if (bndl.TryLoadAssetBaseField(
                                in data,
                                p,
                                out AssetFileInfo? fileInfo,
                                out AssetTypeValueField? field,
                                out string? assetPath))
                            {
                                return Create(bundle, type, fileInfo, field, assetPath);
                            }
                        }
                    }
                }

                return null;
            }
            finally
            {
                if (hasLock)
                    ExitLock(@lock!);
            }
        }
    }

    private static UnityObject? Create(IBundleProxy bundle, UnityObjectAssetType type, AssetFileInfo fileInfo, AssetTypeValueField field, string assetPath)
    {
        return new UnityObject(type, assetPath, bundle);
    }
}
