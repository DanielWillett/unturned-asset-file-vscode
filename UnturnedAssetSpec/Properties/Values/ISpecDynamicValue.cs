using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Globalization;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface ISpecDynamicValue
{
    ISpecPropertyType? ValueType { get; }
    bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition);
    bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull);
}

public enum SpecDynamicValueContext
{
    Optional,
    AssumeProperty,
    AssumeBang
}

public static class SpecDynamicValue
{
    public static SpecDynamicConcreteNullValue Null => SpecDynamicConcreteNullValue.Instance;

    public static SpecDynamicConcreteValue<bool> True { get; } = new SpecDynamicConcreteValue<bool>(true, KnownTypes.Boolean);
    public static SpecDynamicConcreteValue<bool> False { get; } = new SpecDynamicConcreteValue<bool>(false, KnownTypes.Boolean);

    public static SpecDynamicConcreteValue<bool> Included { get; } = new SpecDynamicConcreteValue<bool>(true, KnownTypes.Flag);
    public static SpecDynamicConcreteValue<bool> Excluded { get; } = new SpecDynamicConcreteValue<bool>(false, KnownTypes.Flag);

    public static SpecDynamicConcreteValue<bool> Flag(bool v) => v ? Included : Excluded;

    public static SpecDynamicConcreteValue<bool> Boolean(bool v) => v ? True : False;
    public static SpecDynamicConcreteValue<bool> Boolean(bool v, ISpecPropertyType type)
    {
        if (ReferenceEquals(type, KnownTypes.Flag))
        {
            return Flag(v);
        }

        if (!ReferenceEquals(type, KnownTypes.Boolean) && type is ISpecPropertyType<bool> b)
        {
            return new SpecDynamicConcreteValue<bool>(v, b);
        }

        return Boolean(v);
    }

    public static SpecDynamicConcreteValue<byte> UInt8(byte v) => new SpecDynamicConcreteValue<byte>(v, KnownTypes.UInt8);
    public static SpecDynamicConcreteValue<byte> UInt8(byte v, ISpecPropertyType type) => type is ISpecPropertyType<byte> b ? new SpecDynamicConcreteValue<byte>(v, b) : UInt8(v);

    public static SpecDynamicConcreteValue<ushort> UInt16(ushort v) => new SpecDynamicConcreteValue<ushort>(v, KnownTypes.UInt16);
    public static SpecDynamicConcreteValue<ushort> UInt16(ushort v, ISpecPropertyType type) => type is ISpecPropertyType<ushort> b ? new SpecDynamicConcreteValue<ushort>(v, b) : UInt16(v);

    public static SpecDynamicConcreteValue<uint> UInt32(uint v) => new SpecDynamicConcreteValue<uint>(v, KnownTypes.UInt32);
    public static SpecDynamicConcreteValue<uint> UInt32(uint v, ISpecPropertyType type) => type is ISpecPropertyType<uint> b ? new SpecDynamicConcreteValue<uint>(v, b) : UInt32(v);

    public static SpecDynamicConcreteValue<ulong> UInt64(ulong v) => new SpecDynamicConcreteValue<ulong>(v, KnownTypes.UInt64);
    public static SpecDynamicConcreteValue<ulong> UInt64(ulong v, ISpecPropertyType type) => type is ISpecPropertyType<ulong> b ? new SpecDynamicConcreteValue<ulong>(v, b) : UInt64(v);

    public static SpecDynamicConcreteValue<sbyte> Int8(sbyte v) => new SpecDynamicConcreteValue<sbyte>(v, KnownTypes.Int8);
    public static SpecDynamicConcreteValue<sbyte> Int8(sbyte v, ISpecPropertyType type) => type is ISpecPropertyType<sbyte> b ? new SpecDynamicConcreteValue<sbyte>(v, b) : Int8(v);

    public static SpecDynamicConcreteValue<short> Int16(short v) => new SpecDynamicConcreteValue<short>(v, KnownTypes.Int16);
    public static SpecDynamicConcreteValue<short> Int16(short v, ISpecPropertyType type) => type is ISpecPropertyType<short> b ? new SpecDynamicConcreteValue<short>(v, b) : Int16(v);

    public static SpecDynamicConcreteValue<int> Int32(int v) => new SpecDynamicConcreteValue<int>(v, KnownTypes.Int32);
    public static SpecDynamicConcreteValue<int> Int32(int v, ISpecPropertyType type) => type is ISpecPropertyType<int> b ? new SpecDynamicConcreteValue<int>(v, b) : Int32(v);

