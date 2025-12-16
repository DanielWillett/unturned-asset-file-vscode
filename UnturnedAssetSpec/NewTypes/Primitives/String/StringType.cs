using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A string with various settings.
/// <para>Example: <c>$local$::ItemAsset.Name</c></para>
/// <code>
/// Prop Plain Text
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="int"/> MinimumLength</c> - Minimum character count (inclusive).</item>
///     <item><c><see cref="int"/> MaximumLength</c> - Maximum character count (inclusive).</item>
///     <item><c><see cref="bool"/> SupportsRichText</c> - Whether or not rich text is allowed.</item>
///     <item><c><see cref="bool"/> SupportsNewLines</c> - Whether or not the line break tag is allowed (<c>&lt;br&gt;</c>).</item>
///     <item><c><see cref="int"/> FormatArguments</c> - Maximum number of format arguments allowed.</item>
///     <item><c><see cref="Regex"/> ExtraTag or <see cref="Regex"/>[] ExtraTags</c> - One or more additional rich text tags that can be used.</item>
/// </list>
/// </para>
/// </summary>
public sealed class StringType : PrimitiveType<string, StringType>, ITypeParser<string>
{
    public const string TypeId = "String";

    private static readonly object Boxed999 = 999;

    private readonly int _minCount;
    private readonly int _maxCount;
    private readonly bool _allowRichText;
    private readonly bool _allowLineBreakTag;
    private readonly OneOrMore<Regex> _extraRichTextTags;
    private readonly int _maxFormatArguments;
    private object[]? _formatArgs;

    public override string Id => TypeId;

    public override string DisplayName => Resources.Type_Name_String;

    public override ITypeParser<string> Parser => this;

    public StringType()
    {
        _maxCount = int.MaxValue;
        _extraRichTextTags = OneOrMore<Regex>.Null;
    }

    public StringType(
        int minCount,
        int maxCount,
        bool allowRichText,
        bool allowLineBreakTag,
        int maxFormatArguments,
        OneOrMore<Regex> extraRichTextTags
    ) : this()
    {
        _minCount = minCount;
        _maxCount = maxCount;

        _allowRichText = allowRichText;
        _allowLineBreakTag = allowLineBreakTag;
        _extraRichTextTags = extraRichTextTags;
        _maxFormatArguments = maxFormatArguments;
    }

    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (!TypeParsers.String.TryParse(ref args, in ctx, out value))
        {
            return false;
        }

        IDiagnosticSink? diagnostics = args.DiagnosticSink;
        if (diagnostics == null || args.ShouldIgnoreFailureDiagnostic || !value.HasValue)
            return true;

        string str = value.Value;

        diagnostics.CheckUNT1024_String(str.Length, ref args, args.ParentNode, _minCount, _maxCount);

        IValueSourceNode valueNode = (IValueSourceNode?)args.ValueNode!;

        if (_allowLineBreakTag)
        {
            diagnostics.CheckUNT1021(ref args, valueNode);
        }
        else
        {
            diagnostics.CheckUNT1022_UNT106(ref args, valueNode);
        }

        if (!_allowRichText)
        {
            diagnostics.CheckUNT1006(ref args, valueNode);
        }

        if (_maxFormatArguments > 0)
        {
            if (_formatArgs == null)
            {
                _formatArgs = new object[_maxFormatArguments];
                for (int i = 0; i < _maxFormatArguments; ++i)
                    _formatArgs[i] = Boxed999;
            }

            bool malformed = false;
            try
            {
                _ = string.Format(str, _formatArgs);
            }
            catch (FormatException ex)
            {
                diagnostics.UNT2012(ref args, str, ex);
                malformed = true;
            }

            if (!malformed)
            {
                for (uint i = 0; i < _maxFormatArguments; ++i)
                {
                    string iStr = i.ToString(CultureInfo.InvariantCulture);
                    if (str.Contains($"{{{iStr}"))
                        continue;

                    diagnostics.UNT102(ref args, iStr);
                }
            }
        }

        return true;
    }

    #region JSON

    public bool TryReadValueFromJson(in JsonElement json, out Optional<string> value, IType<string> valueType)
    {
        return TypeParsers.String.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, string value, IType<string> valueType)
    {
        TypeParsers.String.WriteValueToJson(writer, value, valueType);
    }

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context)
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        int minCount = 0, maxCount = int.MaxValue, maxFmt = 0;
        bool rt = false, nl = false;
        OneOrMore<Regex> tags = OneOrMore<Regex>.Null;

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

        if (typeDefinition.TryGetProperty("SupportsRichText"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            rt = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("SupportsNewLines"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            nl = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("FormatArguments"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            maxFmt = element.GetInt32();
        }

        if (typeDefinition.TryGetProperty("ExtraTag"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            tags = new OneOrMore<Regex>(new Regex(element.GetString()!, RegexOptions.Compiled | RegexOptions.Singleline));
        }
        else if (typeDefinition.TryGetProperty("ExtraTags"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            int len = element.GetArrayLength();
            Regex[] arr = new Regex[len];
            for (int i = 0; i < len; ++i)
            {
                arr[i] = new Regex(element[i].GetString() ?? throw new JsonException(
                    string.Format(Resources.JsonException_InvalidJsonToken, nameof(JsonValueKind.Null), context)
                    ), RegexOptions.Compiled | RegexOptions.Singleline
                );
            }

            tags = new OneOrMore<Regex>(arr);
        }

        return minCount != 0 || maxCount != int.MaxValue || rt || nl || maxFmt != 0 || !tags.IsNull
            ? new StringType(minCount, maxCount, rt, nl, maxFmt, tags)
            : Instance;
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_minCount == 0 && _maxCount == int.MaxValue && !_allowRichText && !_allowLineBreakTag && _maxFormatArguments == 0 && _extraRichTextTags.IsNull)
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
        if (_allowRichText)
            writer.WriteBoolean("SupportsRichText"u8, true);
        if (_allowLineBreakTag)
            writer.WriteBoolean("SupportsNewLines"u8, true);
        if (_maxFormatArguments != 0)
            writer.WriteNumber("FormatArguments"u8, _maxFormatArguments);
        if (!_extraRichTextTags.IsNull)
        {
            if (_extraRichTextTags.IsSingle)
            {
                writer.WriteString("ExtraTag"u8, _extraRichTextTags[0].ToString());
            }
            else
            {
                writer.WritePropertyName("ExtraTags"u8);
                writer.WriteStartArray();
                foreach (Regex r in _extraRichTextTags)
                    writer.WriteStringValue(r.ToString());
                writer.WriteEndArray();
            }
        }

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(StringType other)
    {
        return other._minCount == _minCount
               && other._maxCount == _maxCount
               && other._allowRichText == _allowRichText
               && other._allowLineBreakTag == _allowLineBreakTag
               && other._maxFormatArguments == _maxFormatArguments
               && other._extraRichTextTags.Equals(
                   _extraRichTextTags,
                   (r1, r2) => string.Equals(r1.ToString(), r2.ToString(), StringComparison.Ordinal)
               );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(42241426, _minCount, _maxCount, _allowRichText, _allowLineBreakTag, _maxFormatArguments, _extraRichTextTags.GetHashCode(f => f.ToString().GetHashCode()));
    }
}