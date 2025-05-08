using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface ISpecDynamicValue
{
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

    public static SpecDynamicConcreteValue<bool> True { get; } = new SpecDynamicConcreteValue<bool>(true);
    public static SpecDynamicConcreteValue<bool> False { get; } = new SpecDynamicConcreteValue<bool>(false);

    public static SpecDynamicConcreteValue<byte> UInt8(byte v) => new SpecDynamicConcreteValue<byte>(v);
    public static SpecDynamicConcreteValue<ushort> UInt16(ushort v) => new SpecDynamicConcreteValue<ushort>(v);
    public static SpecDynamicConcreteValue<uint> UInt32(uint v) => new SpecDynamicConcreteValue<uint>(v);
    public static SpecDynamicConcreteValue<ulong> UInt64(ulong v) => new SpecDynamicConcreteValue<ulong>(v);
    public static SpecDynamicConcreteValue<sbyte> Int8(sbyte v) => new SpecDynamicConcreteValue<sbyte>(v);
    public static SpecDynamicConcreteValue<short> Int16(short v) => new SpecDynamicConcreteValue<short>(v);
    public static SpecDynamicConcreteValue<int> Int32(int v) => new SpecDynamicConcreteValue<int>(v);
    public static SpecDynamicConcreteValue<long> Int64(long v) => new SpecDynamicConcreteValue<long>(v);
    public static SpecDynamicConcreteValue<float> Float32(float v) => new SpecDynamicConcreteValue<float>(v);
    public static SpecDynamicConcreteValue<double> Float64(double v) => new SpecDynamicConcreteValue<double>(v);
    public static SpecDynamicConcreteValue<decimal> Float128(decimal v) => new SpecDynamicConcreteValue<decimal>(v);
    public static SpecDynamicConcreteValue<string> String(string v) => new SpecDynamicConcreteValue<string>(v);

    public static SpecDynamicConcreteEnumValue Enum(EnumSpecType type, int value) => new SpecDynamicConcreteEnumValue(type, value);
    public static SpecDynamicConcreteEnumValue Enum(EnumSpecTypeValue value) => new SpecDynamicConcreteEnumValue(value.Type, value.Index);

    public static bool TryParse(string value, SpecDynamicValueContext context, ISpecPropertyType expectedType, out ISpecDynamicValue reference)
    {
        if (!string.IsNullOrEmpty(value))
            return TryParse(value.AsSpan(), value, context, expectedType, out reference);

        reference = null!;
        return false;
    }

    public static bool TryParse(ReadOnlySpan<char> value, SpecDynamicValueContext context, ISpecPropertyType expectedType, out ISpecDynamicValue reference)
    {
        if (!value.IsEmpty)
            return TryParse(value, null, context, expectedType, out reference);

        reference = null!;
        return false;

    }

    private static bool TryParse(ReadOnlySpan<char> value, string? optionalString, SpecDynamicValueContext context, ISpecPropertyType expectedType, out ISpecDynamicValue reference)
    {
        reference = null!;
        // #(bang) or @(prop), or #bang, @prop
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
        reference = null!;
        return false;
    }

    private static bool TryParseBangRef(ReadOnlySpan<char> value, string? optionalString, out ISpecDynamicValue reference)
    {
        reference = null!;
        return false;
    }

    private static bool TryParseValue(ReadOnlySpan<char> value, string? optionalString, ISpecPropertyType expectedType, out ISpecDynamicValue reference)
    {
        if (expectedType is ISpecPropertyType<string>)
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
            object val = Convert.ChangeType(optionalString ?? value.ToString(), expectedType.ValueType);
            reference = (ISpecDynamicValue)Activator.CreateInstance(typeof(SpecDynamicConcreteValue<>).MakeGenericType(expectedType.ValueType), val);
            return true;
        }
        catch (InvalidCastException)
        {
            reference = null!;
            return false;
        }
    }
}