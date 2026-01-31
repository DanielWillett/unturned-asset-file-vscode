using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A property using this type defines the key of a localization property by it's value.
/// <para>Example: <c>NPCCondition.TextId</c></para>
/// <code>
/// // Asset:
/// Prop Asset_Name
/// 
/// // Local:
/// Asset_Name "Some Name"
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="int"/> MinimumLength</c> - Minimum character count of the key (inclusive).</item>
///     <item><c><see cref="int"/> MaximumLength</c> - Maximum character count of the key (inclusive).</item>
///     <item><c><see cref="int"/> MinimumValueLength</c> - Minimum character count of the value (inclusive).</item>
///     <item><c><see cref="int"/> MaximumValueLength</c> - Maximum character count of the value (inclusive).</item>
///     <item><c><see cref="bool"/> ValueSupportsRichText</c> - Whether or not rich text is allowed in the localization value.</item>
///     <item><c><see cref="bool"/> ValueSupportsNewLines</c> - Whether or not the line break tag is allowed in the localization value (<c>&lt;br&gt;</c>).</item>
///     <item><c><see cref="int"/> ValueFormatArguments</c> - Maximum number of format arguments allowed in the localization value.</item>
///     <item><c><see cref="Regex"/> ValueExtraTag or <see cref="Regex"/>[] ValueExtraTags</c> - One or more additional rich text tags that can be used in the localization value.</item>
/// </list>
/// </para>
/// </summary>
public sealed class LocalizationKeyType : BaseType<string, LocalizationKeyType>, ITypeParser<string>, ITypeFactory
{
    public static readonly LocalizationKeyType Instance = new LocalizationKeyType();

    private readonly int _keyMinLength;
    private readonly int _keyMaxLength;

    private readonly int _valueMinLength;
    private readonly int _valueMaxLength;
    private readonly bool _valueAllowRichText;
    private readonly bool _valueAllowLineBreakTag;
    private readonly uint _valueFormattingArgs;
    private readonly OneOrMore<Regex> _valueExtraTags;

    public const string TypeId = "LocalizationKey";

    /// <summary>
    /// Factory used to create <see cref="LocalizationKeyType"/> values from JSON.
    /// </summary>
    public static ITypeFactory Factory => Instance;

    public override ITypeParser<string> Parser => this;

    public override string Id => TypeId;

    public override string DisplayName => Resources.Type_Name_LocalizationKey;

    public LocalizationKeyType(
        int keyMinLen = -1, int keyMaxLen = -1,
        int valueMinLen = -1, int valueMaxLen = -1,
        bool richText = false,
        bool newLines = false,
        uint fmtArgs = 0,
        OneOrMore<Regex?> extraTags = default)
    {
        _keyMinLength = keyMinLen < 0 ? 0 : keyMinLen;
        _keyMaxLength = keyMinLen < 0 ? int.MaxValue : keyMaxLen;

        _valueMinLength = valueMinLen < 0 ? 0 : valueMinLen;
        _valueMaxLength = valueMinLen < 0 ? int.MaxValue : valueMaxLen;
        _valueAllowRichText = richText;
        _valueAllowLineBreakTag = newLines;
        _valueFormattingArgs = fmtArgs;
        _valueExtraTags = extraTags.Where(x => x != null)!;
    }

    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        string key;

        // value not given
        if (args.ValueNode == null)
        {
            if (args.Property == null)
            {
                args.DiagnosticSink?.UNT2004_Generic(ref args, string.Empty, this);
                value = Optional<string>.Null;
                return false;
            }

            IValue? defaultValue = args.ParentNode is IDictionarySourceNode
                ? args.Property.DefaultValue
                : args.Property.IncludedDefaultValue ?? args.Property.DefaultValue;

            if (defaultValue == null
                || defaultValue.IsNull
                || !defaultValue.TryGetValueAs(in ctx, out Optional<string> result)
                || !result.HasValue
                || string.IsNullOrEmpty(result.Value))
            {
                args.DiagnosticSink?.UNT2004_Generic(ref args, string.Empty, this);
                value = Optional<string>.Null;
                return false;
            }

            key = result.Value;
        }
        else
        {
            if (!TypeParsers.TryParseStringValueOnly(ref args, out IValueSourceNode? valueNode))
            {
                value = Optional<string>.Null;
                return false;
            }

            key = valueNode.Value;
            if (string.IsNullOrEmpty(key))
            {
                args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                value = Optional<string>.Null;
                return false;
            }
        }

        value = key;

        ISourceFile file = args.ParentNode.File;
        if (file is not IAssetSourceFile assetFile || assetFile.GetDefaultLocalizationFile() is not { } localFile)
        {
            args.DiagnosticSink?.UNT1030_Property(ref args, args.ReferenceNode);
            return true;
        }

        // todo if (!localFile.TryGetProperty(key))
        // todo {
        // todo
        // todo }

