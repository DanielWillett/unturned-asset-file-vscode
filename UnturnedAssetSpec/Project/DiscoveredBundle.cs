using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// A Bundle or MasterBundle file.
/// </summary>
public sealed class DiscoveredBundle : IDisposable, IEquatable<DiscoveredBundle>
{
    private BundleData _openedfile;
    private AssetsManager? _assetsManager;
    private InstallationEnvironment? _installationEnvironment;

    /// <summary>
    /// When this bundle is disposed, if a new file took its place,
    /// this will contain a reference to the new file.
    /// </summary>
    internal DiscoveredBundle? BundleReplacement;

    internal int Index = -1;
    internal bool IsDisposed;

    private ImmutableDictionary<long, string>? _pathCache;
    private ImmutableDictionary<string, int>? _nameCache;

    private BundleOperatingSystems? _operatingSystems;
    private UnityEngineVersion? _buildVersion;

    /// <summary>
    /// Contains GameObjects, Transforms, and RectTransforms.
    /// </summary>
    internal ImmutableDictionary<long, AssetFileInfo>? FilePreloadCache;

    /// <summary>
    /// Whether or not the bundle is a legacy (unity3d) bundle.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Prefix))]
    public bool IsLegacyBundle { get; }

    /// <summary>
    /// The directory this bundle applies to. This is the folder the bundle is in.
    /// </summary>
    public string Directory { get; }

    /// <summary>
    /// The version of Unity Engine used to build this bundle.
    /// </summary>
    public UnityEngineVersion BuildVersion
    {
        get
        {
            if (_buildVersion.HasValue)
                return _buildVersion.Value;

            UpdateBuildVersion();
            return _buildVersion.Value;
        }
    }
    
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

    /// <summary>
    /// Prefix of masterbundle files.
    /// </summary>
    public string? Prefix { get; }

    /// <summary>
    /// Mask specifying which operating systems have explicit bundles.
    /// </summary>
    /// <remarks>For legacy bundles this only checks for the default (Windows) bundle.</remarks>
    public BundleOperatingSystems OperatingSystems
    {
        get
        {
            if (_operatingSystems.HasValue)
                return _operatingSystems.Value;

            UpdateBundleOperatingSystems();
            return _operatingSystems.Value;
        }
    }

    public DiscoveredBundle(bool isLegacyBundle, string directory, string configurationFile, string bundleFile, string? prefix, int version)
    {
        IsLegacyBundle = isLegacyBundle;
        Directory = directory;
        ConfigurationFile = configurationFile;
        BundleFile = bundleFile;
        Version = version;
        Prefix = !isLegacyBundle ? prefix ?? throw new ArgumentNullException(nameof(prefix)) : null;
        UpdateBuildVersion();
    }

    /// <inheritdoc />
    public bool Equals(DiscoveredBundle? other)
    {
        return this == other;
    }

    private enum MasterBundleConfigProperty { Unknown, Name, Prefix, MasterBundleVersion, AssetBundleVersion }

    /// <inheritdoc cref="FromMasterBundleConfig(string,ReadOnlySpan{char},IDiagnosticSink?,Action{string,string}?)"/>
    public static DiscoveredBundle? FromMasterBundleConfig(
        string configurationFilePath,
        IDiagnosticSink? diagMessages = null,
        Action<string, string>? log = null)
    {
        return FromMasterBundleConfig(
            configurationFilePath,
            File.ReadAllText(configurationFilePath),
            diagMessages,
            log
        );
    }

