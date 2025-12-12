using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class DateTimeTypeConverter : ITypeConverter<DateTime>
{
    private static string GetFormat(in DateTime dt)
    {
        return dt.Hour != 0 || dt.Minute != 0 || dt.Second != 0 ? "yyyy'-'MM'-'dd HH':'mm':'ss" : "yyyy'-'MM'-'dd";
    }

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<DateTime> args, out DateTime parsedValue)
    {
        bool s = DateTime.TryParse(
            args.StringOrSpan(text),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out DateTime value
        );
        parsedValue = value.ToUniversalTime();
        return s;
    }

    public string Format(DateTime value, ref TypeConverterFormatArgs args)
    {
        return value.ToString(GetFormat(in value));
    }

    public bool TryFormat(Span<char> output, DateTime value, out int size, ref TypeConverterFormatArgs args)
    {
        string fmt = GetFormat(in value);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (value.TryFormat(output, out size, fmt, CultureInfo.InvariantCulture))
        {
            return true;
        }

        size = fmt.Length;
        return false;
#else
        if (output.Length < fmt.Length)
        {
            size = fmt.Length;
            return false;
        }

        string str = args.FormatCache ?? value.ToString(fmt, CultureInfo.InvariantCulture);
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
#endif
    }

    public override bool Equals(object? obj) => obj is DateTimeTypeConverter;
    public override int GetHashCode() => 192993432;

    public bool TryConvertTo<TTo>(Optional<DateTime> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(DateTime))
        {
            result = Unsafe.As<Optional<DateTime>, Optional<TTo>>(ref obj);
            return true;
        }

        DateTime value = obj.Value;

        if (typeof(TTo) == typeof(bool))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != DateTime.MinValue);
            return true;
        }
        if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString("O"));
            return true;
        }
        if (typeof(TTo) == typeof(DateTimeOffset))
        {
            DateTimeOffset o;
            if (value.Kind == DateTimeKind.Utc)
            {
                o = new DateTimeOffset(value);
            }
            else
            {
                try
                {
                    o = new DateTimeOffset(value);
                }
                catch (ArgumentOutOfRangeException)
                {
                    result = Optional<TTo>.Null;
                    return false;
                }
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<DateTimeOffset, TTo>(o);
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }
}