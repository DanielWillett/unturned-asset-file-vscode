using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;
public interface ISpecPropertyType : IEquatable<ISpecPropertyType>
{
    string DisplayName { get; }
    string Type { get; }
    SpecPropertyTypeKind Kind { get; }
    ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue>;
}

public interface ISpecPropertyType<TValue> : ISpecPropertyType, IEquatable<ISpecPropertyType<TValue>>
    where TValue : IEquatable<TValue>
{
    bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);
}

public interface IElementTypeSpecPropertyType : ISpecPropertyType
{
    QualifiedType ElementType { get; }
}

public enum SpecPropertyTypeKind
{
    String,
    Number,
    Boolean,
    Struct,
    Class,
    Other
}