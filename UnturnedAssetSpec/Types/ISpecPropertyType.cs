using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A value type that can be stored in a property.
/// </summary>
public interface ISpecPropertyType : IEquatable<ISpecPropertyType?>
{
    /// <summary>
    /// Human-readable name of the type.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Assembly-qualified name of this type.
    /// </summary>
    string Type { get; }

    /// <summary>
    /// The type of value this type is parsed into.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Kind of value used for UI hints and syntax coloring.
    /// </summary>
    SpecPropertyTypeKind Kind { get; }

    /// <summary>
    /// Parse a dynamic value from the current parse context.
    /// </summary>
    bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value);

    /// <summary>
    /// Invokes the strongly typed <see cref="ISpecPropertyTypeVisitor.Visit{T}"/> on the <paramref name="visitor"/>.
    /// <para>
    /// If this type is not strongly typed (such as <see cref="UnresolvedSpecPropertyType"/>), the visitor will not be invoked.
    /// </para>
    /// </summary>
    void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : ISpecPropertyTypeVisitor;
}

/// <summary>
/// A strongly-typed value type that can be stored in a property.
/// </summary>
/// <typeparam name="TValue">The type of value this type is parsed into.</typeparam>
public interface ISpecPropertyType<TValue> : ISpecPropertyType, IEquatable<ISpecPropertyType<TValue>?>
    where TValue : IEquatable<TValue>
{
    bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);
}

/// <summary>
/// An object that has a generic <see cref="Visit{T}"/> function that can be used to enter a generic context when working with strongly-typed property types.
/// </summary>
public interface ISpecPropertyTypeVisitor
{
    /// <summary>
    /// Invoked by <see cref="ISpecPropertyType.Visit{TVisitor}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value this type is parsed into.</typeparam>
    void Visit<T>(ISpecPropertyType<T> type) where T : IEquatable<T>;
}

/// <summary>
/// A property type that defines an element type.
/// </summary>
public interface IElementTypeSpecPropertyType : ISpecPropertyType
{
    /// <summary>
    /// A sub-type of this type. This is basically a free string parameter that types can do whatever they want with
    /// but usually it's used to express some kind of inner type, like the element type of the list type.
    /// </summary>
    string? ElementType { get; }
}

/// <summary>
/// A property type that defines a list of secondary types.
/// </summary>
public interface ISpecialTypesSpecPropertyType : ISpecPropertyType
{
    /// <summary>
    /// Sub-types of this type. This is basically a free list of string parameters that types can do whatever they want with
    /// but usually they're used to express some kind of inner type, like the element type of the list type.
    /// </summary>
    /// <remarks>Commonly used in conjunction with <see cref="IElementTypeSpecPropertyType"/>.</remarks>
    OneOrMore<string?> SpecialTypes { get; }
}

/// <summary>
/// Allows a type to provide custom hover text for IDEs.
/// </summary>
public interface IValueHoverProviderSpecPropertyType : ISpecPropertyType
{
    /// <summary>
    /// Gets some common elements of the description used to generate hover text for IDEs.
    /// </summary>
    ValueHoverProviderResult? GetDescription(in SpecPropertyTypeParseContext ctx, ISpecDynamicValue value);
}

public class ValueHoverProviderResult(string displayName, QualifiedType declaringType, string? variable, string? description, Version? version, string? docs, bool isDeprecated, QualifiedType correspondingType)
{
    public string DisplayName { get; set; } = displayName;
    public QualifiedType DeclaringType { get; set; } = declaringType;
    public string? Variable { get; set; } = variable;
    public string? Description { get; set; } = description;
    public Version? Version { get; set; } = version;
    public string? Docs { get; set; } = docs;
    public string? LinkName { get; set; }
    public bool IsDeprecated { get; set; } = isDeprecated;
    public QualifiedType CorrespondingType { get; set; } = correspondingType;
}

/// <summary>
/// A type which stores a list of other types.
/// </summary>
public interface IListTypeSpecPropertyType : IElementTypeSpecPropertyType
{
    /// <summary>
    /// Gets the element type of the list.
    /// </summary>
    ISpecPropertyType? GetInnerType(IAssetSpecDatabase database);
}

/// <summary>
/// A type which stores a dictionary of strings to other types.
/// </summary>
public interface IDictionaryTypeSpecPropertyType : IElementTypeSpecPropertyType
{
    /// <summary>
    /// Gets the value type of the dictionary.
    /// </summary>
    ISpecPropertyType? GetInnerType(IAssetSpecDatabase database);
}

/// <summary>
/// A property type that can be converted to and parsed from a string.
/// </summary>
public interface IStringParseableSpecPropertyType
{
    /// <summary>
    /// Attempts to parse a value of this type from a string or span.
    /// </summary>
    /// <param name="span">The value being parsed.</param>
    /// <param name="stringValue">
    /// May also have the same value as <paramref name="span"/> but as a string, or may be <see langword="null"/>.
    /// This avoids creating new string instances when not necessary.
    /// </param>
    /// <param name="dynamicValue">The parsed value.</param>
    bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue);

    /// <summary>
    /// Converts a <paramref name="value"/> to a <see cref="string"/> representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    string? ToString(ISpecDynamicValue value);
}

/// <summary>
/// A type which couldn't be resolved during the initial deserialization and must be revisited during a second pass.
/// </summary>
public interface ISecondPassSpecPropertyType : ISpecPropertyType
{
    /// <summary>
    /// Converts this type to a new type which may have needed some metadata from other types after they'd finished being read.
    /// </summary>
    /// <param name="property">The property this type is on.</param>
    /// <param name="database">The database service.</param>
    /// <param name="assetFile">The file this property belongs to.</param>
    /// <returns>The new type, or the same reference to the old type if a new type didn't need to be created.</returns>
    ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile);
}

