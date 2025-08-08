using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[JsonConverter(typeof(AssetSpecTypeConverter))]
[DebuggerDisplay("Type: {Type.GetTypeName()}")]
public sealed class AssetSpecType : IPropertiesSpecType, IEquatable<AssetSpecType?>
{
    public QualifiedType Type { get; internal set; }

    public EnumSpecTypeValue Category { get; set; } = AssetCategory.None;

    public string? Docs { get; set; }

    public QualifiedType Parent { get; set; }

    public ushort VanillaIdLimit { get; set; }

    public bool RequireId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; set; } = OneOrMore<KeyValuePair<string, object?>>.Null;

#nullable disable
    public SpecProperty[] Properties { get; set; }
    public SpecProperty[] LocalizationProperties { get; set; }
    public SpecBundleAsset[] BundleAssets { get; set; }
    public ISpecType[] Types { get; set; }
#nullable restore

    AssetSpecType ISpecType.Owner { get => this; set => throw new NotSupportedException(); }

    public bool Equals(AssetSpecType? other)
    {
        if (other == null)
            return false;

        return !Type.Equals(other.Type)
            || !Category.Equals(other.Category)
            || !string.Equals(Docs, other.Docs, StringComparison.Ordinal)
            || !Parent.Equals(other.Parent)
            || VanillaIdLimit != other.VanillaIdLimit
            || RequireId != other.RequireId
            || !string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal)
            || !EquatableArray.EqualsEquatable(Properties, other.Properties)
            || !EquatableArray.EqualsEquatable(LocalizationProperties, other.LocalizationProperties)
            || !EquatableArray.EqualsEquatable(BundleAssets, other.BundleAssets)
            || !EquatableArray.EqualsEquatable(Types, other.Types);
    }

    public bool Equals(ISpecType? other) => other is AssetSpecType t && Type.Equals(t.Type);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AssetSpecType ti && Equals(ti);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Type.GetHashCode();
    }

    /// <inheritdoc />
    public override string ToString() => Type.ToString();

    public SpecProperty? FindProperty(string propertyName, SpecPropertyContext context)
    {
        if (Properties != null && context is SpecPropertyContext.Property or SpecPropertyContext.Unspecified)
        {
            foreach (SpecProperty property in Properties)
            {
                if (property.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
        }
        if (LocalizationProperties != null && context is SpecPropertyContext.Localization or SpecPropertyContext.Unspecified)
        {
            foreach (SpecProperty property in LocalizationProperties)
            {
                if (property.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    return property;
            }
        }

        return null;
    }
}