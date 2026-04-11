using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

partial class DiscoveredDatFile : IBundleProxy
{
    // uses forward slashes always, does not include bundle root
    private string? _bundlePath;
    private DiscoveredBundle? _bundle;
    private bool _ownsBundle;

    bool IBundleProxy.Exists
    {
        get
        {
            AssertBundleProxyCreated();
            return _bundle != null;
        }
    }

    DiscoveredBundle? IBundleProxy.Bundle
    {
        get
        {
            AssertBundleProxyCreated();
            DiscoveredBundle? oldBundle = _bundle;
            while (oldBundle is { IsDisposed: true })
            {
                DiscoveredBundle? replacement = oldBundle.BundleReplacement;
                DiscoveredBundle? newBundle = Interlocked.CompareExchange(ref _bundle, replacement, oldBundle);
                if (newBundle != oldBundle)
                    return newBundle;

                oldBundle = replacement;
            }

            return oldBundle;
        }
    }

    bool IBundleProxy.ConvertShadersToStandard
    {
        get
        {
            AssertBundleProxyCreated();
            return EffectiveBundleVersion < 2;
        }
    }

    bool IBundleProxy.ConsolidateShaders
    {
        get
        {
            AssertBundleProxyCreated();
            return EffectiveBundleVersion < 3 || (_bundleFlags & 8) != 0;
        }
    }

    string IBundleProxy.Path
    {
        get
        {
            AssertBundleProxyCreated();
            return _bundlePath;
        }
    }
#pragma warning disable CS8774

    [MemberNotNull(nameof(_bundlePath))]
    private void AssertBundleProxyCreated()
    {
        if ((_bundleFlags & 4) == 0)
            throw new InvalidOperationException("Use GetBundleProxy to create a bundle broxy for this asset.");
    }

#pragma warning restore CS8774

    private void CreateBundleInfo(IParsingServices services)
    {
        DiscoveredBundle? bundle;
        int assetBundleVersion = AssetBundleVersion;
        if (IsDefaultBundle)
        {
            bundle = services.Installation.FindMasterBundleForPath(FilePath);
        }
        else if (!string.IsNullOrEmpty(_masterBundleName))
        {
            OneOrMore<DiscoveredBundle> masterBundles = services.Installation.FindMasterBundleByName(_masterBundleName);
            bundle = masterBundles.FirstOrDefault();
        }
        else
        {
            // Exclude_From_Master_Bundle
            string? bundleOverridePath = _bundleOverridePath;
            string unity3dPath;
            _bundlePath = string.Empty;
            if (bundleOverridePath != null)
            {
                int fileNameIndex = bundleOverridePath.LastIndexOf('/');
                ReadOnlySpan<char> fileName = fileNameIndex >= 0
                    ? bundleOverridePath.AsSpan(fileNameIndex + 1)
                    : bundleOverridePath;

                if (services.GameDirectory.TryGetInstallDirectory(out GameInstallDir dir))
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
                unity3dPath = OSPathHelper.CombineAndConcat(OSPathHelper.GetDirectoryName(FilePath), AssetName, ".unity3d");
            }

            _bundle = new DiscoveredBundle(true, Path.GetDirectoryName(unity3dPath) ?? string.Empty, FilePath, unity3dPath, null, assetBundleVersion);
            _ownsBundle = true;
            _bundleFlags = (_bundleFlags & ~(0xFFF << 20)) | ((assetBundleVersion & 0xFFF) << 20) | 4;
            return;
        }

        if (bundle != null)
        {
            string? bundleOverridePath = _bundleOverridePath;
            if (!string.IsNullOrEmpty(bundleOverridePath))
            {
                _bundlePath = bundleOverridePath;
            }
            else if (_bundlePathIncludeFilename)
            {
                ReadOnlySpan<char> path = OSPathHelper.RemoveExtension(FilePath.AsSpan());

                // make relative to bundle path
                path = path.Slice(bundle.Directory.Length);
                
                _bundlePath = OSPathHelper.ReplaceWithUnixSeparators(path);
            }
            else
            {
                ReadOnlySpan<char> path = OSPathHelper.GetDirectoryName(FilePath);

                // make relative to bundle path
                path = path.Slice(bundle.Directory.Length);

                _bundlePath = OSPathHelper.ReplaceWithUnixSeparators(path);
            }

            _bundleFlags = (_bundleFlags & ~(0xFFF << 20)) | ((Math.Max(bundle.Version, assetBundleVersion) & 0xFFF) << 20) | 4;
        }
        else
        {
            _bundleFlags |= 4;
        }

        _ownsBundle = false;
        _bundle = bundle;
    }
}