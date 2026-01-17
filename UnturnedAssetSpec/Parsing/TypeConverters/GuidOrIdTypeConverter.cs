using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class GuidOrIdTypeConverter : ITypeConverter<GuidOrId>
{
    public IType<GuidOrId> DefaultType => GuidOrIdType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<GuidOrId> args, out GuidOrId parsedValue)
    {
        return GuidOrId.TryParse(args.StringOrSpan(text), out parsedValue);
    }

    public string Format(GuidOrId value, ref TypeConverterFormatArgs args)
    {
        if (value.IsId)
            return value.Id.ToString(CultureInfo.InvariantCulture);
        return value.Guid.ToString("N");
    }

    public bool TryFormat(Span<char> output, GuidOrId value, out int size, ref TypeConverterFormatArgs args)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        if (value.IsId)
        {
            if (value.Id.TryFormat(output, out size, provider: CultureInfo.InvariantCulture))
            {
                return true;
            }

            size = StringHelper.CountDigits(value.Id);
        }
#if NETSTANDARD2_1_OR_GREATER
        else if (Unsafe.AsRef(in value.Guid).TryFormat(output, out size, "N"))
#else
        else if (value.Guid.TryFormat(output, out size, "N"))
#endif
        {
            return true;
        }
        else
        {
            size = 32;
        }

        return false;
#else
        string str = args.FormatCache ?? Format(value, ref args);
        size = str.Length;
        if (str.AsSpan().TryCopyTo(output))
            return true;
        args.FormatCache = str;
        return false;
#endif
    }

    public override bool Equals(object? obj) => obj is GuidOrIdTypeConverter;
    public override int GetHashCode() => 2017317496;

    public bool TryConvertTo<TTo>(Optional<GuidOrId> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(GuidOrId))
        {
            result = Unsafe.As<Optional<GuidOrId>, Optional<TTo>>(ref obj);
            return true;
        }

        ref readonly GuidOrId value = ref obj.Value;

        if (typeof(TTo).IsPrimitive)
        {
            if (typeof(TTo) == typeof(bool))
            {
                result = MathMatrix.As<bool, TTo>(value.IsId ? value.Id != 0 : value.Guid != Guid.Empty);
                return true;
            }

            if (typeof(TTo) == typeof(int))
            {
                result = MathMatrix.As<int, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(nint))
            {
                result = MathMatrix.As<nint, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(short))
            {
                result = MathMatrix.As<short, TTo>((short)value.Id);
                return value.IsId && value.Id <= short.MaxValue;
            }

            if (typeof(TTo) == typeof(ushort))
            {
                result = MathMatrix.As<ushort, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(sbyte))
            {
                result = MathMatrix.As<sbyte, TTo>((sbyte)value.Id);
                return value.IsId && value.Id <= sbyte.MaxValue;
            }

            if (typeof(TTo) == typeof(long))
            {
                result = MathMatrix.As<long, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(ulong))
            {
                result = MathMatrix.As<ulong, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(nuint))
            {
                result = MathMatrix.As<nuint, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(uint))
            {
                result = MathMatrix.As<uint, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(byte))
            {
                result = MathMatrix.As<byte, TTo>((byte)value.Id);
                return value is { IsId: true, Id: <= byte.MaxValue };
            }

            if (typeof(TTo) == typeof(float))
            {
                result = MathMatrix.As<float, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(double))
            {
                result = MathMatrix.As<double, TTo>(value.Id);
                return value.IsId;
            }

            if (typeof(TTo) == typeof(char))
            {
                result = MathMatrix.As<char, TTo>((char)(value.Id % 10 + '0'));
                return value is { IsId: true, Id: < 10 };
            }
        }
        else if (typeof(TTo) == typeof(string))
        {
            result = MathMatrix.As<string, TTo>(value.ToString());
            return true;
        }
        else if (typeof(TTo) == typeof(decimal))
        {
            result = MathMatrix.As<decimal, TTo>(value.Id);
            return value.IsId;
        }

        if (value.IsId && VectorTypes.TryConvertToVector<ushort, TTo>(value.Id, out TTo? parsedVector))
        {
            result = parsedVector;
            return true;
        }

        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, GuidOrId value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        if (value.IsId)
            writer.WriteNumberValue(value.Id);
        else
            writer.WriteStringValue(value.Guid);
    }

    public bool TryReadJson(in JsonElement json, out Optional<GuidOrId> value, ref TypeConverterParseArgs<GuidOrId> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<GuidOrId>.Null;
                return true;

            case JsonValueKind.String:
                if (json.TryGetGuid(out Guid guid))
                {
                    value = new GuidOrId(guid);
                    return true;
                }

                string str = json.GetString()!;
                if (GuidOrId.TryParse(str, out GuidOrId v))
                {
                    value = v;
                    return true;
                }

                goto default;

            case JsonValueKind.Number:
                if (json.TryGetUInt16(out ushort id))
                {
                    value = new GuidOrId(id);
                    return true;
                }

                goto default;

            default:
                value = Optional<GuidOrId>.Null;
                return false;
        }
    }
}