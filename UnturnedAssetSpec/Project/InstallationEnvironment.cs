using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// Keeps track of all asset files in the game installation.
/// </summary>
/// <remarks>Use <see cref="UnturnedInstallationEnvironmentExtensions"/> to easily add all enabled workshop items and vanilla content.</remarks>
public class InstallationEnvironment : IDisposable
{
    public delegate void HandleFileUpdate(DiscoveredDatFile oldFile, DiscoveredDatFile newFile);
    public delegate void HandleFile(DiscoveredDatFile file);

    public delegate void HandleBundleUpdate(DiscoveredBundle oldBundle, DiscoveredBundle newBundle);
    public delegate void HandleBundle(DiscoveredBundle bundle);
    private readonly struct SourceDirectory
    {
        public readonly FileSystemWatcher Watcher;
        public readonly string Path;
        public SourceDirectory(FileSystemWatcher watcher, string path)
        {
            Watcher = watcher;
            Path = path;
        }
    }

    private readonly TfmLock _fileSync = new TfmLock();
    internal readonly TfmLock AssetBundleLock = new TfmLock();

    private readonly IAssetSpecDatabase _database;
    private readonly List<SourceDirectory> _sourceDirs;
    private readonly Action<string, string> _logAction;
    private int _previousCount = 112;

    private DiscoveredDatFile? _head;
    private DiscoveredDatFile? _tail;
    private int _fileCount;

    private readonly Dictionary<Guid, OneOrMore<DiscoveredDatFile>> _guidIndex;
    private readonly Dictionary<ushort, OneOrMore<DiscoveredDatFile>>[] _idIndex;
    private readonly Dictionary<ushort, List<DiscoveredDatFile>> _caliberIndex;
    private readonly Dictionary<byte, List<DiscoveredDatFile>> _bladeIndex;

    private readonly Dictionary<string, OneOrMore<DiscoveredBundle>> _masterBundleIndex;
    private readonly List<DiscoveredBundle> _masterBundles;

    [field: MaybeNull]
    internal ImmutableHashSet<AssetClassID> RelevantBundleClasses
    {
        get
        {
            if (field == null && _database.Information != null)
            {
                ImmutableHashSet<AssetClassID>.Builder relevantClasses = ImmutableHashSet.CreateBuilder<AssetClassID>();

                if (_database.Information.RelevantBundleAssetClasses != null)
                {
                    foreach (string? str in _database.Information.RelevantBundleAssetClasses)
                    {
                        if (Enum.TryParse(str, out AssetClassID id))
                        {
                            relevantClasses.Add(id);
                        }
                        else
                        {
                            Logger.LogWarning("Unknown RelevantBundleAssetClasses: {0}.", str);
                        }
                    }
                }

                field = relevantClasses.ToImmutable();
            }

            return field ?? ImmutableHashSet<AssetClassID>.Empty;
        }
    }

    /// <remarks>
    /// Inverse of <see cref="KnownClassIdsByTypes"/>.
    /// </remarks>
    [field: MaybeNull]
    internal ImmutableDictionary<AssetClassID, IBundleAssetType> KnownUnityClassTypes
    {
        get
        {
            if (field == null && _database.Information != null)
            {
                ImmutableDictionary<AssetClassID, IBundleAssetType>.Builder knownClassTypes = ImmutableDictionary.CreateBuilder<AssetClassID, IBundleAssetType>();

                if (_database.Information.KnownUnityClassTypes != null)
                {
                    foreach (KeyValuePair<string, QualifiedType> pair in _database.Information.KnownUnityClassTypes)
                    {
                        if (Enum.TryParse(pair.Key, out AssetClassID id))
                        {
                            knownClassTypes.Add(id, UnityObjectAssetType.Create(pair.Value));
                        }
                        else
                        {
                            Logger.LogWarning("Unknown KnownUnityClassTypes: {0}.", pair.Key);
                        }
                    }
                }

                field = knownClassTypes.ToImmutable();
            }

            return field ?? ImmutableDictionary<AssetClassID, IBundleAssetType>.Empty;
        }
    }

    /// <remarks>
    /// Inverse of <see cref="KnownUnityClassTypes"/>.
    /// </remarks>
    [field: MaybeNull]
    internal ImmutableDictionary<QualifiedType, AssetClassID> KnownClassIdsByTypes
    {
        get
        {
            if (field == null && _database.Information != null)
            {
                ImmutableDictionary<QualifiedType, AssetClassID>.Builder knownClassTypes = ImmutableDictionary.CreateBuilder<QualifiedType, AssetClassID>();

                if (_database.Information.KnownUnityClassTypes != null)
                {
                    foreach (KeyValuePair<string, QualifiedType> pair in _database.Information.KnownUnityClassTypes)
                    {
                        if (Enum.TryParse(pair.Key, out AssetClassID id))
                        {
                            knownClassTypes[pair.Value.CaseInsensitive.Normalized] = id;
                        }
                        else
                        {
                            Logger.LogWarning("Unknown KnownUnityClassTypes: {0}.", pair.Key);
                        }
                    }
                }

                field = knownClassTypes.ToImmutable();
            }

            return field ?? ImmutableDictionary<QualifiedType, AssetClassID>.Empty;
        }
    }


    internal AssetsManager? AssetBundleManager;
    internal InstallationEnvironmentAssetBundleCache BundleCache;

    protected readonly ILogger Logger;

    public event HandleFileUpdate? OnFileUpdated;
    public event HandleFile? OnFileRemoved;
    public event HandleFile? OnFileAdded;

    public event HandleBundleUpdate? OnBundleUpdated;
    public event HandleBundle? OnBundleRemoved;
    public event HandleBundle? OnBundleAdded;

    public int FileCount => _fileCount;

    public int MasterBundleCount => _masterBundles.Count;

    public string[] SourceDirectories
    {
        get
        {
            lock (_fileSync)
            {
                string[] arr = new string[_sourceDirs.Count];
                for (int i = 0; i < arr.Length; ++i)
                    arr[i] = _sourceDirs[i].Path;
                return arr;
            }
        }
    }

