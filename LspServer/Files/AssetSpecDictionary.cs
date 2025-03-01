#if DEBUG
//#define FROM_INTERNET
#endif

using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LspServer.Files;

public class AssetSpecDictionary
{
    private readonly ILogger<AssetSpecDictionary> _logger;

    private const string AssetInfoFallbackResource = "LspServer..Asset_Spec.Assets.json";
    private const string AssetSpecFallbackResource = "LspServer..Asset_Spec.{0}.json";

    private readonly ConcurrentDictionary<string, AssetSpec> _cachedSpecs = new ConcurrentDictionary<string, AssetSpec>(StringComparer.OrdinalIgnoreCase);

    private AssetInformation? _assetInfo;

    public AssetSpecDictionary(ILogger<AssetSpecDictionary> logger)
    {
        _logger = logger;
    }

    public ValueTask<AssetInformation> GetAssetInformation(CancellationToken token = default)
    {
        if (_assetInfo != null)
            return new ValueTask<AssetInformation>(_assetInfo);

        return new ValueTask<AssetInformation>(Core(token));

        async Task<AssetInformation> Core(CancellationToken token)
        {
            await DownloadAssetInformationAsync(token).ConfigureAwait(false);

            return _assetInfo ??= new AssetInformation
            {
                AssetAliases = new Dictionary<string, string>(0),
                AssetTypes = Array.Empty<string>(),
                UseableAliases = new Dictionary<string, string>(0),
                UseableTypes = Array.Empty<string>()
            };
        }
    }

    public ValueTask<AssetSpec?> GetAssetSpecAsync(string type, bool onlyClrType, CancellationToken token = default)
    {
        if (!onlyClrType && !type.Contains('.'))
        {
            if (_assetInfo?.AssetAliases == null)
                return new ValueTask<AssetSpec?>(GetAssetSpecAfterDownloadingAsync(type, token));

            if (_assetInfo.AssetAliases.TryGetValue(type, out string? v))
                type = v;
        }

        string fileName = NormalizeAssemblyQualifiedName(type);

        if (_cachedSpecs.TryGetValue(fileName, out AssetSpec? spec))
        {
            return new ValueTask<AssetSpec?>(spec);
        }

        return new ValueTask<AssetSpec?>(DownloadAssetSpec(fileName, token));
    }

    private async Task<AssetSpec?> GetAssetSpecAfterDownloadingAsync(string type, CancellationToken token = default)
    {
        await DownloadAssetInformationAsync(token);
        if (_assetInfo?.AssetAliases == null || !_assetInfo.AssetAliases.TryGetValue(type, out string? value) || !value.Contains('.'))
            return null;

        return await GetAssetSpecAsync(value, true, token);
    }

    public static string NormalizeAssemblyQualifiedName(string aqn)
    {
        ReadOnlySpan<char> fileName = aqn.AsSpan().Trim();
        int versionIndex = fileName.IndexOf('=');
        while (versionIndex > 0 && fileName[versionIndex] != ',')
            --versionIndex;
        while (versionIndex > 0 && char.IsWhiteSpace(fileName[versionIndex - 1]))
            --versionIndex;

        if (versionIndex >= 0)
            fileName = fileName.Slice(0, versionIndex);

        int firstComma = fileName.IndexOf(',');
        if (firstComma == -1)
        {
            return string.Concat(fileName.Trim(), ", Assembly-CSharp");
        }

        if (firstComma > 0 && char.IsWhiteSpace(fileName[firstComma - 1]))
        {
            int endIndex = firstComma - 1;
            while (endIndex > 0 && char.IsWhiteSpace(fileName[endIndex]))
                --endIndex;

            int nextPart = firstComma;
            do
                ++nextPart;
            while (nextPart < fileName.Length && char.IsWhiteSpace(fileName[nextPart]));

            if (nextPart >= fileName.Length)
                return string.Concat(fileName.Slice(0, endIndex + 1), ", Assembly-CSharp");

            return string.Concat(fileName.Slice(0, endIndex + 1), ", ", fileName.Slice(nextPart));
        }

        return fileName.Length == aqn.Length ? aqn : new string(fileName);
    }

