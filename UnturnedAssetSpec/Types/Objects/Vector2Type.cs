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
/// Type for all 2-component vector types.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="VectorTypeOptions"/> Mode</c> - Acceptable syntaxes (comma-separated).</item>
///     <item><c><see cref="Vector2Kind"/> Kind</c> - Kind/unit of value to parse.</item>
///     <item><c><see cref="string"/> XKey</c> - Overrides the "X" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> YKey</c> - Overrides the "Y" property name for modern (and maybe legacy) parsing.</item>
///     <item><c><see cref="string"/> LegacyXKey</c> - Overrides the "X" property name for legacy parsing. Defaults to "XKey".</item>
///     <item><c><see cref="string"/> LegacyYKey</c> - Overrides the "Y" property name for legacy parsing. Defaults to "YKey".</item>
///     <item><c><see cref="bool"/> SkipLegacyCombineWithUnderscore</c> - If <see langword="true"/>, the '_' placed between the base key and the defined legacy key will be excluded.</item>
/// </list>
/// </para>
/// </summary>
public sealed class Vector2Type : BaseVectorType<Vector2, Vector2Type>
{
    public const string TypeId = "Vector2";

    public static readonly Vector2Type Instance = new Vector2Type();
    static Vector2Type() { }

    private readonly string? _xKey;
    private readonly string? _yKey;
    private readonly string? _legacyXKey;
    private readonly string? _legacyYKey;
    private readonly bool _skipLegacyCombineWithUnderscore;

    public Vector2Kind Kind { get; }

    public override string Id => TypeId;

    public override string DisplayName => Resources.Type_Name_Vector2_Unspecified;

    public static ITypeFactory Factory => Instance;

    public Vector2Type() : this(Vector2Kind.Unspecified, VectorTypeOptions.Object | VectorTypeOptions.String) { }

    public Vector2Type(Vector2Kind kind, VectorTypeOptions options) : base(options)
    {
        if (kind != Vector2Kind.Unspecified)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
    }

    public Vector2Type(Vector2Kind kind, VectorTypeOptions options, string? xKey, string? yKey, string? legacyXKey = null, string? legacyYKey = null, bool skipLegacyCombineWithUnderscore = false)
        : this(kind, options)
    {
        if (kind != Vector2Kind.Unspecified)
            throw new ArgumentOutOfRangeException(nameof(kind));

        _xKey = xKey;
        _yKey = yKey;
        _legacyXKey = legacyXKey;
        _legacyYKey = legacyYKey;
        _skipLegacyCombineWithUnderscore = skipLegacyCombineWithUnderscore;
    }

    protected override string ToString(Vector2 vector)
    {
        return KnownTypeValueHelper.ToComponentString(vector);
    }

    protected override bool TryParseFromString(ReadOnlySpan<char> text, out Vector2 value, ISourceNode? property, ref TypeConverterParseArgs<Vector2> args)
    {
        return KnownTypeValueHelper.TryParseVector2Components(text, out value);
    }

    protected override bool TryParseLegacy(ref TypeParserArgs<Vector2> args, IDictionarySourceNode dictionary, string baseKey, out Vector2 value, out bool hadOneComp)
    {
        string xKey = _legacyXKey ?? _xKey ?? "X";
        string yKey = _legacyYKey ?? _yKey ?? "Y";

        if (_skipLegacyCombineWithUnderscore)
        {
            xKey = baseKey + xKey;
            yKey = baseKey + yKey;
        }
        else
        {
            xKey = baseKey + "_" + xKey;
            yKey = baseKey + "_" + yKey;
        }

        if (!dictionary.TryGetProperty(xKey, out IPropertySourceNode? xProperty)
            & !dictionary.TryGetProperty(yKey, out IPropertySourceNode? yProperty))
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
        }

