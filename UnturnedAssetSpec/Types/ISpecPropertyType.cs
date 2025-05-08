using System;
using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public interface ISpecPropertyType : IEquatable<ISpecPropertyType>
{
    string DisplayName { get; }
    string Type { get; }
    Type ValueType { get; }
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

public interface IStringParseableSpecPropertyType
{
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue);
}

public interface INestedSpecPropertyType : ISpecPropertyType
{
    void ResolveInnerTypes(SpecProperty property, AssetSpecDatabase database, AssetTypeInformation assetFile);
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