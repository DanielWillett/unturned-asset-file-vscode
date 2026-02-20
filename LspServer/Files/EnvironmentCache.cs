using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class EnvironmentCache : ISpecDatabaseCache
{
    public const string EnvVarSpecCacheFolder = "UNTURNED_ASSET_SPEC_CACHE_FOLDER";

    private const int LatestVersion = 1;

    private readonly ILogger<EnvironmentCache> _logger;
    private readonly string _cacheDir;
    private readonly string _cacheMetaFile;
    private readonly string _informationFile;

    private CacheMetadata _cacheMetadataInfo;

    private const string CacheMetaName = "_cache.json";

    public string? LatestCommit => _cacheMetadataInfo.LatestCommit;

    public string RootDirectory => _cacheDir;

    public EnvironmentCache(ILogger<EnvironmentCache> logger)
    {
        _logger = logger;
        
        if (Environment.GetEnvironmentVariable(EnvVarSpecCacheFolder) is { Length: > 0 } str)
        {
            try
            {
                str = Path.GetFullPath(str);
                Directory.CreateDirectory(str);
                _cacheDir = str;
            }
            catch (ArgumentException) // invalid path
            {
                _logger.LogError("Invalid " + EnvVarSpecCacheFolder + " path: {0}.", str);
            }
            catch (IOException)
            {
                _logger.LogError("Failed to create " + EnvVarSpecCacheFolder + " path: {0}.", str);
            }
        }

        if (_cacheDir == null)
        {
            bool windows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            if (windows || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _cacheDir = Path.Combine(
                    Environment.GetFolderPath(
                        windows
                            ? Environment.SpecialFolder.CommonApplicationData
                            : Environment.SpecialFolder.InternetCache),
                    "UnturnedAssetFileLsp", "Cache"
                );
            }
            else
            {
                // Linux, FreeBSD
                _cacheDir = "/var/cache/UnturnedAssetFileLsp/Cache";
            }
        }

        _cacheMetaFile = Path.Combine(_cacheDir, CacheMetaName);
        _informationFile = Path.Combine(_cacheDir, "assets.json");

        ReadCacheMetadata();
    }

    [MemberNotNull(nameof(_cacheMetadataInfo))]
    private void ReadCacheMetadata()
    {
        CacheMetadata? cacheMetadataInfo = null;
        try
        {
            FileInfo fileInfo = new FileInfo(_cacheMetaFile);
            if (fileInfo.Length > 8192)
            {
                cacheMetadataInfo = null;
                _logger.LogWarning("Cache file too long: {0}. Files > 8192 B ignored.", _cacheMetaFile);
            }
            else
            {
                byte[] cacheMeta = File.ReadAllBytes(_cacheMetaFile);
                cacheMetadataInfo = (CacheMetadata?)JsonSerializer.Deserialize(cacheMeta, typeof(CacheMetadata), EnvironmentCacheGeneratedSerializerContext.Default);
                if (cacheMetadataInfo == null)
                {
                    _logger.LogWarning("Failed to read cache file {0}, null value.", _cacheMetaFile);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to read cache file {0}.", _cacheMetaFile);
        }
        catch (FileNotFoundException) { }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to access cache file {0}.", _cacheMetaFile);
        }

        _cacheMetadataInfo = cacheMetadataInfo ?? new CacheMetadata { Version = LatestVersion };
    }

    /// <inheritdoc />
    public Task CacheNewFilesAsync(IAssetSpecDatabase database, CancellationToken token = default)
    {
        // note: moved to using 304 Not Modified for caches instead of pulling the commit hash from the API
        //       not sure if this is better or not but I think it will be more consistant
        //       and simpler from a developer standpoint

        // ill leave this function here in case I need it in the future

        return Task.CompletedTask;
    }

    private string GetFileName(QualifiedType type)
    {
        return Path.Combine(_cacheDir, type.Normalized.Type.ToLowerInvariant() + ".json");
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        string path = GetFileName(type);
        return ReadFileAsync(path, state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        switch (file)
        {
            case KnownConfigurationFile.Assets:
                return ReadFileAsync(_informationFile, state, action, token);

            default:
                return Task.FromResult(false);
        }
    }

    private async Task<bool> ReadFileAsync<TState>(
        string file,
        TState state,
        Func<Stream, TState, CancellationToken, Task> action,
        CancellationToken token = default,
        int maxSize = 524288  /* 512 KiB */
    )
    {
        FileInfo fileInfo = new FileInfo(file);
        if (fileInfo.Length > maxSize)
        {
            _logger.LogWarning("Cache file too long: {0}. Files > {1} B ignored.", file, maxSize);
            return false;
        }

        using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
        try
        {
            await action(fs, state, token).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read cache file {0}.", file);
            return false;
        }
    }

    public class CacheMetadata
    {
        public int Version;
        public string? LatestCommit;

        public CacheMetadata Clone() => new CacheMetadata
        {
            Version = Version,
            LatestCommit = LatestCommit
        };
    }
}

[JsonSerializable(typeof(EnvironmentCache.CacheMetadata))]
[JsonSourceGenerationOptions(IncludeFields = true)]
internal partial class EnvironmentCacheGeneratedSerializerContext : JsonSerializerContext;

internal class EnvironmentCacheFile : IDisposable
{
    private readonly LspInstallationEnvironment _installEnvironment;

    public EnvironmentCacheEntry[] Entries { get; private set; }

    public EnvironmentCacheFile(LspInstallationEnvironment installEnvironment)
    {
        _installEnvironment = installEnvironment;
        Entries = Array.Empty<EnvironmentCacheEntry>();

        _installEnvironment.OnFileAdded += OnFileAdded;
        _installEnvironment.OnFileRemoved += OnFileRemoved;
        _installEnvironment.OnFileUpdated += OnFileUpdated;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _installEnvironment.OnFileAdded -= OnFileAdded;
        _installEnvironment.OnFileRemoved -= OnFileRemoved;
        _installEnvironment.OnFileUpdated -= OnFileUpdated;
    }

    private void OnFileUpdated(DiscoveredDatFile oldFile, DiscoveredDatFile newFile)
    {
        
    }

    private void OnFileRemoved(DiscoveredDatFile file)
    {
        
    }

    private void OnFileAdded(DiscoveredDatFile file)
    {
        
    }
}

public struct EnvironmentCacheEntry
{
    public Guid Guid { get; set; }
    public int Category { get; set; }
    public ushort Id { get; set; }
    public int StreamOffset { get; set; }
}