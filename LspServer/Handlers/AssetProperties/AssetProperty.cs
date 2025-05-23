using System.Text.Json.Serialization;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

public class AssetProperty
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("range"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Range? Range { get; set; }

    [JsonPropertyName("value")]
    public object? Value { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("markdown")]
    public string? Markdown { get; set; }
}