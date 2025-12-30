using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class TimeSpanTypeConverter : ITypeConverter<TimeSpan>
{
    public IType<TimeSpan> DefaultType => TimeSpanType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<TimeSpan> args, out TimeSpan parsedValue)
    {
        return TimeSpan.TryParse(
            args.StringOrSpan(text),
            CultureInfo.InvariantCulture,
            out parsedValue
        );
    }

    public string Format(TimeSpan value, ref TypeConverterFormatArgs args)
    {
        return value.ToString("c", CultureInfo.InvariantCulture);
    }

    public bool TryFormat(Span<char> output, TimeSpan value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (args.FormatCache != null)
        {
            size = args.FormatCache.Length;
            return args.FormatCache.AsSpan().TryCopyTo(output);
        }

        if (value.TryFormat(output, out size, "c", formatProvider: CultureInfo.InvariantCulture))
        {
            return true;
        }

        string str = value.ToString("c", formatProvider: CultureInfo.InvariantCulture);
        size = str.Length;
        args.FormatCache = str;
        return false;
#else
        string str = args.FormatCache ?? value.ToString("c", CultureInfo.InvariantCulture);
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
#endif
    }

    public override bool Equals(object? obj) => obj is TimeSpanTypeConverter;
    public override int GetHashCode() => 107487358;

    public bool TryConvertTo<TTo>(Optional<TimeSpan> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(TimeSpan))
        {
            result = Unsafe.As<Optional<TimeSpan>, Optional<TTo>>(ref obj);
            return true;
        }

        TimeSpan value = obj.Value;

        if (typeof(TTo) == typeof(bool))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != TimeSpan.Zero);
            return true;
        }
        if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString("c"));
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, TimeSpan value, ref TypeConverterFormatArgs args)
    {
        writer.WriteStringValue(value.ToString("c"));
    }

    public bool TryReadJson(in JsonElement json, out Optional<TimeSpan> value, ref TypeConverterParseArgs<TimeSpan> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<TimeSpan>.Null;
                return true;

            case JsonValueKind.String:
                if (TimeSpan.TryParse(json.GetString()!, out TimeSpan dt))
                {
                    value = dt;
                    return true;
                }

                goto default;

            default:
                value = Optional<TimeSpan>.Null;
                return false;
        }
    }
}