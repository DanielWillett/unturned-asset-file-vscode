using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A reference to one or more types of assets formatted as either a string or object.
/// Only accepts <see cref="Guid"/> IDs, not <see cref="ushort"/> IDs.
/// <para>Example: <c>AirdropAsset.Landed_Barricade</c></para>
/// <code>
/// // string
/// Prop fe71781c60314468b22c6b0642a51cd9
///
/// // object
/// Prop
/// {
///     GUID fe71781c60314468b22c6b0642a51cd9
/// }
///
/// // this
/// Prop this
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="QualifiedType"/> BaseType or <see cref="QualifiedType"/>[] BaseTypes</c> - One or more allowed base types.</item>
///     <item><c><see cref="bool"/> SupportsThis</c> - Whether or not the <see langword="this"/> keyword can be used to refer to the current file's asset.</item>
///     <item><c><see cref="bool"/> PreventSelfReference</c> - Whether or not a warning will be logged if the value is the same as the current file's asset.</item>
/// </list>
/// </para>
/// </summary>
public sealed class AssetReferenceType : BaseType<Guid, AssetReferenceType>, ITypeParser<Guid>, ITypeFactory
{
    private static readonly AssetReferenceType?[] DefaultInstances = new AssetReferenceType?[(int)AssetReferenceKind.Object];