/// <summary>
/// Kind of value used for UI hints and syntax coloring.
/// </summary>
public enum SpecPropertyTypeKind
{
    /// <summary>
    /// Any string-like value.
    /// </summary>
    String,

    /// <summary>
    /// A number, GUID, etc.
    /// </summary>
    Number,

    /// <summary>
    /// A true or false value.
    /// </summary>
    Boolean,

    /// <summary>
    /// Any data construct.
    /// </summary>
    Struct,

    /// <summary>
    /// A reference to a type or data construct.
    /// </summary>
    Class,
    
    /// <summary>
    /// Any others.
    /// </summary>
    Other,

    /// <summary>
    /// A value which must exist in a set of values.
    /// </summary>
    Enum
}

/// <summary>
/// An object that has a generic <see cref="Visit{T}"/> function that can be used to enter a generic context when working with strongly-typed property types.
/// </summary>
internal interface IVectorSpecPropertyTypeVisitor
{
    /// <summary>
    /// Invoked by <see cref="IVectorSpecPropertyType.Visit{TVisitor}"/>.
    /// </summary>
    /// <typeparam name="T">The type of value this type is parsed into.</typeparam>
    void Visit<T>(IVectorSpecPropertyType<T> type) where T : IEquatable<T>;
}

/// <summary>
/// A property type with multiple components, such as a Vector3 or Color.
/// </summary>
/// <remarks>Vector types allow equations to apply component-wise operations.</remarks>
internal interface IVectorSpecPropertyType : ISpecPropertyType
{
    /// <summary>
    /// Invokes the strongly typed <see cref="IVectorSpecPropertyTypeVisitor.Visit{T}"/> on the <paramref name="visitor"/>.
    /// <para>
    /// This method may be called multiple times if a vector type can be converted to multiple types.
    /// For example, color types will call this method twice; once for <see cref="Color"/> and once for <see cref="Color32"/>.
    /// </para>
    /// </summary>
    new void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IVectorSpecPropertyTypeVisitor;
}

/// <summary>
/// Vector types allow equations to apply component-wise operations.
/// </summary>
internal interface IVectorSpecPropertyType<T> : IVectorSpecPropertyType
{
    /// <summary>
    /// Performs a component-wise multiplication on this vector.
    /// </summary>
    T? Multiply(T? val1, T? val2);

    /// <summary>
    /// Performs a component-wise division on this vector.
    /// </summary>
    T? Divide(T? val1, T? val2);

    /// <summary>
    /// Performs a component-wise addition on this vector.
    /// </summary>
    T? Add(T? val1, T? val2);

    /// <summary>
    /// Performs a component-wise subtraction on this vector.
    /// </summary>
    T? Subtract(T? val1, T? val2);

    /// <summary>
    /// Performs a component-wise modulus on this vector.
    /// </summary>
    T? Modulo(T? val1, T? val2);

    /// <summary>
    /// Performs a component-wise power operation on this vector.
    /// </summary>
    T? Power(T? val1, T? val2);

    /// <summary>
    /// Creates a new vector with the minimum values of each component.
    /// </summary>
    T? Min(T? val1, T? val2);

    /// <summary>
    /// Creates a new vector with the maximum values of each component.
    /// </summary>
    T? Max(T? val1, T? val2);

    /// <summary>
    /// Performs a component-wise mean average opearation on this vector.
    /// </summary>
    T? Avg(T? val1, T? val2);

    /// <summary>
    /// Creates a new vector with the absolute values of each component.
    /// </summary>
    [return: NotNullIfNotNull(nameof(val))]
    T? Absolute(T? val);

    /// <summary>
    /// Creates a new vector with the rounded values of each component.
    /// </summary>
    [return: NotNullIfNotNull(nameof(val))]
    T? Round(T? val);

    /// <summary>
    /// Creates a new vector with the ceil'd values of each component.
    /// </summary>
    [return: NotNullIfNotNull(nameof(val))]
    T? Ceiling(T? val);

    /// <summary>
    /// Creates a new vector with the floor'd values of each component.
    /// </summary>
    [return: NotNullIfNotNull(nameof(val))]
    T? Floor(T? val);

    /// <summary>
    /// Performs a component-wise trigonometric operation on <paramref name="val"/>.
    /// </summary>
    /// <param name="deg">Whether or not to perform the operation in degrees instead of radians.</param>
    /// <param name="op">
    /// Which trigonometric function to perform.
    /// <list type="number">
    ///     <item>sine</item>
    ///     <item>cosine</item>
    ///     <item>tangent</item>
    ///     <item>arcsine</item>
    ///     <item>arccosine</item>
    ///     <item>arctangent</item>
    /// </list>
    /// </param>
    [return: NotNullIfNotNull(nameof(val))]
    T? TrigOperation(T? val, int op, bool deg);

    /// <summary>
    /// Performs a component-wise square root opearation on this vector.
    /// </summary>
    [return: NotNullIfNotNull(nameof(val))]
    T? Sqrt(T? val);

    /// <summary>
    /// Creates a vector where all components are equal to <paramref name="scalar"/>.
    /// </summary>
    /// <remarks>Depending on the type of vector the components may be rounded or clamped to fit the bounds of the vector.</remarks>
    T Construct(double scalar);
}