    public InstallationEnvironment(IAssetSpecDatabase database, ILoggerFactory loggerFactory, params string[] sourceDirectories)
    {
        _database = database;
        _sourceDirs = new List<SourceDirectory>();

        AssetBundleManager = new AssetsManager
        {
            UseMonoTemplateFieldCache = false,
            UseRefTypeManagerCache = false,
            UseQuickLookup = false,
            UseTemplateFieldCache = false
        };

        BundleCache = new InstallationEnvironmentAssetBundleCache(this, loggerFactory, _database);
        Logger = loggerFactory.CreateLogger(GetType());

        _idIndex = new Dictionary<ushort, OneOrMore<DiscoveredDatFile>>[AssetCategory.Instance.Values.Length - 1]; // NONE not included
        for (int i = 0; i < _idIndex.Length; ++i)
            _idIndex[i] = new Dictionary<ushort, OneOrMore<DiscoveredDatFile>>(64);

        _guidIndex = new Dictionary<Guid, OneOrMore<DiscoveredDatFile>>(1024);

        _caliberIndex = new Dictionary<ushort, List<DiscoveredDatFile>>(512);
        _bladeIndex = new Dictionary<byte, List<DiscoveredDatFile>>(128);

        _masterBundleIndex = new Dictionary<string, OneOrMore<DiscoveredBundle>>(StringComparer.InvariantCultureIgnoreCase);
        _masterBundles = new List<DiscoveredBundle>(32);

        _logAction = (file, msg) =>
        {
            Logger.LogInformation(file + " | " + msg);
        };

        foreach (string dir in sourceDirectories)
        {
            AddSearchableDirectory(dir);
        }
    }

    public void ForEachFileWithCaliber(ushort caliber, Action<DiscoveredDatFile> action)
    {
        lock (_fileSync)
        {
            if (_caliberIndex.TryGetValue(caliber, out List<DiscoveredDatFile>? files))
            {
                files.ForEach(action);
            }
        }
    }

    public void ForEachCaliber(Action<ushort, IReadOnlyList<DiscoveredDatFile>> action)
    {
        lock (_fileSync)
        {
            foreach (KeyValuePair<ushort, List<DiscoveredDatFile>> kvp in _caliberIndex)
            {
                action(kvp.Key, kvp.Value);
            }
        }
    }

    public void ForEachFileWithBladeId(byte bladeId, Action<DiscoveredDatFile> action)
    {
        lock (_fileSync)
        {
            if (_bladeIndex.TryGetValue(bladeId, out List<DiscoveredDatFile>? files))
            {
                files.ForEach(action);
            }
        }
    }

    public void ForEachBladeId(Action<byte, IReadOnlyList<DiscoveredDatFile>> action)
    {
        lock (_fileSync)
        {
            foreach (KeyValuePair<byte, List<DiscoveredDatFile>> kvp in _bladeIndex)
            {
                action(kvp.Key, kvp.Value);
            }
        }
    }

