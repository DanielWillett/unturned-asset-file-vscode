using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class AssetReferenceSpecPropertyType :
    BaseSpecPropertyType<Guid>,
    ISpecPropertyType<Guid>,
    IElementTypeSpecPropertyType,
    IEquatable<AssetReferenceSpecPropertyType>
{
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "AssetReference";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public Type ValueType => typeof(Guid);

    public AssetReferenceSpecPropertyType(QualifiedType elementType)
    {
        if (elementType.Type == null || elementType.Equals(QualifiedType.AssetBaseType))
        {
            ElementType = QualifiedType.AssetBaseType;
            DisplayName = "Asset Reference";
        }
        else
        {
            ElementType = elementType;
            DisplayName = $"Asset Reference to {QualifiedType.ExtractTypeName(elementType.Type.AsSpan()).ToString()}";
        }
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out Guid value)
    {
        InverseTypeHierarchy parents = parse.Database.Information.GetParentTypes(ElementType);
        if (!parents.IsValid)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2005,
                Message = string.Format(DiagnosticResources.UNT2005, $"AssetReference<{QualifiedType.ExtractTypeName(ElementType.Type.AsSpan()).ToString()}>"),
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
            return KnownTypeValueHelper.TryParseGuid(stringValue.Value, out value) || FailedToParse(in parse, out value);
        }

        if (parse.Node is not AssetFileDictionaryValueNode dictionary)
        {
            return FailedToParse(in parse, out value);
        }

        if (!dictionary.TryGetValue("GUID", out AssetFileValueNode? node))
        {
            return MissingProperty(in parse, "GUID", out value);
        }

        if (node is not AssetFileStringValueNode stringValue2 || !KnownTypeValueHelper.TryParseGuid(stringValue2.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(AssetReferenceSpecPropertyType other) => other != null && ElementType.Equals(other.ElementType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is AssetReferenceSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<Guid> other) => other is AssetReferenceSpecPropertyType t && Equals(t);
}