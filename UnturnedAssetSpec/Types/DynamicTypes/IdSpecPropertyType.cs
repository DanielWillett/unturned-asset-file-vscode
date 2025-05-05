using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class IdSpecPropertyType :
    BaseSpecPropertyType<ushort>,
    ISpecPropertyType<ushort>,
    IElementTypeSpecPropertyType,
    IEquatable<IdSpecPropertyType>
{
    public EnumSpecTypeValue Category { get; }

    public EquatableArray<QualifiedType> OtherElementTypes { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "Id";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public Type ValueType => typeof(ushort);

    public IdSpecPropertyType(EnumSpecTypeValue category, string[]? specialTypes)
        : this(
            category,
            default,
            category == AssetCategory.None ? "Any ID" : $"{category.Casing} ID",
            specialTypes
        )
    {
        if (!category.Type.Equals(AssetCategory.TypeOf))
        {
            throw new ArgumentException("Expected asset category enum value.", nameof(category));
        }
    }

    public IdSpecPropertyType(QualifiedType qualifiedType, string[]? specialTypes)
        : this(
            AssetCategory.None,
            qualifiedType,
            QualifiedType.ExtractTypeName(qualifiedType.Type.AsSpan()).ToString() + " ID",
            specialTypes
        )
    {
    }

    private IdSpecPropertyType(EnumSpecTypeValue category, QualifiedType elementType, string displayName, string[]? specialTypes)
    {
        Category = category;
        ElementType = elementType;
        DisplayName = displayName;
        if (specialTypes == null || specialTypes.Length == 0)
        {
            OtherElementTypes = new EquatableArray<QualifiedType>(0);
            return;
        }

        OtherElementTypes = new EquatableArray<QualifiedType>(specialTypes.Length);
        for (int i = 0; i < specialTypes.Length; ++i)
            OtherElementTypes.Array[i] = new QualifiedType(specialTypes[i]);
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
    public bool Equals(IdSpecPropertyType other) => other != null && GetType() == other.GetType() && Category.Equals(other.Category) && OtherElementTypes.Equals(other.OtherElementTypes);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is IdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<ushort> other) => other is IdSpecPropertyType t && Equals(t);
}