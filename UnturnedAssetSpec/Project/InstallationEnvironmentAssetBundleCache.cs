using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
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
                byte[] indexBytes = new byte[4];
                int size = fs.Read(indexBytes, 0, 4);
                if (size == 4)
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
                        if (!bundleFile.Equals(state.BundleFile))
                            continue;

                        state.CacheFile = cacheFile.ToString();
                        return;
                    }
                }

                int index = BitConverter.ToInt32(indexBytes, 0);
                int nextIndex = unchecked ( index + 1 );
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                BitConverter.TryWriteBytes(indexBytes.AsSpan(), nextIndex);
#else
                indexBytes = BitConverter.GetBytes(nextIndex);
#endif
                fs.Seek(0L, SeekOrigin.Begin);
                fs.Write(indexBytes, 0, 4);

                string newCacheFile = Path.Combine(
                    state.CacheFile,
                    nextIndex.ToString(CultureInfo.InvariantCulture) + ".unity3d"
                );

                int sz1 = Encoding.UTF8.GetByteCount(newCacheFile);
                int sz2 = Encoding.UTF8.GetByteCount(state.BundleFile);
                int sz = sz1 + 1 + sz2;
                byte[] buffer = new byte[sz];
                sz1 = Encoding.UTF8.GetBytes(newCacheFile, 0, newCacheFile.Length, buffer, 0);
                buffer[sz1] = (byte)'\n';
                sz2 = Encoding.UTF8.GetBytes(newCacheFile, 0, newCacheFile.Length, buffer, sz1 + 1);

                fs.Seek(0L, SeekOrigin.End);
                fs.Write(buffer, 0, sz1 + 1 + sz2);
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
        int hash = OSPathHelper.PathComparer.GetHashCode(bundleFile);

        manifestFolder = Path.Combine(_rootDir!, hash.ToString("x8", CultureInfo.InvariantCulture));
        return manifestFolder + ".toc";
    }

    private static void ReadOrEditManifest<TState>(string manifestFile, ref TState state, EditManifest<TState> action)
    {
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

                Thread.Sleep(new Random().Next(50, 200));
            }
        }
    }
}