using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class IPv4FilterTypeConverter : ITypeConverter<IPv4Filter>
{
    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<IPv4Filter> args, out IPv4Filter parsedValue)
    {
        return IPv4Filter.TryParse(text, out parsedValue);
    }

    public string Format(IPv4Filter value, ref TypeConverterFormatArgs args)
    {
        return value.ToString();
    }

    public bool TryFormat(Span<char> output, IPv4Filter value, out int size, ref TypeConverterFormatArgs args)
    {
        return value.TryFormat(output, out size);
    }

    public override bool Equals(object? obj) => obj is IPv4FilterTypeConverter;
    public override int GetHashCode() => 408628152;

    public bool TryConvertTo<TTo>(Optional<IPv4Filter> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(IPv4Filter))
        {
            result = Unsafe.As<Optional<IPv4Filter>, Optional<TTo>>(ref obj);
            return true;
        }

        IPv4Filter value = obj.Value;

        if (typeof(TTo) == typeof(bool))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(value != IPv4Filter.All);
            return true;
        }
        if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value.ToString());
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, IPv4Filter value, ref TypeConverterFormatArgs args)
    {
        writer.WriteStringValue(value.ToString());
    }

    public bool TryReadJson(in JsonElement json, out Optional<IPv4Filter> value, ref TypeConverterParseArgs<IPv4Filter> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<IPv4Filter>.Null;
                return true;

            case JsonValueKind.String:
                if (IPv4Filter.TryParse(json.GetString()!, out IPv4Filter dt))
                {
                    value = dt;
                    return true;
                }

                goto default;

            default:
                value = Optional<IPv4Filter>.Null;
                return false;
        }
    }
}