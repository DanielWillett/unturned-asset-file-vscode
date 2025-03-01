using SDG.Unturned;
using System.Text.Json.Serialization;

namespace LspServer.Files;

public class AssetInformation
{
    public string[]? AssetTypes { get; set; }
    public string[]? UseableTypes { get; set; }
    public Dictionary<string, string>? AssetAliases { get; set; }
    public Dictionary<string, string>? UseableAliases { get; set; }

    [JsonIgnore]
    public Dictionary<string, EAssetType>? AssetCategories { get; set; }

    [JsonPropertyName(nameof(AssetCategories))]
    public Dictionary<string, string>? AssetCategoriesJson
    {
        get => AssetCategories == null ? null : new Dictionary<string, string>(AssetCategories.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())));
        set => AssetCategories = value == null ? null : new Dictionary<string, EAssetType>(value.Select(x => new KeyValuePair<string, EAssetType>(x.Key, Enum.Parse<EAssetType>(x.Value))));
    }
}