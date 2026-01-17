using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A backwards-compatable reference to one or more types of assets formatted as either a string or sometimes as an object.
/// Accepts both <see cref="Guid"/> and <see cref="ushort"/> IDs, although in some cases the ID can't be assumed and a <see cref="AssetCategory"/> also has to be specified ('Type').
/// <para>Example: <c>ItemVehicleLockpickToolAsset.FailureEffect</c></para>
/// <code>
/// // string
/// Prop fe71781c60314468b22c6b0642a51cd9
/// // if category can be assumed
/// Prop 1374
/// // if category can't be assumed
/// Prop ITEM:1374
///
/// // object
/// Prop
/// {
///     GUID fe71781c60314468b22c6b0642a51cd9
/// }
/// Prop
/// {
///     Type Item
///     ID 1374
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
/// <para>
/// If an amount is supppled (i.e. "102 x 3") a warning will be logged.
/// </para>
/// </summary>
public sealed class BackwardsCompatibleAssetReferenceType : BaseType<GuidOrId, BackwardsCompatibleAssetReferenceType>, ITypeParser<GuidOrId>, ITypeFactory
{
    private static readonly BackwardsCompatibleAssetReferenceType?[] DefaultInstances = new BackwardsCompatibleAssetReferenceType?[(int)BackwardsCompatibleAssetReferenceKind.BcAssetReferenceString + 1];

    private readonly AssetCategoryValue _defaultCategory;


