using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Handles reading asset files from some source.
/// </summary>
public interface ISpecificationFileProvider
{
    /// <summary>
    /// The priority of this file provider.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether or not this file provider can be used.
    /// </summary>
    bool IsEnabled { get; }

    Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);

    Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);
}

public enum KnownConfigurationFile
{
    /// <summary>
    /// <c>Asset Spec/Assets.json</c> file.
    /// </summary>
    Assets,

    /// <summary>
    /// <c>Unturned/Status.json</c> file.
    /// </summary>
    GameStatus,

    /// <summary>
    /// <c>Unturned/Localization/English/Player/PlayerDashboardInventory.dat</c> file.
    /// </summary>
    InventoryLocalization,

    /// <summary>
    /// <c>Unturned/Localization/English/Menu/Survivors/MenuSurvivorsCharacter.dat</c> file.
    /// </summary>
    CharacterLocalization,

    /// <summary>
    /// <c>Unturned/Localization/English/Player/PlayerDashboardSkills.dat</c> file.
    /// </summary>
    SkillsLocalization
}

/// <summary>
/// Pulls specification files from the assembly's embedded resources.
/// </summary>
public sealed class EmbeddedResourceSpecificationFileProvider : ISpecificationFileProvider
{
    private readonly ILogger<EmbeddedResourceSpecificationFileProvider> _logger;

    internal const string EmbeddedResourceLocation = "DanielWillett.UnturnedDataFileLspServer.Data..Asset_Spec.{0}.json";

    public int Priority => 0;

    public bool IsEnabled => true;

    public EmbeddedResourceSpecificationFileProvider(ILogger<EmbeddedResourceSpecificationFileProvider> logger)
    {
        _logger = logger;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        string resourceLocation = string.Format(EmbeddedResourceLocation, type.Type.ToLowerInvariant());
        return ReadResourceAsync(resourceLocation, state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file != KnownConfigurationFile.Assets)
            return Task.FromResult(false);

        string resourceLocation = string.Format(EmbeddedResourceLocation, "Assets");
        return ReadResourceAsync(resourceLocation, state, action, token);
    }

    private async Task<bool> ReadResourceAsync<TState>(string resourceLocation, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        Stream? stream;
        try
        {
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation);
            if (stream == null)
            {
                _logger.LogWarning(Resources.Log_FailedToFindEmbeddedResource, resourceLocation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Resources.Log_FailedToFindEmbeddedResource, resourceLocation);
            stream = null;
        }

        if (stream == null)
        {
            return false;
        }

        try
        {
            await action(stream, state, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToParseResource, resourceLocation);
            return false;
        }
        finally
        {
            stream.Dispose();
        }

        return true;
    }
}

/// <summary>
/// Pulls specification files from this project's GitHub repository.
/// </summary>
public sealed class GitHubSpecificationFileProvider : ISpecificationFileProvider, IDisposable
{
    private readonly bool _allowInternet;
    private readonly ILogger<GitHubSpecificationFileProvider> _logger;
    private readonly Lazy<HttpClient> _httpClient;
    private readonly Func<AssetInformation?> _getAssetInfo;

    internal const string ThisProjectRepository = "DanielWillett/unturned-asset-file-vscode";
    internal const string MergedFileUrl = "https://raw.githubusercontent.com/" + ThisProjectRepository + "/refs/heads/master/Asset%20Spec/asset-spec.g.bin";

    private MemoryStream? _mergedFile;
    private int _tries;
    private Dictionary<string, FileIndexEntry>? _fileIndex;

    public int Priority => 1;
    public bool IsEnabled => _allowInternet;

    public GitHubSpecificationFileProvider(ILogger<GitHubSpecificationFileProvider> logger, HttpClient httpClient, bool allowInternet, Func<AssetInformation?> getAssetInfo) : this(logger,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        new Lazy<HttpClient>(httpClient),
#else
        new Lazy<HttpClient>(() => httpClient),
#endif
        allowInternet,
        getAssetInfo
    ) { }

