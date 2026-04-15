using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

public static class BundleExtensions
{

    extension(IBundleProxy bundle)
    {
        /// <summary>
        /// A bundle that doesn't exist.
        /// </summary>
        public static IBundleProxy Null => NullBundleProxy.Instance;

        /// <summary>
        /// Creates a <see cref="UnityObject"/> in a <see cref="IBundleProxy"/> from an asset name.
        /// </summary>
        /// <param name="assetName">The asset name, such as <c>Item</c>.</param>
        /// <param name="type">The type of asset to get.</param>
        /// <param name="parsingServices">Workspace services.</param>
        /// <returns>An object representing the given unity asset, or <see langword="null"/> if it's not present.</returns>
        public UnityObject? GetCorrespondingAsset(string assetName, IPropertyType type, ref FileEvaluationContext ctx)
        {
            if (bundle is NullBundleProxy)
            {
                return null;
            }

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

        /// <summary>
        /// Enumerate all <see cref="UnityObject"/>s logically contained in the bundle.
        /// </summary>
        /// <param name="services">Workspace services.</param>
        /// <param name="classId">Type filter. If <see cref="AssetClassID.Object"/>, all assets will be selected. Use <paramref name="selector"/> for more control.</param>
        /// <param name="selector">Predicate applied to each asset's header.</param>
        public BundleProxyEnumerator EnumerateAssets(
            IParsingServices services,
            AssetClassID classId = AssetClassID.Object,
            Func<AssetFileInfo, bool>? selector = null
        )
        {
            BundleProxyEnumerator enumerator = new BundleProxyEnumerator(services, bundle, classId, selector);

            if (bundle is NullBundleProxy)
            {
                return enumerator;
            }

            try
            {
                if (!BundleUtility.TryLockBundle(
                        bundle,
                        services,
                        ref enumerator.Lock,
                        ref enumerator.HasLock,
                        out enumerator.Bundle,
                        out enumerator.File,
                        out enumerator.Manager))
                {
                    throw new InvalidOperationException("Unable to enumerate children. Bundle could not be loaded.");
                }

                if (bundle.Path != null)
                {
                    enumerator.Prefix = enumerator.Bundle.Prefix == null
                        ? bundle.Path
                        : OSPathHelper.CombineWithUnixSeparators(enumerator.Bundle.Prefix, bundle.Path);
                }

                return enumerator;
            }
            catch
            {
                if (enumerator.HasLock)
                    PlatformLockHelper.ExitLock(enumerator.Lock!);
                throw;
            }
        }

    }

    private static UnityObject Create(IBundleProxy bundle, IBundleAssetType type, AssetsFileInstance file, AssetFileInfo fileInfo, IParsingServices parsingServices, string assetPath)
    {
        return new UnityObject(type, assetPath, bundle, file, fileInfo, parsingServices);
    }

