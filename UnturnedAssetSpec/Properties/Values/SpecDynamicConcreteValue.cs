using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public sealed class SpecDynamicConcreteNullValue :
    ISpecConcreteValue,
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
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
            return !condition.IsInverted;
        if (condition.Operation == ConditionOperation.Excluded)
            return condition.IsInverted;

        return condition.EvaluateNulls(true, condition.Comparand == null);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        isNull = true;
        value = default;
        return true;
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = null;
        return true;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        writer.WriteNullValue();
    }

    public bool TryEvaluateValue<TValue>(out TValue? value, out bool isNull)
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
    ICorrespondingTypeSpecDynamicValue,
    ISpecConcreteValue
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
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
            return !condition.IsInverted;

        if (condition.Operation == ConditionOperation.Excluded)
            return condition.IsInverted;

        if (condition.Comparand is not string str
            || !Type.TryParse(str.AsSpan(), out EnumSpecTypeValue v, ignoreCase: condition.Operation.IsCaseInsensitive()))
        {
            return condition.EvaluateNulls(Value < 0, true);
        }

        return Value < 0
            ? condition.EvaluateNulls(true, false)
            : condition.Evaluate(Value, v.Index, ctx.Information.Information);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        return TryEvaluateValue(out value, out isNull);
    }

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = Value < 0 ? null : Type.Values[Value].Value;
        return true;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        if (Value < 0)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(Type.Values[Value].Value);
    }

    /// <inheritdoc />
    public bool TryEvaluateValue<TValue>(out TValue? value, out bool isNull)
    {
        isNull = Value < 0;

        if (typeof(TValue) == typeof(int))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<int, TValue>(Value);
        else if (typeof(TValue) == typeof(EnumSpecTypeValue))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<EnumSpecTypeValue, TValue>(Type.Values[Value]);
        else if (typeof(TValue) == typeof(string))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<string, TValue>(Type.Values[Value].Value);
        else if (typeof(TValue) == typeof(uint))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<uint, TValue>((uint)Value);
        else if (typeof(TValue) == typeof(long))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<long, TValue>(Value);
        else if (typeof(TValue) == typeof(ulong))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<ulong, TValue>((ulong)Value);
        else if (typeof(TValue) == typeof(short))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<short, TValue>(checked( (short)Value ));
        else if (typeof(TValue) == typeof(ushort))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<ushort, TValue>(checked( (ushort)Value ));
        else if (typeof(TValue) == typeof(sbyte))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<sbyte, TValue>(checked( (sbyte)Value ));
        else if (typeof(TValue) == typeof(byte))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<byte, TValue>(checked( (byte)Value ));
        else if (typeof(TValue) == typeof(float))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<float, TValue>(Value);
        else if (typeof(TValue) == typeof(double))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<double, TValue>(Value);
        else if (typeof(TValue) == typeof(decimal))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<decimal, TValue>(Value);
        else if (typeof(TValue) == typeof(char))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<char, TValue>(checked( (char)Value ));
        else if (typeof(TValue) == typeof(bool))
            value = isNull ? default : SpecDynamicEquationTreeValueHelpers.As<bool, TValue>(Value != 0);
        else
        {
            value = default;
            return false;
        }

        return true;
    }

    public QualifiedType GetCorrespondingType(IAssetSpecDatabase database) => Value < 0 ? QualifiedType.None : Type.Values[Value].CorrespondingType;


    public static implicit operator string?(SpecDynamicConcreteEnumValue v) => v.Value < 0 ? null : v.Type.Values[v.Value].Value;
}

public abstract class SpecDynamicConcreteValue :
    ISpecConcreteValue,
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
    public abstract bool Equals(SpecDynamicConcreteValue? other);

    /// <inheritdoc />
    public bool Equals(ISpecDynamicValue? other)
    {
        if (other is SpecDynamicConcreteValue v && Equals(v))
            return true;

        if (IsNull && other is SpecDynamicConcreteNullValue)
            return true;

        return false;
    }

    public abstract bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition);
    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        return TryEvaluateValue(out value, out isNull);
    }
    public abstract bool TryEvaluateValue<TValue>(out TValue? value, out bool isNull);
    public abstract bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value);

    public abstract void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options);
}

public class SpecDynamicConcreteConvertibleValue<T> : SpecDynamicConcreteValue<T> where T : IEquatable<T>, IConvertible
{
    /// <inheritdoc />
    public SpecDynamicConcreteConvertibleValue(T? value, ISpecPropertyType<T>? type) : base(value, type) { }

    /// <inheritdoc />
    public SpecDynamicConcreteConvertibleValue(ISpecPropertyType<T>? type) : base(type) { }

    /// <inheritdoc />
    public override bool TryEvaluateValue<TValue>(out TValue value, out bool isNull)
    {
        if (base.TryEvaluateValue(out value, out isNull))
        {
            return true;
        }

        if (SpecDynamicEquationTreeValueHelpers.TryConvert(ValueIntl, IsNull, out value!, out isNull))
        {
            return true;
        }

        return false;
    }
}

