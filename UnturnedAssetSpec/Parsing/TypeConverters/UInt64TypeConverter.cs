using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class UInt64TypeConverter : ITypeConverter<ulong>
{
    public IType<ulong> DefaultType => UInt64Type.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<ulong> args, out ulong parsedValue)
    {
        return ulong.TryParse(args.StringOrSpan(text), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }

    public string Format(ulong value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, ulong value, out int size, ref TypeConverterFormatArgs args)
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

    public override bool Equals(object? obj) => obj is UInt64TypeConverter;
    public override int GetHashCode() => 475326387;

    public bool TryConvertTo<TTo>(Optional<ulong> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(ulong))
        {
            result = Unsafe.As<Optional<ulong>, Optional<TTo>>(ref obj);
            return true;
        }

        ulong value = obj.Value;

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
                return value <= int.MaxValue;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>((nint)value);
                return value <= (IntPtr.Size == 8 ? long.MaxValue : (ulong)int.MaxValue);
            }

            if (typeof(TTo) == typeof(short))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>((short)value);
                return value <= (ulong)short.MaxValue;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>((sbyte)value);
                return value <= (ulong)sbyte.MaxValue;
            }

            if (typeof(TTo) == typeof(long))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>((long)value);
                return value <= long.MaxValue;
            }

            if (typeof(TTo) == typeof(uint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>((uint)value);
                return value <= uint.MaxValue;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>((nuint)value);
                return UIntPtr.Size == 8 || value <= uint.MaxValue;
            }

            if (typeof(TTo) == typeof(ushort))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>((ushort)value);
                return value <= ushort.MaxValue;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId((ushort)value));
                return value <= ushort.MaxValue;
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

        if (VectorTypes.TryConvertToVector<ulong, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, ulong value, ref TypeConverterFormatArgs args)
    {
        writer.WriteNumberValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<ulong> value, ref TypeConverterParseArgs<ulong> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<ulong>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (ulong.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out ulong v))
                {
                    value = v;
                    return true;
                }

                goto default;

            case JsonValueKind.Number:
                if (json.TryGetUInt64(out v))
                {
                    value = v;
                    return true;
                }

                goto default;

            default:
                value = Optional<ulong>.Null;
                return false;
        }
    }
}