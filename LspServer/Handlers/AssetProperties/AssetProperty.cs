using Newtonsoft.Json;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

public class AssetProperty
{
    [JsonProperty("key")]
    public required string Key { get; set; }

    [JsonProperty("range"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Range? Range { get; set; }

    [JsonProperty("value")]
    public object? Value { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("markdown")]
    public string? Markdown { get; set; }
}