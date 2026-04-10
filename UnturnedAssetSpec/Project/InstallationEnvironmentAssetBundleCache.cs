using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

internal class InstallationEnvironmentAssetBundleCache
{
    private readonly InstallationEnvironment _environment;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISpecDatabaseCache? _cache;
    private readonly string? _rootDir;

    private delegate void EditManifest<TState>(ref TState state, FileStream fs)
#if NET9_0_OR_GREATER
        where TState : allows ref struct
#endif
        ;

    [field: AllowNull, MaybeNull]
    private ILogger<InstallationEnvironmentAssetBundleCache> Logger
        => field ??= _loggerFactory.CreateLogger<InstallationEnvironmentAssetBundleCache>();

    [MethodImpl(MethodImplOptions.NoInlining)]
    public InstallationEnvironmentAssetBundleCache(InstallationEnvironment env, ILoggerFactory loggerFactory, IAssetSpecDatabase database)
    {
        _environment = env;
        _loggerFactory = loggerFactory;
        _cache = (database.ReadContext as SpecificationFileReader)?.Cache;

        if (_cache?.RootDirectory is not { Length: > 0 } rootDir)
        {
            return;
        }

        _rootDir = Path.Combine(rootDir, "Bundles");

        string readmePath = Path.Combine(_rootDir, "README.md");
        if (File.Exists(readmePath))
            return;

        Directory.CreateDirectory(_rootDir);
        Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DanielWillett.UnturnedDataFileLspServer.Data.Project.InstallationEnvironmentAssetBundleCache_README.md");
        if (stream != null)
        {
            try
            {
                using FileStream fs = new FileStream(readmePath, FileMode.Create, FileAccess.Write, FileShare.Read, 1024, FileOptions.SequentialScan);
                stream.CopyTo(fs, 1024);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error writing bundle cache README.");
            }
        }
        else
        {
            Logger.LogWarning("Bundle cache README file not found in assembly.");
        }
    }

    internal AssetBundleFile? CreateBundleCacheFile(string bundleFile, string cacheLocation, BundleFileInstance file)
    {
        string? dir = Path.GetDirectoryName(cacheLocation);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        const int tryCt = 10;
        for (int i = 0; i < tryCt; ++i)
        {
            // if multiple instances are running at once
            // sharing violations could occur, so keep retrying
            try
            {
                if (i == 0 || !File.Exists(cacheLocation))
                {
                    // 1024 is a bit over what the header uses up. can't find a way to calculate the exact header size.
                    long decompressedBytes = file.file.BlockAndDirInfo.BlockInfos.Sum(x => x.DecompressedSize) + 1024;

                    try
                    {
                        if (!FileHelper.HasSpaceToStoreBytesAt(cacheLocation, decompressedBytes))
                        {
                            Logger.LogWarning(
                                "Not enough storage to store decompressed bundle {0} at {1} ({2:0.#} MiB required). " +
                                "Bundle will be decompressed to memory, which could use a significant amount of RAM. " +
                                "Consider changing the cache location in configuration to another drive or freeing up space.",
                                Path.GetFileName(bundleFile),
                                cacheLocation,
                                decompressedBytes / 1048576d
                            );

                            file.file = BundleHelper.UnpackBundle(file.file);
                            return file.file;
                        }

                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning(
                            ex,
                            "Unable to check free disk space before decompressing bundle {0} to {1}. " +
                            "Assuming there's enough space.",
                            Path.GetFileName(bundleFile),
                            cacheLocation
                        );
                    }

                    /*
                     * This is a kinda hacky way to handle multiple clients accessing the same
                     * bundle file at the same time.
                     *
                     * After the first try, if the file exists, we can assume that another client
                     * already converted the file.
                     *
                     * We need to make 2 separate streams here because we can't allow 'fs' to lock
                     * the file the entire time the bundle is open.
                     *
                     *
                     * Unfortunately this means that while client A has the file open,
                     * client B will not be able to update the file. However client A should eventually
                     * update it so it's probably fine (hopefully).
                     *
                     */


                    using FileStream fs = new FileStream(
                        cacheLocation,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        16384,
                        FileOptions.SequentialScan
                    );

                    long bundleFileSize = new FileInfo(bundleFile).Length;
                    if (bundleFileSize > 20971520 /* 20 MiB */)
                    {
                        Logger.LogInformation(
                            "Unpacking bundle {0} ({1:0.#} MiB) bundleFileSize to cache. This may take some time for larger bundles.",
                            bundleFile,
                            bundleFileSize / 1048576d
                        );
                    }

                    AssetsFileWriter writer = new AssetsFileWriter(fs);
                    file.file.Unpack(writer);

                    writer.Dispose();
                }

                FileStream readFs = new FileStream(
                    cacheLocation,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    4096,
                    FileOptions.RandomAccess
                );

                AssetBundleFile newFile;
                try
                {
                    newFile = new AssetBundleFile();
                    newFile.Read(new AssetsFileReader(readFs));
                }
                catch
                {
                    readFs.Dispose();
                    throw;
                }

                file.file.Close();
                file.file = newFile;
                return file.file;
            }
            catch (IOException ex) when (/* sharing violation: */ ex.HResult is -2147024864 /* Windows */ or 11 /* Unix */)
            {
                if (i == tryCt - 1)
                {
                    throw;
                }

#if NET6_0_OR_GREATER
                Thread.Sleep(Random.Shared.Next(250, 1000));
#else
                Thread.Sleep(new Random().Next(250, 1000));
#endif
            }
        }

        // unreachable
        return null;
    }

