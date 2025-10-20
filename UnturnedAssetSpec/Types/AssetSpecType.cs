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
public sealed class AssetSpecType : IPropertiesSpecType, IEquatable<AssetSpecType?>, ISpecPropertyType<CustomSpecTypeInstance>
{
    /// <summary>
    /// The GitHub commit (SHA) where this information was taken from, if any. Note that some information may have been pulled from other places.
    /// </summary>
    public string? Commit { get; set; }

    public QualifiedType Type { get; internal set; }

    /// <inheritdoc />
    public Type ValueType => typeof(CustomSpecTypeInstance);

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out CustomSpecTypeInstance? inst) || inst == null)
        {
            value = null!;
            return false;
        }

        value = inst;
        return true;
    }

    /// <inheritdoc />
    public void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : ISpecPropertyTypeVisitor
    {
        visitor.Visit(this);
    }

    public EnumSpecTypeValue Category { get; set; } = AssetCategory.None;

    public string? Docs { get; set; }

    public QualifiedType Parent { get; set; }

    public ushort VanillaIdLimit { get; set; }

    public bool RequireId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    /// <inheritdoc />
    string ISpecPropertyType.Type => Type.Type;

    public Version? Version { get; set; }

    public OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; set; } = OneOrMore<KeyValuePair<string, object?>>.Null;

#nullable disable
    public SpecProperty[] Properties { get; set; }
    public SpecProperty[] LocalizationProperties { get; set; }
    public SpecBundleAsset[] BundleAssets { get; set; }
    public ISpecType[] Types { get; set; }
#nullable restore

    AssetSpecType ISpecType.Owner { get => this; set => throw new NotSupportedException(); }

    /// <summary>
    /// Temporarily add a property for unit test purposes.
    /// </summary>
    internal IDisposable AddRootPropertyForTest(SpecPropertyContext context, SpecProperty property)
    {
        SpecProperty[] props = this.GetProperties(context);

        SpecProperty[] newArray = new SpecProperty[props.Length + 1];
        newArray[^1] = property;
        Array.Copy(props, 0, newArray, 0, props.Length);
        this.SetProperties(newArray, context);
        return new AddedTestProperty(this, property, context);
    }

    private class AddedTestProperty(AssetSpecType type, SpecProperty property, SpecPropertyContext context) : IDisposable
    {
        public void Dispose()
        {
            SpecProperty[] props = type.GetProperties(context);

            int index = Array.IndexOf(props, property);
            if (index < 0)
                return;

            SpecProperty[] newArray = new SpecProperty[props.Length - 1];
            Array.Copy(props, 0, newArray, 0, index);
            Array.Copy(props, index + 1, newArray, index, props.Length - (index + 1));
            type.SetProperties(newArray, context);
        }
    }

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
    public bool Equals(ISpecPropertyType? other) => other is AssetSpecType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<CustomSpecTypeInstance>? other) => other is AssetSpecType t && Equals(t);

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

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out CustomSpecTypeInstance? value)
    {
        // todo
        value = null;
        return false;
    }
}