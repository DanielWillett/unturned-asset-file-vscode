using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

// TODO: https://learn.microsoft.com/en-us/windows/win32/lwef/disk-cleanup#using-the-datadrivencleaner-object
internal class EnvironmentCache : ISpecDatabaseCache
{
    public const string EnvVarSpecCacheFolder = "UNTURNED_ASSET_SPEC_CACHE_FOLDER";

    private const int LatestVersion = 1;

    private readonly ILogger<EnvironmentCache> _logger;
    private readonly string _cacheDir;
    private readonly string _cacheMetaFile;

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

        _cacheDir ??= Path.Combine(UnturnedAssetFileLspServer.DataPath, "Cache");

        _cacheMetaFile = Path.Combine(_cacheDir, CacheMetaName);

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