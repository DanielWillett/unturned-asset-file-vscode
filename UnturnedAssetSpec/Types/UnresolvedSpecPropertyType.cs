using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A type which couldn't be resolved during the initial deserialization and must be revisited during a second pass.
/// </summary>
internal sealed class UnresolvedSpecPropertyType :
    IEquatable<UnresolvedSpecPropertyType>,
    ISecondPassSpecPropertyType
{
    public string Value { get; }
    public bool IsKnownType { get; }
    public string? ElementType { get; }
    public OneOrMore<string> SpecialTypes { get; }

    public UnresolvedSpecPropertyType(string value, bool isKnownType = false)
    {
        Value = value;
        IsKnownType = isKnownType;
        ElementType = null;
        SpecialTypes = OneOrMore<string>.Null;
    }

    public UnresolvedSpecPropertyType(string value, string? elementType)
    {
        Value = value;
        ElementType = elementType;
        SpecialTypes = OneOrMore<string>.Null;
        IsKnownType = true;
    }

    public UnresolvedSpecPropertyType(string value, string? elementType, OneOrMore<string> specialTypes)
    {
        Value = value;
        ElementType = elementType;
        SpecialTypes = specialTypes;
        IsKnownType = true;
    }

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is UnresolvedSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(UnresolvedSpecPropertyType other) =>
        other != null && string.Equals(Value, other.Value, StringComparison.Ordinal)
                      && string.Equals(ElementType, other.ElementType, StringComparison.Ordinal)
                      && SpecialTypes.Equals(other.SpecialTypes, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is UnresolvedSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public override int GetHashCode() => Value == null ? 0 : Value.GetHashCode();

    /// <inheritdoc />
    public string DisplayName => "Unresolved Type";

    /// <inheritdoc />
    public string Type => Value;

    /// <inheritdoc />
    public Type ValueType => throw new NotSupportedException();

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => throw new NotSupportedException();

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        throw new NotSupportedException();
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }

    public ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        if (IsKnownType)
        {
            ISpecPropertyType? knownType = KnownTypes.GetType(database, Value, ElementType, SpecialTypes, property);
            if (knownType != null)
                return knownType;

            throw new SpecTypeResolveException($"Failed to resolve type: \"{Value}\".");
        }

        if (property.Owner is ISpecPropertyType specPropertyType && Value.Equals(specPropertyType.Type, StringComparison.Ordinal))
        {
            return specPropertyType;
        }

        ISpecPropertyType propType;

        int nsIndex = Value.IndexOf("::", StringComparison.Ordinal);
        if (nsIndex > 0 && nsIndex < Value.Length - 2)
        {
            if (database.Types.TryGetValue(new QualifiedType(Value.Substring(0, nsIndex)), out AssetSpecType t))
            {
                if (TryResolveType(t, database, out propType, Value.Substring(nsIndex + 2)))
                    return propType;
            }
        }

        if (TryResolveType(assetFile, database, out propType, Value))
            return propType;

        if (database.Types.TryGetValue(new QualifiedType(Value), out AssetSpecType type))
        {
            return type;
        }

        throw new SpecTypeResolveException($"Failed to resolve type: \"{Value}\".");
    }

    /// <inheritdoc />
    public override string ToString() => DisplayName;

    private static bool TryResolveType(ISpecType type, IAssetSpecDatabase database, out ISpecPropertyType propType, string value)
    {
        if (type is AssetSpecType nestedAssetType)
        {
            foreach (ISpecType t in nestedAssetType.Types)
            {
                if (TryResolveType(t, database, out propType, value))
                    return true;
            }

            if (!nestedAssetType.Parent.IsNull && database.Types.TryGetValue(nestedAssetType.Parent, out AssetSpecType info))
            {
                if (TryResolveType(info, database, out propType, value))
                    return true;
            }
        }
        else if (type is ISpecPropertyType propertyType && type.Type.Equals(value))
        {
            propType = propertyType;
            return true;
        }

        propType = null!;
        return false;
    }
}