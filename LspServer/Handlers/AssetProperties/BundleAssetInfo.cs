using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

public class BundleAssetInfo
{
    [JsonProperty("key")]
    public required string Key { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("typeName")]
    public required string TypeName { get; init; }

    [JsonPropertyName("path"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Path { get; set; }

    [JsonPropertyName("path"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Description { get; set; }

    [JsonPropertyName("path"), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string? Markdown { get; set; }

    [JsonPropertyName("isComponent")]
    public bool IsComponent { get; set; }
}