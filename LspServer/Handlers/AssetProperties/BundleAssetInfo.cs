using Newtonsoft.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

public class BundleAssetInfo
{
    [JsonProperty("key")]
    public required string Key { get; init; }

    [JsonProperty("type")]
    public required string Type { get; init; }

    [JsonProperty("typeName")]
    public required string TypeName { get; init; }

    [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
    public string? Path { get; set; }

    [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    [JsonProperty("markdown", NullValueHandling = NullValueHandling.Ignore)]
    public string? Markdown { get; set; }

    [JsonProperty("isComponent")]
    public bool IsComponent { get; set; }

    [JsonProperty("hasChildren")]
    public bool HasChildren { get; set; }

    [JsonProperty("isRequired")]
    public bool IsRequired { get; set; }

    [JsonProperty("isUnknown")]
    public bool IsUnknown { get; set; }
}