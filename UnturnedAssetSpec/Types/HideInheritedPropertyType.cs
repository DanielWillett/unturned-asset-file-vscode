using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal sealed class HideInheritedPropertyType : ISpecPropertyType, IEquatable<HideInheritedPropertyType>
{
    public static readonly HideInheritedPropertyType Instance = new HideInheritedPropertyType();
    static HideInheritedPropertyType() { }
    private HideInheritedPropertyType() { }

    public bool Equals(HideInheritedPropertyType? other) => other != null;
    public bool Equals(ISpecPropertyType? other) => other is HideInheritedPropertyType;
    public override bool Equals(object? obj) => obj is HideInheritedPropertyType;
    public override int GetHashCode() => 0;

    public string DisplayName => "Hide Inherited";
    public string Type => string.Empty;

    public Type ValueType => throw new NotSupportedException();

    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Other;

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        value = null!;
        return false;
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }
}