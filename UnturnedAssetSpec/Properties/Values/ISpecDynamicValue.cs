using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// An abstraction for values that can be resolved to a concrete value given the context of the current workspace.
/// </summary>
public interface ISpecDynamicValue
{
    /// <summary>
    /// The type this value was parsed from.
    /// </summary>
    ISpecPropertyType? ValueType { get; }

    /// <summary>
    /// Evaluates this value and checks a condition on the current value, returning whether or not the condition passed.
    /// </summary>
    bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition);

    /// <summary>
    /// Evaluates this value's concrete value and attempts to convert it to <typeparamref name="TValue"/> if needed.
    /// </summary>
    /// <typeparam name="TValue">The type of value to output.</typeparam>
    /// <param name="isNull">Whether or not <paramref name="value"/> is <see langword="null"/> instead of <see langword="default"/> (this matters for value types).</param>
    /// <returns>Whether or not the evaluation succeeded.</returns>
    bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);

    /// <summary>
    /// Evaluates this value's concrete value and boxes it for non-generic usage.
    /// </summary>
    /// <returns>Whether or not the evaluation succeeded.</returns>
    bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value);

    /// <summary>
    /// Writes this value to a <see cref="Utf8JsonWriter"/> in a round-trip format.
    /// </summary>
    void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options);
}

/// <summary>
/// A type of <see cref="ISpecDynamicValue"/> that doesn't require workspace context to evaluate.
/// </summary>
public interface ISpecConcreteValue : ISpecDynamicValue
{
    /// <summary>
    /// Evaluates this value's concrete value and attempts to convert it to <typeparamref name="TValue"/> if needed.
    /// </summary>
    /// <typeparam name="TValue">The type of value to output.</typeparam>
    /// <param name="isNull">Whether or not <paramref name="value"/> is <see langword="null"/> instead of <see langword="default"/> (this matters for value types).</param>
    /// <returns>Whether or not the evaluation succeeded.</returns>
    bool TryEvaluateValue<TValue>(out TValue? value, out bool isNull);
}

/// <summary>
/// A type of <see cref="ISpecDynamicValue"/> in which values can have a corresponding type, such as the enum for Useable types.
/// </summary>
public interface ICorrespondingTypeSpecDynamicValue : ISpecDynamicValue
{
    /// <summary>
    /// Retreives the corresponding type for this value, or <see cref="QualifiedType.None"/> if not defined.
    /// </summary>
    QualifiedType GetCorrespondingType(IAssetSpecDatabase database);
}

/// <summary>
/// A type of value that needs a transformation applied to it after all types have been loaded.
/// </summary>
public interface ISecondPassSpecDynamicValue : ISpecDynamicValue
{
    /// <summary>
    /// Converts this value to a new value which may have needed some metadata from other types after they'd finished being read.
    /// </summary>
    /// <param name="property">The property this value is on.</param>
    /// <param name="database">The database service.</param>
    /// <param name="assetFile">The file this value's property belongs to.</param>
    /// <returns>The new value, or the same reference to the old value if a new value didn't need to be created.</returns>
    ISpecDynamicValue Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile);
}

/// <summary>
/// Context for evaluating a dynamic value.
/// </summary>
[Flags]
public enum SpecDynamicValueContext
{
    /// <summary>
    /// Assumes that the value is a raw value (%).
    /// </summary>
    Optional = 0,

    /// <summary>
    /// Assumes that the value is a property reference (@).
    /// </summary>
    AssumeProperty = 1,

    /// <summary>
    /// Assumes that the value is a data-ref (#).
    /// </summary>
    AssumeDataRef = 2,

    /// <summary>
    /// Allows for a switch-case object (<see cref="SpecDynamicSwitchCaseValue"/>) to be supplied.
    /// </summary>
    AllowSwitchCase = 4,

    /// <summary>
    /// Allows for a condition object (<see cref="SpecCondition"/>) to be supplied.
    /// </summary>
    AllowCondition = 8,

    /// <summary>
    /// Allows for a switch expression (<see cref="SpecDynamicSwitchValue"/>) to be supplied.
    /// </summary>
    AllowSwitch = 16,

    /// <summary>
    /// Allows all conditional objects (switch-case, switches, and conditions).
    /// </summary>
    AllowConditionals = AllowSwitchCase | AllowSwitch | AllowCondition,

    /// <summary>
    /// The default options for parsing values.
    /// </summary>
    Default = Optional | AllowConditionals
}

/// <summary>
/// Contains helpers for known dynamic value types and utility methods.
/// </summary>
public static class SpecDynamicValue
{
    /// <summary>
    /// The <see langword="null"/> concrete value.
    /// </summary>
    public static SpecDynamicConcreteNullValue Null => SpecDynamicConcreteNullValue.Instance;

    /// <summary>
    /// The <see langword="true"/> concrete value of type <see cref="KnownTypes.Boolean"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> True { get; } = new SpecDynamicConcreteConvertibleValue<bool>(true, KnownTypes.Boolean);

    /// <summary>
    /// The <see langword="false"/> concrete value of type <see cref="KnownTypes.Boolean"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> False { get; } = new SpecDynamicConcreteConvertibleValue<bool>(false, KnownTypes.Boolean);

    /// <summary>
    /// The <see langword="true"/> concrete value of type <see cref="KnownTypes.Flag"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> Included { get; } = new SpecDynamicConcreteConvertibleValue<bool>(true, KnownTypes.Flag);

    /// <summary>
    /// The <see langword="false"/> concrete value of type <see cref="KnownTypes.Flag"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> Excluded { get; } = new SpecDynamicConcreteConvertibleValue<bool>(false, KnownTypes.Flag);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Flag"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> Flag(bool v) => v ? Included : Excluded;

    /// <summary>
    /// A value of type <see cref="KnownTypes.Boolean"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> Boolean(bool v) => v ? True : False;

