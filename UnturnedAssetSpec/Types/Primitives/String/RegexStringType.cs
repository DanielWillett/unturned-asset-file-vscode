using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A regular expression string.
/// <para>Example: <c>ServerListCurationFile.Regex</c></para>
/// <code>
/// // basic RegEx to match phone numbers.
/// Prop ^[\s\-\(]*\d{3}[\s\-\)]*\d{3}[\s-]*\d{4}[\s-]*$
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="int"/> MinimumLength</c> - Minimum character count (inclusive).</item>
///     <item><c><see cref="int"/> MaximumLength</c> - Maximum character count (inclusive).</item>
/// </list>
/// </para>
/// </summary>
public sealed class RegexStringType : PrimitiveType<string, RegexStringType>, ITypeParser<string>
{
    public const string TypeId = "RegEx";

    private readonly int _minCount;
    private readonly int _maxCount;

    public override ITypeParser<string> Parser => this;

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_RegEx;

    public RegexStringType()
    {
        _maxCount = int.MaxValue;
    }

    public RegexStringType(int? minCount, int? maxCount)
    {
        _minCount = minCount ?? 0;
        _maxCount = maxCount ?? int.MaxValue;
    }

    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (!StringType.Instance.Parser.TryParse(ref args, in ctx, out value))
        {
            return false;
        }

        if (args.DiagnosticSink == null || args.ShouldIgnoreFailureDiagnostic || !value.HasValue)
            return true;

        try
        {
            _ = new Regex(value.Value, RegexOptions.None, TimeSpan.FromSeconds(1d));

            args.DiagnosticSink.CheckUNT1024_String(value.Value.Length, ref args, args.ParentNode, _minCount, _maxCount);
        }
        catch (Exception ex)
        {
            args.DiagnosticSink!.UNT2004_Regex(ref args, ex, value.Value);
        }

        return true;
    }

    #region JSON

    public bool TryReadValueFromJson(in JsonElement json, out Optional<string> value, IType<string> valueType)
    {
        return TypeParsers.String.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, string value, IType<string> valueType, JsonSerializerOptions options)
    {
        TypeParsers.String.WriteValueToJson(writer, value, valueType, options);
    }

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        int? minCount = null, maxCount = null;

        if (typeDefinition.TryGetProperty("MinimumLength"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            minCount = element.GetInt32();
        }

        if (typeDefinition.TryGetProperty("MaximumLength"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            maxCount = element.GetInt32();
        }

        return minCount.HasValue || maxCount.HasValue ? new RegexStringType(minCount, maxCount) : Instance;
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_minCount == 0 && _maxCount == int.MaxValue)
        {
            base.WriteToJson(writer, options);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        if (_minCount != 0)
            writer.WriteNumber("MinimumLength"u8, _minCount);
        if (_maxCount != int.MaxValue)
            writer.WriteNumber("MaximumLength"u8, _maxCount);

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(RegexStringType other)
    {
        return other._minCount == _minCount && other._maxCount == _maxCount;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(240497972, _minCount, _maxCount);
    }
}