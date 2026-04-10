using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

public class AssetProperty
{
    [JsonIgnore]
    public required DatProperty Property { get; init; }

    [JsonProperty("key")]
    public required string Key { get; init; }

    [JsonProperty("range")]
    public Range? Range { get; set; }

    [JsonProperty("ordinal", DefaultValueHandling = DefaultValueHandling.Ignore)]
    public int IndexPlusOne { get; set; }

    [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
    public JToken? Value { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("markdown")]
    public string? Markdown { get; set; }

    [JsonProperty("children", NullValueHandling = NullValueHandling.Ignore)]
    public AssetProperty[]? Children { get; set; }

    [JsonProperty("typeHierarchy", NullValueHandling = NullValueHandling.Ignore)]
    public TypeHierarchyElement[]? TypeHierarchy { get; set; }

    [JsonProperty("bundlePath", NullValueHandling = NullValueHandling.Ignore)]
    public string? BundlePath { get; set; }

    public class TypeHierarchyElement
    {
        public required string Type { get; init; }
        public string? DisplayName { get; init; }
    }
}