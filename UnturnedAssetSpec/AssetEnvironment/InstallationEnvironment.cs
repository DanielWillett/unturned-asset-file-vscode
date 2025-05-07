using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

/// <summary>
/// Keeps track of all asset files in the game installation.
/// </summary>
/// <remarks>Use <see cref="UnturnedInstallationEnvironmentExtensions"/> to easily add all enabled workshop items and vanilla content.</remarks>
public class InstallationEnvironment : IDisposable
{
    public delegate void HandleFileUpdate(DiscoveredDatFile oldFile, DiscoveredDatFile newFile);
    public delegate void HandleFile(DiscoveredDatFile file);
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

    private readonly FileEnumerable _fileSync;
    private readonly AssetSpecDatabase _database;
    private readonly List<SourceDirectory> _sourceDirs;
    private readonly Action<string, string> _logAction;
    private int _previousCount = 112;

    private DiscoveredDatFile? _head;
    private DiscoveredDatFile? _tail;
    private int _fileCount;

    private readonly Dictionary<Guid, OneOrMore<DiscoveredDatFile>> _guidIndex;
    private readonly Dictionary<ushort, OneOrMore<DiscoveredDatFile>>[] _idIndex;

    public event HandleFileUpdate? OnFileUpdated;
    public event HandleFile? OnFileRemoved;
    public event HandleFile? OnFileAdded;

    public int FileCount => _fileCount;

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

