using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A value parsed from a <see cref="DatCustomType"/>.
/// </summary>
public class DatObjectValue :
    IValue<DatObjectValue>,
    IEquatable<DatObjectValue?>
{
    private int? _hash;

    /// <inheritdoc cref="IValue{TValue}.Type"/>
    public DatCustomType Type { get; }

    /// <summary>
    /// Defined properties for this object.
    /// </summary>
    public ImmutableArray<DatObjectPropertyValue> Properties { get; }

    public DatObjectValue(DatCustomType type, ImmutableArray<DatObjectPropertyValue> properties)
    {
        Type = type;
        Properties = properties;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        Type.WriteValueToJson(writer, this, Type, options);
    }

    /// <inheritdoc />
    public bool Equals(DatObjectValue? other)
    {
        if (other == null)
            return false;

        if (this == other)
            return true;

        if (!Type.Equals(other.Type))
            return false;

        ImmutableArray<DatObjectPropertyValue> thisProps = Properties;
        ImmutableArray<DatObjectPropertyValue> otherProps = other.Properties;
        
        if (thisProps.Length != otherProps.Length)
            return false;

        for (int i = 0; i < thisProps.Length; ++i)
        {
            if (!thisProps[i].Equals(otherProps[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return other is DatObjectValue v && Equals(v);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is DatObjectValue v && Equals(v);
    }

    /// <inheritdoc />
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        if (_hash.HasValue)
            return _hash.Value;

        HashCode hc = new HashCode();
        hc.Add(1596513245);
        hc.Add(Type);
        ImmutableArray<DatObjectPropertyValue> properties = Properties;
        hc.Add(properties.Length);
        foreach (DatObjectPropertyValue v in properties)
        {
            hc.Add(v.GetHashCode());
        }

        _hash = hc.ToHashCode();
        return _hash.Value;
    }

    bool IValue<DatObjectValue>.TryGetConcreteValue(out Optional<DatObjectValue> value)
    {
        value = this;
        return true;
    }
    bool IValue<DatObjectValue>.TryEvaluateValue(out Optional<DatObjectValue> value, in FileEvaluationContext ctx)
    {
        value = this;
        return true;
    }
    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(new Optional<DatObjectValue>(this));
        return true;
    }
    bool IValue.VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        visitor.Accept(new Optional<DatObjectValue>(this));
        return true;
    }
    IType<DatObjectValue> IValue<DatObjectValue>.Type => Type;
    bool IValue.IsNull => false;
}

/// <summary>
/// A key-value pair for a property and it's value.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct DatObjectPropertyValue(IValue value, DatProperty property) : IEquatable<DatObjectPropertyValue>
{
    /// <summary>
    /// The value of the property.
    /// </summary>
    public IValue Value { get; } = value;

    /// <summary>
    /// The property that was evaluated.
    /// </summary>
    public DatProperty Property { get; } = property;

    /// <inheritdoc />
    public bool Equals(DatObjectPropertyValue other)
    {
        return Property == other.Property && (Value?.Equals(other.Value) ?? other.Value == null);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DatObjectPropertyValue v && Equals(v);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(StringComparer.OrdinalIgnoreCase.GetHashCode(Property.Key), Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value.TryGetConcreteValueAs(out Optional<string> v))
        {
            return v.HasValue ? $"\"{Property.Key}\" \"{v.Value}\"" : $"\"{Property.Key}\"";
        }

        return $"\"{Property.Key}\" {{{v}}}";
    }
}