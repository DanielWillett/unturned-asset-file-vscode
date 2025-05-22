using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ListSpecPropertyType<TElementType> :
    BaseSpecPropertyType<EquatableArray<TElementType>>,
    ISpecPropertyType<EquatableArray<TElementType>>,
    IEquatable<ListSpecPropertyType<TElementType>>
    where TElementType : IEquatable<TElementType>
{
    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "List";

    /// <inheritdoc />
    public Type ValueType => typeof(EquatableArray<TElementType>);

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    public ISpecPropertyType<TElementType> InnerType { get; }

    public ListSpecPropertyType(ISpecPropertyType<TElementType> innerType)
    {
        InnerType = innerType;
        DisplayName = "List of " + innerType.DisplayName;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out EquatableArray<TElementType> val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<EquatableArray<TElementType>>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out EquatableArray<TElementType> value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileListValueNode listNode)
        {
            return FailedToParse(in parse, out value);
        }

        EquatableArray<TElementType> eqArray = new EquatableArray<TElementType>(listNode.Elements.Count);

        bool parsedAll = true;
        int index = 0;
        foreach (AssetFileValueNode node in listNode.Elements)
        {
            SpecPropertyTypeParseContext context = parse with
            {
                Node = node,
                Parent = listNode
            };

            if (!InnerType.TryParseValue(in context, out TElementType? element) || element == null)
            {
                value = eqArray;
                parsedAll = false;
            }
            else
            {
                eqArray.Array[index] = element;
            }
            ++index;
        }

        if (index < eqArray.Array.Length)
        {
            eqArray = new EquatableArray<TElementType>(eqArray.Array, index);
        }

        value = eqArray;
        return parsedAll;
    }

    /// <inheritdoc />
    public bool Equals(ListSpecPropertyType<TElementType> other) => other != null && InnerType.Equals(other.InnerType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType other) => other is ListSpecPropertyType<TElementType> t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<EquatableArray<TElementType>> other) => other is ListSpecPropertyType<TElementType> t && Equals(t);
}

internal sealed class UnresolvedListSpecPropertyType :
    IEquatable<UnresolvedListSpecPropertyType>,
    ISecondPassSpecPropertyType
{
    public ISecondPassSpecPropertyType InnerType { get; }

    public string DisplayName => "List";
    public string Type => "List";
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    public Type ValueType => throw new NotSupportedException();

    public UnresolvedListSpecPropertyType(ISecondPassSpecPropertyType innerType)
    {
        InnerType = innerType ?? throw new ArgumentNullException(nameof(innerType));
    }

    public bool Equals(UnresolvedListSpecPropertyType other) => other != null && InnerType.Equals(other.InnerType);
    public bool Equals(ISpecPropertyType other) => other is UnresolvedListSpecPropertyType l && Equals(l);
    public override bool Equals(object? obj) => obj is UnresolvedListSpecPropertyType l && Equals(l);
    public override int GetHashCode() => InnerType.GetHashCode();
    public override string ToString() => $"Unresolved List of {InnerType.Type}";

    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => null;

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        value = null!;
        return false;
    }

    public ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        return KnownTypes.List(InnerType.Transform(property, database, assetFile));
    }
}