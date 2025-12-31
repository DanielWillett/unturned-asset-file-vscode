using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class BooleanTypeConverter : ITypeConverter<bool>
{
    public IType<bool> DefaultType => BooleanType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<bool> args, out bool parsedValue)
    {
        switch (text.Length)
        {
            case 0 or 2 or 3:
                parsedValue = false;
                return false;

            case 1:
                switch (text[0])
                {
                    // this is not case-insensitive in-game either
                    case '0' or 'f' or 'n':
                        parsedValue = false;
                        return true;

                    case '1' or 't' or 'y':
                        parsedValue = true;
                        return true;

                    case 'F' or 'N' or 'T' or 'Y':
                        args.DiagnosticSink?.UNT2004_BooleanSingleCharCapitalized(ref args, args.GetString(text), args.Type);
                        break;
                }

                parsedValue = false;
                return false;

            case 4:
                if (text[0] is 't' or 'T' && text[1] is 'r' or 'R' && text[2] is 'u' or 'U' && text[3] is 'e' or 'E')
                {
                    parsedValue = true;
                    return true;
                }

                break;

            case 5:
                if (text[0] is 'f' or 'F' && text[1] is 'a' or 'A' && text[2] is 'l' or 'L' && text[3] is 's' or 'S' && text[4] is 'e' or 'E')
                {
                    parsedValue = false;
                    return true;
                }

                break;
        }

        return bool.TryParse(args.StringOrSpan(text), out parsedValue);
    }

    public string Format(bool value, ref TypeConverterFormatArgs args)
    {
        return value ? "true" : "false";
    }

    public bool TryFormat(Span<char> output, bool value, out int size, ref TypeConverterFormatArgs args)
    {
        int l = 4 + (!value ? 1 : 0);
        size = l;
        if (output.Length < l)
            return false;

#if NET7_0_OR_GREATER
        if (value)
        {
            ReadOnlySpan<char> @true = [ 't', 'r', 'u', 'e' ];
            @true.CopyTo(output);
        }
        else
        {
            ReadOnlySpan<char> @false = [ 'f', 'a', 'l', 's', 'e' ];
            @false.CopyTo(output);
        }
#else
        if (value)
        {
            output[0] = 't';
            output[1] = 'r';
            output[2] = 'u';
            output[3] = 'e';
        }
        else
        {
            output[0] = 'f';
            output[1] = 'a';
            output[2] = 'l';
            output[3] = 's';
            output[4] = 'e';
        }
#endif

        return true;
    }

    public override bool Equals(object? obj) => obj is BooleanTypeConverter;
    public override int GetHashCode() => 333748762;

    public bool TryConvertTo<TTo>(Optional<bool> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(bool))
        {
            result = Unsafe.As<Optional<bool>, Optional<TTo>>(ref obj);
            return true;
        }

        bool value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(int))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>(value ? 1 : 0);
                return true;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>(value ? 1 : 0);
                return true;
            }

            if (typeof(TTo) == typeof(short))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>(value ? (short)1 : (short)0);
                return true;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>(value ? (sbyte)1 : (sbyte)0);
                return true;
            }

            if (typeof(TTo) == typeof(long))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>(value ? 1L : 0L);
                return true;
            }

            if (typeof(TTo) == typeof(uint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>(value ? 1u : 0u);
                return true;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>(value ? 1u : 0u);
                return true;
            }

            if (typeof(TTo) == typeof(ushort))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>(value ? (ushort)1 : (ushort)0);
                return true;
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId(value ? (ushort)1 : (ushort)0));
                return true;
            }

            if (typeof(TTo) == typeof(byte))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<byte, TTo>(value ? (byte)1 : (byte)0);
                return true;
            }

            if (typeof(TTo) == typeof(ulong))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TTo>(value ? 1ul : 0ul);
                return true;
            }

            if (typeof(TTo) == typeof(float))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<float, TTo>(value ? 1f : 0f);
                return true;
            }

            if (typeof(TTo) == typeof(double))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<double, TTo>(value ? 1d : 0d);
                return true;
            }

            if (typeof(TTo) == typeof(char))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>(value ? '1' : '0');
                return true;
            }
        }
        else if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(value ? "true" : "false");
            return true;
        }
        else if (typeof(TTo) == typeof(decimal))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<decimal, TTo>(value ? decimal.One : decimal.Zero);
            return true;
        }

        if (VectorTypes.TryConvertToVector<bool, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, bool value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<bool> value, ref TypeConverterParseArgs<bool> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<bool>.Null;
                return true;

            case JsonValueKind.String:
                if (TryParse(json.GetString()!, ref args, out bool parsedValue))
                {
                    value = parsedValue;
                    return true;
                }

                goto default;

            case JsonValueKind.False:
                value = false;
                return true;

            case JsonValueKind.True:
                value = true;
                return true;

            case JsonValueKind.Number:
                if (json.TryGetDouble(out double d))
                {
                    value = d != 0;
                    return true;
                }

                goto default;

            default:
                value = Optional<bool>.Null;
                return false;
        }
    }
}