    private async Task DownloadAssetInformationAsync(CancellationToken token = default)
    {
        AssetInformation? aliases = null;
#if RELEASE || FROM_INTERNET
        Uri uri = new Uri("https://github.com/DanielWillett/UnturnedAssetFileLsp/tree/master/Asset%20Spec/Asset%20Aliases.json");

        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5d);
            using HttpResponseMessage message = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token);
            if (message.IsSuccessStatusCode)
            {
                byte[] content = await message.Content.ReadAsByteArrayAsync(token);
                try
                {
                    aliases = (AssetInformation?)JsonSerializer.Deserialize(content, typeof(AssetInformation), AssetSpecGeneratedSerializable.Default);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing aliases from the internet.");
                    aliases = null;
                }
            }
            else
            {
                _logger.LogError("Error downloading updated asset aliases: {0} - {1} ({2}) from {3}.", message.ReasonPhrase, message.StatusCode, (int)message.StatusCode, uri);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading updated asset aliases.");
        }
#endif
        if (aliases == null)
        {
            await using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(AssetInfoFallbackResource);
            if (stream == null)
            {
                _logger.LogError("Resource not found: {0}.", AssetInfoFallbackResource);
            }
            else
            {
                try
                {
                    aliases = (AssetInformation?)JsonSerializer.Deserialize(stream, typeof(AssetInformation), AssetSpecGeneratedSerializable.Default);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing aliases.");
                    aliases = null;
                }
            }
        }

        if (aliases is { AssetAliases: not null, UseableAliases: not null, AssetTypes: not null, UseableTypes: not null, AssetCategories: not null })
            _assetInfo = aliases;
    }

    private async Task<AssetSpec?> DownloadAssetSpec(string fileName, CancellationToken token = default)
    {
        if (_cachedSpecs.TryGetValue(fileName, out AssetSpec? spec))
        {
            return spec;
        }

#if RELEASE || FROM_INTERNET
        Uri uri = new Uri($"https://github.com/DanielWillett/UnturnedAssetFileLsp/tree/master/Asset%20Spec/{Uri.EscapeDataString(fileName.ToLowerInvariant())}.json");

        try
        {
            using HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5d);
            using HttpResponseMessage message = await client.GetAsync(uri, HttpCompletionOption.ResponseContentRead, token);
            if (message.IsSuccessStatusCode)
            {
                byte[] content = await message.Content.ReadAsByteArrayAsync(token);
                try
                {
                    spec = (AssetSpec?)JsonSerializer.Deserialize(content, typeof(AssetSpec), AssetSpecGeneratedSerializable.Default);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing spec from the internet: {0}.", fileName);
                    spec = null;
                }
            }
            else
            {
                _logger.LogError("Error downloading updated asset spec for {0}: {1} - {2} ({3}) from {4}.", fileName, message.ReasonPhrase, message.StatusCode, (int)message.StatusCode, uri);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading updated asset aliases.");
        }
#endif

        if (spec == null)
        {
            string resource = string.Format(AssetSpecFallbackResource, fileName.ToLowerInvariant());

            await using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (stream == null)
            {
                _logger.LogError("Resource not found: {0}.", resource);
            }
            else
            {
                try
                {
                    spec = (AssetSpec?)JsonSerializer.Deserialize(stream, typeof(AssetSpec), AssetSpecGeneratedSerializable.Default);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error deserializing spec: {0}.", fileName);
                    spec = null;
                }
            }
        }

        // ReSharper disable RedundantAlwaysMatchSubpattern
        if (spec is { DisplayName: not null, Type: not null })
        {
            spec.Type = fileName;
            if (spec.Parent != null && spec.Parent.Contains('.'))
            {
                spec.ParentSpec = await GetAssetSpecAsync(spec.Parent, true, token);
            }

            _cachedSpecs[fileName] = spec;
        }
        // ReSharper restore RedundantAlwaysMatchSubpattern

        return spec;
    }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(AssetSpec), GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(AssetInformation), GenerationMode = JsonSourceGenerationMode.Metadata)]
internal partial class AssetSpecGeneratedSerializable : JsonSerializerContext;