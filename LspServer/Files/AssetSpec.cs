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
}