using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class SpecDynamicConcreteNullValue :
    ISpecDynamicValue,
    IEquatable<SpecDynamicConcreteNullValue>,
    IEquatable<ISpecDynamicValue>
{
    public static SpecDynamicConcreteNullValue Instance = new SpecDynamicConcreteNullValue();

    ISpecPropertyType? ISpecDynamicValue.ValueType => null;

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

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return condition.Operation.EvaluateNulls(true, condition.Comparand == null);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        isNull = true;
        value = default;
        return true;
    }
}

[DebuggerDisplay("{Name,nq}")]
public sealed class SpecDynamicConcreteEnumValue :
    IEquatable<SpecDynamicConcreteEnumValue>,
    IEquatable<ISpecDynamicValue>,
    ISpecDynamicValue
{
    public EnumSpecType Type { get; }
    public int Value { get; }
    public string Name => Value < 0 ? "null" : Type.Values[Value].Value;

    public SpecDynamicConcreteEnumValue(EnumSpecType type)
    {
        Value = -1;
        Type = type;
    }

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

        if (Value == other.Value)
            return true;

        if (Value < 0)
            return other.Value < 0;
        if (other.Value < 0)
            return false;

        return false;
    }

    /// <inheritdoc />
    public bool Equals(ISpecDynamicValue other)
    {
        if (Value < 0 && other is SpecDynamicConcreteNullValue)
            return true;

        return other is SpecDynamicConcreteEnumValue enumValue && Equals(enumValue);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Type.GetHashCode() ^ Value;

    /// <inheritdoc />
    public override string ToString() => Value >= 0 ? Type.Type.GetTypeName() + "." + Type.Values[Value].Value : "null";

    ISpecPropertyType ISpecDynamicValue.ValueType => Type;

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Operation == ConditionOperation.Included)
            return true;

        if (condition.Comparand is not string str
            || !Type.TryParse(str.AsSpan(), out EnumSpecTypeValue v, ignoreCase: condition.Operation.IsCaseInsensitive()))
        {
            return condition.Operation.EvaluateNulls(Value < 0, true);
        }

        return Value < 0
            ? condition.Operation.EvaluateNulls(true, false)
            : condition.Operation.Evaluate(Value, v.Index, ctx.Information.Information);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        if (typeof(TValue) != typeof(int))
        {
            throw new ArgumentException("Invalid type, expected int.");
        }

        isNull = Value < 0;
        if (isNull)
        {
            value = default!;
        }
        else
        {
            int v = Value;
            value = Unsafe.As<int, TValue>(ref v);
        }
        return true;
    }


    public static implicit operator string?(SpecDynamicConcreteEnumValue v) => v.Value < 0 ? null : v.Type.Values[v.Value].Value;
}

public abstract class SpecDynamicConcreteValue :
    ISpecDynamicValue,
    IEquatable<SpecDynamicConcreteValue>,
    IEquatable<ISpecDynamicValue>
{
    public ISpecPropertyType? ValueType { get; }

    public bool IsNull { get; }

    protected SpecDynamicConcreteValue(bool isNull, ISpecPropertyType? type)
    {
        ValueType = type;
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

    public abstract bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition);
    public abstract bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);
}

public sealed class SpecDynamicConcreteValue<T> :
    SpecDynamicConcreteValue,
    IEquatable<SpecDynamicConcreteValue<T>>,
    IEquatable<T>,
    ISpecDynamicValue where T : IEquatable<T>
{
    /// <summary>
    /// The value of this dynamic value.
    /// </summary>
    public T? Value { get; }

    public SpecDynamicConcreteValue(T? value, ISpecPropertyType<T>? type) : base(value == null, type)
    {
        Value = value;
    }

    public SpecDynamicConcreteValue(ISpecPropertyType<T>? type) : base(true, type)
    {
        Value = default!;
    }

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
    public override bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        if (condition.Operation == ConditionOperation.Included)
            return true;

        if (condition.Comparand is not T comparand)
        {
            try
            {
                comparand = (T)Convert.ChangeType(condition.Comparand, typeof(T), CultureInfo.InvariantCulture);
            }
            catch
            {
                return condition.Operation.EvaluateNulls(IsNull, true);
            }
        }

        return IsNull
            ? condition.Operation.EvaluateNulls(true, comparand == null)
            : condition.Operation.Evaluate(Value, comparand, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue value, out bool isNull)
    {
        if (typeof(TValue) != typeof(T))
        {
            throw new ArgumentException("Invalid type, expected int.");
        }

        isNull = IsNull;
        if (isNull)
        {
            value = default!;
        }
        else
        {
            T? v = Value;
            value = Unsafe.As<T, TValue>(ref v!);
        }
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode() => IsNull || Value == null ? 0 : Value.GetHashCode();

    /// <inheritdoc />
    public override string ToString() => IsNull || Value == null ? "null" : Value.ToString();

    public static implicit operator T?(SpecDynamicConcreteValue<T> v) => v.IsNull ? default! : v.Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ValuesEqual(T? t1, T? t2)
    {
        if (t1 == null)
        {
            return t2 == null;
        }

        return t2 != null && t1.Equals(t2);
    }
}