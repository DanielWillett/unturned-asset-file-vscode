using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Type for all 32-bit color types.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="VectorTypeOptions"/> Mode</c> - Acceptable syntaxes (comma-separated).</item>
///     <item><c><see cref="bool"/> Alpha</c> - Whether or not the alpha channel can be parsed. Defaults to <see langword="false"/>.</item>
///     <item><c><see cref="bool"/> StrictHex</c> - Whether or not the color has to conform to the <c>Palette.hex</c> method's expected format (exactly 7 characters long). Defaults to <see langword="false"/>.</item>
///     <item><c><see cref="string"/> RKey</c> - Overrides the "R" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> GKey</c> - Overrides the "G" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> BKey</c> - Overrides the "B" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> AKey</c> - Overrides the "A" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> LegacyRKey</c> - Overrides the "R" property name for legacy parsing. Defaults to "RKey".</item>
///     <item><c><see cref="string"/> LegacyGKey</c> - Overrides the "G" property name for legacy parsing. Defaults to "GKey".</item>
///     <item><c><see cref="string"/> LegacyBKey</c> - Overrides the "B" property name for legacy parsing. Defaults to "BKey".</item>
///     <item><c><see cref="string"/> LegacyAKey</c> - Overrides the "A" property name for legacy parsing. Defaults to "AKey".</item>
///     <item><c><see cref="bool"/> SkipLegacyCombineWithUnderscore</c> - If <see langword="true"/>, the '_' placed between the base key and the defined legacy key will be excluded.</item>
/// </list>
/// </para>
/// </summary>
public sealed class Color32Type : BaseVectorType<Color32, Color32Type>
{
    public const string TypeId = "Color32";

    public static readonly Color32Type Instance = new Color32Type();

    static Color32Type() { }

    private readonly string? _rKey;
    private readonly string? _gKey;
    private readonly string? _bKey;
    private readonly string? _aKey;
    private readonly string? _legacyRKey;
    private readonly string? _legacyGKey;
    private readonly string? _legacyBKey;
    private readonly string? _legacyAKey;
    private readonly bool _skipLegacyCombineWithUnderscore;

    public override string Id => TypeId;

    public override string DisplayName => Resources.Type_Name_Color32;

    public static ITypeFactory Factory => Instance;

    /// <summary>
    /// Whether or not the alpha channel can be parsed.
    /// </summary>
    public bool AllowAlpha { get; }

    /// <summary>
    /// Whether or not the color has to conform to the <c>Palette.hex(<see cref="string"/>)</c> method's expected format (exactly 7 characters long).
    /// </summary>
    public bool StrictHex { get; }

    public Color32Type() : this(VectorTypeOptions.Object | VectorTypeOptions.String, allowAlpha: false) { }

    public Color32Type(VectorTypeOptions options, bool allowAlpha, bool strictHex = false) : base(options)
    {
        StrictHex = strictHex;
        AllowAlpha = allowAlpha;
    }

    public Color32Type(VectorTypeOptions options, bool allowAlpha, bool strictHex, string? rKey, string? gKey, string? bKey, string? aKey, string? legacyRKey = null, string? legacyGKey = null, string? legacyBKey = null, string? legacyAKey = null, bool skipLegacyCombineWithUnderscore = false)
        : this(options, allowAlpha, strictHex)
    {
        _rKey = rKey;
        _gKey = gKey;
        _bKey = bKey;
        _aKey = aKey;
        _legacyRKey = legacyRKey;
        _legacyGKey = legacyGKey;
        _legacyBKey = legacyBKey;
        _legacyAKey = legacyAKey;
        _skipLegacyCombineWithUnderscore = skipLegacyCombineWithUnderscore;
    }

    protected override bool TryParseFromString(ReadOnlySpan<char> text, out Color32 value, ISourceNode? property, ref TypeConverterParseArgs<Color32> args)
    {
        if (!KnownTypeValueHelper.TryParseColorHex(text, out value, true))
            return false;

        if (!AllowAlpha && value.A < byte.MaxValue)
        {
            args.DiagnosticSink?.UNT1026(ref args, property);
        }

        if (StrictHex && text.Length != (AllowAlpha ? 9 : 7))
        {
            args.DiagnosticSink?.UNT2004_StrictColor(ref args, args.GetString(text), args.Type, property, AllowAlpha, value.ToHex(AllowAlpha));
        }

        return true;
    }

