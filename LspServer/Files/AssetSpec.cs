using LspServer.Types;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SDG.Unturned;
using System.Text.Json.Serialization;

namespace LspServer.Files;

public class AssetSpec
{
    public required string Type { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter<EAssetType>))]
    public EAssetType Category { get; set; }

    public required string DisplayName { get; set; }
    public required string? Parent { get; set; }

    public AssetSpecProperty[]? Properties { get; set; }

    [JsonIgnore]
    public AssetSpec? ParentSpec { get; set; }

    public AssetSpecProperty? FindProperty(string key)
    {
        for (AssetSpec? spec = this; spec != null; spec = spec.ParentSpec)
        {
            if (spec.Properties == null)
                continue;

            foreach (AssetSpecProperty property in spec.Properties)
            {
                if (property.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
            foreach (AssetSpecProperty property in spec.Properties)
            {
                if (property.Aliases == null)
                    continue;

                foreach (string alias in property.Aliases)
                {
                    if (alias.Equals(key, StringComparison.OrdinalIgnoreCase))
                        return property;
                }
            }
        }

        return null;
    }
}

public class AssetSpecProperty
{
    public required string Key { get; set; }
    public string[]? Aliases { get; set; }
    public required string Type { get; set; }
    public bool Required { get; set; }
    public bool CanBeInMetadata { get; set; }

    public object? DefaultValue { get; set; }
    public string? Description { get; set; }
    public string? Markdown { get; set; }
    public bool Deprecated { get; set; }

    public SymbolKind GetSymbolKind()
    {
        return KnownTypes.GetSymbolKind(Type);
    }
}