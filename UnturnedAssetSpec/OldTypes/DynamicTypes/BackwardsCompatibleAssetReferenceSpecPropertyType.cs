using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A backwards-compatable reference to one or more types of assets formatted as either a string or sometimes as an object (see <see cref="CanParseDictionary"/>).
/// Accepts both <see cref="Guid"/> and <see cref="ushort"/> IDs, although in some cases the ID can't be assumed and a <see cref="AssetCategory"/> also has to be specified ('Type').
/// <para>Example: <c>AirdropAsset.Landed_Barricade</c></para>
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
/// // if category can be assumed
/// Prop
/// {
///     ID 1374
/// }
/// // if category can't be assumed
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
/// Also supports the <c>PreventSelfReference</c> additional property to log a warning if the current asset is referenced.
/// </para>
/// <para>
/// If "this" is one of the element types, the word 'this' will be resolved to the current asset.
/// </para>
/// <para>
/// If an amount is supppled (i.e. "102 x 3") a warning will be logged.
/// </para>
/// </summary>
public class BackwardsCompatibleAssetReferenceSpecPropertyType :
    BaseSpecPropertyType<BackwardsCompatibleAssetReferenceSpecPropertyType, GuidOrId>,
    ISpecPropertyType<GuidOrId>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<BackwardsCompatibleAssetReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    private readonly IAssetSpecDatabase _database;
    private readonly AssetCategoryValue _category;
    public OneOrMore<QualifiedType> OtherElementTypes { get; }
    public bool CanParseDictionary { get; }
    public bool SupportsThis { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public sealed override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "BcAssetReference";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<GuidOrId>()?.ToString();
    }

    public override int GetHashCode()
    {
        // 59 - 62
        return (59 + (CanParseDictionary ? 1 : 0) + (SupportsThis ? 1 : 0) * 2)
               ^ HashCode.Combine(ElementType, OtherElementTypes);
    }

    public BackwardsCompatibleAssetReferenceSpecPropertyType(IAssetSpecDatabase database, QualifiedType elementType, bool canParseDictionary, OneOrMore<string> specialTypes)
    {
        _database = database.ResolveFacade();
        CanParseDictionary = canParseDictionary;

        SupportsThis = AssetReferenceSpecPropertyType.ExtractThisElementType(ref elementType, ref specialTypes);

        if (specialTypes.Contains(QualifiedType.AssetBaseType.Type) || elementType == QualifiedType.AssetBaseType)
        {
            specialTypes = OneOrMore<string>.Null;
            elementType = QualifiedType.AssetBaseType;
        }

        if (elementType.Type == null || elementType == QualifiedType.AssetBaseType)
        {
            ElementType = QualifiedType.AssetBaseType;
            _category = AssetCategoryValue.None;
        }
        else if (AssetCategory.TryParse(elementType.Type, out EnumSpecTypeValue category))
        {
            ElementType = new QualifiedType(category.Value);
            _category = new AssetCategoryValue(category.Index);
            DisplayName = $"{category.Casing} Asset Reference (Backwards-Compatible)";
        }
        else
        {
            specialTypes = specialTypes.Add(elementType);
            ElementType = QualifiedType.AssetBaseType;
            _category = AssetCategoryValue.None;
        }

        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x))
            .Remove(ElementType);

        if (DisplayName != null)
            return;

        switch (OtherElementTypes.Length)
        {
            case 0:
                DisplayName = "Asset Reference";
                break;

            case 1:
                DisplayName = $"Asset Reference to {OtherElementTypes[0].GetTypeName()} (Backwards-Compatible)";
                break;

            default:
                StringBuilder sb = new StringBuilder("Asset Reference to ");
                for (int i = 0; i < OtherElementTypes.Length; i++)
                {
                    QualifiedType t = OtherElementTypes[i];
                    if (i == OtherElementTypes.Length - 1)
                        sb.Append(i == 1 ? " or " : ", or ");
                    else if (i != 0)
                        sb.Append(", ");

                    sb.Append(t.GetTypeName());
                }

                sb.Append(" (Backwards-Compatible)");
                DisplayName = sb.ToString();
                break;
        }
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (TryParse(span, stringValue, out GuidOrId guidOrId))
        {
            dynamicValue = new SpecDynamicConcreteValue<GuidOrId>(guidOrId, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out GuidOrId guidOrId)
    {
        return stringValue != null
            ? KnownTypeValueHelper.TryParseGuidOrId(stringValue, _category, out guidOrId)
            : KnownTypeValueHelper.TryParseGuidOrId(span, _category, out guidOrId);
    }

    private bool? _isTypeValid;

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out GuidOrId value)
    {
        if (!_isTypeValid.HasValue)
        {
            InverseTypeHierarchy parents = _database.Information.GetParentTypes(ElementType);
            _isTypeValid = parents.IsValid;
        }

        if (!_isTypeValid.Value)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2005,
                Message = string.Format(DiagnosticResources.UNT2005, ElementType == QualifiedType.AssetBaseType ? "CachedBcAssetRef" : $"CachedBcAssetRef<{ElementType.Type}>"),
                Range = parse.Node?.Range ?? parse.Parent?.Range ?? default
            });
            value = default;
            return false;
        }

        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is IValueSourceNode stringValue)
        {
            if (SupportsThis && string.Equals(stringValue.Value, "this", StringComparison.InvariantCultureIgnoreCase))
            {
                if (parse.File is IAssetSourceFile asset)
                {
                    if (asset.Guid is { } guid2 && guid2 != Guid.Empty)
                    {
                        value = new GuidOrId(guid2);
                        return true;
                    }

                    AssetCategoryValue category;
                    if (asset.Id is { } id2 && id2 != 0 && (category = asset.Category) != AssetCategoryValue.None)
                    {
                        value = new GuidOrId(id2, category);
                        return true;
                    }
                }

                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Diagnostic = DatDiagnostics.UNT2010,
                        Message = DiagnosticResources.UNT2010,
                        Range = stringValue.Range
                    });
                }

                value = GuidOrId.Empty;
                return false;
            }

            AssetReferenceSpecPropertyType.CheckInappropriateAmount(in parse, stringValue);

            return TryParse(stringValue.Value.AsSpan(), stringValue.Value, out value) || FailedToParse(in parse, out value);
        }

        if (!CanParseDictionary || parse.Node is not IDictionarySourceNode dictionary)
        {
            return FailedToParse(in parse, out value);
        }

        if (!dictionary.TryGetPropertyValue("GUID", out IAnyValueSourceNode? node))
        {
            if (!dictionary.TryGetPropertyValue("ID", out IAnyValueSourceNode? idNode)
                || !dictionary.TryGetPropertyValue("Type", out IAnyValueSourceNode? typeNode))
            {
                return MissingProperty(in parse, "GUID", out value);
            }

            if (idNode is not IValueSourceNode strIdNode
                || typeNode is not IValueSourceNode strTypeNode
                || !KnownTypeValueHelper.TryParseUInt16(strIdNode.Value, out ushort id)
                || !AssetCategory.TryParse(strTypeNode.Value, out EnumSpecTypeValue category)
                || category == AssetCategory.None)
            {
                return FailedToParse(in parse, out value);
            }

            value = new GuidOrId(id);
            return true;
        }

        if (node is not IValueSourceNode strGuidNode || !KnownTypeValueHelper.TryParseGuid(strGuidNode.Value, out Guid guid))
        {
            return FailedToParse(in parse, out value);
        }

        value = new GuidOrId(guid);
        return true;
    }

    /// <inheritdoc />
    public bool Equals(BackwardsCompatibleAssetReferenceSpecPropertyType? other) => ReferenceEquals(this, other) || (other != null && ElementType.Equals(other.ElementType) && OtherElementTypes.Equals(other.OtherElementTypes) && GetType() == other.GetType());
}