    /// <summary>
    /// Gets the default instance of a certain kind of asset reference.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static AssetReferenceType GetInstance(AssetReferenceKind kind)
    {
        if (kind is <= AssetReferenceKind.Unspecified or > AssetReferenceKind.Object)
            throw new ArgumentOutOfRangeException(nameof(kind));
        ref AssetReferenceType? t = ref DefaultInstances[(int)kind - 1];
        AssetReferenceType? r = t;
        if (r != null)
            return r;

        Interlocked.CompareExchange(ref t, new AssetReferenceType(kind), null);
        return t;
    }

    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_AssetReference,
        Resources.Type_Name_AssetReferenceString,
        Resources.Type_Name_AssetReference
    ];

    private static readonly string[] DisplayNamesFormattable =
    [
        Resources.Type_Name_AssetReference_Type,
        Resources.Type_Name_AssetReferenceString_Type,
        Resources.Type_Name_AssetReference_Type
    ];

    /// <summary>
    /// Type IDs of this type indexed by <see cref="AssetReferenceKind"/>.
    /// </summary>
    public static readonly ImmutableArray<string> TypeIds = ImmutableArray.Create<string>
    (
        "AssetReference",
        "AssetReferenceString",
        "AssetReference"
    );

    public static ITypeFactory Factory => GetInstance(AssetReferenceKind.Object);

    private readonly OneOrMore<QualifiedType> _baseTypes;

    public AssetReferenceKind Kind { get; }

    public override string Id => TypeIds[(int)Kind];

    public override string DisplayName { get; }

    public override ITypeParser<Guid> Parser => this;

    public bool SupportsThis { get; }
    public bool PreventSelfReference { get; }

    public AssetReferenceType() : this(AssetReferenceKind.Object) { }

    public AssetReferenceType(AssetReferenceKind kind, bool supportsThis = false, bool preventSelfReference = false)
    {
        if (kind is <= AssetReferenceKind.Unspecified or > AssetReferenceKind.Object)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        _baseTypes = OneOrMore<QualifiedType>.Null;
        DisplayName = DisplayNames[(int)Kind];
        SupportsThis = supportsThis;
        PreventSelfReference = preventSelfReference;
    }

    public AssetReferenceType(AssetReferenceKind kind, OneOrMore<QualifiedType> baseTypes, bool supportsThis = false, bool preventSelfReference = false)
    {
        if (kind is <= AssetReferenceKind.Unspecified or > AssetReferenceKind.Object)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        _baseTypes = baseTypes;
        SupportsThis = supportsThis;
        PreventSelfReference = preventSelfReference;
        DisplayName = AssetReferenceHelper.GetDisplayName(baseTypes, DisplayNames[(int)kind], DisplayNamesFormattable[(int)kind]);
    }

    public bool TryParse(ref TypeParserArgs<Guid> args, in FileEvaluationContext ctx, out Optional<Guid> value)
    {
        value = Optional<Guid>.Null;

        switch (args.ValueNode)
        {
            default:
                if (args.MissingValueBehavior != TypeParserMissingValueBehavior.FallbackToDefaultValue)
                {
                    args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                }
                else
                {
                    if (args.Property?.GetIncludedDefaultValue(args.ParentNode is IPropertySourceNode) is { } defValue)
                    {
                        return defValue.TryGetValueAs(in ctx, out value);
                    }

                    return false;
                }
                break;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                break;

            case IValueSourceNode v:
                if (!KnownTypeValueHelper.TryParseGuid(v.Value, out Guid guid))
                {
                    args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                    return false;
                }

                value = guid;
                return true;

            case IDictionarySourceNode d:
                if (Kind == AssetReferenceKind.String)
                {
                    args.DiagnosticSink?.UNT2004_AssetReferenceStringOnly(ref args, args.Type, args.ParentNode);
                    return false;
                }

                if (!d.TryGetProperty("GUID", out IPropertySourceNode? guidNode))
                {
                    args.DiagnosticSink?.UNT1007(ref args, d, "GUID");
                    return false;
                }

                args.ReferencedPropertySink?.AcceptReferencedProperty(guidNode);

                args.CreateSubTypeParserArgs(out TypeParserArgs<Guid> guidArgs, guidNode.Value, guidNode, this, PropertyResolutionContext.Modern);
                switch (guidNode.Value)
                {
                    default:
                        args.DiagnosticSink?.UNT2004_NoValue(ref guidArgs, guidNode);
                        return false;

                    case IListSourceNode list:
                        args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref guidArgs, list, this);
                        return false;

                    case IDictionarySourceNode dict:
                        args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref guidArgs, dict, this);
                        return false;

                    case IValueSourceNode guidValue:
                        if (!KnownTypeValueHelper.TryParseGuid(guidValue.Value, out guid))
                        {
                            args.DiagnosticSink?.UNT2004_Generic(ref args, guidValue.Value, args.Type);
                            return false;
                        }

                        value = guid;
                        return true;
                }
        }

        return false;
    }

    #region JSON

    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<Guid> value,
        IType<Guid> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        return TypeParsers.Guid.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, Guid value, IType<Guid> valueType, JsonSerializerOptions options)
    {
        TypeParsers.Guid.WriteValueToJson(writer, value, valueType, options);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        AssetReferenceKind mode = AssetReferenceKind.Unspecified;
        for (int i = 1; i < TypeIds.Length; ++i)
        {
            if (!typeId.Equals(TypeIds[i], StringComparison.Ordinal))
                continue;

            mode = (AssetReferenceKind)i;
            break;
        }

        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return GetInstance(mode);
        }

        AssetReferenceHelper.ReadCommonJsonProperties(in typeDefinition, out OneOrMore<QualifiedType> baseTypes, out bool allowThis, out bool preventSelfRef, out bool isDefault);

        return isDefault
            ? GetInstance(mode)
            : new AssetReferenceType(mode, baseTypes, allowThis, preventSelfRef);
    }

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_baseTypes.IsNull && !SupportsThis && !PreventSelfReference)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();

        WriteTypeName(writer);
        AssetReferenceHelper.WriteCommonJsonProperties(writer, _baseTypes, SupportsThis, PreventSelfReference);

        writer.WriteEndObject();
    }

    #endregion

    protected override bool Equals(AssetReferenceType other)
    {
        return other.Kind == Kind && other._baseTypes.Equals(_baseTypes) && other.SupportsThis == SupportsThis && other.PreventSelfReference == PreventSelfReference;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(1499429627, Kind, _baseTypes.Value, SupportsThis, PreventSelfReference);
    }
}