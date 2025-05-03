using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public class AssetSpecDatabase
{
    /// <summary>
    /// Allow downloading the latest version of files from the internet instead of using a possibly outdated embedded version.
    /// </summary>
    public bool UseInternet { get; set; } = true;

    public JsonSerializerOptions? Options { get; set; }

    public IReadOnlyDictionary<QualifiedType, AssetTypeInformation> Types { get; private set; } = new Dictionary<QualifiedType, AssetTypeInformation>(0);

    public AssetInformation Information { get; private set; } = new AssetInformation
    {
        AssetAliases = new Dictionary<string, QualifiedType>(0),
        AssetCategories = new Dictionary<QualifiedType, string>(0),
        Types = new Dictionary<QualifiedType, TypeHierarchy>(0),
        ParentTypes = new Dictionary<QualifiedType, InverseTypeHierarchy>(0)
    };

    public ISpecType? FindType(string type, AssetFileType fileType)
    {
        type = QualifiedType.NormalizeType(type);
        if (AssetCategory.TypeOf.Type.Equals(type))
        {
            return AssetCategory.TypeOf;
        }

        string? assetType = null;
        int divIndex = type.IndexOf("::", 0, StringComparison.Ordinal);
        if (divIndex >= 0 && divIndex < type.Length - 2)
        {
            if (divIndex != 0)
                assetType = type.Substring(0, divIndex);
            type = type.Substring(divIndex + 2);
        }

        InverseTypeHierarchy hierarchy = Information.GetParentTypes(assetType != null ? new QualifiedType(assetType) : fileType.Type);

        for (int i = -1; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType qt = i < 0 ? hierarchy.Type : hierarchy.ParentTypes[hierarchy.ParentTypes.Length - i - 1];

            if (!Types.TryGetValue(qt, out AssetTypeInformation info))
            {
                continue;
            }

            ISpecType? t = info.Types.Find(p => p.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

            if (t != null)
            {
                return t;
            }
        }

        return null;
    }

    public SpecProperty? FindPropertyInfo(string property, AssetFileType fileType, SpecPropertyContext context = SpecPropertyContext.Property)
    {
        if (context is not SpecPropertyContext.Localization and not SpecPropertyContext.Property)
            throw new ArgumentOutOfRangeException(nameof(context));

        if (!fileType.IsValid)
        {
            return null;
        }

        string? assetType = null;
        int divIndex = property.IndexOf("::", 0, StringComparison.Ordinal);
        if (divIndex >= 0 && divIndex < property.Length - 2)
        {
            if (divIndex != 0)
                assetType = property.Substring(0, divIndex);
            property = property.Substring(divIndex + 2);
        }

        InverseTypeHierarchy hierarchy = Information.GetParentTypes(assetType != null ? new QualifiedType(assetType) : fileType.Type);

        for (int i = -1; i < hierarchy.ParentTypes.Length; ++i)
        {
            QualifiedType type = i < 0 ? hierarchy.Type : hierarchy.ParentTypes[hierarchy.ParentTypes.Length - i - 1];

            if (!Types.TryGetValue(type, out AssetTypeInformation info))
            {
                continue;
            }

            List<SpecProperty> props = context == SpecPropertyContext.Localization ? info.LocalizationProperties : info.Properties;
            SpecProperty? prop = props.Find(p => p.Key.Equals(property, StringComparison.OrdinalIgnoreCase));
            prop ??= props.Find(p =>
            {
                for (int i = 0; i < p.Aliases.Length; ++i)
                {
                    if (string.Equals(p.Aliases[i], property, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                return false;
            });

            if (prop != null)
            {
                return prop;
            }
        }

        return null;
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        Options ??= new JsonSerializerOptions
        {
            WriteIndented = true
        };

        AssetInformation? assetInfo;
        using (Stream? stream = await GetFileAsync(
            "https://raw.githubusercontent.com/DanielWillett/unturned-asset-file-vscode/refs/heads/master/Asset%20Spec/Assets.json",
            "Assets"
        ))
        {
            if (stream != null)
            {
                try
                {
                    assetInfo = await JsonSerializer.DeserializeAsync<AssetInformation>(stream, Options, token);
                }
                catch (Exception ex)
                {
                    Log("Error parsing Assets.json file.");
                    Log(ex.ToString());
                    assetInfo = null;
                }
            }
            else
            {
                assetInfo = null;
            }
        }

        if (assetInfo == null && UseInternet)
        {
            using Stream? stream = await GetFileAsync(null, "Assets");
            if (stream != null)
            {
                try
                {
                    assetInfo = await JsonSerializer.DeserializeAsync<AssetInformation>(stream, Options, token);
                }
                catch (Exception ex)
                {
                    Log("Error parsing embedded Assets.json file.");
                    Log(ex.ToString());
                }
            }
        }

        assetInfo ??= new AssetInformation();
        Dictionary<string, QualifiedType>? aliases = assetInfo.AssetAliases;
        assetInfo.AssetAliases = new Dictionary<string, QualifiedType>(aliases?.Count ?? 0, StringComparer.OrdinalIgnoreCase);
        if (aliases != null)
        {
            foreach (KeyValuePair<string, QualifiedType> kvp in aliases)
                assetInfo.AssetAliases.Add(kvp.Key, kvp.Value);
        }
        assetInfo.AssetCategories ??= new Dictionary<QualifiedType, string>(0);
        assetInfo.Types ??= new Dictionary<QualifiedType, TypeHierarchy>(0);
        assetInfo.ParentTypes ??= new Dictionary<QualifiedType, InverseTypeHierarchy>(0);

        Information = assetInfo;

        Dictionary<QualifiedType, AssetTypeInformation> types = new Dictionary<QualifiedType, AssetTypeInformation>(assetInfo.ParentTypes.Count);
        foreach (QualifiedType type in assetInfo.ParentTypes.Keys)
        {
            string normalizedTypeName = type.Normalized.Type.ToLowerInvariant();
            AssetTypeInformation? typeInfo;
            using (Stream? stream = await GetFileAsync(
                       $"https://raw.githubusercontent.com/DanielWillett/unturned-asset-file-vscode/refs/heads/master/Asset%20Spec/{Uri.EscapeDataString(normalizedTypeName)}.json",
                       normalizedTypeName
                   ))
            {
                if (stream != null)
                {
                    try
                    {
                        typeInfo = await JsonSerializer.DeserializeAsync<AssetTypeInformation>(stream, Options, token);
                    }
                    catch (Exception ex)
                    {
                        Log($"Error parsing {normalizedTypeName}.json file.");
                        Log(ex.ToString());
                        typeInfo = null;
                    }
                }
                else
                {
                    typeInfo = null;
                }
            }

            if (typeInfo == null && UseInternet)
            {
                using Stream? stream = await GetFileAsync(null, normalizedTypeName);
                if (stream != null)
                {
                    try
                    {
                        typeInfo = await JsonSerializer.DeserializeAsync<AssetTypeInformation>(stream, Options, token);
                    }
                    catch (Exception ex)
                    {
                        Log($"Error parsing embedded {normalizedTypeName}.json file.");
                        Log(ex.ToString());
                    }
                }
            }

            typeInfo ??= new AssetTypeInformation();
            types[typeInfo.Type] = typeInfo;
        }

        Types = types;
    }

    protected virtual async Task<Stream?> GetFileAsync(string? url, string fallbackEmbeddedResource)
    {
        if (!UseInternet || url == null)
            return GetEmbeddedStream(fallbackEmbeddedResource);

        try
        {
            using HttpClient httpClient = new HttpClient();

            using HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, url);

            using HttpResponseMessage response = await httpClient.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead);

            MemoryStream ms = new MemoryStream();

            await response.Content.CopyToAsync(ms);
            return ms;
        }
        catch (Exception ex)
        {
            Log($"Error downloading \"{url}\".");
            Log(ex.ToString());
        }

        return GetEmbeddedStream(fallbackEmbeddedResource);
    }

    private Stream? GetEmbeddedStream(string fallbackEmbeddedResource)
    {
        if (!fallbackEmbeddedResource.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            fallbackEmbeddedResource = "DanielWillett.UnturnedDataFileLspServer.Data..Asset_Spec." + fallbackEmbeddedResource + ".json";

        Stream? stream;
        try
        {
            stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fallbackEmbeddedResource);
            if (stream == null)
            {
                Log($"Couldn't find embedded resource \"{fallbackEmbeddedResource}\".");
            }
        }
        catch (Exception ex)
        {
            Log($"Error finding embedded resource \"{fallbackEmbeddedResource}\".");
            Log(ex.ToString());
            stream = null;
        }

        return stream;
    }

    protected virtual void Log(string msg)
    {
        Console.Write("AssetSpecDatabase >> ");
        Console.WriteLine(msg);
    }
}