    public GitHubSpecificationFileProvider(ILogger<GitHubSpecificationFileProvider> logger, Lazy<HttpClient> httpClient, bool allowInternet, Func<AssetInformation?> getAssetInfo)
    {
        _logger = logger;
        _httpClient = httpClient;
        _allowInternet = allowInternet;
        _getAssetInfo = getAssetInfo;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        return ReadFromMergedInternetFileAsync(type.Type.ToLowerInvariant() + ".json", state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file == KnownConfigurationFile.Assets)
        {
            return ReadFromMergedInternetFileAsync("Assets.json", state, action, token);
        }

        AssetInformation? info = _getAssetInfo();

        return info == null
            ? Task.FromResult(false)
            : ReadFromInternetAsync(file switch
            {
                KnownConfigurationFile.GameStatus => info.StatusJsonFallbackUrl,
                KnownConfigurationFile.SkillsLocalization => info.SkillsLocalizationFallbackUrl,
                KnownConfigurationFile.CharacterLocalization => info.SkillsetsLocalizationFallbackUrl,
                KnownConfigurationFile.InventoryLocalization => info.PlayerDashboardInventoryLocalizationFallbackUrl,
                _ => null
            }, state, action, token);
    }

    private async Task<bool> ReadFromMergedInternetFileAsync<TState>(string fileName, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        if (_mergedFile == null)
        {
            switch (_tries)
            {
                case >= 2:
                    return false;

                case 1:
                    await Task.Delay(500, token);
                    break;
            }

            ++_tries;

            try
            {
                using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, MergedFileUrl);
                message.Version = HttpVersionUtility.LatestVersion;

                using HttpResponseMessage response = await _httpClient.Value.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                string? contentLengthStr = response.Headers.GetValues("content-length").FirstOrDefault();
                if (!long.TryParse(contentLengthStr, NumberStyles.Number, CultureInfo.InvariantCulture, out long contentLength))
                {
                                 // 1 MiB
                    contentLength = 1_048_576;
                }

                MemoryStream ms = new MemoryStream(new byte[contentLength], writable: true);

                await response.Content.CopyToAsync(ms
#if NET5_0_OR_GREATER
                    , token
#endif
                ).ConfigureAwait(false);

                ms.Seek(0, SeekOrigin.Begin);

                if (ms.Length < 17)
                    goto corrupted;

                Dictionary<string, FileIndexEntry> fileIndex = new Dictionary<string, FileIndexEntry>(128, StringComparer.OrdinalIgnoreCase);
                
                byte[] buffer = ms.GetBuffer();

                // int version = BitConverter.ToInt32(buffer, 0);
                int maxFileNameSize = BitConverter.ToInt32(buffer, 4);
                int fileCt = BitConverter.ToInt32(buffer, 8);
                int hdrSize = BitConverter.ToInt32(buffer, 12);
                if (buffer[16] != '\n' || ms.Length < hdrSize)
                    goto corrupted;

                int index = 17;
                char[] fileNameBuffer = new char[maxFileNameSize];
                Decoder decoder = Encoding.UTF8.GetDecoder();
                for (int i = 0; i < fileCt; ++i)
                {
                    int len = buffer[index];
                    ++index;
                    string fn;
                    unsafe
                    {
                        fixed (byte* binPtr = buffer)
                        fixed (char* chrPtr = fileNameBuffer)
                        {
                            decoder.Convert(binPtr + index, len, chrPtr, maxFileNameSize, true, out _, out int charsUsed, out _);
                            fn = new string(chrPtr, 0, charsUsed);
                        }
                    }

                    index += len;
                    long offset = BitConverter.ToInt64(buffer, index);
                    index += 8;
                    long length = BitConverter.ToInt64(buffer, index);
                    index += 8;
                    if (length is < 0 or >= 1_000_000 || offset < hdrSize || offset >= ms.Length || offset > int.MaxValue)
                        goto corrupted;

                    fileIndex[fn] = new FileIndexEntry
                    {
                        Offset = (int)offset,
                        Length = (int)length
                    };

                    if (buffer[index] != '\n')
                        goto corrupted;
                    ++index;
                }

                Interlocked.Exchange(ref _mergedFile, ms)?.Dispose();
                _fileIndex = fileIndex;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.Log_FailedToDownloadInternetResource, MergedFileUrl);
                return false;
            }
        }

        if (_fileIndex == null || !_fileIndex.TryGetValue(fileName, out FileIndexEntry entry))
        {
            return false;
        }

        MemoryStream stream = new MemoryStream(_mergedFile.GetBuffer(), entry.Offset, entry.Length);
        try
        {
            await action(stream, state, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToParseResource, fileName);
            return false;
        }

        return true;

        corrupted:
        _logger.LogError(Resources.Log_InternetResourceCorrupted, MergedFileUrl);
        return false;
    }

    private async Task<bool> ReadFromInternetAsync<TState>(string? url, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        if (string.IsNullOrEmpty(url))
            return false;

        try
        {
            using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);
            message.Version = HttpVersionUtility.LatestVersion;

            using HttpResponseMessage response = await _httpClient.Value.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            Stream? stream = null;
            try
            {
                stream = await response.Content.ReadAsStreamAsync(
#if NET5_0_OR_GREATER
                    token
#endif
                );

                try
                {
                    await action(stream, state, token).ConfigureAwait(false);
                    return true;
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, Resources.Log_FailedToParseResource, url);
                    return false;
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToDownloadInternetResource, url);
            return false;
        }
    }

    public void Dispose()
    {
        _mergedFile?.Dispose();
    }

    private struct FileIndexEntry
    {
        public int Offset;
        public int Length;
    }
}