    public InstallationEnvironment(AssetSpecDatabase database, params string[] sourceDirectories)
    {
        _fileSync = new FileEnumerable(this);

        _database = database;
        _sourceDirs = new List<SourceDirectory>();

        _idIndex = new Dictionary<ushort, OneOrMore<DiscoveredDatFile>>[AssetCategory.TypeOf.Values.Length - 1]; // NONE not included
        for (int i = 0; i < _idIndex.Length; ++i)
            _idIndex[i] = new Dictionary<ushort, OneOrMore<DiscoveredDatFile>>(64);

        _guidIndex = new Dictionary<Guid, OneOrMore<DiscoveredDatFile>>(1024);

        _logAction = Log;

        foreach (string dir in sourceDirectories)
        {
            AddSearchableDirectory(dir);
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
                if (file.FilePath.Equals(fullName, StringComparison.OrdinalIgnoreCase))
                    return file;
            }

            if (!matchLocalizationAlso)
                return null;

            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (string.Equals(file.LocalizationFilePath, fullName, StringComparison.OrdinalIgnoreCase))
                    return file;
            }
        }

        return null;
    }

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

    public OneOrMore<DiscoveredDatFile> FindFile(ushort id, EnumSpecTypeValue assetCategory)
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
            if (_sourceDirs.Exists(x => x.Path.StartsWith(dir, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            FileSystemWatcher watcher = new FileSystemWatcher(dir, "*");

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

        return true;
    }

    public bool RemoveSearchableDirectory(string dir)
    {
        dir = Path.GetFullPath(dir);
        lock (_fileSync)
        {
            int index = _sourceDirs.FindIndex(x => x.Path.Equals(dir, StringComparison.OrdinalIgnoreCase));
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
            bool foundAny = false;
            DiscoveredDatFile? newFile;
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (file.FilePath.Equals(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    newFile = ReadFile(fullName);
                    if (newFile == null)
                    {
                        RemoveFile(file);
                    }
                    else
                    {
                        ApplyFileUpdate(newFile, file);
                        Log($"File updated: {fullName}");
                    }
                    foundAny = true;
                }
                else if (string.Equals(file.LocalizationFilePath, fullName, StringComparison.OrdinalIgnoreCase))
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
            for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
            {
                if (file.FilePath.Equals(fullName, StringComparison.OrdinalIgnoreCase))
                {
                    RemoveFile(file);
                }
                else if (string.Equals(file.LocalizationFilePath, fullName, StringComparison.OrdinalIgnoreCase))
                {
                    file.FriendlyName = null;
                }
            }
        }
    }

    private void FileRenamed(string oldFullName, string fullName)
    {
        lock (_fileSync)
        {
            if (Directory.Exists(fullName))
            {
                List<string> discoveredFiles = new List<string>();
                foreach (string datFile in Directory.EnumerateFiles(fullName, "*.dat", SearchOption.AllDirectories))
                {
                    CheckFile(discoveredFiles, datFile, isDatFile: true);
                }

                foreach (string datFile in Directory.EnumerateFiles(fullName, "*.asset", SearchOption.AllDirectories))
                {
                    CheckFile(discoveredFiles, datFile, isDatFile: false);
                }

                foreach (string newFile in discoveredFiles)
                {
                    string oldPath = newFile.Replace(fullName, oldFullName);
                    FileRenamed(oldPath, newFile);
                }
            }
            else
            {
                bool foundAny = false;
                DiscoveredDatFile? newFile;
                for (DiscoveredDatFile? file = _head; file != null; file = file.Next)
                {
                    if (file.FilePath.Equals(oldFullName, StringComparison.OrdinalIgnoreCase))
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
                                Log($"File updated (renamed): {oldFullName} -> {newFile.FilePath}");
                            }
                        }
                        foundAny = true;
                    }
                    else if (string.Equals(file.LocalizationFilePath, oldFullName, StringComparison.OrdinalIgnoreCase))
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

        OneOrMore<DiscoveredDatFile> index;
        if (!ReferenceEquals(file, oldFile) || file.Guid != oldFile.Guid)
        {
            if (oldFile.Guid != Guid.Empty && _guidIndex.TryGetValue(oldFile.Guid, out index))
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
        }

        if (!ReferenceEquals(file, oldFile) || file.Id != oldFile.Id)
        {
            int oldCategory = oldFile.Category;
            if (oldFile.Id != 0 && oldCategory > 0 && oldCategory <= _idIndex.Length && _idIndex[oldCategory - 1].TryGetValue(oldFile.Id, out index))
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

        try
        {
            OnFileUpdated?.Invoke(oldFile, file);
        }
        catch (Exception ex)
        {
            Log("Error invoking OnFileUpdated.");
            Log(ex.ToString());
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
                if (index.IsNull)
                    indexGroup.Remove(file.Id);
                else
                    indexGroup[file.Id] = index;
            }
        }

        try
        {
            OnFileRemoved?.Invoke(file);
        }
        catch (Exception ex)
        {
            Log("Error invoking OnFileRemoved.");
            Log(ex.ToString());
        }

        Log($"File removed: {file.FilePath}");
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

        try
        {
            OnFileAdded?.Invoke(file);
        }
        catch (Exception ex)
        {
            Log("Error invoking OnFileAdded.");
            Log(ex.ToString());
        }

        if (log)
            Log($"File added: {file.FilePath}");
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
                Log($"Error {(isWatching ? "watching" : "stopping watching")} for file updates in {directory.Path}.");
                Log(ex.ToString());
            }
        }
    }

    public void Discover(CancellationToken token = default)
    {
        lock (_fileSync)
        {
            List<string> discoveredFiles = new List<string>(_previousCount + 16);
            UpdateWatch(false);
            try
            {
                foreach (SourceDirectory directory in _sourceDirs)
                {
                    foreach (string datFile in Directory.EnumerateFiles(directory.Path, "*.dat", SearchOption.AllDirectories))
                    {
                        CheckFile(discoveredFiles, datFile, isDatFile: true);
                    }

                    foreach (string datFile in Directory.EnumerateFiles(directory.Path, "*.asset", SearchOption.AllDirectories))
                    {
                        CheckFile(discoveredFiles, datFile, isDatFile: false);
                    }
                }

                _previousCount = discoveredFiles.Count;

                DiscoveredDatFile[] files = new DiscoveredDatFile[discoveredFiles.Count];

                List<string> fileList2 = discoveredFiles;
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
                Log("Error processing files.");
                Log(ex.ToString());
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
                lock (this)
                {
                    Log($"IO exception parsing {fileName}, retrying {i + 1}/{tryCt}:");
                    Log(ioEx.ToString());
                }
                Thread.Sleep(5 + i * 2);
                continue;
            }
            catch (FormatException ex)
            {
                lock (this)
                {
                    Log($"Error parsing {fileName}: {ex.Message}.");
                }
            }
            catch (Exception ex)
            {
                lock (this)
                {
                    Log($"Error parsing {fileName}:");
                    Log(ex.ToString());
                }
            }

            break;
        }

        return null;
    }

    private static bool ShouldLoadAssetFile(string filePath)
    {
        string? dirName = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);

        if (dirName == null)
            return false;

        if (fileName.Equals("Asset.dat", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        if (nameWithoutExt.Equals(Path.GetFileName(dirName), StringComparison.OrdinalIgnoreCase))
        {
            return fileName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) ||
                   fileName.EndsWith(".asset", StringComparison.OrdinalIgnoreCase);
        }

        return filePath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
               && !File.Exists(Path.Combine(dirName, fileName + ".asset"))
               && !File.Exists(Path.Combine(dirName, fileName + ".dat"))
               && !File.Exists(Path.Combine(dirName, "Asset.dat"));
    }

    private static void CheckFile(List<string> discoveredFiles, string filePath, bool isDatFile)
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
                if (existingAsset[existingAsset.Length - 2] is not 'e' and not 'E' // .asset
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
                    discoveredFiles[i] = discoveredFiles[discoveredFiles.Count - 1];
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

    protected virtual void Log(string fileName, string msg)
    {
        lock (this)
        {
            Console.Write("InstallationEnvironment >> ");
            Console.Write(fileName);
            Console.Write(" | ");
            Console.WriteLine(msg);
        }
    }
    protected virtual void Log(string msg)
    {
        lock (this)
        {
            Console.Write("InstallationEnvironment >>");
            Console.WriteLine(msg);
        }
    }

    private void DisposeWatcher(FileSystemWatcher watcher)
    {
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();

        watcher.Changed += FileWatcherFileUpdated;
        watcher.Deleted += FileWatcherFileRemoved;
        watcher.Created += FileWatcherFileUpdated;
        watcher.Renamed += FileWatcherFileRenamed;
    }

    protected virtual void Dispose(bool disposing)
    {
        ClearSearchableDirectories();
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

    private class FileEnumerable : IEnumerable<DiscoveredDatFile>
    {
        private readonly InstallationEnvironment _environment;

        public FileEnumerable(InstallationEnvironment environment)
        {
            _environment = environment;
        }

        /// <inheritdoc />
        public IEnumerator<DiscoveredDatFile> GetEnumerator() => new Enumerator(_environment);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class Enumerator : IEnumerator<DiscoveredDatFile>
        {
            private readonly InstallationEnvironment _environment;
            private DiscoveredDatFile? _head;

            /// <inheritdoc />
            public DiscoveredDatFile Current => _head!;

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            public Enumerator(InstallationEnvironment env)
            {
                _environment = env;
                _head = env._head;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                return (_head = _head?.Next) != null;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _head = _environment._head;
            }

            /// <inheritdoc />
            public void Dispose() { }
        }
    }
}