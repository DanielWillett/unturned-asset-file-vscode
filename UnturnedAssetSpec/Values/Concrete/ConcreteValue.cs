using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A concrete/constant value that isn't dynamic in any way.
/// </summary>
/// <typeparam name="TValue">The type of value.</typeparam>
public sealed class ConcreteValue<TValue> : IValue<TValue>, IEquatable<TValue?>, IEquatable<ConcreteValue<TValue>?> where TValue : IEquatable<TValue>
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

    public ConcreteValue(IType<TValue> type)
    {
        IsNull = true;
        Type = type;
    }

    public ConcreteValue(TValue value, IType<TValue> type)
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

    public bool Equals(TValue? value)
    {
        return IsNull ? value == null : value != null && _value.Equals(value);
    }

    public bool Equals(ConcreteValue<TValue>? value)
    {
        return IsNull ? value == null || value.IsNull : value is { IsNull: false, _value: not null } && _value.Equals(value._value);
    }

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

    public override int GetHashCode()
    {
        return IsNull ? 0 : _value.GetHashCode();
    }

    IType IValue.Type => Type;
}