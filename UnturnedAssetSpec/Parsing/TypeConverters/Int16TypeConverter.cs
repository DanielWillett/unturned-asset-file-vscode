using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class Int16TypeConverter : ITypeConverter<short>
{
    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<short> args, out short parsedValue)
    {
        return short.TryParse(args.StringOrSpan(text), NumberStyles.Any, CultureInfo.InvariantCulture, out parsedValue);
    }

    public string Format(short value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, short value, out int size, ref TypeConverterFormatArgs args)
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

    public override bool Equals(object? obj) => obj is Int16TypeConverter;
    public override int GetHashCode() => 955811054;

    public bool TryConvertTo<TTo>(Optional<short> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(short))
        {
            result = Unsafe.As<Optional<short>, Optional<TTo>>(ref obj);
            return true;
        }

        short value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != 0);
                return true;
            }

            if (typeof(TTo) == typeof(long))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>(value);
                return true;
            }

            if (typeof(TTo) == typeof(int))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>(value);
                return true;
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
                return value >= 0;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>((nuint)value);
                return value >= 0;
            }

            if (typeof(TTo) == typeof(ushort))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>((ushort)value);
                return value >= 0;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId((ushort)value));
                return value >= 0;
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
}