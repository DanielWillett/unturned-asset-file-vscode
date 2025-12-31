using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class StringTypeConverter : ITypeConverter<string>
{
    public IType<string> DefaultType => StringType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<string> args, [NotNullWhen(true)] out string? parsedValue)
    {
        parsedValue = args.GetString(text);
        return true;
    }

    public string Format(string value, ref TypeConverterFormatArgs args)
    {
        return value;
    }

    public bool TryFormat(Span<char> output, string value, out int size, ref TypeConverterFormatArgs args)
    {
        size = value.Length;
        if (output.Length < value.Length)
            return false;

        value.AsSpan().CopyTo(output);
        return true;
    }

    public override bool Equals(object? obj) => obj is StringTypeConverter;
    public override int GetHashCode() => 1637896321;

    public bool TryConvertTo<TTo>(Optional<string> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(string))
        {
            result = Unsafe.As<Optional<string>, Optional<TTo>>(ref obj);
            return true;
        }

        string value = obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                return ConvertBool(value, out result);
            }

            if (typeof(TTo) == typeof(int))
            {
                return ConvertI4(value, out result);
            }

            if (typeof(TTo) == typeof(nint))
            {
                return ConvertI(value, out result);
            }

            if (typeof(TTo) == typeof(short))
            {
                return ConvertI2(value, out result);
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                return ConvertI1(value, out result);
            }

            if (typeof(TTo) == typeof(long))
            {
                return ConvertI8(value, out result);
            }

            if (typeof(TTo) == typeof(uint))
            {
                return ConvertU4(value, out result);
            }

            if (typeof(TTo) == typeof(nuint))
            {
                return ConvertU(value, out result);
            }

            if (typeof(TTo) == typeof(ushort))
            {
                return ConvertU2(value, out result);
            }

            if (typeof(TTo) == typeof(GuidOrId))
            {
                return ConvertGuidOrId(value, out result);
            }

            if (typeof(TTo) == typeof(IPv4Filter))
            {
                return ConvertIPv4Filter(value, out result);
            }

            if (typeof(TTo) == typeof(BundleReference))
            {
                return ConvertBundleReference(value, out result);
            }

            if (typeof(TTo) == typeof(QualifiedType))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<QualifiedType, TTo>(new QualifiedType(value, isCaseInsensitive: true));
                return true;
            }

            if (typeof(TTo) == typeof(QualifiedOrAliasedType))
            {
                result = SpecDynamicExpressionTreeValueHelpers.As<QualifiedOrAliasedType, TTo>(QualifiedOrAliasedType.FromType(value));
                return true;
            }

            if (typeof(TTo) == typeof(byte))
            {
                return ConvertU1(value, out result);
            }

            if (typeof(TTo) == typeof(ulong))
            {
                return ConvertU8(value, out result);
            }

            if (typeof(TTo) == typeof(float))
            {
                return ConvertR4(value, out result);
            }

            if (typeof(TTo) == typeof(double))
            {
                return ConvertR8(value, out result);
            }

            if (typeof(TTo) == typeof(char))
            {
                return ConvertChar(value, out result);
            }
        }
        else if (typeof(TTo) == typeof(decimal))
        {
            return ConvertR16(value, out result);
        }

        if (VectorTypes.TryConvertToVector<string, TTo>(value, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    private static bool ConvertBool<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        bool parsed;
        if (value.Length == 1)
        {
            switch (value[0])
            {
                case 't' or 'T' or 'y' or 'Y' or '1':
                    parsed = true;
                    break;

                case 'f' or 'F' or 'n' or 'N' or '0' or '\0':
                    parsed = false;
                    break;

                default:
                    result = Optional<TTo>.Null;
                    return false;
            }
        }
        else if (!bool.TryParse(value, out parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(parsed);
        return true;
    }

    private static bool ConvertChar<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (value.Length != 1)
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>(value[0]);
        return true;
    }

    private static bool ConvertI8<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out long parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<long, TTo>(parsed);
        return true;
    }

    private static bool ConvertI4<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<int, TTo>(parsed);
        return true;
    }

    private static bool ConvertI2<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out short parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<short, TTo>(parsed);
        return true;
    }

    private static bool ConvertI1<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!sbyte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out sbyte parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TTo>(parsed);
        return true;
    }

    private static bool ConvertI<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        nint v;
        if (IntPtr.Size == 8)
        {
            if (!long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out long parsed))
            {
                result = Optional<TTo>.Null;
                return false;
            }

            v = (nint)parsed;
        }
        else
        {
            if (!int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int parsed))
            {
                result = Optional<TTo>.Null;
                return false;
            }

            v = parsed;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<nint, TTo>(v);
        return true;
    }

    private static bool ConvertU8<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out ulong parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TTo>(parsed);
        return true;
    }

    private static bool ConvertU4<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out uint parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<uint, TTo>(parsed);
        return true;
    }

    private static bool ConvertU2<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out ushort parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TTo>(parsed);
        return true;
    }

    private static bool ConvertGuidOrId<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out ushort parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId(parsed));
        return true;
    }

    private static bool ConvertIPv4Filter<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!IPv4Filter.TryParse(value, out IPv4Filter parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<IPv4Filter, TTo>(parsed);
        return true;
    }

    private static bool ConvertBundleReference<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!BundleReference.TryParse(value, out BundleReference parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<BundleReference, TTo>(parsed);
        return true;
    }

    private static bool ConvertU1<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out byte parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<byte, TTo>(parsed);
        return true;
    }

    private static bool ConvertU<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        nuint v;
        if (IntPtr.Size == 8)
        {
            if (!ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out ulong parsed))
            {
                result = Optional<TTo>.Null;
                return false;
            }

            v = (nuint)parsed;
        }
        else
        {
            if (!uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out uint parsed))
            {
                result = Optional<TTo>.Null;
                return false;
            }

            v = parsed;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<nuint, TTo>(v);
        return true;
    }

    private static bool ConvertR16<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<decimal, TTo>(parsed);
        return true;
    }

    private static bool ConvertR8<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<double, TTo>(parsed);
        return true;
    }

    private static bool ConvertR4<TTo>(string value, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsed))
        {
            result = Optional<TTo>.Null;
            return false;
        }

        result = SpecDynamicExpressionTreeValueHelpers.As<float, TTo>(parsed);
        return true;
    }

    public void WriteJson(Utf8JsonWriter writer, string value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }

    public bool TryReadJson(in JsonElement json, out Optional<string> value, ref TypeConverterParseArgs<string> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<string>.Null;
                return true;

            case JsonValueKind.String:
                value = json.GetString()!;
                return true;

            case JsonValueKind.False:
            case JsonValueKind.True:
            case JsonValueKind.Number:
                value = json.GetRawText();
                return true;

            default:
                value = Optional<string>.Null;
                return false;
        }
    }
}