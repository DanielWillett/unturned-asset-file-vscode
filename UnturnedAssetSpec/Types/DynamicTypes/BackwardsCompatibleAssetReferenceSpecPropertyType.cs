using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

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
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "BcAssetReference";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(GuidOrId);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    public BackwardsCompatibleAssetReferenceSpecPropertyType(QualifiedType elementType, bool canParseDictionary, OneOrMore<string> specialTypes)
    {
        CanParseDictionary = canParseDictionary;
        if (elementType.Type == null || elementType.Equals(QualifiedType.AssetBaseType))
        {
            ElementType = QualifiedType.AssetBaseType;
            DisplayName = "Asset Reference (Backwards-Compatible)";
        }
        else if (AssetCategory.TryParse(elementType.Type, out EnumSpecTypeValue category))
        {
            ElementType = elementType;
            DisplayName = $"{category.Casing} Asset Reference (Backwards-Compatible)";
        }
        else
        {
            ElementType = elementType;
            DisplayName = $"Asset Reference to {QualifiedType.ExtractTypeName(elementType.Type.AsSpan()).ToString()} (Backwards-Compatible)";
        }

        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x));
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
                Message = string.Format(DiagnosticResources.UNT2005, $"CachedBcAssetRef<{QualifiedType.ExtractTypeName(ElementType.Type.AsSpan()).ToString()}>"),
                Range = parse.Node?.Range ?? parse.Parent?.Range ?? default
            });
            value = default;
            return false;
        }

        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is AssetFileStringValueNode stringValue)
        {
            if (parse.HasDiagnostics)
            {
                int indexOfX = stringValue.Value.IndexOf('x');
                if (indexOfX != -1)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Diagnostic = DatDiagnostics.UNT1017,
                        Message = DiagnosticResources.UNT1017,
                        Range = stringValue.Range with
                        {
                            Start = new FilePosition(stringValue.Range.Start.Line, stringValue.Range.Start.Character + indexOfX)
                        }
                    });
                }
            }

            return TryParse(stringValue.Value.AsSpan(), stringValue.Value, out value) || FailedToParse(in parse, out value);
        }

        if (!CanParseDictionary || parse.Node is not AssetFileDictionaryValueNode dictionary)
        {
            return FailedToParse(in parse, out value);
        }

        if (!dictionary.TryGetValue("GUID", out AssetFileValueNode? node))
        {
            if (!dictionary.TryGetValue("ID", out AssetFileValueNode? idNode)
                || !dictionary.TryGetValue("Type", out AssetFileValueNode? typeNode))
            {
                return MissingProperty(in parse, "GUID", out value);
            }

            if (idNode is not AssetFileStringValueNode strIdNode
                || typeNode is not AssetFileStringValueNode strTypeNode
                || !KnownTypeValueHelper.TryParseUInt16(strIdNode.Value, out ushort id)
                || !AssetCategory.TryParse(strTypeNode.Value, out EnumSpecTypeValue category)
                || category == AssetCategory.None)
            {
                return FailedToParse(in parse, out value);
            }

            value = new GuidOrId(id);
            return true;
        }

        if (node is not AssetFileStringValueNode strGuidNode || !KnownTypeValueHelper.TryParseGuid(strGuidNode.Value, out Guid guid))
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
}