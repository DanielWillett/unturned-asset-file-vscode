using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[DebuggerDisplay("{DisplayName,nq}")]
public sealed class CustomSpecType : ISpecType, ISpecPropertyType, IEquatable<CustomSpecType>
{
    public QualifiedType Type { get; internal set; }

    public Type ValueType => throw new NotImplementedException();

    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => null;

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        value = null!;
        return false;
    }

    public string? Docs { get; set; }

    public QualifiedType Parent { get; set; }

    public string DisplayName { get; set; } = string.Empty;
    string ISpecPropertyType.Type => Type.Type;

#nullable disable
    public List<SpecProperty> Properties { get; set; }
    public List<SpecProperty> LocalizationProperties { get; set; }
#nullable restore

    /// <inheritdoc />
    public bool Equals(CustomSpecType other) => other != null && Type.Equals(other.Type);
    /// <inheritdoc />
    public bool Equals(ISpecType other) => other is CustomSpecType t && Equals(t);
    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is CustomSpecType t && Equals(t);
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