/// <summary>
/// Pulls specification files from a <see cref="ISpecDatabaseCache"/>.
/// </summary>
public sealed class CacheSpecificationFileProvider : ISpecificationFileProvider
{
    private readonly ISpecDatabaseCache? _cache;
    private readonly string? _latestCommit;

    public int Priority => 2;
    public bool IsEnabled => _cache != null && _latestCommit != null && _cache.IsUpToDateCache(_latestCommit);

    public CacheSpecificationFileProvider(ISpecDatabaseCache? cache, string? latestCommit)
    {
        _cache = cache;
        _latestCommit = latestCommit;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        return _cache?.ReadAssetAsync(type, state, action, token) ?? Task.FromResult(false);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        return _cache?.ReadKnownFileAsync(file, state, action, token) ?? Task.FromResult(false);
    }
}

/// <summary>
/// Pulls files from an available Unturned installation.
/// </summary>
public sealed class UnturnedInstallationFileProvider : ISpecificationFileProvider
{
    private readonly InstallDirUtility? _installDir;
    private readonly ILogger<UnturnedInstallationFileProvider> _logger;
    public int Priority => 3;
    public bool IsEnabled => _installDir != null;

    public UnturnedInstallationFileProvider(InstallDirUtility? installDir, ILogger<UnturnedInstallationFileProvider> logger)
    {
        _installDir = installDir;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file is not KnownConfigurationFile.GameStatus
                and not KnownConfigurationFile.CharacterLocalization
                and not KnownConfigurationFile.InventoryLocalization
                and not KnownConfigurationFile.SkillsLocalization
            || _installDir == null
            || !_installDir.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            return false;
        }

        string? statusPath = installDir.GetFile(file switch
        {
            KnownConfigurationFile.GameStatus            => "Status.json",
            KnownConfigurationFile.CharacterLocalization => @"Localization\English\Menu\Survivors\MenuSurvivorsCharacter.dat",
            KnownConfigurationFile.InventoryLocalization => @"Localization\English\Player\PlayerDashboardInventory.dat",
            KnownConfigurationFile.SkillsLocalization    => @"Localization\English\Player\PlayerDashboardSkills.dat",
            _ => null
        });

        if (File.Exists(statusPath))
        {
            try
            {
                using FileStream fs = new FileStream(statusPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
                await action(fs, state, token).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.Log_FailedToParseResource, statusPath);
            }
        }

        return false;
    }
}