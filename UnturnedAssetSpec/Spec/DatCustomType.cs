using System.Collections.Immutable;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// A type referenced by a file type.
/// </summary>
public class DatCustomType : DatTypeWithProperties
{
    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.Custom;

    /// <inheritdoc />
    private protected override string FullName => $"{Owner.TypeName.GetFullTypeName()}/{TypeName.GetFullTypeName()}";

    /// <inheritdoc />
    public override DatFileType Owner { get; }

    internal DatCustomType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element, DatFileType file) : base(type, baseType, element)
    {
        Owner = file;
    }
}

/// <summary>
/// A type referenced by an asset file type.
/// </summary>
public class DatCustomAssetType : DatCustomType, IDatTypeWithLocalizationProperties, IDatTypeWithBundleAssets
{
    /// <inheritdoc />
    public override DatSpecificationType Type => DatSpecificationType.CustomAsset;

    /// <inheritdoc />
    public ImmutableArray<DatProperty> LocalizationProperties { get; internal set; }

    /// <inheritdoc />
    public ImmutableArray<DatBundleAsset> BundleAssets { get; internal set; }

    internal DatCustomAssetType(QualifiedType type, DatTypeWithProperties? baseType, JsonElement element, DatAssetFileType file)
        : base(type, baseType, element, file)
    {
        LocalizationProperties = ImmutableArray<DatProperty>.Empty;
        BundleAssets = ImmutableArray<DatBundleAsset>.Empty;
    }
}