using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class UInt32TypeConverter : ITypeConverter<uint>
{
    public IType<uint> DefaultType => UInt32Type.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<uint> args, out uint parsedValue)
    {
        return uint.TryParse(args.StringOrSpan(text), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }

    public string Format(uint value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, uint value, out int size, ref TypeConverterFormatArgs args)
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

    public override bool Equals(object? obj) => obj is UInt32TypeConverter;
    public override int GetHashCode() => 786951669;

    public bool TryConvertTo<TTo>(Optional<uint> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(uint))
        {
            result = Unsafe.As<Optional<uint>, Optional<TTo>>(ref obj);
            return true;
        }

        uint value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                result = MathMatrix.As<bool, TTo>(value != 0);
                return true;
            }

            if (typeof(TTo) == typeof(int))
            {
                result = MathMatrix.As<int, TTo>((int)value);
                return value <= int.MaxValue;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = MathMatrix.As<nint, TTo>((nint)value);
                return UIntPtr.Size == 8 || value <= int.MaxValue;
            }

            if (typeof(TTo) == typeof(short))
            {
                result = MathMatrix.As<short, TTo>((short)value);
                return value <= (uint)short.MaxValue;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                result = MathMatrix.As<sbyte, TTo>((sbyte)value);
                return value <= (uint)sbyte.MaxValue;
            }

            if (typeof(TTo) == typeof(long))
            {
                result = MathMatrix.As<long, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(ulong))
            {
                result = MathMatrix.As<ulong, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = MathMatrix.As<nuint, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(ushort))
            {
                result = MathMatrix.As<ushort, TTo>((ushort)value);
                return value <= ushort.MaxValue;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                result = MathMatrix.As<GuidOrId, TTo>(new GuidOrId((ushort)value));
                return value <= ushort.MaxValue;
            }

            if (typeof(TTo) == typeof(byte))
            {
                result = MathMatrix.As<byte, TTo>((byte)value);
                return value <= byte.MaxValue;
            }

            if (typeof(TTo) == typeof(float))
            {
                result = MathMatrix.As<float, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(double))
            {
                result = MathMatrix.As<double, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(char))
            {
                result = MathMatrix.As<char, TTo>((char)(value % 10 + '0'));
                return value < 10;
            }
        }
        else if (typeof(TTo) == typeof(string))
        {
            result = MathMatrix.As<string, TTo>(value.ToString(CultureInfo.InvariantCulture));
            return true;
        }
        else if (typeof(TTo) == typeof(decimal))
        {
            result = MathMatrix.As<decimal, TTo>(value);
            return true;
        }

        if (VectorTypes.TryConvertToVector<uint, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, uint value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<uint> value, ref TypeConverterParseArgs<uint> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<uint>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (uint.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out uint v))
                {
                    value = v;
                    return true;
                }

                goto default;

            case JsonValueKind.Number:
                if (json.TryGetUInt32(out v))
                {
                    value = v;
                    return true;
                }

                goto default;

            default:
                value = Optional<uint>.Null;
                return false;
        }
    }
}