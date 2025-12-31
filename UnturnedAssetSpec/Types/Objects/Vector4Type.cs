using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Numerics;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Type for all 4-component vector types.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="VectorTypeOptions"/> Mode</c> - Acceptable syntaxes (comma-separated).</item>
///     <item><c><see cref="Vector4Kind"/> Kind</c> - Kind/unit of value to parse.</item>
///     <item><c><see cref="string"/> XKey</c> - Overrides the "X" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> YKey</c> - Overrides the "Y" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> ZKey</c> - Overrides the "Z" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> WKey</c> - Overrides the "W" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> LegacyXKey</c> - Overrides the "X" property name for legacy parsing. Defaults to "XKey".</item>
///     <item><c><see cref="string"/> LegacyYKey</c> - Overrides the "Y" property name for legacy parsing. Defaults to "YKey".</item>
///     <item><c><see cref="string"/> LegacyZKey</c> - Overrides the "Z" property name for legacy parsing. Defaults to "ZKey".</item>
///     <item><c><see cref="string"/> LegacyWKey</c> - Overrides the "W" property name for legacy parsing. Defaults to "WKey".</item>
///     <item><c><see cref="bool"/> SkipLegacyCombineWithUnderscore</c> - If <see langword="true"/>, the '_' placed between the base key and the defined legacy key will be excluded.</item>
/// </list>
/// </para>
/// </summary>
public sealed class Vector4Type : BaseVectorType<Vector4, Vector4Type>
{
    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_Vector4_Unspecified,
        Resources.Type_Name_Vector4_Quaternion
    ];

    public const string TypeId = "Vector4";

    public static readonly Vector4Type Instance = new Vector4Type();

    static Vector4Type() { }

    private readonly string? _xKey;
    private readonly string? _yKey;
    private readonly string? _zKey;
    private readonly string? _wKey;
    private readonly string? _legacyXKey;
    private readonly string? _legacyYKey;
    private readonly string? _legacyZKey;
    private readonly string? _legacyWKey;
    private readonly bool _skipLegacyCombineWithUnderscore;

    public Vector4Kind Kind { get; }

    public override string Id => TypeId;

    public override string DisplayName => DisplayNames[(int)Kind];

    public static ITypeFactory Factory => Instance;


    public Vector4Type() : this(Vector4Kind.Unspecified, VectorTypeOptions.Object | VectorTypeOptions.String) { }

    public Vector4Type(Vector4Kind kind, VectorTypeOptions options) : base(options)
    {
        if (kind is < Vector4Kind.Unspecified or >= Vector4Kind.Quaternion)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
    }

    public Vector4Type(Vector4Kind kind, VectorTypeOptions options, string? xKey, string? yKey, string? zKey, string? wKey, string? legacyXKey = null, string? legacyYKey = null, string? legacyZKey = null, string? legacyWKey = null, bool skipLegacyCombineWithUnderscore = false)
        : this(kind, options)
    {
        if (kind is < Vector4Kind.Unspecified or >= Vector4Kind.Quaternion)
            throw new ArgumentOutOfRangeException(nameof(kind));

        _xKey = xKey;
        _yKey = yKey;
        _zKey = zKey;
        _wKey = wKey;
        _legacyXKey = legacyXKey;
        _legacyYKey = legacyYKey;
        _legacyZKey = legacyZKey;
        _legacyWKey = legacyWKey;
        _skipLegacyCombineWithUnderscore = skipLegacyCombineWithUnderscore;
    }

    protected override string ToString(Vector4 vector)
    {
        return KnownTypeValueHelper.ToComponentString(vector);
    }

    protected override bool TryParseFromString(ReadOnlySpan<char> text, out Vector4 value)
    {
        return KnownTypeValueHelper.TryParseVector4Components(text, out value);
    }

    protected override bool TryParseLegacy(ref TypeParserArgs<Vector4> args, IDictionarySourceNode dictionary, string baseKey, out Vector4 value, out bool hadOneComp)
    {
        string xKey = _legacyXKey ?? _xKey ?? "X";
        string yKey = _legacyYKey ?? _yKey ?? "Y";
        string zKey = _legacyZKey ?? _zKey ?? "Z";
        string wKey = _legacyWKey ?? _wKey ?? "W";

        if (_skipLegacyCombineWithUnderscore)
        {
            xKey = baseKey + xKey;
            yKey = baseKey + yKey;
            zKey = baseKey + zKey;
            wKey = baseKey + wKey;
        }
        else
        {
            xKey = baseKey + "_" + xKey;
            yKey = baseKey + "_" + yKey;
            zKey = baseKey + "_" + zKey;
            wKey = baseKey + "_" + wKey;
        }

        if (!dictionary.TryGetProperty(xKey, out IPropertySourceNode? xProperty)
            & !dictionary.TryGetProperty(yKey, out IPropertySourceNode? yProperty)
            & !dictionary.TryGetProperty(zKey, out IPropertySourceNode? zProperty)
            & !dictionary.TryGetProperty(wKey, out IPropertySourceNode? wProperty))
        {
            hadOneComp = false;
            value = default;
            return false;
        }

        hadOneComp = true;

        if (xProperty == null || yProperty == null || zProperty == null || wProperty == null)
        {
            if (xProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, xKey);
            }
            if (yProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, yKey);
            }
            if (zProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, zKey);
            }
            if (wProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, wKey);
            }

            value = default;
            return false;
        }

        return VectorTypes.TryParseFloatArg(ref args, out value.X, xProperty)
               & VectorTypes.TryParseFloatArg(ref args, out value.Y, yProperty)
               & VectorTypes.TryParseFloatArg(ref args, out value.Z, zProperty)
               & VectorTypes.TryParseFloatArg(ref args, out value.W, wProperty);
    }

    protected override bool TryParseFromDictionary(ref TypeParserArgs<Vector4> args, IDictionarySourceNode dictionary, out Vector4 value)
    {
        string xKey = _xKey ?? "X";
        string yKey = _yKey ?? "Y";
        string zKey = _zKey ?? "Z";
        string wKey = _wKey ?? "W";

        if (!dictionary.TryGetProperty(xKey, out IPropertySourceNode? xProperty)
            & !dictionary.TryGetProperty(yKey, out IPropertySourceNode? yProperty)
            & !dictionary.TryGetProperty(zKey, out IPropertySourceNode? zProperty)
            & !dictionary.TryGetProperty(wKey, out IPropertySourceNode? wProperty))
        {
            value = default;
            return false;
        }

        if (xProperty == null || yProperty == null || zProperty == null || wProperty == null)
        {
            if (xProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, xKey);
            }
            if (yProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, yKey);
            }
            if (zProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, zKey);
            }
            if (wProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, wKey);
            }

            value = default;
            return false;
        }


        return VectorTypes.TryParseFloatArg(ref args, out value.X, xProperty)
               & VectorTypes.TryParseFloatArg(ref args, out value.Y, yProperty)
               & VectorTypes.TryParseFloatArg(ref args, out value.Z, zProperty)
               & VectorTypes.TryParseFloatArg(ref args, out value.W, wProperty);
    }


    #region JSON

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        Vector4Kind kind = Vector4Kind.Unspecified;

        if (typeDefinition.TryGetProperty("Kind"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString()!, ignoreCase: true, out kind))
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToParseEnum,
                        nameof(Vector4Kind),
                        element.GetString(),
                        context.Length != 0 ? $"{owner.FullName}.{context}.Mode" : $"{owner.FullName}.Mode"
                    )
                );
            }
        }

        VectorTypeOptions mode = VectorTypeOptions.Default;

        if (typeDefinition.TryGetProperty("Mode"u8, out element)
            && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString()!, ignoreCase: true, out mode))
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToParseEnum,
                        nameof(VectorTypeOptions),
                        element.GetString(),
                        context.Length != 0 ? $"{owner.FullName}.{context}.Mode" : $"{owner.FullName}.Mode"
                    )
                );
            }
        }

        string? xKey = null, yKey = null, zKey = null, wKey = null, legacyXKey = null, legacyYKey = null, legacyZKey = null, legacyWKey = null;
        bool skipUnderscore = false;
        if (typeDefinition.TryGetProperty("XKey"u8, out element))
            xKey = element.GetString();
        if (typeDefinition.TryGetProperty("YKey"u8, out element))
            yKey = element.GetString();
        if (typeDefinition.TryGetProperty("ZKey"u8, out element))
            zKey = element.GetString();
        if (typeDefinition.TryGetProperty("WKey"u8, out element))
            wKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyXKey"u8, out element))
            legacyXKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyYKey"u8, out element))
            legacyYKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyZKey"u8, out element))
            legacyZKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyWKey"u8, out element))
            legacyWKey = element.GetString();
        if (typeDefinition.TryGetProperty("SkipLegacyCombineWithUnderscore"u8, out element) && element.ValueKind != JsonValueKind.Null)
            skipUnderscore = element.GetBoolean();

        return mode == VectorTypeOptions.Default && kind == Vector4Kind.Unspecified && xKey == null && yKey == null && zKey == null && wKey == null && legacyXKey == null && legacyYKey == null && legacyZKey == null && legacyWKey == null && !skipUnderscore
            ? Instance
            : new Vector4Type(kind, mode, xKey, yKey, zKey, wKey, legacyXKey, legacyYKey, legacyZKey, legacyWKey, skipUnderscore);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (Kind == Vector4Kind.Unspecified && Options == VectorTypeOptions.Default && _xKey == null && _yKey == null && _zKey == null && _wKey == null && _legacyXKey == null && _legacyYKey == null && _legacyZKey == null && _legacyWKey == null && !_skipLegacyCombineWithUnderscore)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        WriteOptions(writer);

        if (Kind != Vector4Kind.Unspecified)
            writer.WriteString("Kind"u8, Kind.ToString());

        if (_xKey != null) writer.WriteString("XKey"u8, _xKey);
        if (_yKey != null) writer.WriteString("YKey"u8, _yKey);
        if (_zKey != null) writer.WriteString("ZKey"u8, _zKey);
        if (_wKey != null) writer.WriteString("WKey"u8, _wKey);

        if (_legacyXKey != null) writer.WriteString("LegacyXKey"u8, _legacyXKey);
        if (_legacyYKey != null) writer.WriteString("LegacyYKey"u8, _legacyYKey);
        if (_legacyZKey != null) writer.WriteString("LegacyZKey"u8, _legacyZKey);
        if (_legacyWKey != null) writer.WriteString("LegacyWKey"u8, _legacyWKey);

        if (_skipLegacyCombineWithUnderscore) writer.WriteBoolean("SkipLegacyCombineWithUnderscore"u8, true);

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(Vector4Type other)
    {
        if (ReferenceEquals(other, this))
            return true;

        return EqualsHelper(other)
               && other.Kind == Kind
               && other._skipLegacyCombineWithUnderscore == _skipLegacyCombineWithUnderscore
               && string.Equals(_xKey, other._xKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_yKey, other._yKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_zKey, other._zKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_wKey, other._wKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyXKey, other._legacyXKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyYKey, other._legacyYKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyZKey, other._legacyZKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyWKey, other._legacyWKey, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        HashCode hc = new HashCode();
        hc.Add(1544051181);
        hc.Add(base.GetHashCode());
        hc.Add(Kind);
        hc.Add(_skipLegacyCombineWithUnderscore);
        hc.Add(_xKey);
        hc.Add(_yKey);
        hc.Add(_zKey);
        hc.Add(_wKey);
        hc.Add(_legacyXKey);
        hc.Add(_legacyYKey);
        hc.Add(_legacyZKey);
        hc.Add(_legacyWKey);
        return hc.ToHashCode();
    }
}

/// <summary>
/// Defines what kind/unit of <see cref="Vector4"/> that's being parsed.
/// </summary>
public enum Vector4Kind
{
    /// <summary>
    /// Any kind of 4-component vector.
    /// </summary>
    Unspecified,

    /// <summary>
    /// A quaternion rotation.
    /// </summary>
    Quaternion
}