using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[DebuggerDisplay("Type: {Type.GetTypeName()}")]
public sealed class CustomSpecType : IPropertiesSpecType, ISpecPropertyType<CustomSpecTypeInstance>, IEquatable<CustomSpecType>
{
    public required QualifiedType Type { get; init; }
    public required string DisplayName { get; init; }
    public required QualifiedType Parent { get; init; }
    public required SpecProperty[] Properties { get; set; }
    public required SpecProperty[] LocalizationProperties { get; set; }
    public required string? Docs { get; init; }
    public required OneOrMore<KeyValuePair<string, object?>> ExtendedData { get; init; }


    public Type ValueType => typeof(CustomSpecTypeInstance);
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    string ISpecPropertyType.Type => Type.Type;
    SpecBundleAsset[] IPropertiesSpecType.BundleAssets
    {
        get => Array.Empty<SpecBundleAsset>();
        set
        {
            if (value.Length > 0)
                throw new ArgumentException("Custom types do not support bundle assets.", nameof(value));
        }
    }

#nullable disable
    public AssetSpecType Owner { get; set; }
#nullable restore

    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => null;

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        value = null!;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(CustomSpecType? other) => other != null && Type.Equals(other.Type);

    /// <inheritdoc />
    public bool Equals(ISpecType? other) => other is CustomSpecType t && Equals(t);
    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is CustomSpecType t && Equals(t);

    public bool Equals(ISpecPropertyType<CustomSpecTypeInstance>? other) => other is CustomSpecType t && Equals(t);

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out CustomSpecTypeInstance? value)
    {
        // todo
        value = null;
        return false;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CustomSpecType t && Equals(t);

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

public class CustomSpecTypeInstance : IEquatable<CustomSpecTypeInstance>, ISpecDynamicValue
{
    public CustomSpecType Type { get; }

    public CustomSpecTypeInstance(CustomSpecType type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public bool Equals(CustomSpecTypeInstance other) => Equals(other, StringComparison.Ordinal);
    public bool Equals(CustomSpecTypeInstance other, StringComparison comparisonType)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(other, this))
            return true;

        return Type.Equals(other.Type);
    }


    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Comparand is not CustomSpecTypeInstance customInstance || !customInstance.Type.Equals(Type))
        {
            return condition.Operation.EvaluateNulls(false, true);
        }

        bool equal = Equals(customInstance, condition.Operation.IsCaseInsensitive() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        return equal && condition.Operation.IsEquality() || !equal && condition.Operation.IsInequality();
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        if (typeof(TValue) != typeof(CustomSpecTypeInstance))
        {
            isNull = false;
            value = default;
            return false;
        }

        value = SpecDynamicEquationTreeValueHelpers.As<CustomSpecTypeInstance, TValue>(this);
        isNull = false;
        return true;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStartObject();

        // todo

        writer.WriteEndObject();
    }

    ISpecPropertyType ISpecDynamicValue.ValueType => Type;
}