    public static SpecDynamicConcreteValue<long> Int64(long v) => new SpecDynamicConcreteValue<long>(v, KnownTypes.Int64);
    public static SpecDynamicConcreteValue<long> Int64(long v, ISpecPropertyType type) => type is ISpecPropertyType<long> b ? new SpecDynamicConcreteValue<long>(v, b) : Int64(v);

    public static SpecDynamicConcreteValue<float> Float32(float v) => new SpecDynamicConcreteValue<float>(v, KnownTypes.Float32);
    public static SpecDynamicConcreteValue<float> Float32(float v, ISpecPropertyType type) => type is ISpecPropertyType<float> b ? new SpecDynamicConcreteValue<float>(v, b) : Float32(v);

    public static SpecDynamicConcreteValue<double> Float64(double v) => new SpecDynamicConcreteValue<double>(v, KnownTypes.Float64);
    public static SpecDynamicConcreteValue<double> Float64(double v, ISpecPropertyType type) => type is ISpecPropertyType<double> b ? new SpecDynamicConcreteValue<double>(v, b) : Float64(v);

    public static SpecDynamicConcreteValue<decimal> Float128(decimal v) => new SpecDynamicConcreteValue<decimal>(v, KnownTypes.Float128);
    public static SpecDynamicConcreteValue<decimal> Float128(decimal v, ISpecPropertyType type) => type is ISpecPropertyType<decimal> b ? new SpecDynamicConcreteValue<decimal>(v, b) : Float128(v);

    public static SpecDynamicConcreteValue<string> String(string v) => new SpecDynamicConcreteValue<string>(v, KnownTypes.String);
    public static SpecDynamicConcreteValue<string> String(string v, ISpecPropertyType type) => type is ISpecPropertyType<string> b ? new SpecDynamicConcreteValue<string>(v, b) : String(v);

    public static SpecDynamicConcreteValue<Guid> Guid(Guid v) => new SpecDynamicConcreteValue<Guid>(v, KnownTypes.Guid);
    public static SpecDynamicConcreteValue<Guid> Guid(Guid v, ISpecPropertyType type) => type is ISpecPropertyType<Guid> b ? new SpecDynamicConcreteValue<Guid>(v, b) : Guid(v);

    public static SpecDynamicConcreteEnumValue Enum(EnumSpecType type, int value) => new SpecDynamicConcreteEnumValue(type, value);
    public static SpecDynamicConcreteEnumValue Enum(EnumSpecTypeValue value) => new SpecDynamicConcreteEnumValue(value.Type, value.Index);
    public static SpecDynamicConcreteEnumValue EnumNull(EnumSpecType type) => new SpecDynamicConcreteEnumValue(type);

    public static bool TryParse(string value, SpecDynamicValueContext context, ISpecPropertyType? expectedType, out ISpecDynamicValue reference)
    {
        if (!string.IsNullOrEmpty(value))
            return TryParse(value.AsSpan(), value, context, expectedType, out reference);

        reference = null!;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> value, SpecDynamicValueContext context, ISpecPropertyType? expectedType, out ISpecDynamicValue reference)
    {
        if (!value.IsEmpty)
            return TryParse(value, null, context, expectedType, out reference);

        reference = null!;
        return false;
    }

