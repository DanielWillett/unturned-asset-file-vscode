using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class UInt16TypeConverter : ITypeConverter<ushort>
{
    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<ushort> args, out ushort parsedValue)
    {
        return ushort.TryParse(args.StringOrSpan(text), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }

    public string Format(ushort value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, ushort value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (value.TryFormat(output, out size, provider: CultureInfo.InvariantCulture))
        {
            return true;
        }

        size = StringHelper.CountDigits(value);
        return false;
#else
        string str = args.FormatCache ?? value.ToString(CultureInfo.InvariantCulture);
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
#endif
    }

    public override bool Equals(object? obj) => obj is UInt16TypeConverter;
    public override int GetHashCode() => 207360682;

    public bool TryConvertTo<TTo>(Optional<ushort> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(ushort))
        {
            result = Unsafe.As<Optional<ushort>, Optional<TTo>>(ref obj);
            return true;
        }

        ushort value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != 0);
                return true;
            }

            if (typeof(TTo) == typeof(int))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(short))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>((short)value);
                return value <= short.MaxValue;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>((sbyte)value);
                return value <= sbyte.MaxValue;
            }

            if (typeof(TTo) == typeof(long))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(ulong))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(uint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId(value));
                return true;
            }

            if (typeof(TTo) == typeof(byte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<byte, TTo>((byte)value);
                return value <= byte.MaxValue;
            }

            if (typeof(TTo) == typeof(float))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<float, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(double))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<double, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(char))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>((char)(value % 10 + '0'));
                return value < 10;
            }
        }
        else if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString(CultureInfo.InvariantCulture));
            return true;
        }
        else if (typeof(TTo) == typeof(decimal))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<decimal, TTo>(value);
            return true;
        }

        if (VectorConversionHelper.TryConvertToVector<uint, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, ushort value, ref TypeConverterFormatArgs args)
    {
        writer.WriteNumberValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<ushort> value, ref TypeConverterParseArgs<ushort> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<ushort>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (ushort.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out ushort v))
                {
                    value = v;
                    return true;
                }

                goto default;

            case JsonValueKind.Number:
                if (json.TryGetUInt16(out v))
                {
                    value = v;
                    return true;
                }

                goto default;

            default:
                value = Optional<ushort>.Null;
                return false;
        }
    }
}