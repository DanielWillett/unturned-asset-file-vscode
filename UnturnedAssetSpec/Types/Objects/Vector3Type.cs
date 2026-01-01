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
/// Type for all 3-component vector types.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="VectorTypeOptions"/> Mode</c> - Acceptable syntaxes (comma-separated).</item>
///     <item><c><see cref="Vector3Kind"/> Kind</c> - Kind/unit of value to parse.</item>
///     <item><c><see cref="string"/> XKey</c> - Overrides the "X" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> YKey</c> - Overrides the "Y" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> ZKey</c> - Overrides the "Z" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> LegacyXKey</c> - Overrides the "X" property name for legacy parsing. Defaults to "XKey".</item>
///     <item><c><see cref="string"/> LegacyYKey</c> - Overrides the "Y" property name for legacy parsing. Defaults to "YKey".</item>
///     <item><c><see cref="string"/> LegacyZKey</c> - Overrides the "Z" property name for legacy parsing. Defaults to "ZKey".</item>
///     <item><c><see cref="bool"/> SkipLegacyCombineWithUnderscore</c> - If <see langword="true"/>, the '_' placed between the base key and the defined legacy key will be excluded.</item>
/// </list>
/// </para>
/// </summary>
public sealed class Vector3Type : BaseVectorType<Vector3, Vector3Type>
{
    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_Vector3_Unspecified,
        Resources.Type_Name_Vector3_Position,
        Resources.Type_Name_Vector3_Rotation,
        Resources.Type_Name_Vector3_Scale
    ];

    public const string TypeId = "Vector3";

    public static readonly Vector3Type Instance = new Vector3Type();

    static Vector3Type() { }

    private readonly string? _xKey;
    private readonly string? _yKey;
    private readonly string? _zKey;
    private readonly string? _legacyXKey;
    private readonly string? _legacyYKey;
    private readonly string? _legacyZKey;
    private readonly bool _skipLegacyCombineWithUnderscore;

    public Vector3Kind Kind { get; }

    public override string Id => TypeId;

    public override string DisplayName => DisplayNames[(int)Kind];

    public static ITypeFactory Factory => Instance;


    public Vector3Type() : this(Vector3Kind.Unspecified, VectorTypeOptions.Object | VectorTypeOptions.String) { }

    public Vector3Type(Vector3Kind kind, VectorTypeOptions options) : base(options)
    {
        if (kind is < Vector3Kind.Unspecified or > Vector3Kind.Scale)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
    }

    public Vector3Type(Vector3Kind kind, VectorTypeOptions options, string? xKey, string? yKey, string? zKey, string? legacyXKey = null, string? legacyYKey = null, string? legacyZKey = null, bool skipLegacyCombineWithUnderscore = false)
        : this(kind, options)
    {
        if (kind is < Vector3Kind.Unspecified or >= Vector3Kind.Scale)
            throw new ArgumentOutOfRangeException(nameof(kind));

        _xKey = xKey;
        _yKey = yKey;
        _zKey = zKey;
        _legacyXKey = legacyXKey;
        _legacyYKey = legacyYKey;
        _legacyZKey = legacyZKey;
        _skipLegacyCombineWithUnderscore = skipLegacyCombineWithUnderscore;
    }

    protected override string ToString(Vector3 vector)
    {
        return KnownTypeValueHelper.ToComponentString(vector);
    }

    protected override bool TryParseFromString(ReadOnlySpan<char> text, out Vector3 value, ISourceNode? property, ref TypeConverterParseArgs<Vector3> args)
    {
        return KnownTypeValueHelper.TryParseVector3Components(text, out value);
    }

    protected override bool TryParseLegacy(ref TypeParserArgs<Vector3> args, IDictionarySourceNode dictionary, string baseKey, out Vector3 value, out bool hadOneComp)
    {
        string xKey = _legacyXKey ?? _xKey ?? "X";
        string yKey = _legacyYKey ?? _yKey ?? "Y";
        string zKey = _legacyZKey ?? _zKey ?? "Z";

        if (_skipLegacyCombineWithUnderscore)
        {
            xKey = baseKey + xKey;
            yKey = baseKey + yKey;
            zKey = baseKey + zKey;
        }
        else
        {
            xKey = baseKey + "_" + xKey;
            yKey = baseKey + "_" + yKey;
            zKey = baseKey + "_" + zKey;
        }

        if (!dictionary.TryGetProperty(xKey, out IPropertySourceNode? xProperty)
            & !dictionary.TryGetProperty(yKey, out IPropertySourceNode? yProperty)
            & !dictionary.TryGetProperty(zKey, out IPropertySourceNode? zProperty))
        {
            hadOneComp = false;
            value = default;
            return false;
        }

        if (args.ReferencedPropertySink != null)
        {
            if (xProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(xProperty);
            if (yProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(yProperty);
            if (zProperty != null)
                args.ReferencedPropertySink.AcceptReferencedProperty(zProperty);
        }

        hadOneComp = true;

        if (xProperty == null || yProperty == null || zProperty == null)
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

            value = default;
            return false;
        }

        return VectorTypes.TryParseArg(ref args, out value.X, xProperty)
               & VectorTypes.TryParseArg(ref args, out value.Y, yProperty)
               & VectorTypes.TryParseArg(ref args, out value.Z, zProperty);
    }

    protected override bool TryParseFromDictionary(ref TypeParserArgs<Vector3> args, IDictionarySourceNode dictionary, out Vector3 value)
    {
        string xKey = _xKey ?? "X";
        string yKey = _yKey ?? "Y";
        string zKey = _zKey ?? "Z";

        if (!dictionary.TryGetProperty(xKey, out IPropertySourceNode? xProperty)
            & !dictionary.TryGetProperty(yKey, out IPropertySourceNode? yProperty)
            & !dictionary.TryGetProperty(zKey, out IPropertySourceNode? zProperty))
        {
            value = default;
            return false;
        }

        if (xProperty == null || yProperty == null || zProperty == null)
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

            value = default;
            return false;
        }


        return VectorTypes.TryParseArg(ref args, out value.X, xProperty)
               & VectorTypes.TryParseArg(ref args, out value.Y, yProperty)
               & VectorTypes.TryParseArg(ref args, out value.Z, zProperty);
    }


    #region JSON

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        Vector3Kind kind = Vector3Kind.Unspecified;

        if (typeDefinition.TryGetProperty("Kind"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString()!, ignoreCase: true, out kind))
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToParseEnum,
                        nameof(Vector3Kind),
                        element.GetString(),
                        context.Length != 0 ? $"{owner.FullName}.{context}.Mode" : $"{owner.FullName}.Mode"
                    )
                );
            }
        }

        VectorTypeOptions mode = ReadOptions(in typeDefinition, owner, context);

        string? xKey = null, yKey = null, zKey = null, legacyXKey = null, legacyYKey = null, legacyZKey = null;
        bool skipUnderscore = false;
        if (typeDefinition.TryGetProperty("XKey"u8, out element))
            xKey = element.GetString();
        if (typeDefinition.TryGetProperty("YKey"u8, out element))
            yKey = element.GetString();
        if (typeDefinition.TryGetProperty("ZKey"u8, out element))
            zKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyXKey"u8, out element))
            legacyXKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyYKey"u8, out element))
            legacyYKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyZKey"u8, out element))
            legacyZKey = element.GetString();
        if (typeDefinition.TryGetProperty("SkipLegacyCombineWithUnderscore"u8, out element) && element.ValueKind != JsonValueKind.Null)
            skipUnderscore = element.GetBoolean();

        return mode == VectorTypeOptions.Default && kind == Vector3Kind.Unspecified && xKey == null && yKey == null && zKey == null && legacyXKey == null && legacyYKey == null && legacyZKey == null && !skipUnderscore
            ? Instance
            : new Vector3Type(kind, mode, xKey, yKey, zKey, legacyXKey, legacyYKey, legacyZKey, skipUnderscore);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (Kind == Vector3Kind.Unspecified && Options == VectorTypeOptions.Default && _xKey == null && _yKey == null && _zKey == null && _legacyXKey == null && _legacyYKey == null && _legacyZKey == null && !_skipLegacyCombineWithUnderscore)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        WriteOptions(writer);

        if (Kind != Vector3Kind.Unspecified)
            writer.WriteString("Kind"u8, Kind.ToString());

        if (_xKey != null) writer.WriteString("XKey"u8, _xKey);
        if (_yKey != null) writer.WriteString("YKey"u8, _yKey);
        if (_zKey != null) writer.WriteString("ZKey"u8, _zKey);

        if (_legacyXKey != null) writer.WriteString("LegacyXKey"u8, _legacyXKey);
        if (_legacyYKey != null) writer.WriteString("LegacyYKey"u8, _legacyYKey);
        if (_legacyZKey != null) writer.WriteString("LegacyZKey"u8, _legacyZKey);

        if (_skipLegacyCombineWithUnderscore) writer.WriteBoolean("SkipLegacyCombineWithUnderscore"u8, true);

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(Vector3Type other)
    {
        if (ReferenceEquals(other, this))
            return true;

        return EqualsHelper(other)
               && other.Kind == Kind
               && other._skipLegacyCombineWithUnderscore == _skipLegacyCombineWithUnderscore
               && string.Equals(_xKey, other._xKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_yKey, other._yKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_zKey, other._zKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyXKey, other._legacyXKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyYKey, other._legacyYKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyZKey, other._legacyZKey, StringComparison.OrdinalIgnoreCase);
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
        hc.Add(_legacyXKey);
        hc.Add(_legacyYKey);
        hc.Add(_legacyZKey);
        return hc.ToHashCode();
    }
}

/// <summary>
/// Defines what kind/unit of <see cref="Vector3"/> that's being parsed.
/// </summary>
public enum Vector3Kind
{
    /// <summary>
    /// Any kind of 3-component vector.
    /// </summary>
    Unspecified,

    /// <summary>
    /// A relative or absolute position in 3D space.
    /// </summary>
    Position,

    /// <summary>
    /// A euler rotation in degrees.
    /// </summary>
    Rotation,

    /// <summary>
    /// A scale multiplier for the size of an object along each axis.
    /// </summary>
    Scale
}