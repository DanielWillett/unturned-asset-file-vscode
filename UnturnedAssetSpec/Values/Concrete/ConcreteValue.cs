using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
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
                return !optVal.HasValue ? IsNull : optVal.Value.Equals(Value);

            case IValue value:
                EqualityVisitor visitor;
                visitor.OtherValue = value;
                visitor.IsNull = IsNull;
                visitor.Value = Value;
                visitor.IsEqual = false;
                value.Type.Visit(ref visitor);
                return visitor.IsEqual;

            default:
                return false;
        }
    }

    private struct EqualityVisitor : ITypeVisitor, IGenericVisitor
    {
        public IValue OtherValue;
        public bool IsEqual;
        public bool IsNull;
        public TValue? Value;

        public void Accept<TOtherValue>(IType<TOtherValue> type)
            where TOtherValue : IEquatable<TOtherValue>
        {
            if (OtherValue is not IValue<TOtherValue> strongValue)
                return;

            if (strongValue.IsNull)
            {
                IsEqual = IsNull;
                return;
            }

            if (!strongValue.TryGetConcreteValue(out Optional<TOtherValue> optVal))
                return;

            if (!optVal.HasValue)
            {
                IsEqual = IsNull;
                return;
            }

            if (IsNull)
            {
                IsEqual = false;
                return;
            }

            if (MathMatrix.Equals(Value, optVal.Value, ref this))
            {
                return;
            }

            if (typeof(TOtherValue) == typeof(Guid))
            {
                if (typeof(TValue) == typeof(GuidOrId))
                {
                    IsEqual = Unsafe.As<TValue, GuidOrId>(ref Value!)
                        .Equals(SpecDynamicExpressionTreeValueHelpers.As<TOtherValue, Guid>(optVal.Value));
                }
            }
            else if (typeof(TOtherValue) == typeof(GuidOrId))
            {
                if (typeof(TValue) == typeof(Guid))
                {
                    IsEqual = SpecDynamicExpressionTreeValueHelpers.As<TOtherValue, GuidOrId>(optVal.Value)
                        .Equals(Unsafe.As<TValue, Guid>(ref Value!));
                }
            }
        }

        public void Accept<T>(T? value)
            where T : IEquatable<T>
        {
            if (typeof(T) != typeof(bool))
                return;

            IsEqual = Unsafe.As<T, bool>(ref value!);
        }
    }

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
    public void WriteToJson(Utf8JsonWriter writer)
    {
        if (IsNull)
        {
            writer.WriteNullValue();
        }
        else
        {
            Type.Parser.WriteValueToJson(writer, _value, Type);
        }
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return IsNull ? 0 : _value.GetHashCode();
    }

    IType IValue.Type => Type;
}