        hadOneComp = true;
        if (xProperty == null || yProperty == null)
        {
            if (xProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, xKey);
            }
            if (yProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, yKey);
            }

            value = default;
            return false;
        }

        return VectorTypes.TryParseArg(ref args, out value.X, xProperty)
               & VectorTypes.TryParseArg(ref args, out value.Y, yProperty);
    }

    protected override bool TryParseFromDictionary(ref TypeParserArgs<Vector2> args, IDictionarySourceNode dictionary, out Vector2 value)
    {
        string xKey = _xKey ?? "X";
        string yKey = _yKey ?? "Y";

        if (!dictionary.TryGetProperty(xKey, out IPropertySourceNode? xProperty) & !dictionary.TryGetProperty(yKey, out IPropertySourceNode? yProperty))
        {
            value = default;
            return false;
        }

        if (xProperty == null || yProperty == null)
        {
            if (xProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, xKey);
            }
            if (yProperty == null)
            {
                args.DiagnosticSink?.UNT1007(ref args, args.ParentNode, yKey);
            }

            value = default;
            return false;
        }

        return VectorTypes.TryParseArg(ref args, out value.X, xProperty) & VectorTypes.TryParseArg(ref args, out value.Y, yProperty);
    }


    #region JSON

    protected override IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        Vector2Kind kind = Vector2Kind.Unspecified;

        if (typeDefinition.TryGetProperty("Kind"u8, out JsonElement element)
            && element.ValueKind != JsonValueKind.Null)
        {
            if (!Enum.TryParse(element.GetString()!, ignoreCase: true, out kind))
            {
                throw new JsonException(
                    string.Format(
                        Resources.JsonException_FailedToParseEnum,
                        nameof(Vector2Kind),
                        element.GetString(),
                        context.Length != 0 ? $"{owner.FullName}.{context}.Mode" : $"{owner.FullName}.Mode"
                    )
                );
            }
        }

        VectorTypeOptions mode = ReadOptions(in typeDefinition, owner, context);

        string? xKey = null, yKey = null, legacyXKey = null, legacyYKey = null;
        bool skipUnderscore = false;
        if (typeDefinition.TryGetProperty("XKey"u8, out element))
            xKey = element.GetString();
        if (typeDefinition.TryGetProperty("YKey"u8, out element))
            yKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyXKey"u8, out element))
            legacyXKey = element.GetString();
        if (typeDefinition.TryGetProperty("LegacyYKey"u8, out element))
            legacyYKey = element.GetString();
        if (typeDefinition.TryGetProperty("SkipLegacyCombineWithUnderscore"u8, out element) && element.ValueKind != JsonValueKind.Null)
            skipUnderscore = element.GetBoolean();

        return mode == VectorTypeOptions.Default && kind == Vector2Kind.Unspecified && xKey == null && yKey == null && legacyXKey == null && legacyYKey == null && !skipUnderscore
            ? Instance
            : new Vector2Type(kind, mode, xKey, yKey, legacyXKey, legacyYKey, skipUnderscore);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (Kind == Vector2Kind.Unspecified && Options == VectorTypeOptions.Default && _xKey == null && _yKey == null && _legacyXKey == null && _legacyYKey == null && !_skipLegacyCombineWithUnderscore)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        WriteOptions(writer);
        
        if (Kind != Vector2Kind.Unspecified)
            writer.WriteString("Kind"u8, Kind.ToString());

        if (_xKey != null) writer.WriteString("XKey"u8, _xKey);
        if (_yKey != null) writer.WriteString("YKey"u8, _yKey);

        if (_legacyXKey != null) writer.WriteString("LegacyXKey"u8, _legacyXKey);
        if (_legacyYKey != null) writer.WriteString("LegacyYKey"u8, _legacyYKey);

        if (_skipLegacyCombineWithUnderscore) writer.WriteBoolean("SkipLegacyCombineWithUnderscore"u8, true);
        
        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(Vector2Type other)
    {
        if (ReferenceEquals(other, this))
            return true;

        return EqualsHelper(other)
               && other.Kind == Kind
               && other._skipLegacyCombineWithUnderscore == _skipLegacyCombineWithUnderscore
               && string.Equals(_xKey, other._xKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_yKey, other._yKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyXKey, other._legacyXKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(_legacyYKey, other._legacyYKey, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(173125923, base.GetHashCode(), Kind, _skipLegacyCombineWithUnderscore, _xKey, _yKey, _legacyXKey, _legacyYKey);
    }
}

/// <summary>
/// Defines what kind/unit of <see cref="Vector2"/> that's being parsed.
/// </summary>
public enum Vector2Kind
{
    /// <summary>
    /// Any kind of 2-component vector.
    /// </summary>
    Unspecified
}