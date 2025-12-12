using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class DateTimeOffsetTypeConverter : ITypeConverter<DateTimeOffset>
{
    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<DateTimeOffset> args, out DateTimeOffset parsedValue)
    {
        bool s = DateTimeOffset.TryParse(
            args.StringOrSpan(text),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out DateTimeOffset value
        );
        parsedValue = value.ToUniversalTime();
        return s;
    }

    public string Format(DateTimeOffset value, ref TypeConverterFormatArgs args)
    {
        return value.ToString("O", CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, DateTimeOffset value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (args.FormatCache != null)
        {
            size = args.FormatCache.Length;
            return args.FormatCache.AsSpan().TryCopyTo(output);
        }

        if (value.TryFormat(output, out size, "O", formatProvider: CultureInfo.InvariantCulture))
        {
            return true;
        }

        string str = value.ToString("O", formatProvider: CultureInfo.InvariantCulture);
        size = str.Length;
        args.FormatCache = str;
        return false;
#else
        string str = args.FormatCache ?? value.ToString("O", CultureInfo.InvariantCulture);
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
#endif
    }

    public override bool Equals(object? obj) => obj is DateTimeOffsetTypeConverter;
    public override int GetHashCode() => 605164845;

    public bool TryConvertTo<TTo>(Optional<DateTimeOffset> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(DateTimeOffset))
        {
            result = Unsafe.As<Optional<DateTimeOffset>, Optional<TTo>>(ref obj);
            return true;
        }

        DateTimeOffset value = obj.Value;

        if (typeof(TTo) == typeof(bool))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != DateTimeOffset.MinValue);
            return true;
        }
        if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString("O"));
            return true;
        }
        if (typeof(TTo) == typeof(DateTime))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<DateTime, TTo>(value.DateTime);
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }
}