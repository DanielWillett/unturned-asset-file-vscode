using System;
using System.Diagnostics;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// A Bundle or MasterBundle file.
/// </summary>
public sealed class DiscoveredBundle : IDisposable
{
    private BundleData _openedfile;
    private AssetsManager? _assetsManager;
    private InstallationEnvironment? _installationEnvironment;

    private ImmutableDictionary<long, string>? _pathCache;
    private ImmutableDictionary<string, int>? _nameCache;

    /// <summary>
    /// Whether or not the bundle is a legacy (unity3d) bundle.
    /// </summary>
    public bool IsLegacyBundle { get; }

    /// <summary>
    /// The directory this bundle applies to. This is the folder the bundle is in.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// The version of Unity Engine used to build this bundle.
    /// </summary>
    public UnityEngineVersion BuildVersion { get; internal set; }
    
    /// <summary>
    /// The file this bundle was discovered from.
    /// </summary>
    /// <remarks>
    /// For master bundles this is the <c>Masterbundle.dat</c> file.<br/>
    /// For legacy bundles this is the corresponding asset.
    /// </remarks>
    public string ConfigurationFile { get; }

    /// <summary>
    /// The file that contains the actual bundle contents.
    /// </summary>
    /// <remarks>For master bundles, this is the Windows masterbundle file.</remarks>
    public string BundleFile { get; }

    /// <summary>
    /// The version of this bundle.
    /// </summary>
    public int Version { get; }

    public DiscoveredBundle(bool isLegacyBundle, string directory, string configurationFile, string bundleFile, int version)
    {
        IsLegacyBundle = isLegacyBundle;
        Directory = directory;
        ConfigurationFile = configurationFile;
        BundleFile = bundleFile;
        Version = version;
        UpdateBuildVersion();
    }

    internal void UpdateBuildVersion()
    {
        try
        {
            UnityAssetBundleHeader header = UnityAssetBundleHeader.FromFile(BundleFile, out _);
            BuildVersion = header.EngineVersion;
        }
        catch (FormatException)
        {
            BuildVersion = default;
        }
    }

    internal void ApplyBundleFileChanges()
    {
        UpdateBuildVersion();
    }

    public bool IsReferencingFile(string file)
    {
        if (BundleFile.Equals(file, OSPathHelper.PathComparison) || ConfigurationFile.Equals(file, OSPathHelper.PathComparison))
            return true;

        if (IsLegacyBundle)
            return false;

        ReadOnlySpan<char> directory = OSPathHelper.GetDirectoryName(file);
        if (!directory.Equals(Directory, OSPathHelper.PathComparison))
            return false;

        ReadOnlySpan<char> fileName = OSPathHelper.GetFileName(file);
        if (OSPathHelper.IsExtension(fileName, ".hash"))
        {
            fileName = fileName[..^5];
        }
        else if (OSPathHelper.IsExtension(fileName, ".manifest"))
        {
            fileName = fileName[..^9];
        }

        ReadOnlySpan<char> expectedFileName = OSPathHelper.GetFileName(BundleFile);
        return CompareFileNameWithPrefix(fileName, "_linux", expectedFileName) || CompareFileNameWithPrefix(fileName, "_mac", expectedFileName);
    }

    private static bool CompareFileNameWithPrefix(ReadOnlySpan<char> fileName, string prefix, ReadOnlySpan<char> expected)
    {
        if (fileName.Length - prefix.Length != expected.Length)
        {
            return false;
        }

        int startIndex = expected.IndexOf('.');
        if (!expected.Slice(0, startIndex).Equals(fileName.Slice(0, startIndex), OSPathHelper.PathComparison)
            || !expected.Slice(startIndex + 1).Equals(fileName.Slice(startIndex + prefix.Length + 1), OSPathHelper.PathComparison))
        {
            return false;
        }

        return fileName.Slice(startIndex, prefix.Length).Equals(prefix, OSPathHelper.PathComparison);
    }

    /// <summary>
    /// Gets the lock used for this bundle.
    /// </summary>
    internal
#if NET9_0_OR_GREATER
        System.Threading.Lock
#else
        object
#endif
        GetLock(IParsingServices parsingServices)
    {
        InstallationEnvironment env = ApplyInstallationEnvironment(parsingServices);
        return env.AssetBundleLock;
    }

    private InstallationEnvironment ApplyInstallationEnvironment(IParsingServices parsingServices)
    {
        InstallationEnvironment env = parsingServices.Installation;
        InstallationEnvironment? originalValue = Interlocked.CompareExchange(ref _installationEnvironment, env, null);

        if (originalValue != null && !ReferenceEquals(originalValue, env))
        {
            throw new InvalidOperationException("This file does not belong to the provided InstallationEnvironment.");
        }

        return env;
    }

    /// <summary>
    /// Loads an asset by it's name or path. Name for legacy bundles and path for new bundles.
    /// </summary>
    internal bool TryLoadAssetBaseField(
        in BundleData data,
        string nameOrPath,
        [NotNullWhen(true)] out AssetFileInfo? fileInfo,
        [NotNullWhen(true)] out AssetTypeValueField? baseField,
        [NotNullWhen(true)] out string? path
    )
    {
        fileInfo = null;
        baseField = null;
        path = null;
        if (_nameCache == null || !_nameCache.TryGetValue(nameOrPath, out int index))
        {
            return false;
        }

        fileInfo = data.AssetBundle.file.Metadata.AssetInfos[index];

        AssetTypeValueField? val = data.Manager.GetBaseField(
            data.AssetBundle,
            fileInfo,
            AssetReadFlags.SkipMonoBehaviourFields
        );

        if (val is not { IsDummy: false })
            return false;

        return _pathCache != null && _pathCache.TryGetValue(fileInfo.PathId, out path);
    }