    protected override bool TryParseLegacy(ref TypeParserArgs<Color32> args, IDictionarySourceNode dictionary, string baseKey, out Color32 value, out bool hadOneComp)
    {
        string rKey = _legacyRKey ?? _rKey ?? "R";
        string gKey = _legacyGKey ?? _gKey ?? "G";
        string bKey = _legacyBKey ?? _bKey ?? "B";
        string aKey = _legacyAKey ?? _aKey ?? "A";

        if (_skipLegacyCombineWithUnderscore)
        {
            rKey = baseKey + rKey;
            gKey = baseKey + gKey;
            bKey = baseKey + bKey;
            aKey = baseKey + aKey;
        }
        else
        {
            rKey = baseKey + "_" + rKey;
            gKey = baseKey + "_" + gKey;
            bKey = baseKey + "_" + bKey;
            aKey = baseKey + "_" + aKey;
        }

        if (!dictionary.TryGetProperty(rKey, out IPropertySourceNode? rProperty)
            & !dictionary.TryGetProperty(gKey, out IPropertySourceNode? gProperty)
            & !dictionary.TryGetProperty(bKey, out IPropertySourceNode? bProperty)
            & (!AllowAlpha | !dictionary.TryGetProperty(aKey, out IPropertySourceNode? aProperty)))
        {
            hadOneComp = false;
            value = default;
            return false;
        }

        if (args.ReferencedPropertySink != null)
        {
            if (rProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(rProperty);
            if (gProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(gProperty);
            if (bProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(bProperty);
            if (aProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(aProperty);
        }

        if (!AllowAlpha && aProperty != null)
        {
            args.DiagnosticSink?.UNT1026(ref args, aProperty);
        }

        hadOneComp = true;

        if (rProperty == null || gProperty == null || bProperty == null || (AllowAlpha && aProperty == null))
        {
            if (rProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, rKey);
            }
            if (gProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, gKey);
            }
            if (bProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, bKey);
            }
            if (AllowAlpha && aProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, aKey);
            }

            value = default;
            return false;
        }

        bool rgb = VectorTypes.TryParseArg(ref args, out byte r, rProperty)
                 & VectorTypes.TryParseArg(ref args, out byte g, gProperty)
                 & VectorTypes.TryParseArg(ref args, out byte b, bProperty);
        byte a;
        if (!AllowAlpha)
        {
            a = byte.MaxValue;
        }
        else
        {
            rgb &= VectorTypes.TryParseArg(ref args, out a, aProperty!);
        }

        value = new Color32(a, r, g, b);
        return rgb;
    }

