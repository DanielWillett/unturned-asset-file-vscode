using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Handles reading asset files into a <see cref="IAssetSpecDatabase"/>.
/// </summary>
public class SpecificationFileReader
{
    private readonly Func<SpecificationFileReader, ISpecificationFileProvider[]> _fileProviderFactory;

    private readonly ILoggerFactory _loggerFactory;
    private readonly Lazy<HttpClient> _httpClientFactory;
    private readonly ISpecDatabaseCache? _cache;

    public bool AllowInternet { get; }

    public string? LatestCommitHash { get; private set; }
    
    private bool IsCacheUpToDate { get; set; }

    public SpecificationFileReader(
        bool allowInternet,
        ILoggerFactory loggerFactory,
        Lazy<HttpClient> httpClientFactory,
        Func<SpecificationFileReader, ISpecificationFileProvider[]>? fileProviderFactory = null,
        ISpecDatabaseCache? cache = null)
    {
        _fileProviderFactory = fileProviderFactory ?? (static reader =>
        {
            if (reader.AllowInternet)
            {
                if (reader._cache == null)
                {
                    return
                    [
                        new GitHubSpecificationFileProvider(reader._loggerFactory.CreateLogger<GitHubSpecificationFileProvider>(), reader._httpClientFactory, true),
                        new EmbeddedResourceSpecificationFileProvider(reader._loggerFactory.CreateLogger<EmbeddedResourceSpecificationFileProvider>())
                    ];
                }

                return
                [
                    new CacheSpecificationFileProvider(reader._cache, reader.LatestCommitHash),
                    new GitHubSpecificationFileProvider(reader._loggerFactory.CreateLogger<GitHubSpecificationFileProvider>(), reader._httpClientFactory, true),
                    new EmbeddedResourceSpecificationFileProvider(reader._loggerFactory.CreateLogger<EmbeddedResourceSpecificationFileProvider>())
                ];
            }

            if (reader._cache == null)
            {
                return
                [
                    new EmbeddedResourceSpecificationFileProvider(reader._loggerFactory.CreateLogger<EmbeddedResourceSpecificationFileProvider>())
                ];
            }

            return
            [
                new CacheSpecificationFileProvider(reader._cache, reader.LatestCommitHash),
                new EmbeddedResourceSpecificationFileProvider(reader._loggerFactory.CreateLogger<EmbeddedResourceSpecificationFileProvider>())
            ];
        });

        _loggerFactory = loggerFactory;
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        AllowInternet = allowInternet;
    }

    public async Task ReadSpecifications(CancellationToken token)
    {
        using HttpClient client = new HttpClient();
        IsCacheUpToDate = false;
        if (AllowInternet && _cache != null)
        {
            LatestCommitHash = await GetLatestCommitAsync(client, token).ConfigureAwait(false);
            if (LatestCommitHash != null)
            {
                IsCacheUpToDate = _cache.IsUpToDateCache(LatestCommitHash);
            }
        }

        ISpecificationFileProvider[] providers = _fileProviderFactory(this);
        Array.Sort(providers, (a, b) => b.Priority.CompareTo(a.Priority));


    }

    internal static async Task<string?> GetLatestCommitAsync(HttpClient httpClient, CancellationToken token)
    {
        const string getLatestCommitUrl = $"https://api.github.com/repos/{GitHubSpecificationFileProvider.Repository}/commits?per_page=1";

        JsonDocument? doc = null;
        try
        {
            using (HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, getLatestCommitUrl))
            {
                msg.Headers.Add("Accept", "application/vnd.github+json");
                msg.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
                msg.Headers.Add("User-Agent", $"unturned-asset-file-vscode/{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}");

                using (HttpResponseMessage response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseContentRead, token))
                {
                    // read commit @ root[0].sha
                    doc = await JsonDocument.ParseAsync(
                        await response.Content.ReadAsStreamAsync(),
                        new JsonDocumentOptions
                        {
                            AllowTrailingCommas = false,
                            CommentHandling = JsonCommentHandling.Disallow,
                            MaxDepth = 8
                        }, token);
                }
            }

            JsonElement root = doc.RootElement;
            if (root.ValueKind != JsonValueKind.Array)
                return null;

            JsonElement commitInfo;
            using (JsonElement.ArrayEnumerator arrayEnumerator = root.EnumerateArray())
            {
                if (!arrayEnumerator.MoveNext())
                    return null;

                commitInfo = arrayEnumerator.Current;
            }

            if (!commitInfo.TryGetProperty("sha"u8, out JsonElement sha) || sha.ValueKind != JsonValueKind.String)
                return null;

            return sha.GetString();
        }
        finally
        {
            doc?.Dispose();
        }
    }
}