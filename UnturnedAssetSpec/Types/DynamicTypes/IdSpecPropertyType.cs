using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class IdSpecPropertyType :
    BaseSpecPropertyType<ushort>,
    ISpecPropertyType<ushort>,
    IElementTypeSpecPropertyType,
    IEquatable<IdSpecPropertyType>
{
    public EnumSpecTypeValue Category { get; }

    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "Id";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public Type ValueType => typeof(ushort);

    public IdSpecPropertyType(EnumSpecTypeValue category)
    {
        if (!category.Type.Equals(AssetCategory.TypeOf))
        {
            throw new ArgumentException("Expected asset category enum value.", nameof(category));
        }

        Category = category;
        ElementType = default;
        DisplayName = category == AssetCategory.None ? "Any ID" : $"{category.Casing} ID";
    }

    public IdSpecPropertyType(QualifiedType qualifiedType)
    {
        Category = AssetCategory.None;
        ElementType = qualifiedType;
        DisplayName = QualifiedType.ExtractTypeName(qualifiedType.Type.AsSpan()).ToString() + " ID";
    }

    /// <inheritdoc />
    public virtual bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode
            || !KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        // todo: ID resolution

        return true;
    }

    /// <inheritdoc />
    public bool Equals(IdSpecPropertyType other) => other != null && GetType() == other.GetType() && Category.Equals(other.Category);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is IdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<ushort> other) => other is IdSpecPropertyType t && Equals(t);
}