    /// <summary>
    /// Opens a reader for the bundle file.
    /// </summary>
    /// <exception cref="InvalidOperationException"><paramref name="parsingServices"/> has changed since the last time this was called.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="parsingServices"/>'s <see cref="InstallationEnvironment"/> has been disposed.</exception>
    public BundleData GetOrOpen(IParsingServices parsingServices)
    {
        InstallationEnvironment env = ApplyInstallationEnvironment(parsingServices);

        BundleData? file = _openedfile;
        if (file.HasValue)
        {
            return file.Value;
        }

        lock (env.AssetBundleLock)
        {
            return GetOrOpenIntl(env);
        }
    }

    /// <inheritdoc cref="GetOrOpen"/>
    internal BundleData GetOrOpenNoLock(IParsingServices parsingServices)
    {
        InstallationEnvironment env = ApplyInstallationEnvironment(parsingServices);

        BundleData? file = _openedfile;
        if (file.HasValue)
        {
            return file.Value;
        }

        return GetOrOpenIntl(env);
    }
    private BundleData GetOrOpenIntl(InstallationEnvironment env)
    {
        if (_openedfile.Info != null)
        {
            return _openedfile;
        }

        AssetsManager? assetsManager = env.AssetBundleManager;

        if (assetsManager == null)
        {
            assetsManager = _assetsManager ?? throw new ObjectDisposedException(nameof(InstallationEnvironment));
        }
        else
        {
            _assetsManager = assetsManager;
        }

        BundleFileInstance file = assetsManager.LoadBundleFile(BundleFile, false);
        AssetsFileInstance assetFile = assetsManager.LoadAssetsFileFromBundle(file, 0);
        _openedfile = new BundleData(file, assetFile, assetsManager);

        IList<AssetFileInfo> assets = assetFile.file.Metadata.AssetInfos;

        bool cacheNames = IsLegacyBundle;

        ImmutableDictionary<long, string>.Builder pathIdMap = ImmutableDictionary.CreateBuilder<long, string>();
        ImmutableDictionary<string, int>.Builder nameIndexMap = ImmutableDictionary.CreateBuilder<string, int>(StringComparer.Ordinal);

        int abIndex = assets.Count;
        for (int index = 0; index < assets.Count; index++)
        {
            AssetFileInfo obj = assets[index];
            AssetClassID classId = (AssetClassID)obj.TypeId;
            
            if (!env.RelevantBundleClasses.Contains(classId))
                continue;

            AssetTypeValueField baseField = assetsManager.GetBaseField(assetFile, obj);
            if (baseField.IsDummy)
                continue;

            if (cacheNames)
            {
                AssetTypeValue? nameValue = baseField["m_Name"].Value;
                if (nameValue is { ValueType: AssetValueType.String })
                {
                    string str = nameValue.AsString;
                    if (!string.IsNullOrEmpty(str))
                    {
                        nameIndexMap[str] = index;
                    }
                }

                continue;
            }
            else if (index > abIndex)
            {
                if (pathIdMap.TryGetValue(obj.PathId, out string path))
                {
                    nameIndexMap[path] = index;
                }
            }

            if (classId != AssetClassID.AssetBundle || abIndex >= 0)
                continue;

            abIndex = index;
            AssetTypeValueField container = baseField["m_Container.Array"];
            if (container.IsDummy)
                continue;

            foreach (AssetTypeValueField data in container.Children)
            {
                if (data.Children.Count <= 1)
                    continue;

                AssetTypeValue pathValue = data[0].Value;
                AssetTypeValue? pathIdValue = data[1]["asset.m_PathID"].Value;
                if (pathIdValue == null
                 || pathValue.ValueType != AssetValueType.String
                 || pathIdValue.ValueType != AssetValueType.Int64)
                {
                    continue;
                }

                string name = pathValue.AsString;
                long pathId = pathIdValue.AsLong;
                if (pathId == 0)
                    continue;

                pathIdMap![pathId] = name;
            }
        }

        if (!cacheNames)
        {
            for (int index = 0; index < abIndex; index++)
            {
                AssetFileInfo obj = assets[index];
                AssetClassID classId = (AssetClassID)obj.TypeId;

                if (!env.RelevantBundleClasses.Contains(classId))
                    continue;

                if (pathIdMap.TryGetValue(obj.PathId, out string path))
                {
                    nameIndexMap[path] = index;
                }
            }
        }

        _nameCache = nameIndexMap.ToImmutable();
        _pathCache = pathIdMap.ToImmutable();

        return new BundleData(file, assetFile, assetsManager);
    }

    public void Dispose()
    {
        InstallationEnvironment? env = _installationEnvironment;
        if (env == null)
            return;

        lock (env.AssetBundleLock)
        {
            BundleData data = _openedfile;
            _openedfile = default;
            BundleFileInstance? file = data.Info;
            if (file == null)
                return;

            AssetsManager? manager = _assetsManager;
            try
            {
                if (manager == null)
                    file.file.Close();
                else
                    manager.UnloadBundleFile(file);
            }
            catch (ObjectDisposedException) { }
            catch (NullReferenceException) { }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            _assetsManager = null;
        }
    }

    public record struct BundleData(
        BundleFileInstance Info,
        AssetsFileInstance AssetBundle,
        AssetsManager Manager
    );
}