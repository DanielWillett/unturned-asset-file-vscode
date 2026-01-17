using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

internal sealed class QualifiedTypeTypeConverter : ITypeConverter<QualifiedType>
{
    public IType<QualifiedType> DefaultType => QualifiedTypeType.Instance;

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<QualifiedType> args, out QualifiedType parsedValue)
    {
        if (!QualifiedType.ExtractParts(text, out _, out _))
        {
            parsedValue = QualifiedType.None;
            return false;
        }

        parsedValue = new QualifiedType(args.GetString(text), isCaseInsensitive: true).Normalized;
        return !parsedValue.IsNull;
    }

    public string Format(QualifiedType value, ref TypeConverterFormatArgs args)
    {
        return value.IsNull ? string.Empty : value.Type;
    }

    public bool TryFormat(Span<char> output, QualifiedType value, out int size, ref TypeConverterFormatArgs args)
    {
        string str = Format(value, ref args);
        size = str.Length;
        if (output.Length < str.Length)
            return false;

        str.AsSpan().CopyTo(output);
        return true;
    }

    public override bool Equals(object? obj) => obj is QualifiedTypeTypeConverter;
    public override int GetHashCode() => 631040870;

    public bool TryConvertTo<TTo>(Optional<QualifiedType> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        if (!obj.HasValue)
        {
            result = Optional<TTo>.Null;
            return true;
        }

        if (typeof(TTo) == typeof(QualifiedType))
        {
            result = Unsafe.As<Optional<QualifiedType>, Optional<TTo>>(ref obj);
            return true;
        }

        ref readonly QualifiedType value = ref obj.Value;
        if (typeof(TTo) == typeof(string))
        {
            result = MathMatrix.As<string, TTo>(value.ToString());
            return true;
        }
        
        result = Optional<TTo>.Null;
        return false;
    }

    public void WriteJson(Utf8JsonWriter writer, QualifiedType value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        if (value.IsNull)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Type);
    }

    public bool TryReadJson(in JsonElement json, out Optional<QualifiedType> value, ref TypeConverterParseArgs<QualifiedType> args)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<QualifiedType>.Null;
                return true;

            case JsonValueKind.String:
                string str = json.GetString()!;
                if (QualifiedType.ExtractParts(str, out _, out _))
                {
                    value = new Optional<QualifiedType>(new QualifiedType(str, isCaseInsensitive: true));
                    return true;
                }

                goto default;

            default:
                value = Optional<QualifiedType>.Null;
                return false;
        }
    }
}