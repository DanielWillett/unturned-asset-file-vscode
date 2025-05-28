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
    // example value could be Condition for Conditions
    public const string PluralBaseKeyProperty = "PluralBaseKey";

    public required QualifiedType Type { get; init; }
    public required string DisplayName { get; init; }
    public required QualifiedType Parent { get; init; }
    public required SpecProperty[] Properties { get; set; }
    public required SpecProperty[] LocalizationProperties { get; set; }
    public required string? Docs { get; init; }
    public required bool IsLegacyExpandedType { get; init; }
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
        return TryParseValue(in parse, out value, CustomSpecTypeParseOptions.Object);
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out CustomSpecTypeInstance? value, CustomSpecTypeParseOptions options)
    {
        value = null;
        if (options == CustomSpecTypeParseOptions.Legacy && !IsLegacyExpandedType)
            return false;

        if (parse.Node is not AssetFileDictionaryValueNode dictionary || options == CustomSpecTypeParseOptions.Legacy && parse.BaseKey == null)
        {
            if (parse.HasDiagnostics)
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Diagnostic = DatDiagnostics.UNT1005,
                    Message = string.Format(DiagnosticResources.UNT1005, parse.EvaluationContext.Self.Key),
                    Range = parse.Node?.Range ?? default
                });
            }

            value = null;
            return false;
        }

        List<CustomSpecTypeProperty> properties = new List<CustomSpecTypeProperty>(Properties.Length);
        if (options == CustomSpecTypeParseOptions.Legacy)
        {
            string baseKey = parse.BaseKey!;

            foreach (SpecProperty property in Properties)
            {
                string fullKey = property.Key;
                if (fullKey.Length == 0 || fullKey.Equals("#This.Key", StringComparison.OrdinalIgnoreCase))
                    fullKey = baseKey;
                else
                    fullKey = baseKey + "_" + fullKey;

                if (!dictionary.TryGetValue(fullKey, out AssetFileKeyValuePairNode kvp))
                {
                    properties.Add(new CustomSpecTypeProperty(null, property, fullKey));
                }
                else
                {
                    SpecPropertyTypeParseContext context = new SpecPropertyTypeParseContext(
                        new FileEvaluationContext(in parse.EvaluationContext, property),
                        parse.Diagnostics)
                    {
                        Database = parse.Database,
                        FileType = parse.FileType,
                        Node = kvp.Value,
                        Parent = kvp,
                        BaseKey = fullKey,
                        File = parse.File
                    };

                    if (!property.Type.TryParseValue(in context, out ISpecDynamicValue? propertyValue))
                        propertyValue = null;

                    properties.Add(new CustomSpecTypeProperty(propertyValue, property, fullKey));
                }
            }
        }
        else
        {
            foreach (SpecProperty property in Properties)
            {
                if (!dictionary.TryGetValue(property.Key, out AssetFileKeyValuePairNode kvp))
                {
                    properties.Add(new CustomSpecTypeProperty(null, property, property.Key));
                    continue;
                }

                SpecPropertyTypeParseContext context = new SpecPropertyTypeParseContext(
                    new FileEvaluationContext(in parse.EvaluationContext, property),
                    parse.Diagnostics)
                {
                    Database = parse.Database,
                    FileType = parse.FileType,
                    Node = kvp.Value,
                    Parent = kvp,
                    BaseKey = property.Key,
                    File = parse.File
                };

                if (!property.Type.TryParseValue(in context, out ISpecDynamicValue? propertyValue))
                    propertyValue = null;

                properties.Add(new CustomSpecTypeProperty(propertyValue, property, property.Key));
            }
        }

        value = new CustomSpecTypeInstance(this, properties);
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

public enum CustomSpecTypeParseOptions
{
    Object,
    Legacy
}

/// <summary>
/// An object represented by a <see cref="CustomSpecType"/>.
/// </summary>
public class CustomSpecTypeInstance : IEquatable<CustomSpecTypeInstance>, ISpecDynamicValue
{
    private readonly List<CustomSpecTypeProperty> _properties;
    
    public CustomSpecType Type { get; }

    public IReadOnlyList<CustomSpecTypeProperty> Properties { get; }

    public CustomSpecTypeInstance(CustomSpecType type, List<CustomSpecTypeProperty> properties)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        _properties = properties;
        Properties = _properties.AsReadOnly();
    }

    public ISpecDynamicValue? this[string key]
    {
        get
        {
            foreach (CustomSpecTypeProperty p in _properties)
            {
                if (p.Property.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return p.Value;
            }

            return null;
        }
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

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = this;
        return true;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteStartObject();

        foreach (CustomSpecTypeProperty p in _properties)
        {
            if (p.Value == null)
                continue;

            writer.WritePropertyName(p.Property.Key);
            p.Value.WriteToJsonWriter(writer, options);
        }

        writer.WriteEndObject();
    }

    ISpecPropertyType ISpecDynamicValue.ValueType => Type;
}

public struct CustomSpecTypeProperty
{
    public ISpecDynamicValue? Value { get; }
    public SpecProperty Property { get; }
    public string Key { get; }

    public CustomSpecTypeProperty(ISpecDynamicValue? value, SpecProperty property, string key)
    {
        Value = value;
        Property = property;
        Key = key;
    }
}