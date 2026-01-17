using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A legacy reference to one or more types of assets formatted as a string.
/// Accepts only <see cref="ushort"/> IDs, not <see cref="Guid"/> IDs.
/// <para>Example: <c>AnimalAsset.Meat</c></para>
/// <code>
/// Prop 1120
///
/// // defaultable
/// Prop -1
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
public sealed class LegacyAssetReferenceType : BaseType<ushort, LegacyAssetReferenceType>, ITypeParser<ushort>, ITypeFactory
{
    /// <summary>
    /// Gets the default instance for a <see cref="LegacyAssetReferenceType"/>.
    /// </summary>
    [field: MaybeNull]
    public static LegacyAssetReferenceType Instance => field ??= new LegacyAssetReferenceType();

    public const string TypeId = "LegacyAssetReference";
    public const string DefaultableTypeId = "DefaultableLegacyAssetReference";

    private readonly AssetCategoryValue _defaultCategory;

    public static ITypeFactory Factory => Instance;


    private readonly OneOrMore<QualifiedType> _baseTypes;

    public override string Id => Defaultable ? DefaultableTypeId : TypeId;

    public override string DisplayName { get; }

    public override ITypeParser<ushort> Parser => this;

    /// <summary>
    /// Whether or not a negative number can be supplied to fallback to some default value.
    /// </summary>
    public bool Defaultable { get; }

    public bool SupportsThis { get; }
    public bool PreventSelfReference { get; }

    /// <summary>
    /// The default category used to parse ID strings, if any.
    /// </summary>
    public AssetCategoryValue DefaultCategory => _defaultCategory;

    public LegacyAssetReferenceType() : this(false) { }

    public LegacyAssetReferenceType(bool defaultable, bool supportsThis = false, bool preventSelfReference = false)
    {
        _baseTypes = OneOrMore<QualifiedType>.Null;
        _defaultCategory = AssetCategoryValue.None;
        DisplayName = Resources.Type_Name_LegacyAssetReference;
        Defaultable = defaultable;
        SupportsThis = supportsThis;
        PreventSelfReference = preventSelfReference;
    }

    public LegacyAssetReferenceType(bool defaultable, OneOrMore<QualifiedType> baseTypes, IDatSpecificationReadContext spec, bool supportsThis = false, bool preventSelfReference = false)
    {
        _baseTypes = baseTypes;
        Defaultable = defaultable;
        SupportsThis = supportsThis;
        PreventSelfReference = preventSelfReference;
        DisplayName = AssetReferenceHelper.GetDisplayName(baseTypes, Resources.Type_Name_LegacyAssetReference, Resources.Type_Name_LegacyAssetReference_Type);
        _defaultCategory = AssetReferenceHelper.GetDefaultCategory(baseTypes, spec);
    }

    public bool TryParse(ref TypeParserArgs<ushort> args, in FileEvaluationContext ctx, out Optional<ushort> value)
    {
        value = Optional<ushort>.Null;

        switch (args.ValueNode)
        {
            case null:
                args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                break;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                break;

            case IValueSourceNode v:
                if (!KnownTypeValueHelper.TryParseUInt16(v.Value, out ushort id))
                {
                    if (!Defaultable || !KnownTypeValueHelper.TryParseInt32(v.Value, out int idAsInt) || idAsInt > ushort.MaxValue)
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
                        return false;
                    }

                    value = Optional<ushort>.Null;
                    return true;
                }

                value = id;
                return true;

            case IDictionarySourceNode:
                args.DiagnosticSink?.UNT2004_LegacyAssetReferenceStringOnly(ref args, args.Type, args.ParentNode);
                break;
        }

        return false;
    }

    #region JSON

    public bool TryReadValueFromJson(in JsonElement json, out Optional<ushort> value, IType<ushort> valueType)
    {
        return TypeParsers.UInt16.TryReadValueFromJson(in json, out value, valueType);
    }

    public void WriteValueToJson(Utf8JsonWriter writer, ushort value, IType<ushort> valueType, JsonSerializerOptions options)
    {
        TypeParsers.UInt16.WriteValueToJson(writer, value, valueType, options);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, DatProperty owner, string context)
    {
        bool defaultable = typeId.Equals(DefaultableTypeId, StringComparison.Ordinal);
        if (!defaultable && typeDefinition.ValueKind == JsonValueKind.String)
        {
            return Instance;
        }

        AssetReferenceHelper.ReadCommonJsonProperties(in typeDefinition, out OneOrMore<QualifiedType> baseTypes, out bool allowThis, out bool preventSelfRef, out bool isDefault);

        return !defaultable && isDefault
            ? Instance
            : new LegacyAssetReferenceType(defaultable, baseTypes, spec, allowThis, preventSelfRef);
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

    protected override bool Equals(LegacyAssetReferenceType other)
    {
        return other._baseTypes.Equals(_baseTypes) && other.SupportsThis == SupportsThis && other.PreventSelfReference == PreventSelfReference;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(2142610343, _baseTypes, SupportsThis, PreventSelfReference);
    }
}