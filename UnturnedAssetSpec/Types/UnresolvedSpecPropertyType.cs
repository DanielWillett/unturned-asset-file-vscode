using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal sealed class UnresolvedSpecPropertyType : ISpecPropertyType, IEquatable<UnresolvedSpecPropertyType>
{
    public string Value { get; }

    public UnresolvedSpecPropertyType(string value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is UnresolvedSpecPropertyType t && Equals(t);

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
    public string Type => "";

    /// <inheritdoc />
    public Type ValueType => throw new NotSupportedException();

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => throw new NotSupportedException();

    /// <inheritdoc />
    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => null;

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public override string ToString() => DisplayName;

    public ISpecPropertyType Resolve(SpecProperty property, AssetSpecDatabase database, AssetTypeInformation assetFile)
    {
        if (property.Owner is ISpecPropertyType specPropertyType && Type.Equals(specPropertyType.Type, StringComparison.Ordinal))
        {
            return specPropertyType;
        }

        if (TryResolveType(assetFile, database, out ISpecPropertyType propType))
            return propType;

        throw new Exception($"Failed to resolve type: \"{Type}\".");
    }

    private bool TryResolveType(ISpecType type, AssetSpecDatabase database, out ISpecPropertyType propType)
    {
        if (type is AssetTypeInformation nestedAssetType)
        {
            foreach (ISpecType t in nestedAssetType.Types)
            {
                if (TryResolveType(t, database, out propType))
                    return true;
            }

            if (!nestedAssetType.Parent.IsNull && database.Types.TryGetValue(nestedAssetType.Parent, out AssetTypeInformation info))
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

public readonly struct SpecPropertyTypeResolveContext
{
    public SpecProperty Property { get; init; }
    public ITypeInformation Type { get; init; }
    public AssetTypeInformation File { get; init; }
    public AssetSpecDatabase Database { get; init; }
}