using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A concrete/constant value that isn't dynamic in any way.
/// </summary>
/// <remarks>Create using <see cref="Values.Create"/>.</remarks>
/// <typeparam name="TValue">The type of value.</typeparam>
public sealed class ConcreteValue<TValue>
    : IValue<TValue>,
      IEquatable<ConcreteValue<TValue>?>,
      IValueExpressionNode
    where TValue : IEquatable<TValue>
{
    private readonly TValue? _value;

    /// <summary>
    /// The value stored in this object.
    /// </summary>
    /// <remarks>Check <see cref="IsNull"/> before using this property.</remarks>
    public TValue? Value => _value;

    /// <summary>
    /// Whether or not the value stored in this object is a null value.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(false, nameof(_value))]
    public bool IsNull { get; }

    /// <inheritdoc />
    public IType<TValue> Type { get; }

    internal ConcreteValue(IType<TValue> type)
    {
        IsNull = true;
        Type = type;
    }

    internal ConcreteValue(TValue value, IType<TValue> type)
    {
        _value = value;
        IsNull = _value == null;
        Type = type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TValue> value)
    {
        value = IsNull ? Optional<TValue>.Null : new Optional<TValue>(_value);
        return true;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TValue> value, in FileEvaluationContext ctx)
    {
        return TryGetConcreteValue(out value);
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        visitor.Accept(IsNull ? Optional<TValue>.Null : new Optional<TValue>(_value));
        return true;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        visitor.Accept(IsNull ? Optional<TValue>.Null : new Optional<TValue>(_value));
        return true;
    }

    /// <inheritdoc cref="IEquatable{T}"/>
    public bool Equals(TValue? value)
    {
        return IsNull ? value == null : value != null && _value.Equals(value);
    }

    /// <inheritdoc />
    public bool Equals(ConcreteValue<TValue>? value)
    {
        return IsNull ? value == null || value.IsNull : value is { IsNull: false, _value: not null } && _value.Equals(value._value);
    }

    bool IEquatable<IExpressionNode>.Equals(IExpressionNode? other)
    {
        switch (other)
        {
            case null:
                return IsNull;

            case IValue<TValue> strongValue:
                if (strongValue.IsNull)
                    return IsNull;
                if (!strongValue.TryGetConcreteValue(out Optional<TValue> optVal))
                    return false;
                return optVal.HasValue ? !IsNull && optVal.Value.Equals(Value) : IsNull;

            case IValue value:
                
                EqualityVisitor<TValue> visitor;
                visitor.IsNull = IsNull;
                visitor.Value = Value;
                visitor.IsEqual = false;
                visitor.CaseInsensitive = false;
                visitor.Success = false;
                value.VisitConcreteValue(ref visitor);
                return visitor.IsEqual;

            default:
                return false;
        }
    }

    /// <inheritdoc />
    public bool Equals(IValue? other) => Equals(other as ConcreteValue<TValue>);

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            ConcreteValue<TValue> v => Equals(v),
            TValue v => Equals(v),
            null => IsNull,
            _ => false
        };
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (IsNull)
        {
            writer.WriteNullValue();
        }
        else
        {
            Type.Parser.WriteValueToJson(writer, _value, Type, options);
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return IsNull ? 1302072072 : HashCode.Combine(_value, 1302072072);
    }
}