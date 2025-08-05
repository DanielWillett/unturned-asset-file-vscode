using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal sealed class UnresolvedSpecPropertyType :
    IEquatable<UnresolvedSpecPropertyType>,
    ISecondPassSpecPropertyType
{
    public string Value { get; }

    public UnresolvedSpecPropertyType(string value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is UnresolvedSpecPropertyType t && Equals(t);

    /// <inheritdoc />
    public bool Equals(UnresolvedSpecPropertyType other) =>
        other != null && string.Equals(Value, other.Value, StringComparison.Ordinal);

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

    void ISpecPropertyType.Visit<TVisitor>(TVisitor visitor) { }

    public ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        if (property.Owner is ISpecPropertyType specPropertyType && Value.Equals(specPropertyType.Type, StringComparison.Ordinal))
        {
            return specPropertyType;
        }

        if (TryResolveType(assetFile, database, out ISpecPropertyType propType))
            return propType;

        throw new SpecTypeResolveException($"Failed to resolve type: \"{Value}\".");
    }

    /// <inheritdoc />
    public override string ToString() => DisplayName;

    private bool TryResolveType(ISpecType type, IAssetSpecDatabase database, out ISpecPropertyType propType)
    {
        if (type is AssetSpecType nestedAssetType)
        {
            foreach (ISpecType t in nestedAssetType.Types)
            {
                if (TryResolveType(t, database, out propType))
                    return true;
            }

            if (!nestedAssetType.Parent.IsNull && database.Types.TryGetValue(nestedAssetType.Parent, out AssetSpecType info))
            {
                if (TryResolveType(info, database, out propType))
                    return true;
            }
        }
        else if (type is ISpecPropertyType propertyType && type.Type.Equals(Value))
        {
            propType = propertyType;
            return true;
        }

        propType = null!;
        return false;
    }
}