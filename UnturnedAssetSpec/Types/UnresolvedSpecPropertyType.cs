using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

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

    /// <inheritdoc />
    public override string ToString() => DisplayName;

    public ISpecPropertyType Resolve(SpecProperty property, AssetSpecDatabase database, AssetTypeInformation assetFile)
    {
        ISpecPropertyType? type = KnownTypes.GetType(Value, property, property.ElementType);
        if (type != null)
        {
            if (type is INestedSpecPropertyType nested)
                nested.ResolveInnerTypes(property, database, assetFile);
            return type;
        }

        throw new TypeAccessException();
    }
}

public readonly struct SpecPropertyTypeResolveContext
{
    public SpecProperty Property { get; init; }
    public ITypeInformation Type { get; init; }
    public AssetTypeInformation File { get; init; }
    public AssetSpecDatabase Database { get; init; }
}