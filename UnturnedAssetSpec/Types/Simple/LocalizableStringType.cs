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
/// A string that can either appear in the asset file or localization file.
/// <para>Example: <c>ServerListCurationAsset.Name</c></para>
/// <code>
/// // Asset:
/// Prop "Some Name"
///
/// // - or -
/// 
/// // Local:
/// Prop "Some Name"
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="int"/> MinimumLength</c> - Minimum character count (inclusive).</item>
///     <item><c><see cref="int"/> MaximumLength</c> - Maximum character count (inclusive).</item>
///     <item><c><see cref="bool"/> SupportsRichText</c> - Whether or not rich text is allowed.</item>
///     <item><c><see cref="bool"/> SupportsNewLines</c> - Whether or not the line break tag is allowed (<c>&lt;br&gt;</c>).</item>
///     <item><c><see cref="bool"/> RequiresValue</c> - Whether or not the non-local property must have a value to not use the one in localization. Basically if TryGetValue is used instead of ContainsKey. Defaults to <see langword="false"/>.</item>
///     <item><c><see cref="bool"/> PreferLocalizationValue</c> - Whether or not the localization value will be used first if both exist. Defaults to <see langword="false"/>.</item>
///     <item><c><see cref="int"/> FormatArguments</c> - Maximum number of format arguments allowed.</item>
///     <item><c><see cref="Regex"/> ExtraTag or <see cref="Regex"/>[] ExtraTags</c> - One or more additional rich text tags that can be used.</item>
///     <item><c><see cref="IValue{TValue}"/> of <see cref="string"/> LocalizationKeyOverride</c> - Expression used to get the localization key instead of just using the same key as the given property.</item>
/// </list>
/// </para>
/// </summary>
public sealed class LocalizableStringType : BaseType<string, LocalizableStringType>, ITypeParser<string>, ITypeFactory
{
    public static readonly LocalizableStringType Instance = new LocalizableStringType();

    private readonly int _minLength;
    private readonly int _maxLength;
    private readonly bool _allowRichText;
    private readonly bool _allowLineBreakTag;
    private readonly bool _requiresValue;
    private readonly bool _preferLocalizationValue;
    private readonly uint _formattingArgs;
    private readonly IValue<string>? _keyOverride;
    private readonly OneOrMore<Regex> _extraTags;

    public const string TypeId = "LocalizableString";

    /// <summary>
    /// Factory used to create <see cref="LocalizableStringType"/> values from JSON.
    /// </summary>
    public static ITypeFactory Factory => Instance;

    public override PropertySearchTrimmingBehavior TrimmingBehavior => PropertySearchTrimmingBehavior.CreatesOtherPropertiesInLinkedFiles;

    public override ITypeParser<string> Parser => this;

    public override string Id => TypeId;

    public override string DisplayName => Resources.Type_Name_LocalizableString;

