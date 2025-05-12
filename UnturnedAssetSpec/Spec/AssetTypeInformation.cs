using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

[JsonConverter(typeof(AssetTypeInformationConverter))]
[DebuggerDisplay("{DisplayName,nq}")]
public sealed class AssetTypeInformation : ISpecType
{
    public QualifiedType Type { get; internal set; }

    public string Category { get; set; } = "NONE";

    public string? Docs { get; set; }

    public QualifiedType Parent { get; set; }

    public ushort VanillaIdLimit { get; set; }

    public bool RequireId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

#nullable disable
    public List<SpecProperty> Properties { get; set; }
    public List<SpecProperty> LocalizationProperties { get; set; }
    public List<SpecBundleAsset> BundleAssets { get; set; }
    public List<ISpecType> Types { get; set; }
#nullable restore

    public bool Equals(AssetTypeInformation other) => other != null && Type.Equals(other.Type);
    public bool Equals(ISpecType other) => other is AssetTypeInformation t && Type.Equals(t.Type);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is AssetTypeInformation ti && Equals(ti);

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
                {
                    return property;
                }
            }
        }
        if (LocalizationProperties != null && context is SpecPropertyContext.Localization or SpecPropertyContext.Unspecified)
        {
            foreach (SpecProperty property in LocalizationProperties)
            {
                if (property.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return property;
                }
            }
        }

        return null;
    }
}