    /// <summary>
    /// Read a <see cref="DiscoveredBundle"/> from a <c>MasterBundle.dat</c> file.
    /// </summary>
    /// <param name="configurationFilePath">Path to the <c>MasterBundle.dat</c> file.</param>
    /// <param name="content">Content of the configuration file.</param>
    /// <param name="diagMessages">Sink for diagnostic messages encountered while reading the file.</param>
    /// <param name="log">Callback invoked when errors are found.</param>
    /// <returns>The new bundle, or <see langword="null"/> if crucial properties are missing.</returns>
    public static DiscoveredBundle? FromMasterBundleConfig(
        string configurationFilePath,
        ReadOnlySpan<char> content,
        IDiagnosticSink? diagMessages = null,
        Action<string, string>? log = null)
    {
        DatTokenizer tokenizer = new DatTokenizer(content, diagMessages);

        int dictionaryDepth = 0, listDepth = 0;

        ReadOnlySpan<char> nameKey = "Asset_Bundle_Name";
        ReadOnlySpan<char> prefixKey = "Asset_Prefix";
        ReadOnlySpan<char> mbVersionKey = "Master_Bundle_Version";
        ReadOnlySpan<char> abVersionKey = "Asset_Bundle_Version";

        MasterBundleConfigProperty nextProperty = 0;
        int masterBundleVersion = 2;
        bool hasMasterBundleVersion = false;

        string? name = null, prefix = null;

        while (tokenizer.MoveNext())
        {
            switch (tokenizer.Token.Type)
            {
                case DatTokenType.DictionaryStart:
                    ++dictionaryDepth;
                    break;

                case DatTokenType.DictionaryEnd:
                    dictionaryDepth = Math.Max(0, dictionaryDepth - 1);
                    break;

                case DatTokenType.ListStart:
                    ++listDepth;
                    break;

                case DatTokenType.ListEnd:
                    listDepth = Math.Max(0, listDepth - 1);
                    break;

                case DatTokenType.Key:
                    nextProperty = MasterBundleConfigProperty.Unknown;
                    if (dictionaryDepth != 0 || listDepth != 0)
                        break;

                    if (tokenizer.Token.Content.Equals(nameKey, StringComparison.OrdinalIgnoreCase))
                        nextProperty = MasterBundleConfigProperty.Name;
                    else if (tokenizer.Token.Content.Equals(prefixKey, StringComparison.OrdinalIgnoreCase))
                        nextProperty = MasterBundleConfigProperty.Prefix;
                    else if (tokenizer.Token.Content.Equals(mbVersionKey, StringComparison.OrdinalIgnoreCase))
                        nextProperty = MasterBundleConfigProperty.MasterBundleVersion;
                    else if (tokenizer.Token.Content.Equals(abVersionKey, StringComparison.OrdinalIgnoreCase))
                        nextProperty = MasterBundleConfigProperty.AssetBundleVersion;
                    break;

                case DatTokenType.Value:
                    switch (nextProperty)
                    {
                        case MasterBundleConfigProperty.Name:
                            name = tokenizer.Token.Content.ToString();
                            break;

                        case MasterBundleConfigProperty.Prefix:
                            prefix = tokenizer.Token.Content.ToString();
                            break;

                        case MasterBundleConfigProperty.MasterBundleVersion:
                            hasMasterBundleVersion = true;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                            if (!int.TryParse(tokenizer.Token.Content, NumberStyles.Any, CultureInfo.InvariantCulture, out masterBundleVersion))
#else
                            if (!int.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out masterBundleVersion))
#endif
                            {
                                log?.Invoke(configurationFilePath, "Can't parse \"Master_Bundle_Version\" tag, defaulting to 0.");
                                masterBundleVersion = 0;
                            }

                            break;

                        case MasterBundleConfigProperty.AssetBundleVersion when !hasMasterBundleVersion:

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                            if (!int.TryParse(tokenizer.Token.Content, NumberStyles.Any, CultureInfo.InvariantCulture, out masterBundleVersion))
#else
                            if (!int.TryParse(tokenizer.Token.Content.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out masterBundleVersion))
#endif
                            {
                                log?.Invoke(configurationFilePath, "Can't parse \"Asset_Bundle_Version\" tag, defaulting to 2.");
                                masterBundleVersion = 2;
                            }

                            break;
                    }

                    break;

            }
        }

        if (string.IsNullOrEmpty(name))
        {
            log?.Invoke(configurationFilePath, "Missing \"Asset_Bundle_Name\" tag.");
            return null;
        }

        if (string.IsNullOrEmpty(prefix))
        {
            log?.Invoke(configurationFilePath, "Missing \"Asset_Prefix\" tag.");
            return null;
        }