    /// <summary>
    /// A boolean type which can be of type <see cref="KnownTypes.Flag"/>, <see cref="KnownTypes.Boolean"/>, or some other boolean type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<bool> Boolean(bool v, ISpecPropertyType? type)
    {
        if (ReferenceEquals(type, KnownTypes.Flag))
        {
            return Flag(v);
        }

        if (!ReferenceEquals(type, KnownTypes.Boolean) && type is ISpecPropertyType<bool> b)
        {
            return new SpecDynamicConcreteConvertibleValue<bool>(v, b);
        }

        return Boolean(v);
    }

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt8"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<byte> UInt8(byte v) => new SpecDynamicConcreteConvertibleValue<byte>(v, KnownTypes.UInt8);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt8"/> or some other <see cref="byte"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<byte> UInt8(byte v, ISpecPropertyType? type) => type is ISpecPropertyType<byte> b ? new SpecDynamicConcreteConvertibleValue<byte>(v, b) : UInt8(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt16"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<ushort> UInt16(ushort v) => new SpecDynamicConcreteConvertibleValue<ushort>(v, KnownTypes.UInt16);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt16"/> or some other <see cref="ushort"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<ushort> UInt16(ushort v, ISpecPropertyType? type) => type is ISpecPropertyType<ushort> b ? new SpecDynamicConcreteConvertibleValue<ushort>(v, b) : UInt16(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt32"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<uint> UInt32(uint v) => new SpecDynamicConcreteConvertibleValue<uint>(v, KnownTypes.UInt32);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt32"/> or some other <see cref="uint"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<uint> UInt32(uint v, ISpecPropertyType? type) => type is ISpecPropertyType<uint> b ? new SpecDynamicConcreteConvertibleValue<uint>(v, b) : UInt32(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt64"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<ulong> UInt64(ulong v) => new SpecDynamicConcreteConvertibleValue<ulong>(v, KnownTypes.UInt64);

    /// <summary>
    /// A value of type <see cref="KnownTypes.UInt64"/> or some other <see cref="ulong"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<ulong> UInt64(ulong v, ISpecPropertyType? type) => type is ISpecPropertyType<ulong> b ? new SpecDynamicConcreteConvertibleValue<ulong>(v, b) : UInt64(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int8"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<sbyte> Int8(sbyte v) => new SpecDynamicConcreteConvertibleValue<sbyte>(v, KnownTypes.Int8);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int8"/> or some other <see cref="sbyte"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<sbyte> Int8(sbyte v, ISpecPropertyType? type) => type is ISpecPropertyType<sbyte> b ? new SpecDynamicConcreteConvertibleValue<sbyte>(v, b) : Int8(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int16"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<short> Int16(short v) => new SpecDynamicConcreteConvertibleValue<short>(v, KnownTypes.Int16);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int16"/> or some other <see cref="short"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<short> Int16(short v, ISpecPropertyType? type) => type is ISpecPropertyType<short> b ? new SpecDynamicConcreteConvertibleValue<short>(v, b) : Int16(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int32"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<int> Int32(int v) => new SpecDynamicConcreteConvertibleValue<int>(v, KnownTypes.Int32);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int32"/> or some other <see cref="int"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<int> Int32(int v, ISpecPropertyType? type) => type is ISpecPropertyType<int> b ? new SpecDynamicConcreteConvertibleValue<int>(v, b) : Int32(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int64"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<long> Int64(long v) => new SpecDynamicConcreteConvertibleValue<long>(v, KnownTypes.Int64);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Int64"/> or some other <see cref="long"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<long> Int64(long v, ISpecPropertyType? type) => type is ISpecPropertyType<long> b ? new SpecDynamicConcreteConvertibleValue<long>(v, b) : Int64(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Float32"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<float> Float32(float v) => new SpecDynamicConcreteConvertibleValue<float>(v, KnownTypes.Float32);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Float32"/> or some other <see cref="float"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<float> Float32(float v, ISpecPropertyType? type) => type is ISpecPropertyType<float> b ? new SpecDynamicConcreteConvertibleValue<float>(v, b) : Float32(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Float64"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<double> Float64(double v) => new SpecDynamicConcreteConvertibleValue<double>(v, KnownTypes.Float64);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Float64"/> or some other <see cref="double"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<double> Float64(double v, ISpecPropertyType? type) => type is ISpecPropertyType<double> b ? new SpecDynamicConcreteConvertibleValue<double>(v, b) : Float64(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Float128"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<decimal> Float128(decimal v) => new SpecDynamicConcreteConvertibleValue<decimal>(v, KnownTypes.Float128);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Float128"/> or some other <see cref="decimal"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<decimal> Float128(decimal v, ISpecPropertyType? type) => type is ISpecPropertyType<decimal> b ? new SpecDynamicConcreteConvertibleValue<decimal>(v, b) : Float128(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.String"/>.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<string> String(string v) => new SpecDynamicConcreteConvertibleValue<string>(v, KnownTypes.String);

    /// <summary>
    /// A value of type <see cref="KnownTypes.String"/> or some other <see cref="string"/> type.
    /// </summary>
    public static SpecDynamicConcreteConvertibleValue<string> String(string v, ISpecPropertyType? type) => type is ISpecPropertyType<string> b ? new SpecDynamicConcreteConvertibleValue<string>(v, b) : String(v);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Guid"/>.
    /// </summary>
    public static SpecDynamicConcreteValue<Guid> Guid(Guid v) => new SpecDynamicConcreteValue<Guid>(v, KnownTypes.Guid);

    /// <summary>
    /// A value of type <see cref="KnownTypes.Guid"/> or some other <see cref="System.Guid"/> type.
    /// </summary>
    public static SpecDynamicConcreteValue<Guid> Guid(Guid v, ISpecPropertyType? type) => type is ISpecPropertyType<Guid> b ? new SpecDynamicConcreteValue<Guid>(v, b) : Guid(v);

    /// <summary>
    /// An enum value based on the value index.
    /// </summary>
    /// <param name="type">The enum type.</param>
    /// <param name="value">The index of the value within the enum's value array.</param>
    public static SpecDynamicConcreteEnumValue Enum(EnumSpecType type, int value) => new SpecDynamicConcreteEnumValue(type, value);

    /// <summary>
    /// An enum value based on the value index.
    /// </summary>
    /// <param name="type">The enum type.</param>
    /// <param name="flags">List of indices included in the flag.</param>
    public static SpecDynamicConcreteFlagsEnumValue EnumFlags(EnumSpecType type, params OneOrMore<int> flags) => new SpecDynamicConcreteFlagsEnumValue(type, flags);

    /// <summary>
    /// An enum value based on the composite bitwise value.
    /// </summary>
    /// <param name="type">The enum type.</param>
    /// <param name="composite">Resulting bit-wise composite value.</param>
    public static SpecDynamicConcreteFlagsEnumValue EnumFlags(EnumSpecType type, long composite) => new SpecDynamicConcreteFlagsEnumValue(type, composite);

    /// <summary>
    /// An enum value based on the composite bitwise value.
    /// </summary>
    /// <param name="type">The enum type.</param>
    /// <param name="composite">Resulting bit-wise composite value.</param>
    public static SpecDynamicConcreteFlagsEnumValue EnumFlags(EnumSpecType type, ulong composite) => new SpecDynamicConcreteFlagsEnumValue(type, unchecked ( (long)composite ));

    /// <summary>
    /// An enum value based on the value record.
    /// </summary>
    /// <param name="value">The enum value from the enum type's value array.</param>
    public static SpecDynamicConcreteEnumValue Enum(in EnumSpecTypeValue value) => new SpecDynamicConcreteEnumValue(value.Type, value.Index);

    /// <summary>
    /// A null enum value.
    /// </summary>
    /// <param name="type">The enum type.</param>
    public static SpecDynamicConcreteEnumValue EnumNull(EnumSpecType type)
    {
        return type.Null;
    }

    /// <summary>
    /// Attempts to convert this value to a concrete value without worspace context.
    /// <para>
    /// This only works on implementations of <see cref="ISpecConcreteValue"/>, otherwise an exception will be thrown.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidCastException"/>
    public static T? AsConcrete<T>(this ISpecDynamicValue value)
    {
        if (value is not ISpecConcreteValue conc)
            throw new InvalidCastException($"Failed to cast {value.ValueType} to {typeof(T)}, not a concrete type.");

        if (!conc.TryEvaluateValue(out T? val, out bool isNull))
            throw new InvalidCastException($"Failed to cast {value.ValueType} to {typeof(T)}.");

        if (!isNull)
            return val;

        if (default(T) == null)
            return default;

        throw new InvalidCastException($"Failed to cast <null> to {typeof(T)}.");
    }

    /// <summary>
    /// Attempts to convert this value to a concrete value without worspace context,
    /// returning a <see cref="Nullable{T}"/> struct allowing for <see langword="null"/> values to be represented for value types.
    /// <para>
    /// This only works on implementations of <see cref="ISpecConcreteValue"/>, otherwise an exception will be thrown.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidCastException"/>
    public static T? AsConcreteNullable<T>(this ISpecDynamicValue value) where T : struct
    {
        if (value is not ISpecConcreteValue conc)
            throw new InvalidCastException($"Failed to cast {value.ValueType} to {typeof(T)}, not a concrete type.");

        if (!conc.TryEvaluateValue(out T val, out bool isNull))
            throw new InvalidCastException($"Failed to cast {value.ValueType} to {typeof(T)}.");

        return isNull ? null : val;
    }

    /// <summary>
    /// Evaluates values as <typeparamref name="TValue"/>, doing conversions if necessary. More flexible than <see cref="ISpecDynamicValue.TryEvaluateValue{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The result type.</typeparam>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="val">The dynamic value to evaluate.</param>
    /// <param name="value">The evaluated value.</param>
    /// <param name="isNull">Whether or not <paramref name="value"/> is <see langword="null"/> instead of <see langword="default"/> (this matters for value types).</param>
    public static bool TryGetValueAsType<TValue>(in FileEvaluationContext ctx, ISpecDynamicValue val, out TValue? value, out bool isNull)
    {
        if (val is ISpecConcreteValue concrete)
        {
            return concrete.TryEvaluateValue(out value, out isNull);
        }

        Type? valueType = val.ValueType?.ValueType;
        if (valueType == null || valueType == typeof(TValue))
        {
            return val.TryEvaluateValue(in ctx, out value, out isNull);
        }

        value = default;

        if (typeof(TValue) == typeof(bool))
        {
            bool v;
            if (valueType == typeof(byte))
            {
                if (!val.TryEvaluateValue(in ctx, out byte argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(sbyte))
            {
                if (!val.TryEvaluateValue(in ctx, out sbyte argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(ushort))
            {
                if (!val.TryEvaluateValue(in ctx, out ushort argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(GuidOrId))
            {
                if (!val.TryEvaluateValue(in ctx, out GuidOrId argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = !argValue.IsNull;
            }
            else if (valueType == typeof(short))
            {
                if (!val.TryEvaluateValue(in ctx, out short argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(uint))
            {
                if (!val.TryEvaluateValue(in ctx, out uint argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(int))
            {
                if (!val.TryEvaluateValue(in ctx, out int argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(ulong))
            {
                if (!val.TryEvaluateValue(in ctx, out ulong argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(long))
            {
                if (!val.TryEvaluateValue(in ctx, out long argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(char))
            {
                if (!val.TryEvaluateValue(in ctx, out char argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(float))
            {
                if (!val.TryEvaluateValue(in ctx, out float argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(double))
            {
                if (!val.TryEvaluateValue(in ctx, out double argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(decimal))
            {
                if (!val.TryEvaluateValue(in ctx, out decimal argValue, out isNull))
                {
                    return false;
                }
                if (isNull)
                    return true;

                v = argValue != 0;
            }
            else if (valueType == typeof(string))
            {
                if (!val.TryEvaluateValue(in ctx, out string? argValue, out isNull))
                {
                    return false;
                }
                if (isNull || string.IsNullOrWhiteSpace(argValue))
                    return true;

                v = !string.Equals(argValue, "false", StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return val.TryEvaluateValue(in ctx, out value, out isNull);
            }

            value = Unsafe.As<bool, TValue>(ref v);
            return true;
        }

        // IConvertible
        if (typeof(TValue).IsPrimitive && typeof(TValue) != typeof(IntPtr) && typeof(TValue) != typeof(UIntPtr)
            || typeof(TValue).IsEnum
            || typeof(TValue) == typeof(DateTime)
            || typeof(TValue) == typeof(DBNull)
            || typeof(TValue) == typeof(decimal)
            || typeof(TValue) == typeof(string))
        {
            TypeCode tc = Type.GetTypeCode(valueType);
            switch (tc)
            {
                case TypeCode.Boolean:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<bool, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Byte:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<byte, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Char:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<char, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.DateTime:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<DateTime, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.DBNull:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<DBNull, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Decimal:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<decimal, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Double:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<double, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Int16:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<short, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Int32:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<int, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Int64:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<long, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.SByte:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<sbyte, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.Single:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<float, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.String:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<string, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.UInt16:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<ushort, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.UInt32:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<uint, TValue>(in ctx, val, out value, out isNull);
                case TypeCode.UInt64:
                    return SpecDynamicExpressionTreeValueHelpers.TryConvert<ulong, TValue>(in ctx, val, out value, out isNull);
                case (TypeCode)17:
                default:
                    break;
            }
        }

        if (typeof(TValue) == typeof(Guid))
        {
            if (val.TryEvaluateValue(in ctx, out Guid guid, out isNull))
            {
                value = Unsafe.As<Guid, TValue>(ref guid);
            }
            else if (TryGetValueAsType(in ctx, val, out string? guidStr, out isNull))
            {
                if (!isNull && guidStr != null)
                {
                    if (!System.Guid.TryParse(guidStr, out guid))
                        return false;

                    value = Unsafe.As<Guid, TValue>(ref guid);
                }
                else
                {
                    value = default;
                }

                return true;
            }

            return false;
        }

        if (typeof(TValue) == typeof(DateTimeOffset))
        {
            if (val.TryEvaluateValue(in ctx, out DateTimeOffset dateTimeOffset, out isNull))
            {
                value = Unsafe.As<DateTimeOffset, TValue>(ref dateTimeOffset);
            }
            else if (TryGetValueAsType(in ctx, val, out string? dtoStr, out isNull))
            {
                if (!isNull && dtoStr != null)
                {
                    if (!DateTimeOffset.TryParse(dtoStr, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dateTimeOffset))
                        return false;

                    value = Unsafe.As<DateTimeOffset, TValue>(ref dateTimeOffset);
                }
                else
                {
                    value = default;
                }

                return true;
            }

            return false;
        }

        if (typeof(TValue) == typeof(GuidOrId))
        {
            if (val.TryEvaluateValue(in ctx, out Guid guid, out isNull))
            {
                value = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TValue>(new GuidOrId(guid));
            }
            else if (val.TryEvaluateValue(in ctx, out ushort id, out isNull))
            {
                value = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TValue>(new GuidOrId(id));
            }
            else if (TryGetValueAsType(in ctx, val, out string? guidIdStr, out isNull))
            {
                if (!isNull && guidIdStr != null)
                {
                    if (!GuidOrId.TryParse(guidIdStr, out GuidOrId guidOrId))
                        return false;

                    value = Unsafe.As<GuidOrId, TValue>(ref guidOrId);
                }
                else
                {
                    value = default;
                }

                return true;
            }
        }

        if (typeof(TValue) == typeof(Color32) && valueType == typeof(Color))
        {
            if (val.TryEvaluateValue(in ctx, out Color clr, out isNull))
            {
                value = SpecDynamicExpressionTreeValueHelpers.As<Color32, TValue>(clr);
                return true;
            }
        }
        if (typeof(TValue) == typeof(Color) && valueType == typeof(Color32))
        {
            if (val.TryEvaluateValue(in ctx, out Color32 clr, out isNull))
            {
                value = SpecDynamicExpressionTreeValueHelpers.As<Color, TValue>(clr);
                return true;
            }
        }

        isNull = true;
        return false;
    }

    /// <summary>
    /// Attempts to parse a value, data-ref, math expression, or property-ref from a <see cref="string"/>.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="context">The context of what type of dynamic value should be parsed when not specified.</param>
    /// <param name="expectedType">Optional type of value to expect to parse.</param>
    /// <param name="reference">The parsed value.</param>
    /// <returns>Whether or not parsing was successful.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is an invalid value.</exception>
    public static bool TryParse(string value, SpecDynamicValueContext context, ISpecPropertyType? expectedType, [MaybeNullWhen(false)] out ISpecDynamicValue reference)
    {
        if (value != null)
            return TryParse(value.AsSpan(), value, context, expectedType, out reference);

        reference = null!;
        return false;
    }

    /// <summary>
    /// Attempts to parse a value, data-ref, math expression, or property-ref from a <see cref="string"/>.
    /// </summary>
    /// <param name="value">The value to parse.</param>
    /// <param name="context">The context of what type of dynamic value should be parsed when not specified.</param>
    /// <param name="expectedType">Optional type of value to expect to parse.</param>
    /// <param name="reference">The parsed value.</param>
    /// <returns>Whether or not parsing was successful.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is an invalid value.</exception>
    public static bool TryParse(ReadOnlySpan<char> value, SpecDynamicValueContext context, ISpecPropertyType? expectedType, [MaybeNullWhen(false)] out ISpecDynamicValue reference)
    {
        return TryParse(value, null, context, expectedType, out reference);
    }

    private static bool TryParse(ReadOnlySpan<char> value, string? optionalString, SpecDynamicValueContext context, ISpecPropertyType? expectedType, [MaybeNullWhen(false)] out ISpecDynamicValue reference)
    {
        if (((int)context & 0b11) == 3)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "More than one of [ AssumeProperty, AssumeData ].");
        }

        reference = null!;

        if (value.Length > 1 && value[0] == '=')
        {
            return TryParseExpressionTree(value.Slice(1), out reference, expectedType);
        }

        // #(data) @(prop) #data @prop
        if (value.Length > 1 && value[0] is '#' or '@')
        {
            char c1 = value[0];

            if (!TryTrimParenthesis(ref value, 1))
            {
                if (context != SpecDynamicValueContext.AssumeDataRef)
                    return false;
            }

            return c1 == '#'
                ? TryParseDataRef(value, null, out reference)
                : TryParsePropertyRef(value, null, out reference);
        }

        // basic prop ref or (prop) in an assume value
        if (context is SpecDynamicValueContext.AssumeProperty or SpecDynamicValueContext.AssumeDataRef)
        {
            int l = value.Length;
            if (!TryTrimParenthesis(ref value, 0))
            {
                if (context != SpecDynamicValueContext.AssumeDataRef)
                    return false;
            }

            if (l != value.Length)
                optionalString = null;

            return context == SpecDynamicValueContext.AssumeDataRef
                ? TryParseDataRef(value, optionalString, out reference)
                : TryParsePropertyRef(value, optionalString, out reference);
        }

        // %(value)
        if (value.Length > 0 && value[0] == '%' && !TryTrimParenthesis(ref value, 1))
        {
            return false;
        }

        return TryParseValue(value, optionalString, expectedType, out reference);
    }

    private static bool TryTrimParenthesis(ref ReadOnlySpan<char> value, int start)
    {
        if (value.Length <= start)
            return false;

        if (value[start] == '(')
        {
            if (value.Length < start + 3)
                return false;

            if (value[value.Length - 1] != ')')
                return false;

            value = value.Slice(start + 1, value.Length - start - 2);
        }
        else if (start != 0)
        {
            value = value.Slice(start);
        }
        return true;
    }

    private static bool TryParseExpressionTree(ReadOnlySpan<char> value, out ISpecDynamicValue reference, ISpecPropertyType? expectedType)
    {
        reference = null!;
        value = value.Trim();
        if (value.IsEmpty)
            return false;

        bool wasInParenthesis = false;
        if (value[0] == '(')
        {
            int close = IndexOfClosingBracket(value, 0);
            if (close == -1)
                return false;

            value = value
                .Slice(1, value.Length - close - 1)
                .Trim();
            wasInParenthesis = true;
        }

        int funcArgStart = value.IndexOf('(');
        int spaceIndex = value.IndexOf(' ');
        if (spaceIndex >= 0 && spaceIndex < funcArgStart)
            funcArgStart = -1;

        ReadOnlySpan<char> funcName = value.Slice(0, funcArgStart == -1 ? value.Length : funcArgStart).TrimEnd();
        bool isConstant = funcArgStart == -1;
        if (isConstant)
        {
            if (!wasInParenthesis && funcName.Equals("PI".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                reference = CastFromDouble(Math.PI, expectedType)!;
            }
            else if (!wasInParenthesis && funcName.Equals("TAU".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                reference = CastFromDouble(2d * Math.PI, expectedType)!;
            }
            else if (!wasInParenthesis && funcName.Equals("E".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                reference = CastFromDouble(Math.E, expectedType)!;
            }
            else if (!wasInParenthesis && funcName.Equals("NULL".AsSpan(), StringComparison.OrdinalIgnoreCase))
            {
                reference = Null;
            }
            else if (expectedType is IStringParseableSpecPropertyType stringParseable)
            {
                stringParseable.TryParse(funcName, null, out reference);
            }
            else
            {
                string str = funcName.ToString();
                if (int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out int num))
                    reference = Int32(num, expectedType);
                else if (double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out double numf))
                    reference = Float64(numf, expectedType);
                else
                {
                    if (funcName[0] is '"' or '\'')
                    {
                        int closeIndex = IndexOfClosingBracket(funcName, 0);
                        if (closeIndex != -1)
                        {
                            funcName = funcName.Slice(1, funcName.Length - closeIndex - 1);
                            str = funcName.ToString();
                        }
                    }

                    reference = String(str, expectedType);
                }
            }
            return reference != null;
        }

        int argEndIndex = IndexOfClosingBracket(value, funcArgStart);
        if (argEndIndex <= funcArgStart + 1)
        {
            return false;
        }

        ReadOnlySpan<char> args = value.Slice(funcArgStart + 1, argEndIndex - funcArgStart - 1).Trim();
        if (SpecDynamicExpressionTreeUnaryValue.TryParseOperation(funcName, out SpecDynamicExpressionTreeUnaryOperation unary))
        {
            ReadOnlySpan<char> arg0 = args.Trim();
            if (arg0.IsEmpty)
                return false;

            TryTrimParenthesis(ref arg0, 0);
            if (!TryParse(arg0, SpecDynamicValueContext.Optional, expectedType, out ISpecDynamicValue? arg))
            {
                return false;
            }

            reference = new SpecDynamicExpressionTreeUnaryValue(arg, unary, expectedType);
            return true;
        }

        if (SpecDynamicExpressionTreeBinaryValue.TryParseOperation(funcName, out SpecDynamicExpressionTreeBinaryOperation binary))
        {
            spaceIndex = IndexOfAtCurrentDepth(args, 0, ' ', '(');
            if (spaceIndex < 1 || spaceIndex >= args.Length - 1)
                return false;
            
            ReadOnlySpan<char> arg0 = args.Slice(0, spaceIndex).Trim();
            ReadOnlySpan<char> arg1 = args.Slice(spaceIndex + 1).Trim();
            if (arg0.IsEmpty || arg1.IsEmpty)
                return false;

            TryTrimParenthesis(ref arg0, 0);
            TryTrimParenthesis(ref arg1, 0);
            if (!TryParse(arg0, SpecDynamicValueContext.Optional, expectedType, out ISpecDynamicValue? left)
                || !TryParse(arg1, SpecDynamicValueContext.Optional, expectedType, out ISpecDynamicValue? right))
            {
                return false;
            }

            reference = new SpecDynamicExpressionTreeBinaryValue(left, right, binary, expectedType);
            return true;
        }

        if (SpecDynamicExpressionTreeTertiaryValue.TryParseOperation(funcName, out SpecDynamicExpressionTreeTertiaryOperation tertiary))
        {
            spaceIndex = IndexOfAtCurrentDepth(args, 0, ' ', '(');
            if (spaceIndex < 1 || spaceIndex >= args.Length - 1)
                return false;
            
            int spaceIndex2 = IndexOfAtCurrentDepth(args, spaceIndex + 1, ' ', '(');
            if (spaceIndex2 < 1 || spaceIndex2 >= args.Length - 1)
                return false;

            ReadOnlySpan<char> arg0 = args.Slice(0, spaceIndex).Trim();
            ReadOnlySpan<char> arg1 = args.Slice(spaceIndex + 1, spaceIndex2 - spaceIndex - 1).Trim();
            ReadOnlySpan<char> arg2 = args.Slice(spaceIndex2 + 1).Trim();
            if (arg0.IsEmpty || arg1.IsEmpty || arg2.IsEmpty)
                return false;

            TryTrimParenthesis(ref arg0, 0);
            TryTrimParenthesis(ref arg1, 0);
            TryTrimParenthesis(ref arg2, 0);
            if (!TryParse(arg0, SpecDynamicValueContext.Optional, expectedType, out ISpecDynamicValue? arg1Val)
                || !TryParse(arg1, SpecDynamicValueContext.Optional, expectedType, out ISpecDynamicValue? arg2Val)
                || !TryParse(arg2, SpecDynamicValueContext.Optional, expectedType, out ISpecDynamicValue? arg3Val))
            {
                return false;
            }

            reference = new SpecDynamicExpressionTreeTertiaryValue(arg1Val, arg2Val, arg3Val, tertiary, expectedType);
            return true;
        }

        return false;
    }

    private static ISpecDynamicValue? CastFromDouble(double d, ISpecPropertyType? expectedType)
    {
        if (expectedType == null)
        {
            return Float64(d);
        }

        Type vt = expectedType.ValueType;
        if (vt == typeof(double))
            return Float64(d, expectedType);

        if (vt == typeof(float))
            return Float32((float)d, expectedType);

        if (vt == typeof(decimal))
            return Float128((decimal)d, expectedType);

        if (vt == typeof(bool))
            return Boolean(d != 0, expectedType);

        if (vt == typeof(string))
            return String(d.ToString(CultureInfo.InvariantCulture), expectedType);

        try
        {
            if (vt == typeof(byte))
                return UInt8(checked ( (byte)d ), expectedType);
            
            if (vt == typeof(sbyte))
                return Int8(checked ( (sbyte)d ), expectedType);
            
            if (vt == typeof(ushort))
                return UInt16(checked ( (ushort)d ), expectedType);
            
            if (vt == typeof(GuidOrId))
                return new SpecDynamicConcreteValue<GuidOrId>(new GuidOrId(checked ( (ushort)d )), expectedType as ISpecPropertyType<GuidOrId>);
            
            if (vt == typeof(short))
                return Int16(checked ( (short)d ), expectedType);
            
            if (vt == typeof(uint))
                return UInt32(checked ( (uint)d ), expectedType);
            
            if (vt == typeof(int))
                return Int32(checked ( (int)d ), expectedType);
            
            if (vt == typeof(ulong))
                return UInt64(checked ( (ulong)d ), expectedType);
            
            if (vt == typeof(long))
                return Int64(checked ( (long)d ), expectedType);
        }
        catch (OverflowException) { }

        return null;
    }

    private static int IndexOfClosingBracket(ReadOnlySpan<char> value, int openIndex)
    {
        if (openIndex < 0 || value.Length - openIndex <= 1)
            return -1;

        char open = value[openIndex];
        char close = open switch
        {
            '(' => ')',
            '{' => '}',
            '[' => ']',
            '<' => '>',
            '/' => '\\',
            '\\' => '/',
            _ => open
        };

        if (open == close)
        {
            int index = value.Slice(openIndex + 1).IndexOf(open);
            return index >= 0 ? index + openIndex + 1 : -1;
        }

        int depth = 1;
        for (int i = openIndex + 1; i < value.Length; ++i)
        {
            char c = value[i];
            if (c == open)
            {
                ++depth;
            }
            else if (c == close)
            {
                --depth;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }
    private static int IndexOfAtCurrentDepth(ReadOnlySpan<char> value, int startIndex, char lookFor, char open)
    {
        if (startIndex < 0 || value.Length - startIndex <= 1)
            return -1;

        char close = open switch
        {
            '(' => ')',
            '{' => '}',
            '[' => ']',
            '<' => '>',
            '/' => '\\',
            '\\' => '/',
            _ => open
        };

        if (open == close)
        {
            int index = value.Slice(startIndex + 1).IndexOf(open);
            if (index >= 0)
                value = value.Slice(0, index + startIndex + 1);
        }

        int depth = 1;
        bool isInQuotes = false;

        for (int i = startIndex; i < value.Length; ++i)
        {
            char c = value[i];

            if (c is '\'' or '"')
            {
                isInQuotes = !isInQuotes;
            }
            
            if (isInQuotes)
                continue;

            if (c == lookFor && depth == 1)
                return i;

            if (c == open)
            {
                ++depth;
            }
            else if (c == close)
            {
                --depth;
                if (depth == 0)
                    return -1;
            }
        }

        return -1;
    }

    private static bool TryParsePropertyRef(ReadOnlySpan<char> value, string? optionalString, [MaybeNullWhen(false)] out ISpecDynamicValue reference)
    {
        reference = new PropertyRef(value, optionalString);
        return true;
    }

    private static bool TryParseDataRef(ReadOnlySpan<char> value, string? optionalString, [MaybeNullWhen(false)] out ISpecDynamicValue reference)
    {
        int dot = IndexOfAtCurrentDepth(value, 0, '.', '(');
        if (dot < 0)
            dot = value.Length;


        ReadOnlySpan<char> nameSpace = value.Slice(0, dot).Trim();
        TryTrimParenthesis(ref nameSpace, 0);
        if (nameSpace.Length != value.Length)
            optionalString = null;

        IDataRefTarget? target = DataRef.FromName(nameSpace, optionalString);
        if (target == null)
        {
            reference = null!;
            return false;
        }

        if (dot >= value.Length - 1)
        {
            reference = target;
            return true;
        }

        ReadOnlySpan<char> data = value.Slice(dot + 1);

        int indexerIndex = data.IndexOf('[');
        int dataIndex = data.IndexOf('{');

        ReadOnlySpan<char> propertyName;

        if (indexerIndex == -1 && dataIndex == -1)
        {
            propertyName = data;
        }
        else if (indexerIndex == -1)
        {
            propertyName = data.Slice(0, dataIndex);
        }
        else if (dataIndex == -1)
        {
            propertyName = data.Slice(0, indexerIndex);
        }
        else
        {
            propertyName = data.Slice(0, Math.Min(indexerIndex, dataIndex));
        }

        reference = DataRef.FromName(propertyName, target);

        if (reference is IIndexableDataRef indexable)
        {
            if (indexerIndex == -1 || indexerIndex == data.Length - 1 || indexerIndex == dataIndex - 1)
            {
                reference = null!;
                return false;
            }

            ReadOnlySpan<char> indexSpan = dataIndex < indexerIndex ? data.Slice(indexerIndex + 1) : data.Slice(indexerIndex + 1, dataIndex - indexerIndex - 1);
            int endIndexer = indexSpan.IndexOf(']');
            if (endIndexer == -1
                || endIndexer < 1
                || !int.TryParse(indexSpan.Slice(0, endIndexer).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int index))
            {
                reference = null!;
                return false;
            }

            indexable.Index = index;
        }

        if (dataIndex != -1 && reference is IPropertiesDataRef properties)
        {
            if (dataIndex == -1 || dataIndex == data.Length - 1 || dataIndex == indexerIndex - 1)
            {
                reference = null!;
                return false;
            }

            ReadOnlySpan<char> dataSpan = indexerIndex < dataIndex ? data.Slice(dataIndex + 1) : data.Slice(dataIndex + 1, indexerIndex - dataIndex - 1);
            int endData = dataSpan.IndexOf('}');
            if (endData == -1
                || endData < dataIndex + 2)
            {
                reference = null!;
                return false;
            }

            while (!dataSpan.IsEmpty)
            {
                int commaIndex = dataSpan.IndexOf(',');
                if (commaIndex == -1)
                    break;

                ReadOnlySpan<char> span = dataSpan.Slice(0, commaIndex).Trim();
                dataSpan = dataSpan.Slice(commaIndex + 1);
                int splitIndex = span.IndexOf('=');
                if (splitIndex <= 0 || splitIndex >= span.Length - 1)
                {
                    break;
                }

                properties.SetProperty(span.Slice(0, splitIndex).Trim(), span.Slice(splitIndex + 1).Trim());
            }
        }

        return reference != null;
    }

    private static bool TryParseValue(ReadOnlySpan<char> value, string? optionalString, ISpecPropertyType? expectedType, out ISpecDynamicValue reference)
    {
        if (expectedType is null or ISpecPropertyType<string>)
        {
            reference = String(optionalString ?? value.ToString(), expectedType);
            return true;
        }

        bool allowVectorOnly = false;
        if (expectedType is IStringParseableSpecPropertyType strParsable)
        {
            if (strParsable.TryParse(value, optionalString, out reference))
            {
                return true;
            }

            allowVectorOnly = true;
        }

        if (expectedType is IVectorSpecPropertyType)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double vectorComponent))
#else
            if (double.TryParse(optionalString ?? value.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out double vectorComponent))
#endif
            {
                reference = Float64(vectorComponent);
                return true;
            }
        }
        else if (allowVectorOnly)
        {
            reference = null!;
            return false;
        }

        try
        {
            object? val = value.Equals("null".AsSpan(), StringComparison.Ordinal)
                ? null
                : Convert.ChangeType(optionalString ?? value.ToString(), expectedType.ValueType);

            reference = (ISpecDynamicValue?)Activator.CreateInstance(typeof(SpecDynamicConcreteValue<>).MakeGenericType(expectedType.ValueType), val)!;
            return true;
        }
        catch (InvalidCastException)
        {
            reference = null!;
            return false;
        }
    }

    /// <summary>
    /// Read a dynamic value from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <param name="expectedType">Optional type of value to expect to parse.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is an invalid value.</exception>
    /// <exception cref="JsonException">Failed to parse value.</exception>
    /// <returns>The parsed value.</returns>
    public static ISpecDynamicValue Read(ref Utf8JsonReader reader, JsonSerializerOptions? options, bool expandLists, SpecDynamicValueContext context = SpecDynamicValueContext.Default, ISpecPropertyType? expectedType = null)
    {
        return Read(ref reader, options, context, expectedType == null ? default : new PropertyTypeOrSwitch(expectedType), expandLists);
    }

    /// <summary>
    /// Read a dynamic value from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <param name="expectedType">Optional value type or type-switch to expect to parse.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is an invalid value.</exception>
    /// <exception cref="JsonException">Failed to parse value.</exception>
    /// <returns>The parsed value.</returns>
    [SkipLocalsInit]
    public static ISpecDynamicValue Read(ref Utf8JsonReader reader, JsonSerializerOptions? options, SpecDynamicValueContext context, PropertyTypeOrSwitch expectedType, bool expandLists)
    {
        if (((int)context & 0b11) == 3)
        {
            throw new ArgumentOutOfRangeException(nameof(context), "More than one of [ AssumeProperty, AssumeData ].");
        }

        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        if (reader.TokenType is JsonTokenType.None or JsonTokenType.PropertyName)
        {
            if (!reader.Read())
                throw new JsonException("Failed to parse ISpecDynamicValue, no JSON data.");
        }

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            case JsonTokenType.PropertyName:
                string str = reader.GetString()!;
                if (expandLists && !expectedType.IsSwitch && expectedType.Type is IListTypeSpecPropertyType listType)
                {
                    ISpecPropertyType? t = listType.GetInnerType();
                    if (t != null)
                        expectedType = new PropertyTypeOrSwitch(t);
                }

                if (!TryParse(str, context, expectedType.Type, out ISpecDynamicValue? reference))
                    throw new JsonException("Failed to parse ISpecDynamicValue from a string value.");

                return reference;

            case JsonTokenType.Null:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Number:
                return ReadValue(ref reader, expectedType.Type, (t, type) => t != null
                    ? throw new JsonException($"Failed to parse ISpecDynamicValue from an argument, expected type \"{type.Type}\" but was given type {t}.")
                    : throw new JsonException($"Failed to parse ISpecDynamicValue from an argument, expected type \"{type.Type}\".")
                    , expandLists
                );

            case JsonTokenType.StartArray:
                if (expectedType.Type != null
                    && expectedType.Type.ValueType.IsConstructedGenericType
                    && expectedType.Type.ValueType.GetGenericTypeDefinition() == typeof(EquatableArray<>))
                {
                    Utf8JsonReader readerCopy2 = reader;

                    if (!reader.Read())
                        break;

                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        Type t = expectedType.Type.ValueType.GetGenericArguments()[0];
                        if (reader.TokenType == JsonTokenType.EndArray)
                        {
                            return Null;
                        }

                        Utf8JsonReader readerCopy3 = reader;
                        int count = 1;
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                            ++count;
                        reader = readerCopy3;

                        Array array = Array.CreateInstance(t, count);
                        int i = 0;
                        try
                        {
                            for (; i < count; ++i)
                            {
                                object? value = JsonHelper.Deserialize(ref reader, t, options);
                                reader.Read();

                                array.SetValue(value, i);
                            }
                        }
                        catch (JsonException ex)
                        {
                            throw new JsonException($"Failed to read array value {i} for list value.", ex);
                        }

                        ReadEquatableArrayVisitor v = new ReadEquatableArrayVisitor(array);
                        expectedType.Type.Visit(ref v);
                        return v.Result;
                    }

                    reader = readerCopy2;
                }

                if ((context & SpecDynamicValueContext.AllowSwitch) == 0)
                    break;

                Utf8JsonReader readerCopy = reader;
                try
                {
                    SpecDynamicSwitchValue @switch = SpecDynamicSwitchValueConverter.ReadSwitch(ref readerCopy, options, expectedType, expandLists)!;
                    reader = readerCopy;

                    return @switch;
                }
                catch (JsonException) { }

                throw new JsonException("Failed to read switch statement when parsing ISpecDynamicValue from an array value.");

            case JsonTokenType.StartObject:
                if ((context & SpecDynamicValueContext.AllowCondition) != 0)
                {
                    if (expectedType.IsSwitch || expectedType.Type != null && expectedType.Type.ValueType != typeof(bool))
                        throw new JsonException("Expected boolean type when reading condition when parsing ISpecDynamicValue from an object value.");

                    readerCopy = reader;
                    try
                    {
                        SpecCondition condition = SpecConditionConverter.ReadCondition(ref readerCopy, options);
                        reader = readerCopy;

                        return new ConditionSpecDynamicValue(condition);
                    }
                    catch (JsonException)
                    {
                        readerCopy = reader;
                    }
                }
                if ((context & SpecDynamicValueContext.AllowSwitchCase) != 0)
                {
                    if (expectedType.IsSwitch)
                        throw new JsonException("Unable to read switch case for switchable type when parsing ISpecDynamicValue from an object value.");

                    readerCopy = reader;
                    try
                    {
                        SpecDynamicSwitchCaseValue @case = SpecDynamicSwitchCaseValueConverter.ReadCase(ref readerCopy, options, expectedType.Type, expandLists)!;
                        reader = readerCopy;

                        return @case;
                    }
                    catch (JsonException) { }
                }

                if ((context & (SpecDynamicValueContext.AllowCondition | SpecDynamicValueContext.AllowSwitchCase)) == 0)
                    break;

                if ((context & (SpecDynamicValueContext.AllowCondition | SpecDynamicValueContext.AllowSwitchCase)) == (SpecDynamicValueContext.AllowCondition | SpecDynamicValueContext.AllowSwitchCase))
                    throw new JsonException("Failed to read condition or switch case when parsing ISpecDynamicValue from an object value.");

                if ((context & SpecDynamicValueContext.AllowCondition) == SpecDynamicValueContext.AllowCondition)
                    throw new JsonException("Failed to read condition when parsing ISpecDynamicValue from an object value.");

                throw new JsonException("Failed to read switch case when parsing ISpecDynamicValue from an object value.");
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing ISpecDynamicValue.");
    }

#nullable disable
    private struct ReadEquatableArrayVisitor(Array array) : ISpecPropertyTypeVisitor
    {
        public ISpecDynamicValue Result;
        
        public void Visit<T>(ISpecPropertyType<T> type) where T : IEquatable<T>
        {
            Result = new SpecDynamicConcreteValue<T>((T)Activator.CreateInstance(typeof(T), array), type);
        }
    }
#nullable restore

    /// <summary>
    /// Read a concrete value from a <see cref="Utf8JsonReader"/>.
    /// </summary>
    /// <param name="expectedType">Optional value type to expect to parse.</param>
    /// <param name="invalidTypeThrowHandler">Invoked when an exception should be thrown or fallback value should be returned when <paramref name="expectedType"/> doesn't match the given type.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="context"/> is an invalid value.</exception>
    /// <exception cref="JsonException">Failed to parse value.</exception>
    /// <returns>The parsed value.</returns>
    public static ISpecDynamicValue ReadValue(ref Utf8JsonReader reader, ISpecPropertyType? expectedType, Func<Type?, ISpecPropertyType, ISpecDynamicValue> invalidTypeThrowHandler, bool reduceLists)
    {
        if (reduceLists && expectedType is IListTypeSpecPropertyType list)
        {
            ISpecPropertyType? t = list.GetInnerType();
            if (t != null)
                expectedType = t;
        }

        switch (reader.TokenType)
        {
            case JsonTokenType.False:
                return expectedType == null || expectedType is ISpecPropertyType<bool> ? False : invalidTypeThrowHandler(typeof(bool), expectedType);

            case JsonTokenType.True:
                return expectedType == null || expectedType is ISpecPropertyType<bool> ? True : invalidTypeThrowHandler(typeof(bool), expectedType);

            case JsonTokenType.Null:
                return Null;

            case JsonTokenType.Number:

                if (expectedType == null)
                {
                    if (reader.TryGetInt32(out int i4))
                    {
                        return Int32(i4);
                    }

                    if (reader.TryGetUInt32(out uint u4))
                    {
                        return UInt32(u4);
                    }

                    if (reader.TryGetInt64(out long i8))
                    {
                        return Int64(i8);
                    }

                    if (reader.TryGetUInt64(out ulong u8))
                    {
                        return UInt64(u8);
                    }

                    if (reader.TryGetDouble(out double r8))
                    {
                        return Float64(r8);
                    }

                    throw new JsonException("Failed to read 'Comparand' in SpecCondition.");
                }

                if (expectedType is EnumSpecType { IsFlags: true } enumType)
                {
                    if (reader.TryGetInt64(out long i8))
                    {
                        return EnumFlags(enumType, i8);
                    }
                    if (reader.TryGetUInt64(out ulong u8))
                    {
                        return EnumFlags(enumType, u8);
                    }

                    return invalidTypeThrowHandler(typeof(double), expectedType);
                }

                Type valueType = expectedType.ValueType;
                if (valueType == typeof(byte))
                {
                    return reader.TryGetByte(out byte u1) ? UInt8(u1, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(ushort))
                {
                    return reader.TryGetUInt16(out ushort u2) ? UInt16(u2, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(uint))
                {
                    return reader.TryGetUInt32(out uint u4) ? UInt32(u4, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(ulong))
                {
                    return reader.TryGetUInt64(out ulong u8) ? UInt64(u8, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(sbyte))
                {
                    return reader.TryGetSByte(out sbyte i1) ? Int8(i1, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(short))
                {
                    return reader.TryGetInt16(out short i2) ? Int16(i2, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(int))
                {
                    return reader.TryGetInt32(out int i4) ? Int32(i4, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(long))
                {
                    return reader.TryGetInt64(out long i8) ? Int64(i8, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(float))
                {
                    return reader.TryGetSingle(out float r4) ? Float32(r4, expectedType) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(double))
                {
                    return reader.TryGetDouble(out double r8) ? Float64(r8) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(decimal))
                {
                    return reader.TryGetDecimal(out decimal r16) ? Float128(r16) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(bool))
                {
                    return reader.TryGetInt32(out int i4) ? Boolean(i4 > 0) : invalidTypeThrowHandler(null, expectedType);
                }
                if (valueType == typeof(Guid))
                {
                    return reader.TryGetInt32(out int i4) && i4 == 0 ? Guid(System.Guid.Empty) : invalidTypeThrowHandler(typeof(double), expectedType);
                }
                if (valueType == typeof(GuidOrId))
                {
                    return reader.TryGetUInt16(out ushort id)
                        ? new SpecDynamicConcreteValue<GuidOrId>(new GuidOrId(id), expectedType as ISpecPropertyType<GuidOrId> ?? KnownTypes.GuidOrId(default))
                        : invalidTypeThrowHandler(typeof(double), expectedType);
                }
                if (valueType == typeof(DateTime))
                {
                    return reader.TryGetInt32(out int i4) && i4 == 0
                        ? new SpecDynamicConcreteConvertibleValue<DateTime>(DateTime.MinValue, expectedType as ISpecPropertyType<DateTime> ?? KnownTypes.DateTime)
                        : invalidTypeThrowHandler(typeof(double), expectedType);
                }
                if (valueType == typeof(DateTimeOffset))
                {
                    return reader.TryGetInt32(out int i4) && i4 == 0
                        ? new SpecDynamicConcreteValue<DateTimeOffset>(DateTimeOffset.MinValue, expectedType as ISpecPropertyType<DateTimeOffset> ?? KnownTypes.DateTimeOffset)
                        : invalidTypeThrowHandler(typeof(double), expectedType);
                }
                if (valueType == typeof(char))
                {
                    return reader.TryGetUInt16(out ushort u2) ? new SpecDynamicConcreteConvertibleValue<char>((char)u2, expectedType as ISpecPropertyType<char> ?? KnownTypes.Character) : invalidTypeThrowHandler(typeof(double), expectedType);
                }

                break;

            case JsonTokenType.String:

                if (expectedType == null)
                {
                    if (reader.TryGetDateTime(out DateTime dt))
                    {
                        return new SpecDynamicConcreteConvertibleValue<DateTime>(dt, KnownTypes.DateTime);
                    }
                    if (JsonHelper.TryGetGuid(ref reader, out Guid guid))
                    {
                        return Guid(guid);
                    }
                    if (reader.TryGetDateTimeOffset(out DateTimeOffset dtOffset))
                    {
                        return new SpecDynamicConcreteValue<DateTimeOffset>(dtOffset, KnownTypes.DateTimeOffset);
                    }

                    return String(reader.GetString()!, expectedType as ISpecPropertyType<string> ?? KnownTypes.String);
                }

                valueType = expectedType.ValueType;
                if (valueType == typeof(string))
                {
                    return String(reader.GetString()!, expectedType as ISpecPropertyType<string> ?? KnownTypes.String);
                }
                if (valueType == typeof(Guid))
                {
                    return JsonHelper.TryGetGuid(ref reader, out Guid guid) ? Guid(guid, expectedType as ISpecPropertyType<Guid> ?? KnownTypes.Guid) : invalidTypeThrowHandler(typeof(string), expectedType);
                }
                if (valueType == typeof(DateTime))
                {
                    return reader.TryGetDateTime(out DateTime dt) ? new SpecDynamicConcreteConvertibleValue<DateTime>(dt, expectedType as ISpecPropertyType<DateTime> ?? KnownTypes.DateTime) : invalidTypeThrowHandler(typeof(string), expectedType);
                }
                if (valueType == typeof(DateTimeOffset))
                {
                    return reader.TryGetDateTimeOffset(out DateTimeOffset dt) ? new SpecDynamicConcreteValue<DateTimeOffset>(dt, expectedType as ISpecPropertyType<DateTimeOffset> ?? KnownTypes.DateTimeOffset) : invalidTypeThrowHandler(typeof(string), expectedType);
                }
                if (valueType == typeof(char))
                {
                    string str = reader.GetString()!;
                    if (str.Length != 1)
                        invalidTypeThrowHandler(typeof(string), expectedType);

                    return new SpecDynamicConcreteConvertibleValue<char>(str[0], expectedType as ISpecPropertyType<char> ?? KnownTypes.Character);
                }
                if (expectedType is IStringParseableSpecPropertyType stringParseable)
                {
                    string str = reader.GetString()!;
                    if (!stringParseable.TryParse(str.AsSpan(), str, out ISpecDynamicValue value))
                    {
                        invalidTypeThrowHandler(typeof(string), expectedType);
                    }

                    return value;
                }

                break;
        }

        if (expectedType != null)
            invalidTypeThrowHandler(null, expectedType);

        throw new JsonException($"Failed to read 'Comparand' in SpecCondition, unexpected token {reader.TokenType}.");
    }
}