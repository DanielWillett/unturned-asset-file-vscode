using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Handles reading asset files from some source.
/// </summary>
public interface ISpecificationFileProvider
{
    int Priority { get; }

    bool IsEnabled { get; }

    Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);

    Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);
}

public enum KnownConfigurationFile
{
    /// <summary>
    /// Assets.json file.
    /// </summary>
    Assets
}

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

public sealed class GitHubSpecificationFileProvider : ISpecificationFileProvider
{
    private readonly bool _allowInternet;
    private readonly ILogger<GitHubSpecificationFileProvider> _logger;
    private readonly Lazy<HttpClient> _httpClient;

    internal const string Repository = "DanielWillett/unturned-asset-file-vscode";
    internal const string AssetFileUrl = "https://raw.githubusercontent.com/" + Repository + "/refs/heads/master/Asset%20Spec/{0}.json";

    public int Priority => 1;
    public bool IsEnabled => _allowInternet;

    public GitHubSpecificationFileProvider(ILogger<GitHubSpecificationFileProvider> logger, HttpClient httpClient, bool allowInternet) : this(logger,
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        new Lazy<HttpClient>(httpClient),
#else
        new Lazy<HttpClient>(() => httpClient),
#endif
        allowInternet
    ) { }

    public GitHubSpecificationFileProvider(ILogger<GitHubSpecificationFileProvider> logger, Lazy<HttpClient> httpClient, bool allowInternet)
    {
        _logger = logger;
        _httpClient = httpClient;
        _allowInternet = allowInternet;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        string resourceLocation = string.Format(AssetFileUrl, type.Type.ToLowerInvariant());
        return ReadFromInternetAsync(resourceLocation, state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file != KnownConfigurationFile.Assets)
            return Task.FromResult(false);

        string resourceLocation = string.Format(AssetFileUrl, "Assets");
        return ReadFromInternetAsync(resourceLocation, state, action, token);
    }

    private async Task<bool> ReadFromInternetAsync<TState>(string url, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        try
        {
            using HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Get, url);
#if NET6_0_OR_GREATER
            message.Version = HttpVersion.Version30;
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            message.Version = HttpVersion.Version20;
#else
            message.Version = HttpVersion.Version11;
#endif

            using HttpResponseMessage response = await _httpClient.Value.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            using MemoryStream ms = new MemoryStream();

            await response.Content.CopyToAsync(ms).ConfigureAwait(false);
            ms.Seek(0L, SeekOrigin.Begin);
            try
            {
                await action(ms, state, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.Log_FailedToParseResource, url);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, Resources.Log_FailedToDownloadInternetResource, url);
            return false;
        }
    }
}

public sealed class CacheSpecificationFileProvider : ISpecificationFileProvider
{
    private readonly ISpecDatabaseCache _cache;
    private readonly string? _latestCommit;

    public int Priority => 2;
    public bool IsEnabled => _latestCommit == null || _cache.IsUpToDateCache(_latestCommit);

    public CacheSpecificationFileProvider(ISpecDatabaseCache cache, string? latestCommit)
    {
        _cache = cache;
        _latestCommit = latestCommit;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        return _cache.ReadAssetAsync(type, state, action, token);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        return _cache.ReadKnownFileAsync(file, state, action, token);
    }
}