using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// String-parseable type converter for <c>SDG.Unturned.ENPCLogicType, Assembly-CSharp</c> to parse via operators.
/// </summary>
[StringParseableType]
// ReSharper disable once InconsistentNaming
internal sealed class ENPCLogicTypeConverter : ITypeConverter<DatEnumValue>
{
    private readonly DatEnumType _type;

    public IType<DatEnumValue> DefaultType => _type;

    public ENPCLogicTypeConverter(DatEnumType type)
    {
        _type = type;
    }

    public bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<DatEnumValue> args, [MaybeNullWhen(false)] out DatEnumValue parsedValue)
    {
        parsedValue = null;
        if (text.Length < 2)
            return false;

        int index;
        switch (text[0])
        {
            case '<' when text.Length == 1:
                index = 1; // LESS_THAN
                break;
            case '<' when text.Length == 2 && text[1] == '=':
            case '≤' when text.Length == 1:
                index = 2; // LESS_THAN_OR_EQUAL_TO
                break;

            case '=' when text.Length == 2 && text[1] == '=':
            case '=' when text.Length == 1:
                index = 3; // EQUAL
                break;

            case '!' when text.Length == 2 && text[1] == '=':
            case '≠' when text.Length == 1:
                index = 4; // NOT_EQUAL
                break;

            case '>' when text.Length == 2 && text[1] == '=':
            case '≥' when text.Length == 1:
                index = 5; // GREATER_THAN_OR_EQUAL_TO
                break;
            case '>' when text.Length == 1:
                index = 6; // GREATER_THAN
                break;

            default:
                return false;
        }

        if (_type.Values.Length > index)
        {
            parsedValue = _type.Values[index];
            return true;
        }

        return false;
    }

    public string Format(DatEnumValue value, ref TypeConverterFormatArgs args)
    {
        return ((ITypeConverter<DatEnumValue>)_type).Format(value, ref args);
    }

    public bool TryFormat(Span<char> output, DatEnumValue value, out int size, ref TypeConverterFormatArgs args)
    {
        return ((ITypeConverter<DatEnumValue>)_type).TryFormat(output, value, out size, ref args);
    }

    public bool TryConvertTo<TTo>(Optional<DatEnumValue> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>
    {
        return ((ITypeConverter<DatEnumValue>)_type).TryConvertTo(obj, out result);
    }

    public void WriteJson(Utf8JsonWriter writer, DatEnumValue value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        ((ITypeConverter<DatEnumValue>)_type).WriteJson(writer, value, ref args, options);
    }

    public bool TryReadJson(in JsonElement json, out Optional<DatEnumValue> value, ref TypeConverterParseArgs<DatEnumValue> args)
    {
        return ((ITypeConverter<DatEnumValue>)_type).TryReadJson(in json, out value, ref args);
    }
}
