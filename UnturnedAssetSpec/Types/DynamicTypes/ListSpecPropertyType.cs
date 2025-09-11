using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ListSpecPropertyType<TElementType> :
    BaseSpecPropertyType<EquatableArray<TElementType>>,
    ISpecPropertyType<EquatableArray<TElementType>>,
    IEquatable<ListSpecPropertyType<TElementType>?>,
    IElementTypeSpecPropertyType
    where TElementType : IEquatable<TElementType>
{

    public bool AllowSingle { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => AllowSingle ? "ListOrSingle" : "List";

    /// <inheritdoc />
    public Type ValueType => typeof(EquatableArray<TElementType>);

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    public ISpecPropertyType<TElementType> InnerType { get; }

    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public ListSpecPropertyType(ISpecPropertyType<TElementType> innerType, bool allowSingle)
    {
        InnerType = innerType;
        DisplayName = "List of " + innerType.DisplayName;
        AllowSingle = allowSingle;
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
            if (!AllowSingle)
                return FailedToParse(in parse, out value);

            if (!TryParseElement(parse.Node, parse.Parent, in parse, out TElementType element))
            {
                value = EquatableArray<TElementType>.Empty;
                return false;
            }

            value = new EquatableArray<TElementType>(new TElementType[] { element });
            return true;
        }

        EquatableArray<TElementType> eqArray = new EquatableArray<TElementType>(listNode.Elements.Count);

        bool parsedAll = true;
        int index = 0;
        foreach (AssetFileValueNode node in listNode.Elements)
        {
            if (!TryParseElement(node, listNode, in parse, out TElementType element))
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

    private bool TryParseElement(AssetFileValueNode node, AssetFileNode? parent, in SpecPropertyTypeParseContext parse, out TElementType element)
    {
        SpecPropertyTypeParseContext context = parse with
        {
            Node = node,
            Parent = parent
        };

        return InnerType.TryParseValue(in context, out element!) && element != null;
    }

    /// <inheritdoc />
    public bool Equals(ListSpecPropertyType<TElementType>? other) => other != null && InnerType.Equals(other.InnerType) && AllowSingle == other.AllowSingle;

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is ListSpecPropertyType<TElementType> t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<EquatableArray<TElementType>>? other) => other is ListSpecPropertyType<TElementType> t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}

internal sealed class UnresolvedListSpecPropertyType :
    IEquatable<UnresolvedListSpecPropertyType?>,
    ISecondPassSpecPropertyType,
    IDisposable
{
    public ISecondPassSpecPropertyType InnerType { get; }

    public bool AllowSingle { get; }
    public string DisplayName => "List";
    public string Type => AllowSingle ? "ListOrSingle" : "List";
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    public Type ValueType => throw new NotSupportedException();

    public UnresolvedListSpecPropertyType(ISecondPassSpecPropertyType innerType, bool allowSingle)
    {
        InnerType = innerType ?? throw new ArgumentNullException(nameof(innerType));
        AllowSingle = allowSingle;
    }

    public bool Equals(UnresolvedListSpecPropertyType? other) => other != null && InnerType.Equals(other.InnerType) && AllowSingle == other.AllowSingle;
    public bool Equals(ISpecPropertyType? other) => other is UnresolvedListSpecPropertyType l && Equals(l);
    public override bool Equals(object? obj) => obj is UnresolvedListSpecPropertyType l && Equals(l);
    public override int GetHashCode() => InnerType.GetHashCode();
    public override string ToString() => $"Unresolved List of {InnerType.Type}";
    public void Dispose()
    {
        if (InnerType is IDisposable d)
            d.Dispose();
    }

    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        value = null!;
        return false;
    }

    public ISpecPropertyType Transform(SpecProperty property, IAssetSpecDatabase database, AssetSpecType assetFile)
    {
        return KnownTypes.List(InnerType.Transform(property, database, assetFile), AllowSingle);
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }
}