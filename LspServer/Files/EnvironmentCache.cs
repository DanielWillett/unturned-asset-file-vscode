using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
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
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _cacheDir;
    private readonly string _cacheMetaFile;
    private readonly string _informationFile;

    private CacheMetadata _cacheMetadataInfo;

    private const string CacheMetaName = "_cache.json";

    public string? LatestCommit => _cacheMetadataInfo.LatestCommit;

    public EnvironmentCache(ILogger<EnvironmentCache> logger, JsonSerializerOptions jsonOptions)
    {
        _logger = logger;
        _jsonOptions = jsonOptions;
        
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

    private void WriteCacheMetadata()
    {
        CacheMetadata m = _cacheMetadataInfo.Clone();
        m.Version = LatestVersion;

        try
        {
            Directory.CreateDirectory(_cacheDir);
            using FileStream fs = new FileStream(_cacheMetaFile, FileMode.Create, FileAccess.Write, FileShare.Read, 8192);

            JsonSerializer.Serialize(fs, m, typeof(CacheMetadata), EnvironmentCacheGeneratedSerializerContext.Default);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to write cache file {0}.", _cacheMetaFile);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to access cache file {0}.", _cacheMetaFile);
        }
    }

    /// <inheritdoc />
    public bool IsUpToDateCache(string latestCommit)
    {
        return string.Equals(_cacheMetadataInfo.LatestCommit, latestCommit, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public async Task CacheNewFilesAsync(IAssetSpecDatabase database, CancellationToken token = default)
    {
        string? commit = database.Information.Commit;
        commit ??= database.Types.Values.FirstOrDefault(x => x.Commit != null)?.Commit;

        bool hasNewCommit = !string.Equals(_cacheMetadataInfo.LatestCommit, commit, StringComparison.Ordinal);
        if (hasNewCommit)
        {
            // delete all old files that could be from an older commit
            _cacheMetadataInfo.LatestCommit = commit;
            Thread.BeginCriticalRegion();
            try
            {
                WriteCacheMetadata();

                foreach (string file in Directory.EnumerateFiles(_cacheDir, "*.json", SearchOption.TopDirectoryOnly))
                {
                    if (Path.GetFileName(file.AsSpan()).Equals(CacheMetaName, StringComparison.Ordinal))
                        continue;

                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException)
                    {
                        _logger.LogInformation("Failed to delete old file.");
                    }
                }
            }
            finally
            {
                Thread.EndCriticalRegion();
            }

            _logger.LogInformation("Caching new files from commit {0}.", commit);
        }

        JsonSerializerOptions options = database.Options ?? _jsonOptions;

        if (string.Equals(database.Information.Commit, commit, StringComparison.Ordinal))
        {
            if (hasNewCommit || !File.Exists(_informationFile))
            {
                await WriteCacheFileAsync(_informationFile, database.Information, options, token).ConfigureAwait(false);
            }
        }

        foreach (AssetSpecType t in database.Types.Values)
        {
            if (!string.Equals(t.Commit, commit, StringComparison.Ordinal))
                continue;

            string path = Path.Combine(_cacheDir, t.Type.Normalized.Type.ToLowerInvariant() + ".json");


            if (!hasNewCommit && File.Exists(path))
                continue;

            await WriteCacheFileAsync(path, t, options, token).ConfigureAwait(false);
        }
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        string path = Path.Combine(_cacheDir, type.Normalized.Type.ToLowerInvariant() + ".json");
        return ReadFileAsync(path, state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file != KnownConfigurationFile.Assets)
        {
            return Task.FromResult(false);
        }

        return ReadFileAsync(_informationFile, state, action, token);
    }

    private async Task WriteCacheFileAsync<T>(string file, T value, JsonSerializerOptions options, CancellationToken token = default)
    {
        try
        {
            Directory.CreateDirectory(_cacheDir);
            await using FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Read);

            await JsonSerializer.SerializeAsync(fs, value, options, token).ConfigureAwait(false);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to write cache file {0}.", _cacheMetaFile);
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to access cache file {0}.", _cacheMetaFile);
        }
    }

    private async Task<T?> ReadCacheFileAsync<T>(string file, CancellationToken token = default) where T : class
    {
        T? cacheValue = null;
        try
        {
            FileInfo fileInfo = new FileInfo(file);
            if (fileInfo.Length > 524288)
            {
                cacheValue = null;
                _logger.LogWarning("Cache file too long: {0}. Files > 512 KB ignored.", file);
            }
            else
            {
                using FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
                cacheValue = await JsonSerializer.DeserializeAsync<T>(fs, _jsonOptions, token);
                if (cacheValue == null)
                {
                    _logger.LogWarning("Failed to read cache file {0}, null value.", file);
                }
            }
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to read cache file {0}.", file);
        }
        catch (FileNotFoundException) { }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "Failed to access cache file {0}.", file);
        }

        return cacheValue;
    }

    private async Task<bool> ReadFileAsync<TState>(string file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        FileInfo fileInfo = new FileInfo(file);
        if (fileInfo.Length > 524288)
        {
            _logger.LogWarning("Cache file too long: {0}. Files > 512 KB ignored.", file);
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
            _logger.LogWarning(ex, "Failed to read cache file {0}, null value.", file);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<AssetInformation?> GetCachedInformationAsync(CancellationToken token = default)
    {
        AssetInformation? info = await ReadCacheFileAsync<AssetInformation>(_informationFile, token).ConfigureAwait(false);

        if (info != null)
        {
            info.Commit = _cacheMetadataInfo.LatestCommit;
        }

        return info;
    }

    /// <inheritdoc />
    public async Task<AssetSpecType?> GetCachedTypeAsync(QualifiedType type, CancellationToken token = default)
    {
        string path = Path.Combine(_cacheDir, type.Normalized.Type.ToLowerInvariant() + ".json");

        AssetSpecType? specType = await ReadCacheFileAsync<AssetSpecType>(path, token).ConfigureAwait(false);

        if (specType != null)
        {
            specType.Commit = _cacheMetadataInfo.LatestCommit;
        }

        return specType;
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