        return true;
    }

    /// <inheritdoc />
    protected override bool Equals(LocalizationKeyType other)
    {
        return _keyMinLength == other._keyMinLength
               && _keyMaxLength == other._keyMaxLength
               && _valueMinLength == other._valueMinLength
               && _valueMaxLength == other._valueMaxLength
               && _valueAllowRichText == other._valueAllowRichText
               && _valueAllowLineBreakTag == other._valueAllowLineBreakTag
               && _valueFormattingArgs == other._valueFormattingArgs
               && _valueExtraTags.Equals(
                   other._valueExtraTags,
                   (r1, r2) => string.Equals(r1.ToString(), r2.ToString(), StringComparison.Ordinal)
               );
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hc = new HashCode();
        hc.Add(657153428);
        hc.Add(_keyMinLength);
        hc.Add(_keyMaxLength);
        hc.Add(_valueMinLength);
        hc.Add(_valueMaxLength);
        hc.Add(_valueAllowRichText);
        hc.Add(_valueAllowLineBreakTag);
        hc.Add(_valueFormattingArgs);
        hc.Add(_valueExtraTags.GetHashCode(x => x.ToString().GetHashCode()));
        return hc.ToHashCode();
    }

    public IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        int keyMinCount = 0, keyMaxCount = int.MaxValue, valueMinCount = 0, valueMaxCount = int.MaxValue;
        uint maxFmt = 0;
        bool rt = false, nl = false;
        OneOrMore<Regex?> tags = OneOrMore<Regex?>.Null;

        if (typeDefinition.TryGetProperty("MinimumLength"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            keyMinCount = element.GetInt32();
        }

        if (typeDefinition.TryGetProperty("MaximumLength"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            keyMaxCount = element.GetInt32();
        }

        if (typeDefinition.TryGetProperty("MinimumValueLength"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            valueMinCount = element.GetInt32();
        }

        if (typeDefinition.TryGetProperty("MaximumValueLength"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            valueMaxCount = element.GetInt32();
        }

        if (typeDefinition.TryGetProperty("ValueSupportsRichText"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            rt = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("ValueSupportsNewLines"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            nl = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("ValueFormatArguments"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            maxFmt = element.GetUInt32();
        }

        if (typeDefinition.TryGetProperty("ValueExtraTag"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            tags = new OneOrMore<Regex?>(new Regex(element.GetString()!, RegexOptions.Compiled | RegexOptions.Singleline));
        }
        else if (typeDefinition.TryGetProperty("ValueExtraTags"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            int len = element.GetArrayLength();
            Regex?[] arr = new Regex?[len];
            for (int i = 0; i < len; ++i)
            {
                arr[i] = new Regex(element[i].GetString() ?? throw new JsonException(
                    string.Format(Resources.JsonException_InvalidJsonToken, nameof(JsonValueKind.Null), context)
                    ), RegexOptions.Compiled | RegexOptions.Singleline
                );
            }

            tags = new OneOrMore<Regex?>(arr);
        }

        return keyMinCount != 0 || keyMaxCount != int.MaxValue || valueMinCount != 0 || valueMaxCount != int.MaxValue || rt || nl || maxFmt != 0 || !tags.IsNull
            ? new LocalizationKeyType(keyMinCount, keyMaxCount, valueMinCount, valueMaxCount, rt, nl, maxFmt, tags)
            : Instance;
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_keyMinLength == 0
            && _keyMaxLength == int.MaxValue
            && _valueMinLength == 0
            && _valueMaxLength == int.MaxValue
            && !_valueAllowRichText
            && !_valueAllowLineBreakTag
            && _valueFormattingArgs == 0
            && _valueExtraTags.IsNull)
        {
            writer.WriteStringValue(TypeId);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        if (_keyMinLength != 0)
            writer.WriteNumber("MinimumLength"u8, _keyMinLength);
        if (_keyMaxLength != int.MaxValue)
            writer.WriteNumber("MaximumLength"u8, _keyMaxLength);
        if (_valueMinLength != 0)
            writer.WriteNumber("MinimumValueLength"u8, _valueMinLength);
        if (_valueMaxLength != int.MaxValue)
            writer.WriteNumber("MaximumValueLength"u8, _valueMaxLength);
        if (_valueAllowRichText)
            writer.WriteBoolean("ValueSupportsRichText"u8, true);
        if (_valueAllowLineBreakTag)
            writer.WriteBoolean("ValueSupportsNewLines"u8, true);
        if (_valueFormattingArgs != 0)
            writer.WriteNumber("ValueFormatArguments"u8, _valueFormattingArgs);
        if (!_valueExtraTags.IsNull)
        {
            if (_valueExtraTags.IsSingle)
            {
                writer.WriteString("ValueExtraTag"u8, _valueExtraTags[0].ToString());
            }
            else
            {
                writer.WritePropertyName("ValueExtraTags"u8);
                writer.WriteStartArray();
                foreach (Regex r in _valueExtraTags)
                    writer.WriteStringValue(r.ToString());
                writer.WriteEndArray();
            }
        }

        writer.WriteEndObject();
    }


    public bool TryReadValueFromJson(in JsonElement json, out Optional<string> value, IType<string> valueType)
    {
        return TypeParsers.String.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, string value, IType<string> valueType, JsonSerializerOptions options)
    {
        TypeParsers.String.WriteValueToJson(writer, value, valueType, options);
    }
}