    public LocalizableStringType(
        int minLen = -1, int maxLen = -1,
        bool richText = false,
        bool newLines = false,
        bool requiresValue = false,
        bool preferLocalizationValue = false,
        uint fmtArgs = 0,
        OneOrMore<Regex?> extraTags = default,
        IValue<string>? keyOverride = null)
    {
        _minLength = minLen < 0 ? 0 : minLen;
        _maxLength = minLen < 0 ? int.MaxValue : maxLen;
        _allowRichText = richText;
        _allowLineBreakTag = newLines;
        _requiresValue = requiresValue;
        _preferLocalizationValue = preferLocalizationValue;
        _formattingArgs = fmtArgs;
        _extraTags = extraTags.Where(x => x != null)!;
        _keyOverride = keyOverride;
    }

    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        // value not given
        if (args.ValueNode == null || _preferLocalizationValue)
        {
            if (args.Property == null)
            {
                args.DiagnosticSink?.UNT2004_Generic(ref args, string.Empty, this);
                value = Optional<string>.Null;
                return false;
            }

            ISourceFile file = args.ParentNode.File;
            if (file is not IAssetSourceFile assetFile || assetFile.GetDefaultLocalizationFile() is not { } localFile)
            {
                if (args.Property?.Required != null && args.Property.Required.TryEvaluateValue(out Optional<bool> isRequired, in ctx) && isRequired.Value)
                {
                    args.DiagnosticSink?.UNT1030_Property(ref args, args.ReferenceNode);
                }

                value = Optional<string>.Null;
                return false;
            }

            // todo:
            if (args.DiagnosticSink != null && false)
            {
                DiagnosticSinkExtensions.CheckStringDiagnostics(ref args, null!, _minLength, _maxLength, _allowLineBreakTag, _allowRichText, _extraTags, _formattingArgs);
            }
            value = Optional<string>.Null;
            return false;
        }

        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.TryParseStringValueOnly(ref args, out IValueSourceNode? valueNode))
        {
            value = Optional<string>.Null;
            return false;
        }

        string val = valueNode.Value;
        value = val;
        if (args.DiagnosticSink != null)
        {
            DiagnosticSinkExtensions.CheckStringDiagnostics(ref args, valueNode, _minLength, _maxLength, _allowLineBreakTag, _allowRichText, _extraTags, _formattingArgs);
        }
        return true;
    }

    /// <inheritdoc />
    protected override bool Equals(LocalizableStringType other)
    {
        return _minLength == other._minLength
               && _maxLength == other._maxLength
               && _allowRichText == other._allowRichText
               && _allowLineBreakTag == other._allowLineBreakTag
               && _requiresValue == other._requiresValue
               && _preferLocalizationValue == other._preferLocalizationValue
               && _formattingArgs == other._formattingArgs
               && _extraTags.Equals(
                   other._extraTags,
                   (r1, r2) => string.Equals(r1.ToString(), r2.ToString(), StringComparison.Ordinal)
               )
               && (_keyOverride?.Equals(other._keyOverride) ?? other._keyOverride == null);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hc = new HashCode();
        hc.Add(531638654);
        hc.Add(_minLength);
        hc.Add(_maxLength);
        hc.Add(_allowRichText);
        hc.Add(_allowLineBreakTag);
        hc.Add(_requiresValue);
        hc.Add(_preferLocalizationValue);
        hc.Add(_formattingArgs);
        hc.Add(_extraTags.GetHashCode(x => x.ToString().GetHashCode()));
        hc.Add(_keyOverride);
        return hc.ToHashCode();
    }

    public IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        int minCount = 0, maxCount = int.MaxValue;
        uint maxFmt = 0;
        bool rt = false, nl = false, rv = false, plv = false;
        OneOrMore<Regex?> tags = OneOrMore<Regex?>.Null;
        IValue<string>? keyOverride = null;

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

        if (typeDefinition.TryGetProperty("RequiresValue"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            rv = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("PreferLocalizationValue"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            plv = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("FormatArguments"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            maxFmt = element.GetUInt32();
        }

        if (typeDefinition.TryGetProperty("ExtraTag"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            tags = new OneOrMore<Regex?>(new Regex(element.GetString()!, RegexOptions.Compiled | RegexOptions.Singleline));
        }
        else if (typeDefinition.TryGetProperty("ExtraTags"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            int len = element.GetArrayLength();
            Regex?[] arr = new Regex?[len];
            for (int i = 0; i < len; ++i)
            {
                arr[i] = new Regex(element[i].GetString() ?? throw new JsonException(
                    string.Format(Resources.JsonException_InvalidJsonToken, nameof(JsonValueKind.Null), context == null ? $"{owner.FullName}.Type" : $"{owner.FullName}.{context}.Type")
                    ), RegexOptions.Compiled | RegexOptions.Singleline
                );
            }

            tags = new OneOrMore<Regex?>(arr);
        }

        if (typeDefinition.TryGetProperty("LocalizationKeyOverride", out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            keyOverride = Value.TryReadValueFromJson(in element, ValueReadOptions.AllowConditionals, LocalizationKeyType.Instance, spec.Database, owner);
            if (keyOverride == null)
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToReadValue,
                        LocalizationKeyType.Instance.DisplayName,
                        context == null ? $"{owner.FullName}.Type" : $"{owner.FullName}.{context}.Type"
                    )
                );
            }
        }

        return minCount != 0 || maxCount != int.MaxValue || rt || nl || rv || plv || maxFmt != 0 || !tags.IsNull || keyOverride != null
            ? new LocalizableStringType(minCount, maxCount, rt, nl, rv, plv, maxFmt, tags, keyOverride)
            : Instance;
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_minLength == 0
            && _maxLength == int.MaxValue
            && !_allowRichText
            && !_allowLineBreakTag
            && !_requiresValue
            && !_preferLocalizationValue
            && _formattingArgs == 0
            && _extraTags.IsNull)
        {
            writer.WriteStringValue(TypeId);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        if (_minLength != 0)
            writer.WriteNumber("MinimumLength"u8, _minLength);
        if (_maxLength != int.MaxValue)
            writer.WriteNumber("MaximumLength"u8, _maxLength);
        if (_allowRichText)
            writer.WriteBoolean("SupportsRichText"u8, true);
        if (_allowLineBreakTag)
            writer.WriteBoolean("SupportsNewLines"u8, true);
        if (_requiresValue)
            writer.WriteBoolean("RequiresValue"u8, true);
        if (_preferLocalizationValue)
            writer.WriteBoolean("PreferLocalizationValue"u8, true);
        if (_formattingArgs != 0)
            writer.WriteNumber("FormatArguments"u8, _formattingArgs);
        if (!_extraTags.IsNull)
        {
            if (_extraTags.IsSingle)
            {
                writer.WriteString("ExtraTag"u8, _extraTags[0].ToString());
            }
            else
            {
                writer.WritePropertyName("ExtraTags"u8);
                writer.WriteStartArray();
                foreach (Regex r in _extraTags)
                    writer.WriteStringValue(r.ToString());
                writer.WriteEndArray();
            }
        }

        _keyOverride?.WriteToJson(writer, options);

        writer.WriteEndObject();
    }


    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<string> value,
        IType<string> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        return TypeParsers.String.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, string value, IType<string> valueType, JsonSerializerOptions options)
    {
        TypeParsers.String.WriteValueToJson(writer, value, valueType, options);
    }
}
