using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class IdSpecPropertyType :
    BaseSpecPropertyType<ushort>,
    ISpecPropertyType<ushort>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<IdSpecPropertyType?>,
    IStringParseableSpecPropertyType
{
    public EnumSpecTypeValue Category { get; }

    public OneOrMore<QualifiedType> OtherElementTypes { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "Id";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public Type ValueType => typeof(ushort);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<ushort>()?.ToString();
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (ushort.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out ushort result))
        {
            dynamicValue = SpecDynamicValue.UInt16(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    public IdSpecPropertyType(EnumSpecTypeValue category, OneOrMore<string> specialTypes)
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

    public IdSpecPropertyType(QualifiedType qualifiedType, OneOrMore<string> specialTypes)
        : this(
            AssetCategory.None,
            qualifiedType,
            QualifiedType.ExtractTypeName(qualifiedType.Type.AsSpan()).ToString() + " ID",
            specialTypes
        )
    {
    }

    private IdSpecPropertyType(EnumSpecTypeValue category, QualifiedType elementType, string displayName, OneOrMore<string> specialTypes)
    {
        Category = category;
        ElementType = elementType;
        DisplayName = displayName;

        OtherElementTypes = specialTypes
            .Where(x => !string.IsNullOrEmpty(x))
            .Select(x => new QualifiedType(x));
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out ushort val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteConvertibleValue<ushort>(val, this);
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryParseValue(in SpecPropertyTypeParseContext parse, out ushort value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode
            || !KnownTypeValueHelper.TryParseUInt16(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }

        // todo: ID resolution

        return true;
    }

    /// <inheritdoc />
    public bool Equals(IdSpecPropertyType? other) => other != null && GetType() == other.GetType() && Category.Equals(other.Category) && OtherElementTypes.Equals(other.OtherElementTypes);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is IdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<ushort>? other) => other is IdSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}