    /// <summary>
    /// Gets the default instance of a certain kind of asset reference.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    public static BackwardsCompatibleAssetReferenceType GetInstance(BackwardsCompatibleAssetReferenceKind kind)
    {
        if (kind is <= BackwardsCompatibleAssetReferenceKind.GuidOrLegacyId or > BackwardsCompatibleAssetReferenceKind.BcAssetReferenceString)
            throw new ArgumentOutOfRangeException(nameof(kind));
        ref BackwardsCompatibleAssetReferenceType? t = ref DefaultInstances[(int)kind];
        BackwardsCompatibleAssetReferenceType? r = t;
        if (r != null)
            return r;

        Interlocked.CompareExchange(ref t, new BackwardsCompatibleAssetReferenceType(kind), null);
        return t;
    }

    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_BackwardsCompatibleAssetReferenceGuidOrLegacyId,
        Resources.Type_Name_BackwardsCompatibleAssetReferenceBcAssetReference,
        Resources.Type_Name_BackwardsCompatibleAssetReferenceBcAssetReferenceString
    ];

    private static readonly string[] DisplayNamesFormattable =
    [
        Resources.Type_Name_BackwardsCompatibleAssetReferenceGuidOrLegacyId_Type,
        Resources.Type_Name_BackwardsCompatibleAssetReferenceBcAssetReference_Type,
        Resources.Type_Name_BackwardsCompatibleAssetReferenceBcAssetReferenceString_Type
    ];

    /// <summary>
    /// Type IDs of this type indexed by <see cref="BackwardsCompatibleAssetReferenceKind"/>.
    /// </summary>
    public static readonly ImmutableArray<string> TypeIds = ImmutableArray.Create<string>
    (
        "LegacyAssetReferenceString",
        "BcAssetReference",
        "BcAssetReferenceString"
    );

    public static ITypeFactory Factory => GetInstance(BackwardsCompatibleAssetReferenceKind.GuidOrLegacyId);


    private readonly OneOrMore<QualifiedType> _baseTypes;

    public BackwardsCompatibleAssetReferenceKind Kind { get; }

    public override string Id => TypeIds[(int)Kind];

    public override string DisplayName { get; }

    public override ITypeParser<GuidOrId> Parser => this;

    public bool SupportsThis { get; }
    public bool PreventSelfReference { get; }

    /// <summary>
    /// The default category used to parse ID strings, if any.
    /// </summary>
    public AssetCategoryValue DefaultCategory => _defaultCategory;

    public BackwardsCompatibleAssetReferenceType() : this(BackwardsCompatibleAssetReferenceKind.GuidOrLegacyId) { }

    public BackwardsCompatibleAssetReferenceType(BackwardsCompatibleAssetReferenceKind kind, bool supportsThis = false, bool preventSelfReference = false)
    {
        if (kind is <= BackwardsCompatibleAssetReferenceKind.GuidOrLegacyId or > BackwardsCompatibleAssetReferenceKind.BcAssetReferenceString)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        _baseTypes = OneOrMore<QualifiedType>.Null;
        _defaultCategory = AssetCategoryValue.None;
        DisplayName = DisplayNames[(int)Kind];
        SupportsThis = supportsThis;
        PreventSelfReference = preventSelfReference;
    }

    public BackwardsCompatibleAssetReferenceType(BackwardsCompatibleAssetReferenceKind kind, OneOrMore<QualifiedType> baseTypes, IDatSpecificationReadContext spec, bool supportsThis = false, bool preventSelfReference = false)
    {
        if (kind is <= BackwardsCompatibleAssetReferenceKind.GuidOrLegacyId or > BackwardsCompatibleAssetReferenceKind.BcAssetReferenceString)
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        _baseTypes = baseTypes;
        SupportsThis = supportsThis;
        PreventSelfReference = preventSelfReference;
        DisplayName = AssetReferenceHelper.GetDisplayName(baseTypes, DisplayNames[(int)kind], DisplayNamesFormattable[(int)kind]);
        _defaultCategory = AssetReferenceHelper.GetDefaultCategory(baseTypes, spec);
    }

    public bool TryParse(ref TypeParserArgs<GuidOrId> args, in FileEvaluationContext ctx, out Optional<GuidOrId> value)
    {
        value = Optional<GuidOrId>.Null;

        switch (args.ValueNode)
        {
            case null:
                args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                break;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                break;

            case IValueSourceNode v:
                GuidOrId guidOrId;
                if (Kind is not BackwardsCompatibleAssetReferenceKind.BcAssetReference and not BackwardsCompatibleAssetReferenceKind.BcAssetReferenceString)
                {
                    if (!GuidOrId.TryParse(v.Value, out guidOrId))
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                        return false;
                    }

                    if (guidOrId.IsId && _defaultCategory != AssetCategoryValue.None)
                    {
                        guidOrId = new GuidOrId(guidOrId.Id, _defaultCategory);
                    }
                }
                else
                {
                    if (!KnownTypeValueHelper.TryParseGuidOrId(v.Value, _defaultCategory, out guidOrId))
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                        return false;
                    }
                }

                value = guidOrId;
                return true;

            case IDictionarySourceNode d:
                if (Kind != BackwardsCompatibleAssetReferenceKind.BcAssetReference)
                {
                    args.DiagnosticSink?.UNT2004_BackwardsCompatibleAssetReferenceStringOnly(ref args, args.Type, args.ParentNode);
                    return false;
                }

                if (d.TryGetProperty("GUID", out IPropertySourceNode? guidNode))
                {
                    // Parse GUID dictionary
                    args.ReferencedPropertySink?.AcceptReferencedProperty(guidNode);

                    args.CreateSubTypeParserArgs(out TypeParserArgs<Guid> guidArgs, guidNode.Value, guidNode, GuidType.Instance, LegacyExpansionFilter.Modern);
                    if (!TypeParsers.Guid.TryParse(ref guidArgs, in ctx, out Optional<Guid> guid))
                    {
                        return false;
                    }

                    value = guid.HasValue ? new Optional<GuidOrId>(new GuidOrId(guid.Value)) : Optional<GuidOrId>.Null;
                    return true;
                }

                bool hasType = d.TryGetProperty("Type", out IPropertySourceNode? typeNode);
                bool hasId = d.TryGetProperty("ID", out IPropertySourceNode? idNode);

                if (hasType)
                {
                    args.ReferencedPropertySink?.AcceptReferencedProperty(typeNode!);
                    if (!hasId)
                    {
                        args.DiagnosticSink?.UNT1007(ref args, d, "ID");
                    }
                    else
                    {
                        // Parse Type + ID dictionary
                        args.ReferencedPropertySink?.AcceptReferencedProperty(idNode!);

                        args.CreateSubTypeParserArgs(out TypeParserArgs<string> guidArgs, typeNode!.Value, typeNode, StringType.Instance, LegacyExpansionFilter.Modern);
                        if (!TypeParsers.String.TryParse(ref guidArgs, in ctx, out Optional<string> typeString))
                        {
                            return false;
                        }

                        if (!typeString.HasValue || !AssetCategory.TryParse(typeString.Value, out int categoryIndex) || categoryIndex == 0)
                        {
                            args.DiagnosticSink?.UNT1014(ref args, typeString.Value ?? string.Empty);
                            categoryIndex = -1;
                        }

                        args.CreateSubTypeParserArgs(out TypeParserArgs<ushort> ushortArgs, idNode!.Value, idNode, UInt16Type.Instance, LegacyExpansionFilter.Modern);
                        if (!TypeParsers.UInt16.TryParse(ref ushortArgs, in ctx, out Optional<ushort> id) || categoryIndex < 0 || !id.HasValue)
                        {
                            return false;
                        }

                        value = id.Value == 0
                            ? new Optional<GuidOrId>(GuidOrId.Empty)
                            : new Optional<GuidOrId>(new GuidOrId(id.Value, new AssetCategoryValue(categoryIndex)));
                        return true;
                    }
                }
                else if (hasId)
                {
                    args.ReferencedPropertySink?.AcceptReferencedProperty(idNode!);
                    args.DiagnosticSink?.UNT1007(ref args, d, "Type");
                }
                else
                {
                    args.DiagnosticSink?.UNT1007(ref args, d, "GUID");
                }

                break;
        }

        return false;
    }

    #region JSON

    public bool TryReadValueFromJson(in JsonElement json, out Optional<GuidOrId> value, IType<GuidOrId> valueType)
    {
        return TypeParsers.GuidOrId.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, GuidOrId value, IType<GuidOrId> valueType, JsonSerializerOptions options)
    {
        TypeParsers.GuidOrId.WriteValueToJson(writer, value, valueType, options);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        BackwardsCompatibleAssetReferenceKind mode = BackwardsCompatibleAssetReferenceKind.GuidOrLegacyId;
        for (int i = 1; i < TypeIds.Length; ++i)
        {
            if (!typeId.Equals(TypeIds[i], StringComparison.Ordinal))
                continue;

            mode = (BackwardsCompatibleAssetReferenceKind)i;
            break;
        }

        if (typeDefinition.ValueKind == JsonValueKind.String)
        {
            return GetInstance(mode);
        }

        AssetReferenceHelper.ReadCommonJsonProperties(in typeDefinition, out OneOrMore<QualifiedType> baseTypes, out bool allowThis, out bool preventSelfRef, out bool isDefault);

        return isDefault
            ? GetInstance(mode)
            : new BackwardsCompatibleAssetReferenceType(mode, baseTypes, spec, allowThis, preventSelfRef);
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

    protected override bool Equals(BackwardsCompatibleAssetReferenceType other)
    {
        return other.Kind == Kind && other._baseTypes.Equals(_baseTypes) && other.SupportsThis == SupportsThis && other.PreventSelfReference == PreventSelfReference;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(443291952, Kind, _baseTypes, SupportsThis, PreventSelfReference);
    }
}