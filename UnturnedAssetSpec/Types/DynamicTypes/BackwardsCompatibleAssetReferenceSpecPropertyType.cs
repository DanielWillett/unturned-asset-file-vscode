using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class BackwardsCompatibleAssetReferenceSpecPropertyType :
    BaseSpecPropertyType<GuidOrId>,
    ISpecPropertyType<GuidOrId>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<BackwardsCompatibleAssetReferenceSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    public OneOrMore<QualifiedType> OtherElementTypes { get; }
    public bool CanParseDictionary { get; }
    public bool SupportsThis { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public sealed override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "BcAssetReference";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(GuidOrId);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<GuidOrId>()?.ToString();
    }

    public BackwardsCompatibleAssetReferenceSpecPropertyType(QualifiedType elementType, bool canParseDictionary, OneOrMore<string> specialTypes)
    {
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
        }
        else if (AssetCategory.TryParse(elementType.Type, out EnumSpecTypeValue category))
        {
            ElementType = new QualifiedType(category.Value);
            DisplayName = $"{category.Casing} Asset Reference (Backwards-Compatible)";
        }
        else
        {
            specialTypes = specialTypes.Add(elementType);
            ElementType = QualifiedType.AssetBaseType;
        }

        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x));

        if (DisplayName != null)
            return;

        switch (OtherElementTypes.Length)
        {
            case 0:
                DisplayName = "Asset Reference (Backwards-Compatible)";
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
        return KnownTypeValueHelper.TryParseGuidOrId(span, stringValue, out guidOrId, ElementType.Type);
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out GuidOrId val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<GuidOrId>(val, this);
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryParseValue(in SpecPropertyTypeParseContext parse, out GuidOrId value)
    {
        InverseTypeHierarchy parents = parse.Database.Information.GetParentTypes(ElementType);
        if (!parents.IsValid)
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

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is BackwardsCompatibleAssetReferenceSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<GuidOrId>? other) => other is BackwardsCompatibleAssetReferenceSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}