    public void ForEachFile(Action<DiscoveredDatFile> action)
    {
        lock (_fileSync)
        {
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                action(file);
            }
        }
    }

    public void ForEachFile<TState>(Action<DiscoveredDatFile, TState> action, TState state)
    {
        lock (_fileSync)
        {
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                action(file, state);
            }
        }
    }

    public void ForEachFileWhile(Func<DiscoveredDatFile, bool> action)
    {
        lock (_fileSync)
        {
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (!action(file)) break;
            }
        }
    }

    public void ForEachFileWhile<TState>(Func<DiscoveredDatFile, TState, bool> action, TState state)
    {
        lock (_fileSync)
        {
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (!action(file, state)) break;
            }
        }
    }

    public ParallelLoopResult ForEachFileParallel(Action<DiscoveredDatFile, ParallelLoopState> action, CancellationToken token = default)
    {
        DiscoveredDatFile[] arr = new DiscoveredDatFile[_fileCount];
        lock (_fileSync)
        {
            int index = 0;
            for (DiscoveredDatFile? file = _head; file != null && index < arr.Length; file = file.Next)
            {
                arr[index] = file;
                ++index;
            }
        }

        return token.CanBeCanceled
            ? Parallel.ForEach(arr, new ParallelOptions { CancellationToken = token }, action)
            : Parallel.ForEach(arr, action);
    }

    public DiscoveredDatFile? FindFile(string fullName, bool matchLocalizationAlso = false)
    {
        lock (_fileSync)
        {
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (file.FilePath.Equals(fullName, OSPathHelper.PathComparison))
                    return file;
            }

            if (!matchLocalizationAlso)
                return null;

            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (string.Equals(file.LocalizationFilePath, fullName, OSPathHelper.PathComparison))
                    return file;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds a masterbundle file by any of the related file names.
    /// </summary>
    /// <remarks>Can be the <c>MasterBundle.dat</c> or any of the <c>.hash</c>, <c>.manifest</c>, or masterbundle files for any platform.</remarks>
    /// <param name="fullName">The fully-qualified name of the file.</param>
    public DiscoveredBundle? FindMasterBundleByFile(string fullName)
    {
        lock (_fileSync)
        {
            foreach (DiscoveredBundle masterbundle in _masterBundles)
            {
                if (masterbundle.IsReferencingFile(fullName))
                    return masterbundle;
            }
        }

        return null;
    }

    /// <summary>
    /// Find a masterbundle by it's name.
    /// </summary>
    /// <param name="name">The name of the master bundle, such as <c>vehicles.masterbundle</c>.</param>
    public OneOrMore<DiscoveredBundle> FindMasterBundleByName(string name)
    {
        lock (_fileSync)
        {
            if (_masterBundleIndex.TryGetValue(name, out OneOrMore<DiscoveredBundle> bundles))
            {
                return bundles;
            }
        }

        return OneOrMore<DiscoveredBundle>.Null;
    }

    /// <summary>
    /// Find a masterbundle by a path within it's hierarchy. Chooses the bundle with the longest path (most relevant).
    /// </summary>
    /// <param name="filePath">The path to an asset file that would use the returned masterbundle.</param>
    public DiscoveredBundle? FindMasterBundleForPath(ReadOnlySpan<char> filePath)
    {
        DiscoveredBundle? bestMatch = null;
        int len = 0;

        lock (_fileSync)
        {
            foreach (DiscoveredBundle bundle in _masterBundles)
            {
                int l = bundle.Directory.Length;
                if (l < len)
                    continue;

                if (!OSPathHelper.Contains(bundle.Directory, filePath))
                {
                    continue;
                }

                bestMatch = bundle;
                len = l;
            }
        }

        return bestMatch;
    }

    /// <summary>
    /// Find a file by its GUID or legacy ID. If <paramref name="id"/> is a legacy ID, it must also have a category configured.
    /// </summary>
    public OneOrMore<DiscoveredDatFile> FindFile(GuidOrId id)
    {
        if (!id.IsId)
            return FindFile(id.Guid);

        if (id.Category == 0)
            return OneOrMore<DiscoveredDatFile>.Null;

        return FindFile(id.Id, new AssetCategoryValue(id.Category));
    }

    /// <summary>
    /// Find a file by its GUID.
    /// </summary>
    public OneOrMore<DiscoveredDatFile> FindFile(Guid guid)
    {
        lock (_fileSync)
        {
            if (_guidIndex.TryGetValue(guid, out OneOrMore<DiscoveredDatFile> file))
            {
                return file;
            }
        }

        return OneOrMore<DiscoveredDatFile>.Null;
    }

    /// <summary>
    /// Find a file by its legacy ID.
    /// </summary>
    public OneOrMore<DiscoveredDatFile> FindFile(ushort id, AssetCategoryValue assetCategory)
    {
        int category = assetCategory.Index;
        if (category <= 0 || category > _idIndex.Length)
            return OneOrMore<DiscoveredDatFile>.Null;

        lock (_fileSync)
        {
            if (_idIndex[category - 1].TryGetValue(id, out OneOrMore<DiscoveredDatFile> file))
            {
                return file;
            }
        }

        return OneOrMore<DiscoveredDatFile>.Null;
    }

    public bool AddSearchableDirectoryIfExists(string dir)
    {
        return Directory.Exists(dir) && AddSearchableDirectory(dir);
    }
    public bool AddSearchableDirectory(string dir)
    {
        dir = Path.GetFullPath(dir);
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException();

        lock (_fileSync)
        {
            if (_sourceDirs.Exists(x => x.Path.StartsWith(dir, OSPathHelper.PathComparison)))
            {
                return false;
            }

            FileSystemWatcher watcher = new FileSystemWatcher(dir, "*");

            try
            {
                watcher.NotifyFilter = NotifyFilters.LastWrite
                                       | NotifyFilters.FileName
                                       | NotifyFilters.DirectoryName
                                       | NotifyFilters.CreationTime;
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;

                watcher.Changed += FileWatcherFileUpdated;
                watcher.Deleted += FileWatcherFileRemoved;
                watcher.Created += FileWatcherFileUpdated;
                watcher.Renamed += FileWatcherFileRenamed;

                _sourceDirs.Add(new SourceDirectory(watcher, dir));
            }
            catch
            {
                watcher.Dispose();
                throw;
            }
        }

        return true;
    }

    public bool RemoveSearchableDirectory(string dir)
    {
        dir = Path.GetFullPath(dir);
        lock (_fileSync)
        {
            int index = _sourceDirs.FindIndex(x => x.Path.Equals(dir, OSPathHelper.PathComparison));
            if (index == -1)
                return false;

            SourceDirectory dirInfo = _sourceDirs[index];
            _sourceDirs.RemoveAt(index);
            DisposeWatcher(dirInfo.Watcher);
        }

        return true;
    }

    public void ClearSearchableDirectories()
    {
        lock (_fileSync)
        {
            foreach (SourceDirectory dir in _sourceDirs)
            {
                DisposeWatcher(dir.Watcher);
            }

            _sourceDirs.Clear();
        }
    }

    private void FileWatcherFileUpdated(object sender, FileSystemEventArgs e)
    {
        FileUpdated(e.FullPath);
    }

    private void FileWatcherFileRemoved(object sender, FileSystemEventArgs e)
    {
        FileRemoved(e.FullPath);
    }

    private void FileWatcherFileRenamed(object sender, RenamedEventArgs e)
    {
        FileRenamed(e.OldFullPath, e.FullPath);
    }

    private void FileUpdated(string fullName)
    {
        lock (_fileSync)
        {
            bool foundMasterBundle = false;
            for (int i = _masterBundles.Count - 1; i >= 0; i--)
            {
                DiscoveredBundle bundle = _masterBundles[i];
                if (string.Equals(fullName, bundle.ConfigurationFile, OSPathHelper.PathComparison))
                {
                    DiscoveredBundle? newBundle = DiscoveredBundle.FromMasterBundleConfig(fullName, log: _logAction);
                    if (newBundle == null)
                    {
                        RemoveMasterBundle(bundle);
                    }
                    else
                    {
                        ApplyMasterbundleConfigUpdate(newBundle, bundle);
                    }
                }
                else if (bundle.IsReferencingFile(fullName))
                {
                    ApplyMasterbundleContentsUpdate(bundle);
                }
                else continue;

                foundMasterBundle = true;
            }

            if (foundMasterBundle)
            {
                return;
            }

            bool foundAny = false;
            DiscoveredDatFile? newFile;
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (file.FilePath.Equals(fullName, OSPathHelper.PathComparison))
                {
                    newFile = ReadFile(fullName);
                    if (newFile == null)
                    {
                        RemoveFile(file);
                    }
                    else
                    {
                        ApplyFileUpdate(newFile, file);
                        Logger.LogTrace($"File updated: {fullName}");
                    }
                    foundAny = true;
                }
                else if (string.Equals(file.LocalizationFilePath, fullName, OSPathHelper.PathComparison))
                {
                    file.UpdateLocalizationFile();
                }
            }

            if (foundAny || !ShouldLoadAssetFile(fullName))
                return;

            newFile = ReadFile(fullName);
            if (newFile != null)
            {
                AddFile(newFile);
            }
        }
    }

    private void FileRemoved(string fullName)
    {
        lock (_fileSync)
        {
            bool foundMasterBundle = false;
            for (int i = _masterBundles.Count - 1; i >= 0; i--)
            {
                DiscoveredBundle bundle = _masterBundles[i];
                if (string.Equals(fullName, bundle.ConfigurationFile, OSPathHelper.PathComparison))
                {
                    RemoveMasterBundle(bundle);
                }
                else if (bundle.IsReferencingFile(fullName))
                {
                    ApplyMasterbundleContentsUpdate(bundle);
                }
                else continue;

                foundMasterBundle = true;
            }

            if (foundMasterBundle)
            {
                return;
            }

            // no extension or dot in name of subfolder
            int lastIndexOfDot = fullName.LastIndexOf('.');
            int lastIndexOfFolder = fullName.LastIndexOf(Path.DirectorySeparatorChar);
            ReadOnlySpan<char> ext = lastIndexOfDot < 0 || lastIndexOfDot < lastIndexOfFolder
                ? ReadOnlySpan<char>.Empty
                : fullName.AsSpan(lastIndexOfDot);
            if (ext.IsEmpty || (
                    !ext.Equals(".dat", OSPathHelper.PathComparison)
                    && !ext.Equals(".udat", OSPathHelper.PathComparison)
                    && !ext.Equals(".udatproj", OSPathHelper.PathComparison)
                    && !ext.Equals(".asset", OSPathHelper.PathComparison)
                ))
            {
                if (ext.Equals(".unity3d", OSPathHelper.PathComparison))
                {
                    RemoveLegacyBundle(fullName);
                    return;
                }

                // directory deleted
                for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
                {
                    string path = file.FilePath;
                    if (OSPathHelper.Contains(fullName, path))
                    {
                        RemoveFile(file);
                    }
                    else if (file.LocalizationFilePath != null && OSPathHelper.Contains(fullName, file.LocalizationFilePath))
                    {
                        file.UpdateLocalizationFile();
                    }
                }
            }
            else
            {
                for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
                {
                    if (file.FilePath.Equals(fullName, OSPathHelper.PathComparison))
                    {
                        RemoveFile(file);
                    }
                    else if (string.Equals(file.LocalizationFilePath, fullName, OSPathHelper.PathComparison))
                    {
                        file.UpdateLocalizationFile();
                    }
                }
            }
        }
    }

    private void RemoveLegacyBundle(string fullName)
    {
        for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
        {
            DiscoveredBundle? bundle = file.LegacyBundle;
            if (bundle == null)
                continue;

            if (string.Equals(bundle.BundleFile, fullName, OSPathHelper.PathComparison))
            {
                file.LegacyBundle = null;
                bundle.Dispose();
            }
        }
    }

    private void FileRenamed(string oldFullName, string fullName)
    {
        lock (_fileSync)
        {
            if (Directory.Exists(fullName))
            {
                DiscoveredFilesCollection c;
                c.AssetFiles = new List<string>(256);
                c.MasterbundleConfigs = new List<string>(4);
                foreach (string datFile in Directory.EnumerateFiles(fullName, "*.dat", SearchOption.AllDirectories))
                {
                    CheckFile(in c, datFile, isDatFile: true);
                }

                foreach (string datFile in Directory.EnumerateFiles(fullName, "*.asset", SearchOption.AllDirectories))
                {
                    CheckFile(in c, datFile, isDatFile: false);
                }

                foreach (string masterBundleFile in c.MasterbundleConfigs)
                {
                    string oldPath = masterBundleFile.Replace(fullName, oldFullName);
                    MasterBundleRenamedIntl(oldPath, masterBundleFile, true, true);
                }

                foreach (string newFile in c.AssetFiles)
                {
                    string oldPath = newFile.Replace(fullName, oldFullName);
                    FileRenamedIntl(oldPath, newFile);
                }
            }
            else
            {
                if (OSPathHelper.IsFileName(oldFullName, "MasterBundle.dat"))
                {
                    MasterBundleRenamedIntl(
                        oldFullName,
                        fullName,
                        true,
                        OSPathHelper.IsFileName(fullName, "MasterBundle.dat")
                    );
                }
                else if (OSPathHelper.IsFileName(fullName, "MasterBundle.dat"))
                {
                    MasterBundleRenamedIntl(oldFullName, fullName, false, true);
                }
                else
                {
                    FileRenamedIntl(oldFullName, fullName);
                }
            }
        }
    }

    private void FileRenamedIntl(string oldFullName, string fullName)
    {
        bool foundAny = false;
        DiscoveredDatFile? newFile;
        for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
        {
            if (file.FilePath.Equals(oldFullName, OSPathHelper.PathComparison))
            {
                if (!ShouldLoadAssetFile(fullName))
                {
                    RemoveFile(file);
                }
                else
                {
                    newFile = ReadFile(fullName);
                    if (newFile == null)
                    {
                        RemoveFile(file);
                    }
                    else
                    {
                        ApplyFileUpdate(newFile, file);
                        Logger.LogTrace($"File updated (renamed): {oldFullName} -> {newFile.FilePath}");
                    }
                }
                foundAny = true;
            }
            else if (string.Equals(file.LocalizationFilePath, oldFullName, OSPathHelper.PathComparison))
            {
                file.UpdateLocalizationFile();
            }
        }

        if (foundAny || !ShouldLoadAssetFile(fullName))
            return;

        newFile = ReadFile(fullName);
        if (newFile != null)
        {
            AddFile(newFile);
        }
    }

    private void MasterBundleRenamedIntl(string oldFullName, string fullName, bool wasBundle, bool isBundle)
    {
        DiscoveredBundle? masterBundle = null;
        if (wasBundle)
        {
            foreach (DiscoveredBundle bndl in _masterBundles)
            {
                if (!string.Equals(bndl.ConfigurationFile, oldFullName, OSPathHelper.PathComparison))
                    continue;

                masterBundle = bndl;
                break;
            }

            if (masterBundle == null)
            {
                Logger.LogWarning($"Failed to find previous MasterBundle: {oldFullName} after being renamed to {fullName}.");
                return;
            }
        }

        if (isBundle && wasBundle) // Bundle -> Bundle
        {
            DiscoveredBundle? newBundle = DiscoveredBundle.FromMasterBundleConfig(fullName, log: _logAction);
            if (newBundle == null)
                RemoveMasterBundle(masterBundle!);
            else
                ApplyMasterbundleConfigUpdate(newBundle, masterBundle!);
        }
        else if (isBundle) // Asset -> Bundle
        {
            bool log = true;
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (!string.Equals(file.FilePath, oldFullName, OSPathHelper.PathComparison))
                {
                    continue;
                }

                RemoveFile(file);
                log = false;
                break;
            }

            DiscoveredBundle? newBundle = DiscoveredBundle.FromMasterBundleConfig(fullName, log: _logAction);
            if (newBundle == null)
            {
                if (!log)
                    Logger.LogTrace($"File removed: {oldFullName}. Changed from MasterBundle -> Asset, but couldn't read bundle config.");
            }
            else
            {
                AddMasterBundle(newBundle, log);
                if (!log)
                    Logger.LogTrace($"File updated (renamed): {oldFullName} -> {fullName}. Changed from Asset -> MasterBundle.");
            }
        }
        else // Bundle -> Asset
        {
            RemoveMasterBundle(masterBundle!);
            DiscoveredDatFile? newFile = ReadFile(fullName);
            if (newFile != null)
            {
                AddFile(newFile, log: false);
                Logger.LogTrace($"File updated (renamed): {oldFullName} -> {newFile.FilePath}. Changed from MasterBundle -> Asset.");
            }
        }
    }

    private void ApplyMasterbundleContentsUpdate(DiscoveredBundle bundle)
    {
        lock (AssetBundleLock)
        {
            bundle.UnloadFile();
        }

        bundle.ApplyBundleFileChanges();

        Logger.LogTrace($"MasterBundle content updated: {bundle.ConfigurationFile}");
    }

    private void ApplyMasterbundleConfigUpdate(DiscoveredBundle bundle, DiscoveredBundle oldBundle)
    {
        lock (AssetBundleLock)
        {
            int index = oldBundle.Index;
            oldBundle.Index = -1;
            if (index < 0 || index >= _masterBundles.Count || _masterBundles[index] != oldBundle)
            {
                index = _masterBundles.IndexOf(oldBundle);
            }

            string oldName = Path.GetFileName(oldBundle.BundleFile);
            string newName = Path.GetFileName(bundle.BundleFile);
            if (_masterBundleIndex.TryGetValue(oldName, out OneOrMore<DiscoveredBundle> bundles))
            {
                bundles = bundles.Remove(bundle);
                if (bundles.IsNull)
                    _masterBundleIndex.Remove(oldName);
                else
                    _masterBundleIndex[oldName] = bundles;
            }

            oldBundle.BundleReplacement = bundle;
            oldBundle.Dispose();

            if (index == -1)
            {
                AddMasterBundle(bundle);
                return;
            }

            if (_masterBundleIndex.TryGetValue(newName, out bundles))
                _masterBundleIndex[newName] = bundles.Add(bundle);
            else
                _masterBundleIndex.Add(newName, bundles);

            _masterBundles[index] = bundle;
        }

        try
        {
            OnBundleUpdated?.Invoke(oldBundle, bundle);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking OnBundleUpdated");
        }

        Logger.LogTrace($"MasterBundle configuration updated: {bundle.ConfigurationFile}");
    }

    private void RemoveMasterBundle(DiscoveredBundle bundle, bool log = true)
    {
        lock (AssetBundleLock)
        {
            int index = bundle.Index;
            bundle.Index = -1;
            if (index < 0 || index >= _masterBundles.Count || _masterBundles[index] != bundle)
            {
                index = _masterBundles.IndexOf(bundle);
            }

            if (index == -1)
            {
                Logger.LogWarning($"Tried to remove MasterBundle that isn't registered: {bundle.ConfigurationFile}");
                return;
            }

            if (index == _masterBundles.Count - 1)
            {
                _masterBundles.RemoveAt(index);
            }
            else
            {
                DiscoveredBundle toMove = _masterBundles[^1];
                toMove.Index = index;
                _masterBundles[index] = toMove;
                _masterBundles.RemoveAt(_masterBundles.Count - 1);
            }

            bundle.Index = -1;
            string name = Path.GetFileName(bundle.BundleFile);

            if (_masterBundleIndex.TryGetValue(name, out OneOrMore<DiscoveredBundle> bundles))
            {
                bundles = bundles.Remove(bundle);
                if (bundles.IsNull)
                    _masterBundleIndex.Remove(name);
                else
                    _masterBundleIndex[name] = bundles;
            }

            bundle.Dispose();
        }

        try
        {
            OnBundleRemoved?.Invoke(bundle);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking OnBundleRemoved");
        }

        if (log)
            Logger.LogTrace($"MasterBundle removed: {bundle.ConfigurationFile}");
    }

    private void AddMasterBundle(DiscoveredBundle bundle, bool log = true)
    {
        bundle.Index = _masterBundles.Count;
        _masterBundles.Add(bundle);

        string name = Path.GetFileName(bundle.BundleFile);
        if (_masterBundleIndex.TryGetValue(name, out OneOrMore<DiscoveredBundle> bundles))
            _masterBundleIndex[name] = bundles.Add(bundle);
        else
            _masterBundleIndex.Add(name, bundle);

        try
        {
            OnBundleAdded?.Invoke(bundle);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking OnBundleAdded");
        }

        if (log)
            Logger.LogTrace($"MasterBundle added: {bundle.ConfigurationFile}");
    }

    private void ApplyFileUpdate(DiscoveredDatFile file, DiscoveredDatFile oldFile)
    {
        file.Prev = oldFile.Prev;
        file.Next = oldFile.Next;
        if (file.Prev == null)
            _head = file;

        if (file.Next == null)
            _tail = file;

        oldFile.IsRemoved = true;
        file.IsRemoved = false;

        if (!ReferenceEquals(file, oldFile))
        {
            if (oldFile.Guid != Guid.Empty
                && _guidIndex.TryGetValue(oldFile.Guid, out OneOrMore<DiscoveredDatFile> index))
            {
                index = index.Remove(oldFile);
                if (index.IsNull)
                    _guidIndex.Remove(oldFile.Guid);
                else
                    _guidIndex[oldFile.Guid] = index;
            }
            if (file.Guid != Guid.Empty)
            {
                if (_guidIndex.TryGetValue(file.Guid, out index))
                    _guidIndex[file.Guid] = index.Add(file);
                else
                    _guidIndex.Add(file.Guid, file);
            }

            int oldCategory = oldFile.Category;
            if (oldFile.Id != 0
                && oldCategory > 0
                && oldCategory <= _idIndex.Length
                && _idIndex[oldCategory - 1].TryGetValue(oldFile.Id, out index))
            {
                index = index.Remove(oldFile);
                if (index.IsNull)
                    _idIndex[oldCategory - 1].Remove(oldFile.Id);
                else
                    _idIndex[oldCategory - 1][oldFile.Id] = index;
            }
            if (file.Id != 0 && file.Category > 0 && file.Category <= _idIndex.Length)
            {
                if (_idIndex[oldCategory - 1].TryGetValue(file.Id, out index))
                    _idIndex[oldCategory - 1][file.Id] = index.Add(file);
                else
                    _idIndex[oldCategory - 1].Add(file.Id, file);
            }
        }

        bool isCalEqual = file.Calibers.Equals(file.MagazineCalibers);
        bool wasCalEqual = oldFile.Calibers.Equals(oldFile.MagazineCalibers);

        foreach (ushort caliber in oldFile.Calibers)
        {
            if (caliber == 0)
                continue;

            if (!_caliberIndex.TryGetValue(caliber, out List<DiscoveredDatFile>? f))
            {
                continue;
            }

            if (file.Calibers.Contains(caliber)
                || (!isCalEqual && file.MagazineCalibers.Contains(caliber)))
            {
                // still in new asset
                int ind = f.IndexOf(oldFile);
                if (ind == -1)
                {
                    if (!f.Contains(file))
                        f.Add(file);
                }
                else
                    f[ind] = file;
                continue;
            }

            // removed caliber
            f.Remove(oldFile);
            if (f.Count == 0)
            {
                _caliberIndex.Remove(caliber);
            }
        }

        foreach (ushort caliber in file.Calibers)
        {
            if (caliber == 0)
                continue;

            // check for new calibers
            if (oldFile.Calibers.Contains(caliber)
                || (!wasCalEqual && oldFile.MagazineCalibers.Contains(caliber)))
            {
                continue;
            }

            if (!_caliberIndex.TryGetValue(caliber, out List<DiscoveredDatFile>? f))
            {
                _caliberIndex.Add(caliber, new List<DiscoveredDatFile>(8) { file });
                continue;
            }

            f.Add(file);
        }

        foreach (byte blade in oldFile.BladeIds)
        {
            if (blade == 0)
                continue;

            if (!_bladeIndex.TryGetValue(blade, out List<DiscoveredDatFile>? f))
            {
                continue;
            }

            if (file.Calibers.Contains(blade))
            {
                // still in new asset
                int ind = f.IndexOf(oldFile);
                if (ind == -1)
                {
                    if (!f.Contains(file))
                        f.Add(file);
                }
                else
                    f[ind] = file;
                continue;
            }

            // removed blade
            f.Remove(oldFile);
            if (f.Count == 0)
            {
                _bladeIndex.Remove(blade);
            }
        }

        foreach (byte blade in file.BladeIds)
        {
            if (blade == 0)
                continue;

            // check for new blades
            if (oldFile.BladeIds.Contains(blade))
            {
                continue;
            }

            if (!_bladeIndex.TryGetValue(blade, out List<DiscoveredDatFile>? f))
            {
                _bladeIndex.Add(blade, new List<DiscoveredDatFile>(4) { file });
                continue;
            }

            f.Add(file);
        }

        try
        {
            OnFileUpdated?.Invoke(oldFile, file);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking OnFileUpdated");
        }
    }

    private void RemoveFile(DiscoveredDatFile file)
    {
        if (file.Next != null)
        {
            file.Next.Prev = file.Prev;
        }
        else
        {
            _tail = file.Prev;
        }
        if (file.Prev != null)
        {
            file.Prev.Next = file.Next;
        }
        else
        {
            _head = file.Next;
        }

        Interlocked.Decrement(ref _fileCount);
        file.IsRemoved = true;

        if (file.Guid != Guid.Empty && _guidIndex.TryGetValue(file.Guid, out OneOrMore<DiscoveredDatFile> index))
        {
            index = index.Remove(file);
            if (index.IsNull)
                _guidIndex.Remove(file.Guid);
            else
                _guidIndex[file.Guid] = index;
        }

        if (file.Id != 0 && file.Category > 0 && file.Category <= _idIndex.Length)
        {
            Dictionary<ushort, OneOrMore<DiscoveredDatFile>> indexGroup = _idIndex[file.Category - 1];
            if (indexGroup.TryGetValue(file.Id, out index))
            {
                index = index.Remove(file);
                if (index.IsNull)
                    indexGroup.Remove(file.Id);
                else
                    indexGroup[file.Id] = index;
            }
        }

        foreach (ushort caliber in file.Calibers)
        {
            if (caliber == 0)
                continue;

            if (!_caliberIndex.TryGetValue(caliber, out List<DiscoveredDatFile>? f))
            {
                continue;
            }

            // removed caliber
            f.Remove(file);
            if (f.Count == 0)
            {
                _caliberIndex.Remove(caliber);
            }
        }

        foreach (byte blade in file.BladeIds)
        {
            if (blade == 0)
                continue;

            if (!_bladeIndex.TryGetValue(blade, out List<DiscoveredDatFile>? f))
            {
                continue;
            }

            // removed blade
            f.Remove(file);
            if (f.Count == 0)
            {
                _bladeIndex.Remove(blade);
            }
        }

        try
        {
            OnFileRemoved?.Invoke(file);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking OnFileRemoved");
        }

        Logger.LogTrace($"File removed: {file.FilePath}");
    }

    private void AddFile(DiscoveredDatFile file, bool log = true)
    {
        file.Prev = _tail;
        _tail = file;
        if (file.Prev != null)
            file.Prev.Next = file;
        else
            _head = file;

        Interlocked.Increment(ref _fileCount);

        file.IsRemoved = false;

        OneOrMore<DiscoveredDatFile> files;
        if (file.Guid != Guid.Empty)
        {
            if (_guidIndex.TryGetValue(file.Guid, out files))
                _guidIndex[file.Guid] = files.Add(file);
            else
                _guidIndex.Add(file.Guid, file);
        }

        if (file.Id == 0 || file.Category <= 0 || file.Category > _idIndex.Length)
            return;

        Dictionary<ushort, OneOrMore<DiscoveredDatFile>> indexGroup = _idIndex[file.Category - 1];
        if (indexGroup.TryGetValue(file.Id, out files))
            indexGroup[file.Id] = files.Add(file);
        else
            indexGroup.Add(file.Id, file);

        foreach (ushort caliber in file.Calibers)
        {
            if (caliber == 0)
                continue;

            if (!_caliberIndex.TryGetValue(caliber, out List<DiscoveredDatFile>? f))
            {
                _caliberIndex.Add(caliber, new List<DiscoveredDatFile>(8) { file });
                continue;
            }

            if (!f.Contains(file))
                f.Add(file);
        }

        foreach (byte blade in file.BladeIds)
        {
            if (blade == 0)
                continue;

            if (!_bladeIndex.TryGetValue(blade, out List<DiscoveredDatFile>? f))
            {
                _bladeIndex.Add(blade, new List<DiscoveredDatFile>(8) { file });
                continue;
            }

            if (!f.Contains(file))
                f.Add(file);
        }

        try
        {
            OnFileAdded?.Invoke(file);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error invoking OnFileAdded");
        }

        if (log)
            Logger.LogTrace($"File added: {file.FilePath}");
    }

    private void UpdateWatch(bool isWatching)
    {
        foreach (SourceDirectory directory in _sourceDirs)
        {
            try
            {
                directory.Watcher.EnableRaisingEvents = isWatching;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error {(isWatching ? "watching" : "stopping watching")} for file updates in {directory.Path}.");
            }
        }
    }

    public void Discover(CancellationToken token = default)
    {
        lock (_fileSync)
        {
            DiscoveredFilesCollection c;
            c.AssetFiles = new List<string>(_previousCount + 16);
            c.MasterbundleConfigs = new List<string>(32);
            UpdateWatch(false);
            try
            {
                foreach (SourceDirectory directory in _sourceDirs)
                {
                    foreach (string datFile in Directory.EnumerateFiles(directory.Path, "*.dat", SearchOption.AllDirectories))
                    {
                        CheckFile(in c, datFile, isDatFile: true);
                    }

                    foreach (string datFile in Directory.EnumerateFiles(directory.Path, "*.asset", SearchOption.AllDirectories))
                    {
                        CheckFile(in c, datFile, isDatFile: false);
                    }
                }


                /* Parse MasterBundle.dat files */
                DiscoveredBundle?[] prevBundles = _masterBundles.ToArray();
                _masterBundles.Clear();

                _masterBundleIndex.Clear();
                foreach (string masterBundleConfig in c.MasterbundleConfigs)
                {
                    DiscoveredBundle? masterBundle = null;
                    bool applied = false;
                    try
                    {
                        masterBundle = DiscoveredBundle.FromMasterBundleConfig(
                            masterBundleConfig,
                            log: _logAction
                        );
                    }
                    catch (IOException ex)
                    {
                        Logger.LogWarning(
                            ex,
                            "Unable to read 'MasterBundle.dat' at {0}.",
                            Path.GetDirectoryName(masterBundleConfig)
                        );
                        continue;
                    }
                    finally
                    {
                        for (int i = 0; i < prevBundles.Length; ++i)
                        {
                            DiscoveredBundle? bndl = prevBundles[i];
                            if (bndl == null || !string.Equals(bndl.ConfigurationFile, masterBundleConfig, OSPathHelper.PathComparison))
                            {
                                continue;
                            }

                            // clean up old bundle at same location
                            bndl.BundleReplacement = masterBundle; // maybe null its ok
                            bndl.Dispose();
                            if (masterBundle != null)
                            {
                                ApplyMasterbundleConfigUpdate(masterBundle, bndl);
                                applied = true;
                            }

                            prevBundles[i] = null;
                        }
                    }

                    if (masterBundle == null)
                    {
                        Logger.LogWarning("Unable to create bundle from 'MasterBundle.dat' at {0}.", Path.GetDirectoryName(masterBundleConfig));
                        continue;
                    }

                    if (!applied)
                    {
                        AddMasterBundle(masterBundle, log: false);
                    }
                }

                // clean up removed bundles
                for (int i = 0; i < prevBundles.Length; ++i)
                {
                    prevBundles[i]?.Dispose();
                }

                /* Parse other asset files */
                _previousCount = c.AssetFiles.Count;

                DiscoveredDatFile[] files = new DiscoveredDatFile[c.AssetFiles.Count];

                List<string> fileList2 = c.AssetFiles;
                DiscoveredDatFile?[] fileOut2 = files;
                Parallel.For(
                    0,
                    files.Length,
                    new ParallelOptions { CancellationToken = token },
                    file =>
                    {
                        fileOut2[file] = ReadFile(fileList2[file]);
                    }
                );

                _guidIndex.Clear();
                for (int i = 0; i < _idIndex.Length; ++i)
                    _idIndex[i].Clear();
                _bladeIndex.Clear();
                _caliberIndex.Clear();

                _head = null;
                _tail = null;

                for (int i = 0; i < files.Length; ++i)
                {
                    DiscoveredDatFile? file = files[i];
                    if (file == null)
                        continue;

                    AddFile(file, log: false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing files.");
            }
            finally
            {
                UpdateWatch(true);
            }
        }
    }

    private DiscoveredDatFile? ReadFile(string fileName)
    {
        const int tryCt = 3;
        for (int i = 0; i < tryCt; ++i)
        {
            try
            {
                using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1024, FileOptions.SequentialScan);
                using StreamReader sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
                string text = sr.ReadToEnd();
                return new DiscoveredDatFile(fileName, text.AsSpan(), _database, null, _logAction);
            }
            catch (IOException ioEx)
            {
                // file watchers can cause some access violations, retrying can fix the majority of cases
                Logger.LogWarning(ioEx, $"IO exception parsing {fileName}, retrying {i + 1}/{tryCt}:");
                Thread.Sleep(5 + i * 2);
                continue;
            }
            catch (FormatException ex)
            {
                Logger.LogWarning($"Error parsing {fileName}: {ex.Message}.");
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, $"Error parsing {fileName}.");
            }

            break;
        }

        return null;
    }

    private static bool ShouldLoadAssetFile(string filePath)
    {
        ReadOnlySpan<char> dirName = OSPathHelper.GetDirectoryName(filePath);
        ReadOnlySpan<char> fileName = OSPathHelper.GetFileName(filePath);

        if (dirName == null)
            return false;

        if (fileName.Equals("Asset.dat", OSPathHelper.PathComparison))
        {
            return true;
        }

        ReadOnlySpan<char> nameWithoutExt = OSPathHelper.GetFileNameWithoutExtension(fileName);
        if (nameWithoutExt.Equals(OSPathHelper.GetFileName(dirName), OSPathHelper.PathComparison))
        {
            return OSPathHelper.IsExtension(fileName, ".dat") ||
                   OSPathHelper.IsExtension(fileName, ".asset");
        }

        return OSPathHelper.IsExtension(filePath, ".asset")
               && !File.Exists(OSPathHelper.CombineAndConcat(dirName, fileName, ".asset"))
               && !File.Exists(OSPathHelper.CombineAndConcat(dirName, fileName, ".dat"))
               && !File.Exists(OSPathHelper.CombineAndConcat(dirName, "Asset.dat", ReadOnlySpan<char>.Empty));
    }

    private struct DiscoveredFilesCollection
    {
        public List<string> AssetFiles;
        public List<string> MasterbundleConfigs;
    }

    private static void CheckFile(in DiscoveredFilesCollection collection, string filePath, bool isDatFile)
    {
        int firstDirIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar);
        if (firstDirIndex <= 0)
        {
            return;
        }

        int secondDirIndex = filePath.LastIndexOf(Path.DirectorySeparatorChar, firstDirIndex - 1);
        ReadOnlySpan<char> dirName = filePath.AsSpan(secondDirIndex + 1, firstDirIndex - secondDirIndex - 1);
        ReadOnlySpan<char> fileName = filePath.AsSpan(firstDirIndex + 1, filePath.Length - firstDirIndex - 1 - (isDatFile ? 4 : 6));
        ReadOnlySpan<char> dirPath;

        if (isDatFile && fileName.Equals("MasterBundle", OSPathHelper.PathComparison))
        {
            collection.MasterbundleConfigs?.Add(filePath);
        }

        List<string>? discoveredFiles = collection.AssetFiles;
        if (discoveredFiles == null)
            return;

        if (dirName.Equals(fileName, StringComparison.OrdinalIgnoreCase) || isDatFile && fileName.Equals("Asset".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            if (isDatFile)
            {
                discoveredFiles.Add(filePath);
                return;
            }

            // .asset files are only looked for if there isn't a file in the folder that matches the folder name
            dirPath = filePath.AsSpan(0, firstDirIndex);
            for (int i = discoveredFiles.Count - 1; i >= 0; i--)
            {
                ReadOnlySpan<char> existingAsset = discoveredFiles[i].AsSpan();
                if (existingAsset[^2] is not 'e' and not 'E' // .asset
                    || !existingAsset.StartsWith(dirPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                int lastIndex = existingAsset.LastIndexOf(Path.DirectorySeparatorChar);
                if (lastIndex >= 0)
                {
                    ReadOnlySpan<char> existingDirPath = existingAsset.Slice(0, lastIndex);
                    if (!existingDirPath.Equals(dirPath, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                if (i != discoveredFiles.Count - 1)
                    discoveredFiles[i] = discoveredFiles[^1];
                discoveredFiles.RemoveAt(discoveredFiles.Count - 1);
            }

            discoveredFiles.Add(filePath);
            return;
        }
        
        if (isDatFile)
            return;

        // check to make sure there isn't already a file with the same name as this folder that was found.
        dirPath = filePath.AsSpan(0, firstDirIndex);
        for (int i = 0; i < discoveredFiles.Count; ++i)
        {
            string existingAsset = discoveredFiles[i];
            if (existingAsset[existingAsset.Length - 2] is not 'e' and not 'E' // .asset
                || !existingAsset.AsSpan().StartsWith(dirPath, StringComparison.OrdinalIgnoreCase))
                continue;

            int existingFirstDirIndex = existingAsset.LastIndexOf(Path.DirectorySeparatorChar);
            ReadOnlySpan<char> existingFileName = existingAsset.AsSpan(existingFirstDirIndex + 1, existingAsset.Length - existingFirstDirIndex - 7);
            if (existingFileName.Equals(dirName, StringComparison.OrdinalIgnoreCase))
                return;
        }

        discoveredFiles.Add(filePath);
    }

    private void DisposeWatcher(FileSystemWatcher watcher)
    {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();

        watcher.Changed -= FileWatcherFileUpdated;
        watcher.Deleted -= FileWatcherFileRemoved;
        watcher.Created -= FileWatcherFileUpdated;
        watcher.Renamed -= FileWatcherFileRenamed;
    }

    protected virtual void Dispose(bool disposing)
    {
        ClearSearchableDirectories();
        Interlocked.Exchange(ref AssetBundleManager, null)?.UnloadAll(true);
    }

    ~InstallationEnvironment()
    {
        Dispose(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}