    protected override bool TryParseFromDictionary(ref TypeParserArgs<Color32> args, IDictionarySourceNode dictionary, out Color32 value)
    {
        string rKey = _rKey ?? "R";
        string gKey = _gKey ?? "G";
        string bKey = _bKey ?? "B";
        string aKey = _aKey ?? "A";

        if (!dictionary.TryGetProperty(rKey, out IPropertySourceNode? rProperty)
            & !dictionary.TryGetProperty(gKey, out IPropertySourceNode? gProperty)
            & !dictionary.TryGetProperty(bKey, out IPropertySourceNode? bProperty)
            & (!AllowAlpha | !dictionary.TryGetProperty(aKey, out IPropertySourceNode? aProperty)))
        {
            value = default;
            return false;
        }

        if (!AllowAlpha && aProperty != null)
        {
            args.DiagnosticSink?.UNT1026(ref args, aProperty);
        }

        if (rProperty == null || gProperty == null || bProperty == null || (AllowAlpha && aProperty == null))
        {
            if (rProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, rKey);
            }
            if (gProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, gKey);
            }
            if (bProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, bKey);
            }
            if (AllowAlpha && aProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, aKey);
            }

            value = default;
            return false;
        }

        bool rgb = VectorTypes.TryParseArg(ref args, out byte r, rProperty)
                 & VectorTypes.TryParseArg(ref args, out byte g, gProperty)
                 & VectorTypes.TryParseArg(ref args, out byte b, bProperty);
        byte a;
        if (!AllowAlpha)
        {
            a = byte.MaxValue;
        }
        else
        {
            rgb &= VectorTypes.TryParseArg(ref args, out a, aProperty!);
        }

        value = new Color32(a, r, g, b);
        return rgb;
    }


    #region JSON

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        VectorTypeOptions mode = ReadOptions(in typeDefinition, owner, context);

        string? rKey = null, gKey = null, bKey = null, aKey = null, legacyRKey = null, legacyGKey = null, legacyBKey = null, legacyAKey = null;
        bool skipUnderscore = false, alpha = false, strict = false;
        if (typeDefinition.TryGetProperty("Alpha"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
            alpha = element.GetBoolean();
        if (typeDefinition.TryGetProperty("StrictHex"u8, out element) && element.ValueKind != JsonValueKind.Null)
            strict = element.GetBoolean();
        if (typeDefinition.TryGetProperty("RKey"u8, out element))
            rKey = element.GetString();
        if (typeDefinition.TryGetProperty("GKey"u8, out element))
            gKey = element.GetString();
        if (typeDefinition.TryGetProperty("BKey"u8, out element))
            bKey = element.GetString();
        if (typeDefinition.TryGetProperty("AKey"u8, out element))
            aKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyRKey"u8, out element))
            legacyRKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyGKey"u8, out element))
            legacyGKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyBKey"u8, out element))
            legacyBKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyAKey"u8, out element))
            legacyAKey = element.GetString();
        if (typeDefinition.TryGetProperty("SkipLegacyCombineWithUnderscore"u8, out element) && element.ValueKind != JsonValueKind.Null)
            skipUnderscore = element.GetBoolean();

        return mode == VectorTypeOptions.Default && !alpha && !strict && rKey == null && gKey == null && bKey == null && legacyRKey == null && legacyGKey == null && legacyBKey == null && !skipUnderscore
            ? Instance
            : new Color32Type(mode, alpha, strict, rKey, gKey, bKey, aKey, legacyRKey, legacyGKey, legacyBKey, legacyAKey, skipUnderscore);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (Options == VectorTypeOptions.Default && !AllowAlpha && _rKey == null && _gKey == null && _bKey == null && _aKey == null && _legacyRKey == null && _legacyGKey == null && _legacyBKey == null && _legacyAKey == null && !_skipLegacyCombineWithUnderscore)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        WriteOptions(writer);

        if (AllowAlpha) writer.WriteBoolean("Alpha"u8, true);
        if (StrictHex) writer.WriteBoolean("StrictHex"u8, true);

        if (_rKey != null) writer.WriteString("RKey"u8, _rKey);
        if (_gKey != null) writer.WriteString("GKey"u8, _gKey);
        if (_bKey != null) writer.WriteString("BKey"u8, _bKey);
        if (_aKey != null) writer.WriteString("AKey"u8, _aKey);

        if (_legacyRKey != null) writer.WriteString("LegacyRKey"u8, _legacyRKey);
        if (_legacyGKey != null) writer.WriteString("LegacyGKey"u8, _legacyGKey);
        if (_legacyBKey != null) writer.WriteString("LegacyBKey"u8, _legacyBKey);
        if (_legacyAKey != null) writer.WriteString("LegacyAKey"u8, _legacyAKey);

        if (_skipLegacyCombineWithUnderscore) writer.WriteBoolean("SkipLegacyCombineWithUnderscore"u8, true);

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(Color32Type other)
    {
        if (ReferenceEquals(other, this))
            return true;

        return EqualsHelper(other)
               && other.StrictHex == StrictHex
               && other.AllowAlpha == AllowAlpha
               && other._skipLegacyCombineWithUnderscore == _skipLegacyCombineWithUnderscore
               && string.Equals(_rKey, other._rKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_gKey, other._gKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_bKey, other._bKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_aKey, other._aKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyRKey, other._legacyRKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyGKey, other._legacyGKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyBKey, other._legacyBKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyAKey, other._legacyAKey, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        HashCode hc = new HashCode();
        hc.Add(1626835220);
        hc.Add(base.GetHashCode());
        hc.Add(StrictHex);
        hc.Add(AllowAlpha);
        hc.Add(_skipLegacyCombineWithUnderscore);
        hc.Add(_rKey);
        hc.Add(_gKey);
        hc.Add(_bKey);
        hc.Add(_aKey);
        hc.Add(_legacyRKey);
        hc.Add(_legacyGKey);
        hc.Add(_legacyBKey);
        hc.Add(_legacyAKey);
        return hc.ToHashCode();
    }
}