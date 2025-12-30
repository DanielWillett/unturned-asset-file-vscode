using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class Float128TypeConverter : ITypeConverter<decimal>
{
    public IType<decimal> DefaultType => Float128Type.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<decimal> args, out decimal parsedValue)
    {
        return decimal.TryParse(args.StringOrSpan(text), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }

    public string Format(decimal value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, decimal value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (args.FormatCache != null)
        {
            size = args.FormatCache.Length;
            return args.FormatCache.AsSpan().TryCopyTo(output);
        }

        if (value.TryFormat(output, out size, provider: CultureInfo.InvariantCulture))
        {
            return true;
        }

        string str = value.ToString(CultureInfo.InvariantCulture);
        size = str.Length;
        args.FormatCache = str;
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

    public override bool Equals(object? obj) => obj is Float128TypeConverter;
    public override int GetHashCode() => 497308020;

    public bool TryConvertTo<TTo>(Optional<decimal> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(decimal))
        {
            result = Unsafe.As<Optional<decimal>, Optional<TTo>>(ref obj);
            return true;
        }

        decimal value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != 0);
                return true;
            }

            if (typeof(TTo) == typeof(int))
            {
                if (value is < int.MinValue or > int.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>((int)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(long))
            {
                if (value is < long.MinValue or > long.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>((long)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(nint))
            {
                if (IntPtr.Size == 8)
                {
                    if (value is < long.MinValue or > long.MaxValue)
                    {
                        result = Optional<TTo>.Null;
                        return false;
                    }
                }
                else if (value is < int.MinValue or > int.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>((nint)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(short))
            {
                if (value is < short.MinValue or > short.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>((short)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                if (value is < sbyte.MinValue or > sbyte.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>((sbyte)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(ulong))
            {
                if (value is < ulong.MinValue or > ulong.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TTo>((ulong)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(uint))
            {
                if (value is < uint.MinValue or > uint.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>((uint)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                if (UIntPtr.Size == 8)
                {
                    if (value is < ulong.MinValue or > ulong.MaxValue)
                    {
                        result = Optional<TTo>.Null;
                        return false;
                    }
                }
                else if (value is < uint.MinValue or > uint.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>((nuint)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(ushort))
            {
                if (value is < ushort.MinValue or > ushort.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>((ushort)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                if (value is < ushort.MinValue or > ushort.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId((ushort)decimal.Round(value)));
                return true;
            }

            if (typeof(TTo) == typeof(byte))
            {
                if (value is < byte.MinValue or > byte.MaxValue)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<byte, TTo>((byte)decimal.Round(value));
                return true;
            }

            if (typeof(TTo) == typeof(float))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<float, TTo>((float)value);
                return true;
            }

            if (typeof(TTo) == typeof(double))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<double, TTo>((double)value);
                return true;
            }

            if (typeof(TTo) == typeof(char))
            {
                if (value is < 0 or >= 10)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>((char)((int)decimal.Round(value) % 10 + '0'));
                return true;
            }
        }
        else if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        if (VectorTypes.TryConvertToVector<decimal, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, decimal value, ref TypeConverterFormatArgs args)
    {
        writer.WriteNumberValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<decimal> value, ref TypeConverterParseArgs<decimal> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<decimal>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (decimal.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
                {
                    value = v;
                    return true;
                }

                goto default;

            case JsonValueKind.Number:
                if (json.TryGetDecimal(out v))
                {
                    value = v;
                    return true;
                }

                goto default;

            default:
                value = Optional<decimal>.Null;
                return false;
        }
    }
}