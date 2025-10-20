using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public interface ISpecPropertyType : IEquatable<ISpecPropertyType?>
{
    string DisplayName { get; }
    string Type { get; }
    Type ValueType { get; }
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

public interface ISpecPropertyType<TValue> : ISpecPropertyType, IEquatable<ISpecPropertyType<TValue>?>
    where TValue : IEquatable<TValue>
{
    bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);
}

public interface ISpecPropertyTypeVisitor
{
    void Visit<T>(ISpecPropertyType<T> type) where T : IEquatable<T>;
}

public interface IElementTypeSpecPropertyType : ISpecPropertyType
{
    string? ElementType { get; }
}

public interface IListTypeSpecPropertyType : IElementTypeSpecPropertyType
{
    ISpecPropertyType? GetInnerType(IAssetSpecDatabase database);
}

public interface IDictionaryTypeSpecPropertyType : IElementTypeSpecPropertyType
{
    ISpecPropertyType? GetInnerType(IAssetSpecDatabase database);
}

public interface ISpecialTypesSpecPropertyType : ISpecPropertyType
{
    OneOrMore<string?> SpecialTypes { get; }
}

public interface IStringParseableSpecPropertyType
{
    bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue);

    string? ToString(ISpecDynamicValue value);
}

public interface ISecondPassSpecPropertyType : ISpecPropertyType
{
    ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile);
}

public enum SpecPropertyTypeKind
{
    String,
    Number,
    Boolean,
    Struct,
    Class,
    Other,
    Enum
}

internal interface IVectorSpecPropertyTypeVisitor
{
    void Visit<T>(IVectorSpecPropertyType<T> type) where T : IEquatable<T>;
}

/// <summary>
/// Vector types allow equations to apply component-wise operations.
/// </summary>
internal interface IVectorSpecPropertyType : ISpecPropertyType
{
    new void Visit<TVisitor>(ref TVisitor visitor) where TVisitor : IVectorSpecPropertyTypeVisitor;
}

/// <summary>
/// Vector types allow equations to apply component-wise operations.
/// </summary>
internal interface IVectorSpecPropertyType<T> : IVectorSpecPropertyType
{
    T? Multiply(T? val1, T? val2);
    
    T? Divide(T? val1, T? val2);

    T? Add(T? val1, T? val2);

    T? Subtract(T? val1, T? val2);

    T? Modulo(T? val1, T? val2);

    T? Power(T? val1, T? val2);

    T? Min(T? val1, T? val2);

    T? Max(T? val1, T? val2);

    T? Avg(T? val1, T? val2);

    [return: NotNullIfNotNull(nameof(val))]
    T? Absolute(T? val);

    [return: NotNullIfNotNull(nameof(val))]
    T? Round(T? val);

    [return: NotNullIfNotNull(nameof(val))]
    T? Ceiling(T? val);

    [return: NotNullIfNotNull(nameof(val))]
    T? Floor(T? val);

    /// <param name="op">0-2: sin-tan, 3-5: asin-atan</param>
    [return: NotNullIfNotNull(nameof(val))]
    T? TrigOperation(T? val, int op, bool deg);

    [return: NotNullIfNotNull(nameof(val))]
    T? Sqrt(T? val);

    T Construct(double scalar);
}
