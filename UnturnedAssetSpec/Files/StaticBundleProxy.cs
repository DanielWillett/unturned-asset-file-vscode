using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// A <see cref="IBundleProxy"/> for a bundle outside the current <see cref="InstallationEnvironment"/>.
/// </summary>
public class StaticBundleProxy : IBundleProxy, IEquatable<StaticBundleProxy?>, IDisposable
{
    private readonly bool _forceConsolidateShaders;
    private readonly DiscoveredBundleReference? _reference;
    private readonly bool _ownsBundle;
    private DiscoveredBundle? _bundle;

    /// <inheritdoc />
    public string? Path { get; }

    /// <inheritdoc />
    public DiscoveredBundle Bundle => _bundle ?? throw new ObjectDisposedException(nameof(StaticBundleProxy));

    /// <inheritdoc />
    public bool ConvertShadersToStandard => AssetBundleVersion < 2;

    /// <inheritdoc />
    public bool ConsolidateShaders => AssetBundleVersion < 3 || _forceConsolidateShaders;

    /// <summary>
    /// The version of this asset bundle for this <see cref="IBundleProxy"/>.
    /// </summary>
    public int AssetBundleVersion { get; }

    internal StaticBundleProxy(
        int version,
        bool forceForceConsolidateShaders,
        DiscoveredBundle bundle,
        string relativePath,
        DiscoveredBundleReference? reference,
        bool ownsBundle)
    {
        AssetBundleVersion = version;
        _forceConsolidateShaders = forceForceConsolidateShaders;
        _reference = reference;
        _ownsBundle = ownsBundle;
        _bundle = bundle;
        Path = relativePath;
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        if (string.IsNullOrEmpty(Bundle.Prefix))
        {
            return string.IsNullOrEmpty(Path) ? Bundle.BundleFile : $"{Bundle.BundleFile}/{Path}";
        }

        return string.IsNullOrEmpty(Path) ? $"{Bundle.BundleFile}/{Bundle.Prefix}" : $"{Bundle.BundleFile}/{Bundle.Prefix}/{Path}";
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _reference?.Dispose();
        DiscoveredBundle? bundle = Interlocked.Exchange(ref _bundle, null);
        if (_ownsBundle)
        {
            bundle?.Dispose();
        }
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(StaticBundleProxy? other)
    {
        if (other == null)
        {
            return false;
        }

        return other.Bundle == Bundle
               && other.AssetBundleVersion == AssetBundleVersion
               && other._forceConsolidateShaders == _forceConsolidateShaders
               && string.Equals(other.Path, Path, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return Equals(obj as StaticBundleProxy);
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return HashCode.Combine(Bundle, Path);
    }

    /// <summary>
    /// Create a <see cref="StaticBundleProxy"/> for the given file.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="parsingServices"></param>
    /// <returns></returns>
    public static StaticBundleProxy? Create(IWorkspaceFile file, IParsingServices parsingServices, IBundleCache bundleCache)
    {
        ISourceFile sourceFile = file.SourceFile;
        if (sourceFile is ILocalizationSourceFile lcl)
        {
            sourceFile = lcl.Asset;
        }

        if (sourceFile is not IAssetSourceFile assetSrc)
        {
            return null;
        }

        IDictionarySourceNode asset = assetSrc.AssetData;

        DiscoveredBundle? bundle = null;
        DiscoveredBundleReference? reference = null;
        try
        {
            Lazy<ILogger<StaticBundleProxy>> logger = new Lazy<ILogger<StaticBundleProxy>>(parsingServices.CreateLogger<StaticBundleProxy>, LazyThreadSafetyMode.None);
            if (asset.TryGetPropertyValue("Master_Bundle_Override", out IValueSourceNode? tempValue))
            {
                OneOrMore<DiscoveredBundle> matchingBundles = parsingServices.Installation.FindMasterBundleByName(tempValue.Value);
                if (matchingBundles.IsNull)
                {
                    logger.Value.LogWarning("No masterbundle found with name: \"{0}\" for asset {1}.", tempValue.Value, file.File);
                    bundle = null;
                }
                else
                {
                    bundle = matchingBundles[0];
                }
            }
            else if (asset.ContainsProperty("Exclude_From_Master_Bundle"))
            {
                bundle = null;
            }
            else
            {
                bundle = parsingServices.Installation.FindMasterBundleForPath(file.File);
                if (bundle == null)
                {
                    string? masterBundlePath = BundleExtensions.FindMasterBundleForPath(file.File, parsingServices.GameDirectory);
                    if (masterBundlePath != null)
                    {
                        try
                        {
                            reference = bundleCache.GetOrAddMasterBundle(masterBundlePath);
                            bundle = reference?.Bundle;
                        }
                        catch (FileNotFoundException)
                        {
                            bundle = null;
                        }
                        catch (DirectoryNotFoundException)
                        {
                            bundle = null;
                        }
                    }
                }
            }

            int assetBundleVersion;
            if (asset.TryGetPropertyValue("Asset_Bundle_Version", out tempValue)
                && TryParseInt32(tempValue, parsingServices, out int assetBundleVersionFromAsset))
            {
                assetBundleVersion = Math.Min(6, Math.Max(1, assetBundleVersionFromAsset));
            }
            else
            {
                assetBundleVersion = 1;
            }

            bool forceConsolidateShaders = asset.ContainsProperty("Enable_Shader_Consolidation") && !asset.ContainsProperty("Disable_Shader_Consolidation");

            if (bundle != null)
            {
                assetBundleVersion = Math.Max(bundle.Version, assetBundleVersion);

                string relativePath;
                if (asset.TryGetPropertyValue("Bundle_Override_Path", out tempValue))
                {
                    relativePath = tempValue.Value;
                }
                else if (asset.TryGetPropertyValue("Bundle_Path_Include_Filename", out tempValue)
                         && TryParseBoolean(tempValue, parsingServices, out bool includeFilename)
                         && includeFilename)
                {
                    relativePath = OSPathHelper.ReplaceWithUnixSeparators(
                        OSPathHelper.RemoveExtension(file.File.AsSpan())[bundle.Directory.Length..]
                    );
                }
                else
                {
                    relativePath = OSPathHelper.ReplaceWithUnixSeparators(
                        OSPathHelper.GetDirectoryName(file.File.AsSpan())[bundle.Directory.Length..]
                    );
                }

                return new StaticBundleProxy(assetBundleVersion, forceConsolidateShaders, bundle, relativePath, reference, false);
            }

            string? bundleOverridePath = null;
            if (asset.TryGetPropertyValue("Bundle_Override_Path", out IValueSourceNode? value))
            {
                bundleOverridePath = value.Value;
            }

            string unity3dPath;
            if (bundleOverridePath != null)
            {
                int fileNameIndex = bundleOverridePath.LastIndexOf('/');
                ReadOnlySpan<char> fileName = fileNameIndex >= 0
                    ? bundleOverridePath.AsSpan(fileNameIndex + 1)
                    : bundleOverridePath;

                if (parsingServices.GameDirectory.TryGetInstallDirectory(out GameInstallDir dir))
                {
                    unity3dPath = OSPathHelper.CombineWithBaseFolderAndFixDirectorySeparators(dir.BaseFolder, fileName);
                }
                else
                {
                    unity3dPath = string.Empty;
                }
            }
            else
            {
                unity3dPath = OSPathHelper.CombineAndConcat(OSPathHelper.GetDirectoryName(file.File), assetSrc.AssetName, ".unity3d");
            }

            bundle = new DiscoveredBundle(
                true,
                System.IO.Path.GetDirectoryName(unity3dPath) ?? string.Empty,
                assetSrc.WorkspaceFile.File,
                unity3dPath,
                null,
                assetBundleVersion
            );

            return new StaticBundleProxy(assetBundleVersion, forceConsolidateShaders, bundle, string.Empty, null, true);
        }
        catch
        {
            reference?.Dispose();
            bundle?.Dispose();
            throw;
        }
    }

    private static bool TryParseBoolean(IValueSourceNode value, IParsingServices parsingServices, out bool @bool)
    {
        TypeParserArgs<bool> args = new TypeParserArgs<bool>
        {
            Type = BooleanType.Instance,
            ValueNode = value,
            ParentNode = value.Parent
        };

        FileEvaluationContext ctx = new FileEvaluationContext(
            parsingServices,
            value.File,
            value.Parent == value.File ? AssetDatPropertyPosition.Root : AssetDatPropertyPosition.Asset
        );

        if (TypeParsers.Boolean.TryParse(ref args, ref ctx, out Optional<bool> v))
        {
            @bool = v.Value;
            return true;
        }

        @bool = false;
        return false;
    }

    private static bool TryParseInt32(IValueSourceNode value, IParsingServices parsingServices, out int @int)
    {
        TypeParserArgs<int> args = new TypeParserArgs<int>
        {
            Type = Int32Type.Instance,
            ValueNode = value,
            ParentNode = value.Parent
        };

        FileEvaluationContext ctx = new FileEvaluationContext(
            parsingServices,
            value.File,
            value.Parent == value.File ? AssetDatPropertyPosition.Root : AssetDatPropertyPosition.Asset
        );

        if (TypeParsers.Int32.TryParse(ref args, ref ctx, out Optional<int> v))
        {
            @int = v.Value;
            return true;
        }

        @int = 0;
        return false;
    }
}

/// <summary>
/// Caches bundles outside the current <see cref="InstallationEnvironment"/>.
/// </summary>
public interface IBundleCache
{
    /// <summary>
    /// Get a masterbundle from a cache from the given <c>MasterBundle.dat</c> file.
    /// </summary>
    /// <param name="configPath">Full path to the <c>MasterBundle.dat</c> file.</param>
    /// <param name="bundle">
    /// The bundle if found, otherwise <see langword="null"/>.
    /// If a value is returned, it must be disposed when the caller is done with it.
    /// </param>
    /// <exception cref="FileNotFoundException"><paramref name="configPath"/> not found.</exception>
    /// <exception cref="DirectoryNotFoundException">Directory of <paramref name="configPath"/> not found.</exception>
    /// <exception cref="IOException">Failed to read file at <paramref name="configPath"/>.</exception>
    /// <returns><see langword="true"/> if the masterbundle is already cached, otherwise <see langword="false"/>.</returns>
    bool TryGetCachedMasterBundle(string configPath, [NotNullWhen(true)] out DiscoveredBundleReference? bundle);

    /// <summary>
    /// Get a masterbundle from a cache, or create a new one from the given <c>MasterBundle.dat</c> file.
    /// </summary>
    /// <param name="configPath">Full path to the <c>MasterBundle.dat</c> file.</param>
    /// <exception cref="FileNotFoundException"><paramref name="configPath"/> not found.</exception>
    /// <exception cref="DirectoryNotFoundException">Directory of <paramref name="configPath"/> not found.</exception>
    /// <exception cref="IOException">Failed to read file at <paramref name="configPath"/>.</exception>
    /// <returns>
    /// The read bundle, or <see langword="null"/> if the bundle is invalid.
    /// If a value is returned, it must be disposed when the caller is done with it.
    /// </returns>
    DiscoveredBundleReference? GetOrAddMasterBundle(string configPath);

    /// <summary>
    /// Returns a bundle gotten from the cache. Used by <see cref="DiscoveredBundleReference"/> on dispose.
    /// </summary>
    void ReturnMasterBundle(DiscoveredBundle bundle);
}

public sealed class DiscoveredBundleReference : IDisposable
{
    private int _disposed;
    public DiscoveredBundle Bundle { get; }
    public IBundleCache Cache { get; }

    public DiscoveredBundleReference(DiscoveredBundle bundle, IBundleCache cache)
    {
        Bundle = bundle;
        Cache = cache;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            Cache.ReturnMasterBundle(Bundle);
        }
    }
}