    /// <summary>
    /// Walks up the folder hierarchy until a masterbundle is found.
    /// </summary>
    /// <param name="file">Name of the file to find a masterbundle for.</param>
    /// <returns>The full path to the <c>MasterBundle.dat</c> file, or <see langword="null"/> if one is not found.</returns>
    public static string? FindMasterBundleForPath(ReadOnlySpan<char> file, InstallDirUtility installDir)
    {
        ReadOnlySpan<char> baseFolder = ReadOnlySpan<char>.Empty;
        if (installDir.TryGetInstallDirectory(out GameInstallDir gameInstallDir))
        {
            if (OSPathHelper.Contains(gameInstallDir.BaseFolder, file))
            {
                baseFolder = gameInstallDir.BaseFolder;
            }
            else if (gameInstallDir.WorkshopFolder != null
                     && OSPathHelper.Contains(gameInstallDir.WorkshopFolder, file))
            {
                Span<char> options = stackalloc char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
                int startIndex = gameInstallDir.WorkshopFolder!.Length + 1;
                int nextIndex = file.Slice(startIndex).IndexOfAny(options);
                baseFolder = nextIndex < 0 ? ReadOnlySpan<char>.Empty : file.Slice(0, startIndex + nextIndex);
            }
        }

        for (ReadOnlySpan<char> directory = OSPathHelper.GetDirectoryName(file);
             !directory.IsEmpty;
             directory = OSPathHelper.GetDirectoryName(directory)
        )
        {
            string masterBundlePath = OSPathHelper.CombineAndConcat(directory, "MasterBundle.dat", ReadOnlySpan<char>.Empty);
            if (File.Exists(masterBundlePath))
            {
                return masterBundlePath;
            }

            if (!baseFolder.IsEmpty && !OSPathHelper.Contains(baseFolder, directory))
            {
                break;
            }
        }

        return null;
    }
}


/// <summary>
/// Enumerates bundle assets within a <see cref="IBundleProxy"/>.
/// <para>
/// Must be disposed after usage to avoid deadlocks.
/// </para>
/// </summary>
/// <remarks>Objects are created lazily so they will only be read as the enumerator progresses.</remarks>
public sealed class BundleProxyEnumerator : IEnumerator<UnityObject>
{
    private readonly IParsingServices _services;
    private readonly IBundleProxy _bundle;
    private readonly AssetClassID _classId;
    private readonly Func<AssetFileInfo, bool>? _selector;

    internal TfmLock? Lock;
    internal string? Prefix;
    internal bool HasLock;
    internal DiscoveredBundle? Bundle;
    internal AssetsFileInstance? File;
    internal AssetsManager? Manager;
    private int _state;
    private ImmutableDictionary<long, string>.Enumerator _underlyingEnumerator;

    internal BundleProxyEnumerator(
        IParsingServices services,
        IBundleProxy bundle,
        AssetClassID classId = 0,
        Func<AssetFileInfo, bool>? selector = null)
    {
        _services = services;
        _bundle = bundle;
        _classId = classId;
        _selector = selector;
    }

#nullable disable
    /// <inheritdoc />
    public UnityObject Current { get; private set; }

    object IEnumerator.Current => Current;

#nullable restore

    /// <inheritdoc />
    public void Reset()
    {
        if (_state <= 0)
            return;

        _underlyingEnumerator.Reset();
        _state = 1;
    }

    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Current))]
    public bool MoveNext()
    {
        switch (_state)
        {
            case 0:
                if (Bundle?.PathCache == null || Bundle.FilePreloadCache == null)
                {
                    return false;
                }

                // ReSharper disable once GenericEnumeratorNotDisposed
                _underlyingEnumerator = Bundle.PathCache.GetEnumerator();
                _state = 1;
                goto case 1;

            case 1:
                while (_underlyingEnumerator.MoveNext())
                {
                    KeyValuePair<long, string> pair = _underlyingEnumerator.Current;
                    if (Prefix != null && !OSPathHelper.Contains(Prefix, pair.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!Bundle!.FilePreloadCache!.TryGetValue(pair.Key, out AssetFileInfo? fileInfo))
                    {
                        continue;
                    }

                    if (!_services.Installation.KnownUnityClassTypes.TryGetValue(
                            (AssetClassID)fileInfo.TypeId, out IBundleAssetType? type)
                       )
                    {
                        continue;
                    }

                    if (_classId != AssetClassID.Object && (AssetClassID)fileInfo.TypeId != _classId)
                    {
                        continue;
                    }

                    if (_selector != null && _selector(fileInfo))
                    {
                        continue;
                    }

                    Current = new UnityObject(type, pair.Value, _bundle, File!, fileInfo, _services);
#pragma warning disable CS8775
                    return true;
#pragma warning restore CS8775
                }

                _state = 2;
                goto default;

            default:
                return false;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_state > 0)
        {
            _underlyingEnumerator.Dispose();
        }
        if (!HasLock)
            return;

        PlatformLockHelper.ExitLock(Lock!);
        HasLock = false;
    }
}