    private static bool TryParse(ReadOnlySpan<char> value, string? optionalString, SpecDynamicValueContext context, ISpecPropertyType? expectedType, out ISpecDynamicValue reference)
    {
        reference = null!;
        // #(bang) @(prop) #bang @prop
        if (value.Length > 1 && value[0] is '#' or '@')
        {
            char c1 = value[0];

            if (!TryTrimParenthesis(ref value, 1))
                return false;

            return c1 == '#'
                ? TryParseBangRef(value, null, out reference)
                : TryParsePropertyRef(value, null, out reference);
        }

        // basic prop ref or (prop) in an assume value
        if (context is SpecDynamicValueContext.AssumeProperty or SpecDynamicValueContext.AssumeBang)
        {
            int l = value.Length;
            if (!TryTrimParenthesis(ref value, 0))
                return false;

            if (l != value.Length)
                optionalString = null;

            return context == SpecDynamicValueContext.AssumeBang
                ? TryParseBangRef(value, optionalString, out reference)
                : TryParsePropertyRef(value, optionalString, out reference);
        }

        // %(value)
        if (value[0] == '%' && !TryTrimParenthesis(ref value, 1))
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

            value = value.Slice(start + 1, value.Length - start - 1);
        }
        else if (start != 0)
        {
            value = value.Slice(start);
        }
        return true;
    }

    private static bool TryParsePropertyRef(ReadOnlySpan<char> value, string? optionalString, out ISpecDynamicValue reference)
    {
        reference = new PropertyRef(value, optionalString);
        return true;
    }

    private static bool TryParseBangRef(ReadOnlySpan<char> value, string? optionalString, out ISpecDynamicValue reference)
    {
        int dot = value.IndexOf('.');
        if (dot < 0)
            dot = value.Length;

        ReadOnlySpan<char> nameSpace = value.Slice(0, dot).Trim();
        if (nameSpace.Length != value.Length)
            optionalString = null;

        IBangRefTarget target;
        if (nameSpace.Equals("Self".AsSpan(), StringComparison.Ordinal))
        {
            if (dot >= value.Length - 1)
            {
                reference = SelfBangRef.Instance;
                return true;
            }

            target = SelfBangRef.Instance;
        }
        else if (nameSpace.Equals("This".AsSpan(), StringComparison.Ordinal))
        {
            if (dot >= value.Length - 1)
            {
                reference = ThisBangRef.Instance;
                return true;
            }

            target = ThisBangRef.Instance;
        }
        else
        {
            if (nameSpace.Equals("\\Self".AsSpan(), StringComparison.Ordinal))
            {
                target = new PropertyBangRef("Self");
            }
            else if (nameSpace.Equals("\\This".AsSpan(), StringComparison.Ordinal))
            {
                target = new PropertyBangRef("This");
            }
            else
            {
                target = new PropertyBangRef(optionalString ?? nameSpace.ToString());
            }
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

        if (propertyName.Equals("Excluded".AsSpan(), StringComparison.Ordinal))
        {
            reference = new ExcludedBangRef(target);
        }
        else if (propertyName.Equals("Included".AsSpan(), StringComparison.Ordinal))
        {
            reference = new IncludedBangRef(target);
        }
        else if (propertyName.Equals("Key".AsSpan(), StringComparison.Ordinal))
        {
            reference = new KeyBangRef(target);
        }
        else if (propertyName.Equals("Value".AsSpan(), StringComparison.Ordinal))
        {
            reference = new ValueBangRef(target);
        }
        else if (propertyName.Equals("KeyGroups".AsSpan(), StringComparison.Ordinal))
        {
            reference = new KeyGroupsBangRef(target);
        }
        else
        {
            reference = null!;
            return false;
        }

        if (reference is IIndexableBangRef indexable)
        {
            if (indexerIndex == -1 || indexerIndex == data.Length - 1 || indexerIndex == dataIndex - 1)
            {
                reference = null!;
                return false;
            }

            ReadOnlySpan<char> indexSpan = dataIndex < indexerIndex ? data.Slice(indexerIndex + 1) : data.Slice(indexerIndex + 1, dataIndex - indexerIndex - 1);
            int endIndexer = indexSpan.IndexOf(']');
            if (endIndexer == -1
                || endIndexer < indexerIndex + 2
                || !int.TryParse(indexSpan.Slice(endIndexer).ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out int index))
            {
                reference = null!;
                return false;
            }

            indexable.Index = index;
        }

        if (dataIndex != -1 && reference is IPropertiesBangRef properties)
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

        return true;
    }

    private static bool TryParseValue(ReadOnlySpan<char> value, string? optionalString, ISpecPropertyType? expectedType, out ISpecDynamicValue reference)
    {
        if (expectedType is null or ISpecPropertyType<string>)
        {
            reference = String(optionalString ?? value.ToString());
            return true;
        }

        if (expectedType is IStringParseableSpecPropertyType strParsable)
        {
            return strParsable.TryParse(value, optionalString, out reference);
        }

        try
        {
            object? val = value.Equals("null".AsSpan(), StringComparison.Ordinal)
                ? null
                : Convert.ChangeType(optionalString ?? value.ToString(), expectedType.ValueType);

            reference = (ISpecDynamicValue)Activator.CreateInstance(typeof(SpecDynamicConcreteValue<>).MakeGenericType(expectedType.ValueType), val);
            return true;
        }
        catch (InvalidCastException)
        {
            reference = null!;
            return false;
        }
    }

    public static ISpecDynamicValue Read(ref Utf8JsonReader reader, JsonSerializerOptions options, SpecDynamicValueContext context, ISpecPropertyType? expectedType = null)
    {
        while (reader.TokenType == JsonTokenType.Comment && reader.Read()) ;

        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            case JsonTokenType.PropertyName:
                string str = reader.GetString()!;
                if (!TryParse(str, context, expectedType, out ISpecDynamicValue reference))
                    throw new JsonException("Failed to parse ISpecDynamicValue from a string argument.");

                return reference;

            case JsonTokenType.Null:
            case JsonTokenType.True:
            case JsonTokenType.False:
            case JsonTokenType.Number:
                return ReadValue(ref reader, expectedType, (t, type) => t != null
                    ? throw new JsonException($"Failed to parse ISpecDynamicValue from an argument, expected type \"{type.Type}\" but was given type {t}.")
                    : throw new JsonException($"Failed to parse ISpecDynamicValue from an argument, expected type \"{type.Type}\".")
                );
        }

        throw new JsonException($"Unexpected token type {reader.TokenType} when parsing ISpecDynamicValue from a string argument.");
    }

    public static ISpecDynamicValue ReadValue(ref Utf8JsonReader reader, ISpecPropertyType? expectedType, Func<Type?, ISpecPropertyType, ISpecDynamicValue> invalidTypeThrowHandler)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.False:
                return expectedType == null || expectedType.As<bool>() != null ? False : invalidTypeThrowHandler(typeof(bool), expectedType);

            case JsonTokenType.True:
                return expectedType == null || expectedType.As<bool>() != null ? True : invalidTypeThrowHandler(typeof(bool), expectedType);

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
                if (valueType == typeof(DateTime))
                {
                    return reader.TryGetInt32(out int i4) && i4 == 0
                        ? new SpecDynamicConcreteValue<DateTime>(DateTime.MinValue, expectedType.As<DateTime>() ?? KnownTypes.DateTime)
                        : invalidTypeThrowHandler(typeof(double), expectedType);
                }
                if (valueType == typeof(DateTimeOffset))
                {
                    return reader.TryGetInt32(out int i4) && i4 == 0
                        ? new SpecDynamicConcreteValue<DateTimeOffset>(DateTimeOffset.MinValue, expectedType.As<DateTimeOffset>() ?? KnownTypes.DateTimeOffset)
                        : invalidTypeThrowHandler(typeof(double), expectedType);
                }
                if (valueType == typeof(char))
                {
                    return reader.TryGetUInt16(out ushort u2) ? new SpecDynamicConcreteValue<char>((char)u2, expectedType.As<char>() ?? KnownTypes.Character) : invalidTypeThrowHandler(typeof(double), expectedType);
                }

                break;

            case JsonTokenType.String:

                if (expectedType == null)
                {
                    if (reader.TryGetDateTime(out DateTime dt))
                    {
                        return new SpecDynamicConcreteValue<DateTime>(dt, KnownTypes.DateTime);
                    }
                    if (reader.TryGetGuid(out Guid guid))
                    {
                        return Guid(guid);
                    }
                    if (reader.TryGetDateTimeOffset(out DateTimeOffset dtOffset))
                    {
                        return new SpecDynamicConcreteValue<DateTimeOffset>(dtOffset, KnownTypes.DateTimeOffset);
                    }

                    return String(reader.GetString(), expectedType?.As<string>() ?? KnownTypes.String);
                }

                valueType = expectedType.ValueType;
                if (valueType == typeof(string))
                {
                    return String(reader.GetString(), expectedType.As<string>() ?? KnownTypes.String);
                }
                if (valueType == typeof(Guid))
                {
                    return reader.TryGetGuid(out Guid guid) ? Guid(guid, expectedType.As<Guid>() ?? KnownTypes.Guid) : invalidTypeThrowHandler(typeof(string), expectedType);
                }
                if (valueType == typeof(DateTime))
                {
                    return reader.TryGetDateTime(out DateTime dt) ? new SpecDynamicConcreteValue<DateTime>(dt, expectedType.As<DateTime>() ?? KnownTypes.DateTime) : invalidTypeThrowHandler(typeof(string), expectedType);
                }
                if (valueType == typeof(DateTimeOffset))
                {
                    return reader.TryGetDateTimeOffset(out DateTimeOffset dt) ? new SpecDynamicConcreteValue<DateTimeOffset>(dt, expectedType.As<DateTimeOffset>() ?? KnownTypes.DateTimeOffset) : invalidTypeThrowHandler(typeof(string), expectedType);
                }
                if (valueType == typeof(char))
                {
                    string str = reader.GetString();
                    if (str.Length != 1)
                        invalidTypeThrowHandler(typeof(string), expectedType);

                    return new SpecDynamicConcreteValue<char>(str[0], expectedType.As<char>() ?? KnownTypes.Character);
                }
                if (expectedType is IStringParseableSpecPropertyType stringParseable)
                {
                    string str = reader.GetString();
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