using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A 32 bit integer representing a SteamItemDef_t value (Steam inventory item ID).
/// <para>Valid values fall between 1 and 999,999,999 inclusively.</para>
/// <para>Example: <c>ItemBoxAsset.Generate</c></para>
/// <code>
/// Prop 52400
/// </code>
/// </summary>
public sealed class SteamItemDefType : PrimitiveType<int, SteamItemDefType>, ITypeConverter<int>, ITypeParser<int>
{
    public const string TypeId = "SteamItemDef";

    public const int MinValue = 1;
    public const int MaxValue = 999_999_999;

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_SteamItemDef;

    public override ITypeParser<int> Parser => this;

    /// <summary>
    /// The converter used to parse string values for this type.
    /// </summary>
    public ITypeConverter<int> Converter => this;

    public override int GetHashCode() => 1048421756;

    bool ITypeConverter<int>.TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<int> args, out int parsedValue)
    {
        if (!TypeConverters.Int32.TryParse(text, ref args, out parsedValue))
            return false;

        switch (parsedValue)
        {
            case < MinValue:
                args.DiagnosticSink?.UNT1028_MinimumInclusive(ref args, null, parsedValue.ToString("N"), MinValue.ToString("N"));
                break;

            case > MaxValue:
                args.DiagnosticSink?.UNT1028_MaximumInclusive(ref args, null, parsedValue.ToString("N"), MaxValue.ToString("N"));
                break;
        }

        return true;
    }

    bool ITypeParser<int>.TryParse(ref TypeParserArgs<int> args, in FileEvaluationContext ctx, out Optional<int> value)
    {
        if (!TypeParsers.Int32.TryParse(ref args, in ctx, out value) || !value.HasValue)
            return false;

        switch (value.Value)
        {
            case < MinValue:
                args.DiagnosticSink?.UNT1028_MinimumInclusive(ref args, args.ReferenceNode, value.Value.ToString("N"), MinValue.ToString("N"));
                break;

            case > MaxValue:
                args.DiagnosticSink?.UNT1028_MaximumInclusive(ref args, args.ReferenceNode, value.Value.ToString("N"), MaxValue.ToString("N"));
                break;
        }

        return true;
    }

    IType<int> ITypeConverter<int>.DefaultType => this;

    string ITypeConverter<int>.Format(int value, ref TypeConverterFormatArgs args)
    {
        return TypeConverters.Int32.Format(value, ref args);
    }

    bool ITypeConverter<int>.TryFormat(Span<char> output, int value, out int size, ref TypeConverterFormatArgs args)
    {
        return TypeConverters.Int32.TryFormat(output, value, out size, ref args);
    }

    bool ITypeConverter<int>.TryConvertTo<TTo>(Optional<int> obj, out Optional<TTo> result)
    {
        return TypeConverters.Int32.TryConvertTo(obj, out result);
    }

    void ITypeConverter<int>.WriteJson(Utf8JsonWriter writer, int value, ref TypeConverterFormatArgs args, JsonSerializerOptions options)
    {
        TypeConverters.Int32.WriteJson(writer, value, ref args, options);
    }

    bool ITypeConverter<int>.TryReadJson(in JsonElement json, out Optional<int> value, ref TypeConverterParseArgs<int> args)
    {
        return TypeConverters.Int32.TryReadJson(in json, out value, ref args);
    }

    bool ITypeParser<int>.TryReadValueFromJson(in JsonElement json, out Optional<int> value, IType<int> valueType)
    {
        return TypeParsers.Int32.TryReadValueFromJson(in json, out value, valueType);
    }

    void ITypeParser<int>.WriteValueToJson(Utf8JsonWriter writer, int value, IType<int> valueType, JsonSerializerOptions options)
    {
        TypeParsers.Int32.WriteValueToJson(writer, value, valueType, options);
    }
}