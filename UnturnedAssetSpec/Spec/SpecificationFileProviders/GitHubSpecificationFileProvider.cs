using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

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
    private readonly ISpecDatabaseCache? _cache;

    public int Priority => 1;
    public bool IsEnabled => _allowInternet;

    public GitHubSpecificationFileProvider(
        ILogger<GitHubSpecificationFileProvider> logger,
        HttpClient httpClient,
        bool allowInternet,
        ISpecDatabaseCache? cache,
        Func<AssetInformation?> getAssetInfo) : this(logger,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        new Lazy<HttpClient>(httpClient),
#else
        new Lazy<HttpClient>(() => httpClient),
#endif
        allowInternet,
        cache,
        getAssetInfo
    )
    { }

    public GitHubSpecificationFileProvider(
        ILogger<GitHubSpecificationFileProvider> logger,
        Lazy<HttpClient> httpClient,
        bool allowInternet,
        ISpecDatabaseCache? cache,
        Func<AssetInformation?> getAssetInfo)
    {
        _logger = logger;
        _httpClient = httpClient;
        _allowInternet = allowInternet;
        _getAssetInfo = getAssetInfo;
        _cache = cache;
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
            : TryReadFromInternetAsync(
                file switch
                {
                    KnownConfigurationFile.GameStatus => info.StatusJsonFallbackUrl,
                    KnownConfigurationFile.SkillsLocalization => info.SkillsLocalizationFallbackUrl,
                    KnownConfigurationFile.CharacterLocalization => info.SkillsetsLocalizationFallbackUrl,
                    KnownConfigurationFile.InventoryLocalization => info.PlayerDashboardInventoryLocalizationFallbackUrl,
                    _ => null
                },
                file switch
                {
                    KnownConfigurationFile.GameStatus => "status-json.cache.bin",
                    KnownConfigurationFile.SkillsLocalization => "localization-skills.cache.bin",
                    KnownConfigurationFile.CharacterLocalization => "localization-character.cache.bin",
                    KnownConfigurationFile.InventoryLocalization => "localization-inventory.cache.bin",
                    _ => file + ".bin"
                },
                state,
                action,
                token
            );
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
                MemoryStream? ms = await DownloadFileAsync(MergedFileUrl, "asset-spec.cache.bin", token);
                if (ms == null)
                {
                    return false;
                }

                Dictionary<string, FileIndexEntry>? fileIndex = BuildMergedFileTableOfContents(ms);
                if (fileIndex == null)
                {
                    return false;
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
    }

    private Dictionary<string, FileIndexEntry>? BuildMergedFileTableOfContents(MemoryStream ms)
    {
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

        return fileIndex;

    corrupted:
        _logger.LogError(Resources.Log_InternetResourceCorrupted, MergedFileUrl);
        return null;
    }

    private async Task<bool> TryReadFromInternetAsync<TState>(
        string? url,
        string cacheFile,
        TState state,
        Func<Stream, TState, CancellationToken, Task> action,
        CancellationToken token
    )
    {
        MemoryStream? ms = await DownloadFileAsync(url, cacheFile, token);
        if (ms == null)
        {
            return false;
        }

        try
        {
            await action(ms, state, token);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToParseResource, url);
            return false;
        }
        finally
        {
            ms.Dispose();
        }
    }

    private async Task<MemoryStream?> DownloadFileAsync(string? url, string cacheFile, CancellationToken token)
    {
        string? cacheFilePath = _cache?.RootDirectory is not { Length: > 0 } rootDir ? null : Path.Combine(rootDir, cacheFile);

        CacheFileHeaderInfo hdr = await TryReadFileHeader(cacheFilePath, token);

        return await DownloadFileAsync(url, cacheFilePath, hdr, token);
    }

    private async Task<MemoryStream?> DownloadFileAsync(string? url, string? cacheFilePath, CacheFileHeaderInfo cachedHeader, CancellationToken token)
    {
        using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);

        //message.Version = HttpHelper.LatestVersion;
        message.Version = HttpVersion.Version10;
        bool canCache = !string.IsNullOrEmpty(cachedHeader.ETag)
                        && cachedHeader.HeaderSize != 0
                        && cachedHeader.Length is > 0 and <= int.MaxValue;

        if (canCache)
        {
            message.Headers.Add("If-None-Match", cachedHeader.ETag);
        }

        HttpHelper.AddUserAgentHeader(message);

        try
        {
            using HttpResponseMessage response = await _httpClient.Value.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);

            int amt;
            byte[] buffer;
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                if (!canCache || cacheFilePath == null)
                    return null;

                try
                {
                    buffer = new byte[cachedHeader.Length];
                    using FileStream fs = new FileStream(cacheFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
                    fs.Seek(cachedHeader.HeaderSize, SeekOrigin.Begin);

                    amt = await fs.ReadAsync(buffer, 0, buffer.Length, token);
                    return new MemoryStream(buffer, 0, amt, false, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading cached file at {0}.", cacheFilePath);
                }

                // download without cache
                return await DownloadFileAsync(url, cacheFilePath, default, token);
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"{string.Format(Resources.Log_FailedToDownloadInternetResource, url)} | {{0}} {{1}}",
                    (ushort)response.StatusCode,
                    response.ReasonPhrase
                );
                return null;
            }

            string? contentLengthStr = response.Content.Headers.GetValues("Content-Length").FirstOrDefault();
            if (!long.TryParse(contentLengthStr, NumberStyles.Number, CultureInfo.InvariantCulture, out long contentLength)
                || contentLength is < 0 or > int.MaxValue)
            {
                // 1 MiB
                contentLength = 1_048_576;
                _logger.LogWarning("GitHub didn't report a Content-Length for {0}.", url);
            }


            buffer = new byte[contentLength];
            using Stream stream = await response.Content.ReadAsStreamAsync(
#if NET5_0_OR_GREATER
            token
#endif
            );
            int index = 0;
            do
            {
                amt = await stream.ReadAsync(buffer, index, buffer.Length - index, token);
                index += amt;
            } while (amt > 0);

            if (cacheFilePath != null
                && response.Headers.GetValues("ETag").FirstOrDefault() is { Length: > 0 } etag)
            {
                await SaveLastModifiedCache(cacheFilePath, buffer, index, etag, token);
            }

            return new MemoryStream(buffer, 0, index, false, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToDownloadInternetResource, url);
            return null;
        }
    }

    private async Task SaveLastModifiedCache(string filePath, byte[] data, int dataCount, string etag, CancellationToken token)
    {
        string? dir = Path.GetDirectoryName(filePath);

        byte[] etagBytes = StringHelper.Utf8NoBom.GetBytes(etag);
        if (etagBytes.Length > 120)
        {
            return;
        }

        try
        {
            if (dir != null)
                Directory.CreateDirectory(dir);

            using FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write, 8192, FileOptions.SequentialScan);

            byte[] header = new byte[3 + 8 + etagBytes.Length];

            // version
            header[0] = 0;
            if (BitConverter.IsLittleEndian)
                Unsafe.WriteUnaligned(ref header[1], (ushort)etagBytes.Length);
            else
                Buffer.BlockCopy(BitConverter.GetBytes((ushort)etagBytes.Length), 0, header, 1, sizeof(ushort));

            Buffer.BlockCopy(etagBytes, 0, header, 3, etagBytes.Length);

            if (BitConverter.IsLittleEndian)
                Unsafe.WriteUnaligned<long>(ref header[3 + etagBytes.Length], dataCount);
            else
                Buffer.BlockCopy(BitConverter.GetBytes((long)dataCount), 0, header, 3 + etagBytes.Length, sizeof(long));

            await fs.WriteAsync(header, 0, header.Length, token);
            await fs.WriteAsync(data, 0, dataCount, token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error writing cache file after a new asset spec was downloaded to {0}.", filePath);
        }
    }

    private async Task<CacheFileHeaderInfo> TryReadFileHeader(string? filePath, CancellationToken token)
    {
        if (filePath == null)
        {
            return default;
        }

        try
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 128, FileOptions.SequentialScan);

            byte[] buffer = new byte[128];
            int c = await fs.ReadAsync(buffer, 0, 3, token);
            if (c < 3 || buffer[0] != 0)
                return default;

            ushort size = BitConverter.ToUInt16(buffer, 1);
            c = await fs.ReadAsync(buffer, 0, size + 8, token);
            if (c < size)
                return default;

            CacheFileHeaderInfo info;

            info.HeaderSize = fs.Position;
            info.ETag = StringHelper.Utf8NoBom.GetString(buffer, 0, size);
            info.Length = BitConverter.ToInt64(buffer, size);

            return info;
        }
        catch (FileNotFoundException) { }
        catch (DirectoryNotFoundException) { }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error reading header of cache file for {0}.", filePath);
        }

        return default;
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
    private struct CacheFileHeaderInfo
    {
        public string? ETag;
        public long Length;
        public long HeaderSize;
    }
}