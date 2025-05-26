using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public interface ISpecPropertyType : IEquatable<ISpecPropertyType?>
{
    string DisplayName { get; }
    string Type { get; }
    Type ValueType { get; }
    SpecPropertyTypeKind Kind { get; }
    ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue>;
    bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value);
}

public interface ISpecPropertyType<TValue> : ISpecPropertyType, IEquatable<ISpecPropertyType<TValue>?>
    where TValue : IEquatable<TValue>
{
    bool TryParseValue(in SpecPropertyTypeParseContext parse, out TValue? value);
}

public interface IElementTypeSpecPropertyType : ISpecPropertyType
{
    string? ElementType { get; }
}
public interface ISpecialTypesSpecPropertyType : ISpecPropertyType
{
    OneOrMore<string?> SpecialTypes { get; }
}

public interface IStringParseableSpecPropertyType
{
    bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue);
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