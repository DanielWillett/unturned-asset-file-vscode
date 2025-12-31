using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class CharacterTypeConverter : ITypeConverter<char>
{
    public IType<char> DefaultType => CharacterType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<char> args, out char parsedValue)
    {
        if (text.Length != 1)
        {
            parsedValue = '\0';
            return false;
        }

        parsedValue = text[0];
        return true;
    }

    public string Format(char value, ref TypeConverterFormatArgs args)
    {
        return new string(value, 1);
    }

    public bool TryFormat(Span<char> output, char value, out int size, ref TypeConverterFormatArgs args)
    {
        size = 1;
        if (output.IsEmpty)
            return false;

        output[0] = value;
        return true;
    }

    public override bool Equals(object? obj) => obj is CharacterTypeConverter;
    public override int GetHashCode() => 923953165;

    public bool TryConvertTo<TTo>(Optional<char> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(char))
        {
            result = Unsafe.As<Optional<char>, Optional<TTo>>(ref obj);
            return true;
        }

        char value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                bool b;
                switch (value)
                {
                    case 'f' or 'F' or 'n' or 'N' or '0' or '\0':
                        b = false;
                        break;

                    case 't' or 'T' or 'y' or 'Y' or '1':
                        b = true;
                        break;

                    default:
                        goto f;
                }

                result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(b);
                return true;
            }

            if (value is >= '0' and <= '9')
            {
                int d = value - '0';
                if (typeof(TTo) == typeof(long))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>(d);
                    return true;
                }

                if (typeof(TTo) == typeof(nint))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>(d);
                    return true;
                }

                if (typeof(TTo) == typeof(int))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>(d);
                    return true;
                }

                if (typeof(TTo) == typeof(sbyte))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>((sbyte)d);
                    return true;
                }

                if (typeof(TTo) == typeof(short))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>((short)d);
                    return true;
                }

                if (typeof(TTo) == typeof(ulong))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TTo>((ulong)d);
                    return true;
                }

                if (typeof(TTo) == typeof(uint))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>((uint)d);
                    return true;
                }

                if (typeof(TTo) == typeof(nuint))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>((nuint)d);
                    return true;
                }

                if (typeof(TTo) == typeof(ushort))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>((ushort)d);
                    return true;
                }

                if (typeof(TTo) == typeof(GuidOrId))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId((ushort)d));
                    return true;
                }

                if (typeof(TTo) == typeof(byte))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<byte, TTo>((byte)d);
                    return true;
                }

                if (typeof(TTo) == typeof(float))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<float, TTo>(d);
                    return true;
                }

                if (typeof(TTo) == typeof(double))
                {
                    result = SpecDynamicExpressionTreeValueHelpers.As<double, TTo>(d);
                    return true;
                }
            }
            else goto f;
        }
        else if (typeof(TTo) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(new string(value, 1));
            return true;
        }
        else if (typeof(TTo) == typeof(decimal))
        {
            if (value is >= '0' and <= '9')
            {
                int d = value - '0';
                result = SpecDynamicExpressionTreeValueHelpers.As<decimal, TTo>(d);
                return true;
            }

            goto f;
        }

        if (VectorTypes.TryConvertToVector<long, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        f:
        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, char value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        Span<char> span = stackalloc char[1];
        span[0] = value;
        writer.WriteStringValue(span);
    }

    public bool TryReadJson(in JsonElement json, out Optional<char> value, ref TypeConverterParseArgs<char> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<char>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (str.Length == 1)
                {
                    value = str[0];
                    return true;
                }

                goto default;

            case JsonValueKind.False:
                value = '0';
                return true;

            case JsonValueKind.True:
                value = '1';
                return true;

            case JsonValueKind.Number:
                if (json.TryGetInt32(out int v) && v is >= '0' and <= '9')
                {
                    value = new Optional<char>((char)(v - '0'));
                    return true;
                }

                goto default;

            default:
                value = Optional<char>.Null;
                return false;
        }
    }
}