public class SpecDynamicConcreteValue<T> :
    SpecDynamicConcreteValue,
    IEquatable<SpecDynamicConcreteValue<T>>,
    IEquatable<T>,
    ISpecDynamicValue where T : IEquatable<T>
{
    protected T? ValueIntl;

    /// <summary>
    /// The value of this dynamic value.
    /// </summary>
    public T? Value => ValueIntl;

    public SpecDynamicConcreteValue(T? value, ISpecPropertyType<T>? type) : base(value == null, type)
    {
        ValueIntl = value;
    }

    public SpecDynamicConcreteValue(ISpecPropertyType<T>? type) : base(true, type)
    {
        ValueIntl = default!;
    }

    /// <inheritdoc />
    public bool Equals(T? other) => IsNull ? other == null : ValuesEqual(Value, other);

    /// <inheritdoc />
    public bool Equals(SpecDynamicConcreteValue<T>? other) => other != null && (IsNull ? other.IsNull : !other.IsNull && ValuesEqual(other.Value, Value));

    /// <inheritdoc />
    public override bool Equals(SpecDynamicConcreteValue? other) => other is SpecDynamicConcreteValue<T> b && Equals(b);
    
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
        if (condition.Operation is ConditionOperation.Included or ConditionOperation.ValueIncluded)
            return !condition.IsInverted;
        if (condition.Operation == ConditionOperation.Excluded)
            return condition.IsInverted;

        if (condition.Comparand is not T comparand)
        {
            try
            {
                comparand = (T)Convert.ChangeType(condition.Comparand, typeof(T), CultureInfo.InvariantCulture);
            }
            catch
            {
                return condition.EvaluateNulls(IsNull, true);
            }
        }

        return IsNull
            ? condition.EvaluateNulls(true, comparand == null)
            : condition.Evaluate(Value, comparand, ctx.Information.Information);
    }

    public override bool TryEvaluateValue<TValue>(out TValue value, out bool isNull)
    {
        isNull = IsNull;
        if (typeof(TValue) == typeof(T))
        {
            value = default!;
            value = isNull ? default! : Unsafe.As<T, TValue>(ref ValueIntl!);
            return true;
        }

        if (ValueType is IVectorSpecPropertyType v)
        {
            ConvertVectorVisitor<TValue> visitor = default;
            visitor.Value = this;
            v.Visit(ref visitor);
            value = visitor.OutValue!;
            isNull = visitor.ValueIsNull;
            return visitor.Converted;
        }

        if (typeof(TValue) == typeof(string))
        {
            if (isNull)
            {
                value = default!;
                return true;
            }

            string? str = ValueIntl!.ToString();

            value = Unsafe.As<string, TValue>(ref str);
            return str != null;
        }

        value = default!;
        return false;
    }

    private struct ConvertVectorVisitor<TValue> : IVectorSpecPropertyTypeVisitor
    {
        public SpecDynamicConcreteValue<T> Value;
        public TValue? OutValue;
        public bool ValueIsNull;
        public bool Converted;

        /// <inheritdoc />
        public void Visit<TVector>(IVectorSpecPropertyType<TVector> type) where TVector : IEquatable<TVector>
        {
            if (Converted)
                return;

            if (typeof(T) == typeof(TVector))
            {
                Converted = SpecDynamicEquationTreeValueHelpers.TryConvertVector(Unsafe.As<T, TVector>(ref Value.ValueIntl!), false, out OutValue, out ValueIsNull);
            }
        }
    }

    public override bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        value = IsNull ? null : BoxedPrimitives.Box(ref ValueIntl);
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
        if (typeof(T).IsValueType)
        {
            return t1!.Equals(t2!);
        }

        if (t1 == null)
        {
            return t2 == null;
        }

        return t2 != null && t1.Equals(t2);
    }

    public override void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        if (IsNull)
        {
            writer.WriteNullValue();
            return;
        }

        if (typeof(T) == typeof(bool))
        {
            bool v = Unsafe.As<T, bool>(ref ValueIntl!);
            writer.WriteBooleanValue(v);
        }
        else if (typeof(T) == typeof(byte))
        {
            byte v = Unsafe.As<T, byte>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(sbyte))
        {
            sbyte v = Unsafe.As<T, sbyte>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(ushort))
        {
            ushort v = Unsafe.As<T, ushort>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(short))
        {
            short v = Unsafe.As<T, short>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(uint))
        {
            uint v = Unsafe.As<T, uint>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(int))
        {
            int v = Unsafe.As<T, int>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(ulong))
        {
            ulong v = Unsafe.As<T, ulong>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(long))
        {
            long v = Unsafe.As<T, long>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(float))
        {
            float v = Unsafe.As<T, float>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(double))
        {
            double v = Unsafe.As<T, double>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(decimal))
        {
            decimal v = Unsafe.As<T, decimal>(ref ValueIntl!);
            writer.WriteNumberValue(v);
        }
        else if (typeof(T) == typeof(Guid))
        {
            Guid v = Unsafe.As<T, Guid>(ref ValueIntl!);
            writer.WriteStringValue(v);
        }
        else if (typeof(T) == typeof(GuidOrId))
        {
            GuidOrId v = Unsafe.As<T, GuidOrId>(ref ValueIntl!);
            if (v.IsId)
                writer.WriteNumberValue(v.Id);
            else
                writer.WriteStringValue(v.Guid);
        }
        else if (typeof(T) == typeof(DateTime))
        {
            DateTime v = Unsafe.As<T, DateTime>(ref ValueIntl!);
            writer.WriteStringValue(v);
        }
        else if (typeof(T) == typeof(DateTimeOffset))
        {
            DateTimeOffset v = Unsafe.As<T, DateTimeOffset>(ref ValueIntl!);
            writer.WriteStringValue(v);
        }
        else if (typeof(T) == typeof(char))
        {
            char v = Unsafe.As<T, char>(ref ValueIntl!);
            unsafe
            {
                ReadOnlySpan<char> span = new ReadOnlySpan<char>(&v, 1);
                writer.WriteStringValue(span);
            }
        }
        else
        {
            if (ValueType is IStringParseableSpecPropertyType stringParseable)
            {
                string? s = stringParseable.ToString(this);
                if (s == null)
                    writer.WriteNullValue();
                else
                    writer.WriteStringValue(s);
            }
            else
            {
                JsonSerializer.Serialize(writer, ValueIntl, options);
            }
        }
    }
}