    internal bool TryGetPathToCache(string bundle, [NotNullWhen(true)] out string? cache)
    {
        lock (_environment.AssetBundleLock)
        {
            return TryGetPathToCacheNoLock(bundle, out cache);
        }
    }

    internal bool TryGetPathToCacheNoLock(string bundleFile, [NotNullWhen(true)] out string? cache)
    {
        if (string.IsNullOrEmpty(_rootDir))
        {
            cache = null;
            return false;
        }

        GetPathToCacheState s;
        string manifestFile = GetManifestFile(bundleFile, out string manifestFolder);

        s.BundleFile = bundleFile;
        s.CacheFile = manifestFolder;

        try
        {
            ReadOrEditManifest(manifestFile, ref s, static (ref state, fs) =>
            {
                byte[] indexBytes = new byte[8];
                int size = fs.Read(indexBytes, 0, 8);
                if (size == 8)
                {
                    using StreamReader sr = new StreamReader(fs, Encoding.UTF8, false, 256, leaveOpen: true);
                    while (sr.ReadLine() is { } line)
                    {
                        if (line.Length == 0)
                            continue;

                        int pathEndIndex = line.IndexOf('\0');
                        if (pathEndIndex <= 0 || pathEndIndex == line.Length - 1)
                            continue;

                        ReadOnlySpan<char> cacheFile = line.AsSpan(0, pathEndIndex);
                        ReadOnlySpan<char> bundleFile = line.AsSpan(pathEndIndex + 1);
                        if (!bundleFile.Equals(state.BundleFile, OSPathHelper.PathComparison))
                            continue;

                        state.CacheFile = cacheFile.ToString();
                        return;
                    }
                }

                ulong index = BitConverter.ToUInt64(indexBytes, 0);
                ulong nextIndex = checked ( index + 1 );
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                BitConverter.TryWriteBytes(indexBytes.AsSpan(), nextIndex);
#else
                indexBytes = BitConverter.GetBytes(nextIndex);
#endif
                fs.Seek(0L, SeekOrigin.Begin);
                fs.Write(indexBytes, 0, 8);

                string newCacheFile = state.CacheFile + "_" + nextIndex.ToString(CultureInfo.InvariantCulture) + ".unity3d";

                state.CacheFile = newCacheFile;

                int sz1 = Encoding.UTF8.GetByteCount(newCacheFile);
                int sz2 = Encoding.UTF8.GetByteCount(state.BundleFile);
                int sz = sz1 + 2 + sz2;
                byte[] buffer = new byte[sz];
                sz1 = Encoding.UTF8.GetBytes(newCacheFile, 0, newCacheFile.Length, buffer, 0);
                buffer[sz1] = (byte)'\0';
                sz2 = Encoding.UTF8.GetBytes(state.BundleFile, 0, state.BundleFile.Length, buffer, sz1 + 1);
                buffer[sz1 + 1 + sz2] = (byte)'\n';

                fs.Seek(0L, SeekOrigin.End);
                fs.Write(buffer, 0, sz1 + 2 + sz2);
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error reading or updating cache manifest.");
        }

        if ((object?)s.CacheFile == manifestFolder)
        {
            cache = null;
            return false;
        }

        cache = s.CacheFile;
        return true;
    }

    private struct GetPathToCacheState
    {
        public string BundleFile;
        public string CacheFile;
    }

    private string GetManifestFile(string bundleFile, out string manifestFolder)
    {
        int hash = OSPathHelper.CreateStablePathHash(bundleFile);

        manifestFolder = Path.Combine(_rootDir!, hash.ToString("x8", CultureInfo.InvariantCulture));
        return manifestFolder + ".toc";
    }

    private static void ReadOrEditManifest<TState>(string manifestFile, ref TState state, EditManifest<TState> action)
    {
        string? dirName = Path.GetDirectoryName(manifestFile);
        if (!string.IsNullOrEmpty(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
        const int tryCt = 4;
        for (int i = 0; i < tryCt; ++i)
        {
            // if multiple instances are running at once
            // sharing violations could occur, so keep retrying
            try
            {
                using FileStream fs = new FileStream(
                    manifestFile,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    256,
                    FileOptions.SequentialScan
                );

                action(ref state, fs);

                break;
            }
            catch (IOException ex) when (/* sharing violation: */ ex.HResult is -2147024864 /* Windows */ or 11 /* Unix */)
            {
                if (i == tryCt - 1)
                {
                    throw;
                }

#if NET6_0_OR_GREATER
                Thread.Sleep(Random.Shared.Next(50, 200));
#else
                Thread.Sleep(new Random().Next(50, 200));
#endif
            }
        }
    }
}