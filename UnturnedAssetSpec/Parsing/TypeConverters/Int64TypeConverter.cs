using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class Int64TypeConverter : ITypeConverter<long>
{
    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<long> args, out long parsedValue)
    {
        return long.TryParse(args.StringOrSpan(text), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }

    public string Format(long value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, long value, out int size, ref TypeConverterFormatArgs args)
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

    public override bool Equals(object? obj) => obj is Int64TypeConverter;
    public override int GetHashCode() => 324202715;

    public bool TryConvertTo<TTo>(Optional<long> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(long))
        {
            result = Unsafe.As<Optional<long>, Optional<TTo>>(ref obj);
            return true;
        }

        long value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != 0);
                return true;
            }

            if (typeof(TTo) == typeof(int))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>((int)value);
                return value is >= int.MinValue and <= int.MaxValue;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>((nint)value);
                return IntPtr.Size == 8 || value is >= int.MinValue and <= int.MaxValue;
            }

            if (typeof(TTo) == typeof(short))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>((short)value);
                return value is >= short.MinValue and <= short.MaxValue;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>((sbyte)value);
                return value is >= sbyte.MinValue and <= sbyte.MaxValue;
            }

            if (typeof(TTo) == typeof(ulong))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TTo>((ulong)value);
                return value >= 0;
            }

            if (typeof(TTo) == typeof(uint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>((uint)value);
                return value is >= 0 and <= uint.MaxValue;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>((nuint)value);
                return value >= 0 && (UIntPtr.Size == 8 || value <= uint.MaxValue);
            }

            if (typeof(TTo) == typeof(ushort))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>((ushort)value);
                return value is >= 0 and <= ushort.MaxValue;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId((ushort)value));
                return value is >= 0 and <= ushort.MaxValue;
            }

            if (typeof(TTo) == typeof(byte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<byte, TTo>((byte)value);
                return value is >= 0 and <= byte.MaxValue;
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
                result = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>((char)(Math.Abs(value % 10) + '0'));
                return value is >= 0 and < 10;
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

        if (VectorConversionHelper.TryConvertToVector<long, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, long value, ref TypeConverterFormatArgs args)
    {
        writer.WriteNumberValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<long> value, ref TypeConverterParseArgs<long> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<long>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out long v))
                {
                    value = v;
                    return true;
                }

                goto default;

            case JsonValueKind.Number:
                if (json.TryGetInt64(out v))
                {
                    value = v;
                    return true;
                }

                goto default;

            default:
                value = Optional<long>.Null;
                return false;
        }
    }
}