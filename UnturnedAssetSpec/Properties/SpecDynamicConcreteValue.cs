using System;
using System.Runtime.CompilerServices;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class SpecDynamicConcreteNullValue : ISpecDynamicValue, IEquatable<SpecDynamicConcreteNullValue>, IEquatable<ISpecDynamicValue>
{
    public static SpecDynamicConcreteNullValue Instance = new SpecDynamicConcreteNullValue();

    private SpecDynamicConcreteNullValue() { }

    /// <inheritdoc />
    public bool Equals(SpecDynamicConcreteNullValue other) => true;

    /// <inheritdoc />
    public bool Equals(ISpecDynamicValue other) => other is SpecDynamicConcreteNullValue;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is null or SpecDynamicConcreteNullValue;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public override string ToString() => "null";
}

public sealed class SpecDynamicConcreteEnumValue : ISpecDynamicValue, IEquatable<SpecDynamicConcreteEnumValue>, IEquatable<ISpecDynamicValue>
{
    public EnumSpecType Type { get; }
    public int Value { get; }

    public SpecDynamicConcreteEnumValue(EnumSpecType type, int value)
    {
        if (value < 0 || value >= type.Values.Length)
            throw new ArgumentOutOfRangeException(nameof(value));

        Type = type;
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(SpecDynamicConcreteEnumValue other)
    {
        if (!Type.Equals(other.Type))
            return false;

        return Value == other.Value;
    }

    /// <inheritdoc />
    public bool Equals(ISpecDynamicValue other) => other is SpecDynamicConcreteEnumValue enumValue && Equals(enumValue);

    /// <inheritdoc />
    public override int GetHashCode() => Type.GetHashCode() ^ Value;

    /// <inheritdoc />
    public override string ToString() => Type.Type.GetTypeName() + "." + Type.Values[Value].Value;
}

public abstract class SpecDynamicConcreteValue : ISpecDynamicValue, IEquatable<SpecDynamicConcreteValue>, IEquatable<ISpecDynamicValue>
{
    public abstract object BoxedValue { get; }

    public bool IsNull { get; }

    protected SpecDynamicConcreteValue(bool isNull)
    {
        IsNull = isNull;
    }

    /// <inheritdoc />
    public abstract bool Equals(SpecDynamicConcreteValue other);

    /// <inheritdoc />
    public bool Equals(ISpecDynamicValue other)
    {
        if (other is SpecDynamicConcreteValue v && Equals(v))
            return true;

        if (IsNull && other is SpecDynamicConcreteNullValue)
            return true;

        return false;
    }
}

public sealed class SpecDynamicConcreteValue<T> : SpecDynamicConcreteValue, IEquatable<SpecDynamicConcreteValue<T>>, IEquatable<T> where T : IEquatable<T>
{
    /// <summary>
    /// The value of this dynamic value.
    /// </summary>
    public T Value { get; }

    public SpecDynamicConcreteValue(T value) : base(false)
    {
        Value = value;
    }

    public SpecDynamicConcreteValue() : base(true)
    {
        Value = default!;
    }

    /// <inheritdoc />
    public override object BoxedValue => Value;

    /// <inheritdoc />
    public bool Equals(T other) => IsNull ? other == null : ValuesEqual(Value, other);

    /// <inheritdoc />
    public bool Equals(SpecDynamicConcreteValue<T> other) => other != null && (IsNull ? other.IsNull : !other.IsNull && ValuesEqual(other.Value, Value));

    /// <inheritdoc />
    public override bool Equals(SpecDynamicConcreteValue other) => other is SpecDynamicConcreteValue<T> b && Equals(b);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj switch
    {
        SpecDynamicConcreteValue<T> b => Equals(b),
        T otherValue => !IsNull && ValuesEqual(otherValue, Value),
        null => IsNull,
        _ => false
    };

    /// <inheritdoc />
    public override int GetHashCode() => IsNull || Value == null ? 0 : Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => IsNull || Value == null ? "null" : Value.ToString();

    public static implicit operator T(SpecDynamicConcreteValue<T> v) => v.IsNull ? default! : v.Value;
    public static explicit operator SpecDynamicConcreteValue<T>(T v)
    {
        if (typeof(T) == typeof(bool))
        {
            return (SpecDynamicConcreteValue<T>)(object)(__refvalue(__makeref(v), bool) ? SpecDynamicValue.True : SpecDynamicValue.False);
        }

        return new SpecDynamicConcreteValue<T>(v);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ValuesEqual(T t1, T t2)
    {
        if (t1 == null)
        {
            return t2 == null;
        }

        return t2 != null && t1.Equals(t2);
    }
}