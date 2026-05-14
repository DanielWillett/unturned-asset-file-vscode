using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
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
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="bool"/> AllowNegative</c> - Allows a negative value without logging a warning.</item>
///     <item><c><see cref="bool"/> AllowNegativeForFirstElementOnly</c> - Allows a negative value on only the first element in a list without logging a warning.</item>
/// </list>
/// </para>
/// </summary>
public sealed class SteamItemDefType : PrimitiveType<int, SteamItemDefType>, ITypeConverter<int>, ITypeParser<int>
{
    private readonly bool _allowNegative;

    // special case for Drops list
    private readonly bool _allowNegativeForFirstElement;

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

    public override int GetHashCode() => !_allowNegative ? _allowNegativeForFirstElement ? 1243504125 : 1048421756 : 1597310889;

    public SteamItemDefType() { }

    public SteamItemDefType(bool allowNegative = false, bool onlyIfFirstElement = false)
    {
        _allowNegative = allowNegative && !onlyIfFirstElement;
        _allowNegativeForFirstElement = allowNegative && onlyIfFirstElement;
    }

    bool ITypeConverter<int>.TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<int> args, out int parsedValue)
    {
        if (!TypeConverters.Int32.TryParse(text, ref args, out parsedValue))
            return false;

        if (args.DiagnosticSink != null)
        {
            CheckRange(parsedValue, args.DiagnosticSink, ref args);
        }

        return true;
    }

    bool ITypeParser<int>.TryParse(ref TypeParserArgs<int> args, ref FileEvaluationContext ctx, out Optional<int> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, ref ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.Int32.TryParse(ref args, ref ctx, out value) || !value.HasValue)
        {
            args.Result = TypeParserResult.Failed;
            return false;
        }

        args.Result = TypeParserResult.Successful;

        if (args.DiagnosticSink != null)
        {
            CheckRange(value.Value, args.DiagnosticSink, ref args);
        }

        return true;
    }

    private void CheckRange<TDiagnosticProvider>(int parsedValue, IDiagnosticSink sink, ref TDiagnosticProvider args)
        where TDiagnosticProvider : struct, IDiagnosticProvider
    {
        switch (parsedValue)
        {
            case < 0 when _allowNegative
                         || _allowNegativeForFirstElement && ListType.Index.IsValueCreated && ListType.Index.Value == 0:
                break;
                
            case < MinValue:
                sink.UNT1028_MinimumInclusive(ref args, null, parsedValue.ToString("N"), MinValue.ToString("N"));
                break;

            case > MaxValue:
                sink.UNT1028_MaximumInclusive(ref args, null, parsedValue.ToString("N"), MaxValue.ToString("N"));
                break;
        }
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

    bool ITypeParser<int>.TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<int> value,
        IType<int> valueType,
        ref TDataRefReadContext dataRefContext
    )
    {
        return TypeParsers.Int32.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    void ITypeParser<int>.WriteValueToJson(Utf8JsonWriter writer, int value, IType<int> valueType, JsonSerializerOptions options)
    {
        TypeParsers.Int32.WriteValueToJson(writer, value, valueType, options);
    }

    #region JSON

    /// <inheritdoc />
    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        if (typeDefinition.ValueKind != JsonValueKind.Object)
            return Instance;

        bool allowAll = false, allowFirst = false;
        if (typeDefinition.TryGetProperty("AllowNegative"u8, out JsonElement value) && value.ValueKind != JsonValueKind.Null)
        {
            allowAll = value.GetBoolean();
        }
        if (typeDefinition.TryGetProperty("AllowNegativeForFirstElementOnly", out value) && value.ValueKind != JsonValueKind.Null)
        {
            allowFirst = value.GetBoolean();
        }

        return !allowAll && !allowFirst ? Instance : new SteamItemDefType(allowAll || allowFirst, allowFirst);
    }

    /// <inheritdoc />
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (!_allowNegative && !_allowNegativeForFirstElement)
        {
            base.WriteToJson(writer, options);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);

        if (_allowNegative)
        {
            writer.WriteBoolean(_allowNegativeForFirstElement
                ? "AllowNegativeForFirstElementOnly"u8
                : "AllowNegative"u8,
                true
            );
        }

        writer.WriteEndObject();
    }

    #endregion
}