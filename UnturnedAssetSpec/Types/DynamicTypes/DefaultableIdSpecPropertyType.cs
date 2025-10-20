using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class DefaultableIdSpecPropertyType :
    BaseSpecPropertyType<int>,
    ISpecPropertyType<int>,
    IElementTypeSpecPropertyType,
    ISpecialTypesSpecPropertyType,
    IEquatable<DefaultableIdSpecPropertyType>,
    IStringParseableSpecPropertyType
{
    public EnumSpecTypeValue Category { get; }

    public OneOrMore<QualifiedType> OtherElementTypes { get; }
    public QualifiedType ElementType { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "DefaultableId";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public Type ValueType => typeof(int);

    string IElementTypeSpecPropertyType.ElementType => ElementType.Type;
    OneOrMore<string?> ISpecialTypesSpecPropertyType.SpecialTypes => OtherElementTypes.Select<string?>(x => x.Type);

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<int>()?.ToString();
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (int.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out int result) && result <= ushort.MaxValue)
        {
            dynamicValue = SpecDynamicValue.Int32(result < 0 ? -1 : result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    public override int GetHashCode()
    {
        // note: using string because InnerType could change in Transform()
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return 67 ^ HashCode.Combine(Category.Index, ElementType, OtherElementTypes);
    }

    public DefaultableIdSpecPropertyType(EnumSpecTypeValue category, OneOrMore<string> specialTypes)
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

    public DefaultableIdSpecPropertyType(QualifiedType qualifiedType, OneOrMore<string> specialTypes)
        : this(
            AssetCategory.None,
            qualifiedType,
            QualifiedType.ExtractTypeName(qualifiedType.Type.AsSpan()).ToString() + " ID",
            specialTypes
        )
    {
    }

    private DefaultableIdSpecPropertyType(EnumSpecTypeValue category, QualifiedType elementType, string displayName, OneOrMore<string> specialTypes)
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
        if (!TryParseValue(in parse, out int val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteConvertibleValue<int>(val, this);
        return true;
    }

    /// <inheritdoc />
    public virtual bool TryParseValue(in SpecPropertyTypeParseContext parse, out int value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode
            || !KnownTypeValueHelper.TryParseInt32(strValNode.Value, out value)
            || value > ushort.MaxValue)
        {
            return FailedToParse(in parse, out value);
        }

        if (value < 0)
            value = -1;

        // todo: ID resolution

        return true;
    }

    /// <inheritdoc />
    public bool Equals(DefaultableIdSpecPropertyType other) => other != null && GetType() == other.GetType() && Category.Equals(other.Category) && OtherElementTypes.Equals(other.OtherElementTypes);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is DefaultableIdSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<int>? other) => other is DefaultableIdSpecPropertyType t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}