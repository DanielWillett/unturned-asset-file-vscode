using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

[JsonConverter(typeof(AssetTypeInformationConverter))]
public class AssetTypeInformation
{
    public QualifiedType Type { get; set; }

    public string Category { get; set; } = "NONE";

    public string? Docs { get; set; }

    public QualifiedType Parent { get; set; }

    public ushort VanillaIdLimit { get; set; }

    public bool RequireId { get; set; }

#nullable disable

    public string DisplayName { get; set; }

    public List<SpecProperty> Properties { get; set; }
    public List<SpecProperty> LocalizationProperties { get; set; }
    public List<SpecBundleAsset> BundleAssets { get; set; }
    public List<ISpecType> Types { get; set; }
#nullable restore
}