        string directoryPath = Path.GetDirectoryName(configurationFilePath)!;
        return new DiscoveredBundle(
            isLegacyBundle: false,
            directoryPath,
            configurationFilePath,
            Path.Combine(directoryPath, name),
            prefix,
            masterBundleVersion
        );
    }

    [MemberNotNull(nameof(_operatingSystems))]
    private void UpdateBundleOperatingSystems()
    {
        BundleOperatingSystems os = BundleOperatingSystems.None;
        if (File.Exists(BundleFile))
            os |= BundleOperatingSystems.Windows;

        if (IsLegacyBundle)
        {
            _operatingSystems = os;
            return;
        }

        string fileName = OSPathHelper.CombineAndConcat(Directory, BundleFile, "_mac", true);
        if (File.Exists(fileName))
            os |= BundleOperatingSystems.Mac;

        fileName = OSPathHelper.CombineAndConcat(Directory, BundleFile, "_linux", true);
        if (File.Exists(fileName))
            os |= BundleOperatingSystems.Linux;

        _operatingSystems = os;
    }

    [MemberNotNull(nameof(_buildVersion))]
    internal void UpdateBuildVersion()
    {
        try
        {
            UnityAssetBundleHeader header = UnityAssetBundleHeader.FromFile(BundleFile, out _);
            _buildVersion = header.EngineVersion;
        }
        catch (FormatException)
        {
            _buildVersion = default(UnityEngineVersion);
        }
    }

    internal void ApplyBundleFileChanges()
    {
        _buildVersion = null;
        _operatingSystems = null;
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
    internal TfmLock GetLock(IParsingServices services)
    {
        return GetLock(services.Installation);
    }

    /// <summary>
    /// Gets the lock used for this bundle.
    /// </summary>
    internal TfmLock GetLock(InstallationEnvironment environment)
    {
        InstallationEnvironment env = ApplyInstallationEnvironment(environment);
        return env.AssetBundleLock;
    }

    private InstallationEnvironment ApplyInstallationEnvironment(InstallationEnvironment env)
    {
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
    internal bool TryLoadAssetFileInfo(
        in BundleData data,
        string nameOrPath,
        [NotNullWhen(true)] out AssetFileInfo? fileInfo,
        [NotNullWhen(true)] out string? path
    )
    {
        fileInfo = null;
        path = null;
        if (Prefix != null)
        {
            nameOrPath = OSPathHelper.CombineWithUnixSeparators(Prefix, nameOrPath);
        }

        if (_nameCache == null || data.AssetBundle == null || !_nameCache.TryGetValue(nameOrPath, out int index))
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
    /// <exception cref="ObjectDisposedException"><paramref name="parsingServices"/>'s <see cref="InstallationEnvironment"/> has been disposed, or this bundle is no longer being tracked.</exception>
    public BundleData GetOrOpen(IParsingServices parsingServices)
    {
        InstallationEnvironment env = ApplyInstallationEnvironment(parsingServices.Installation);

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
        InstallationEnvironment env = ApplyInstallationEnvironment(parsingServices.Installation);

        BundleData file = _openedfile;
        if (file.Info != null)
        {
            return file;
        }

        return GetOrOpenIntl(env);
    }
    private BundleData GetOrOpenIntl(InstallationEnvironment env)
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(DiscoveredBundle));

        if (_openedfile.Info != null)
        {
            return _openedfile;
        }

        AssetsManager? assetsManager = env.AssetBundleManager;

        // ReSharper disable InconsistentlySynchronizedField
        if (assetsManager == null)
        {
            assetsManager = _assetsManager ?? throw new ObjectDisposedException(nameof(InstallationEnvironment));
        }
        else
        {
            _assetsManager = assetsManager;
        }
        // ReSharper restore InconsistentlySynchronizedField

        bool needsCaching = false;
        if (!env.BundleCache.TryGetPathToCacheNoLock(BundleFile, out string? bundlePath))
        {
            bundlePath = BundleFile;
        }
        else
        {
            DateTime cacheFileModified = FileHelper.GetLastWriteTimeUTCSafe(bundlePath, DateTime.MinValue);
            DateTime bundleFileModified = FileHelper.GetLastWriteTimeUTCSafe(BundleFile, DateTime.MaxValue);

            if (bundleFileModified > cacheFileModified)
            {
                needsCaching = true;
            }
        }

        BundleFileInstance file;
        try
        {
            file = assetsManager.LoadBundleFile(needsCaching ? BundleFile : bundlePath, !needsCaching);
        }
        catch (FileNotFoundException)
        {
            goto fileNotExists;
        }
        catch (DirectoryNotFoundException)
        {
            goto fileNotExists;
        }
        
        if (needsCaching)
        {
            try
            {
                AssetBundleFile? unpackedFile = env.BundleCache.CreateBundleCacheFile(
                    BundleFile, bundlePath, file
                );

                if (unpackedFile != null)
                {
                    file.file = unpackedFile;
                    needsCaching = false;
                }
            }
            catch (FileNotFoundException) { }
            catch (DirectoryNotFoundException) { }

            if (needsCaching)
            {
                try
                {
                    File.SetLastWriteTime(bundlePath, new DateTime(1970, 1, 1));
                }
                catch
                {
                    // ignored
                }
            }
        }

        AssetsFileInstance assetFile = assetsManager.LoadAssetsFileFromBundle(file, 0);
        _openedfile = new BundleData(file, assetFile, assetsManager);

        IList<AssetFileInfo> assets = assetFile.file.Metadata.AssetInfos;

        bool cacheNames = IsLegacyBundle;

        ImmutableDictionary<long, string>.Builder pathIdMap = ImmutableDictionary.CreateBuilder<long, string>();
        ImmutableDictionary<string, int>.Builder nameIndexMap = ImmutableDictionary.CreateBuilder<string, int>(
            cacheNames ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase
        );

        ImmutableDictionary<long, AssetFileInfo>.Builder fileCache = ImmutableDictionary.CreateBuilder<long, AssetFileInfo>();

        int abIndex = assets.Count;
        for (int index = 0; index < assets.Count; index++)
        {
            AssetFileInfo obj = assets[index];
            AssetClassID classId = (AssetClassID)obj.TypeId;
            
            if (!env.RelevantBundleClasses.Contains(classId))
                continue;

            if (classId is AssetClassID.GameObject or AssetClassID.Transform or AssetClassID.RectTransform)
            {
                fileCache[obj.PathId] = obj;
            }

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
            if (index > abIndex)
            {
                if (pathIdMap.TryGetValue(obj.PathId, out string? path))
                {
                    nameIndexMap[path] = index;
                }
            }

            if (classId != AssetClassID.AssetBundle || abIndex < assets.Count)
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

                pathIdMap[pathId] = name;
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

                if (pathIdMap.TryGetValue(obj.PathId, out string? path))
                {
                    nameIndexMap[path] = index;
                }
            }
        }

        _nameCache = nameIndexMap.ToImmutable();
        _pathCache = pathIdMap.ToImmutable();

        FilePreloadCache = fileCache.ToImmutable();

        if (IsDisposed)
        {
            UnloadFile();
            throw new ObjectDisposedException(nameof(DiscoveredBundle));
        }

        return new BundleData(file, assetFile, assetsManager);

        fileNotExists:
        return default;
    }

    public void UnloadFile()
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

    public void Dispose()
    {
        IsDisposed = true;
        UnloadFile();
    }

    public record struct BundleData(
        BundleFileInstance? Info,
        AssetsFileInstance? AssetBundle,
        AssetsManager Manager
    );
}

/// <summary>
/// Expresses which bundles are present for multi-platform masterbundles.
/// </summary>
[Flags]
public enum BundleOperatingSystems
{
    /// <summary>
    /// There are no bundles present.
    /// </summary>
    None,

    /// <summary>
    /// The default bundle for Windows is present.
    /// </summary>
    Windows = 1,

    /// <summary>
    /// The bundle for MacOS / OSX is present.
    /// </summary>
    Mac = 2,

    /// <summary>
    /// The bundle for Linux is present.
    /// </summary>
    Linux = 4
}