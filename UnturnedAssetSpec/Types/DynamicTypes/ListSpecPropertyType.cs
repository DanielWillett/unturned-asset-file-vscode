using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A list of objects than can be only be formatted in the modern format.
/// If <see cref="AllowSingle"/> is <see langword="true"/>, also can parse one value as a list of length 1.
/// <para>Example: <c>LevelAsset.Crafting_Blacklists</c></para>
/// <code>
/// // list
/// Props
/// [
///     {
///         Value 3
///         Mode Add
///     }
///     {
///         Value 4
///         Mode Subtract
///     }
/// ]
///
/// // single
/// {
///     Value 3
///     Mode Add
/// }
/// </code>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for list element count limits.
/// </para>
/// </summary>
public sealed class ListSpecPropertyType<TElementType> :
    BaseSpecPropertyType<ListSpecPropertyType<TElementType>, EquatableArray<TElementType>>,
    ISpecPropertyType<EquatableArray<TElementType>>,
    ISpecPropertyType<int>,
    IEquatable<ListSpecPropertyType<TElementType>?>,
    IListTypeSpecPropertyType
    where TElementType : IEquatable<TElementType>
{
    public bool AllowSingle { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => AllowSingle ? "ListOrSingle" : "List";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    public ISpecPropertyType<TElementType> InnerType { get; }
    ISpecPropertyType? IListTypeSpecPropertyType.GetInnerType() => InnerType;

    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public override int GetHashCode()
    {
        // 74 - 75
        return (74 + (AllowSingle ? 1 : 0)) ^ InnerType.GetHashCode();
    }

    public ListSpecPropertyType(ISpecPropertyType<TElementType> innerType, bool allowSingle)
    {
        InnerType = innerType;
        DisplayName = "List of " + innerType.DisplayName;
        AllowSingle = allowSingle;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out EquatableArray<TElementType> value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        int suppliedCount;
        bool parsedAll = true;
        if (parse.Node is not IListSourceNode listNode)
        {
            if (!AllowSingle)
                return FailedToParse(in parse, out value);

            suppliedCount = 1;
            if (!TryParseElement(parse.Node, parse.Parent, in parse, out TElementType element))
            {
                value = EquatableArray<TElementType>.Empty;
                parsedAll = false;
            }
            else
            {
                value = new EquatableArray<TElementType>(new TElementType[] { element });
            }
        }
        else
        {
            ImmutableArray<ISourceNode> children = listNode.Children;
            EquatableArray<TElementType> eqArray = new EquatableArray<TElementType>(children.Length);
            
            suppliedCount = listNode.Count;

            int index = 0;
            foreach (ISourceNode node in children)
            {
                if (node is not IAnyValueSourceNode anyVal)
                    continue;

                if (!TryParseElement(anyVal, listNode, in parse, out TElementType element))
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
        }

        KnownTypeValueHelper.TryGetMinimaxCountWarning(suppliedCount, in parse);

        return parsedAll;
    }

    private bool TryParseElement(IAnyValueSourceNode node, IParentSourceNode? parent, in SpecPropertyTypeParseContext parse, out TElementType element)
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
    bool IEquatable<ISpecPropertyType<int>?>.Equals(ISpecPropertyType<int>? other) => other is ListSpecPropertyType<TElementType> l && Equals(l);

    /// <inheritdoc />
    bool ISpecPropertyType<int>.TryParseValue(in SpecPropertyTypeParseContext parse, out int value)
    {
        if (parse.Node == null)
        {
            value = 0;
            return MissingNode(in parse, out _);
        }

        bool parsedAll = true;
        if (parse.Node is not IListSourceNode listNode)
        {
            if (!AllowSingle)
            {
                value = 0;
                return FailedToParse(in parse, out _);
            }

            value = 1;
        }
        else
        {
            value = listNode.Children.Length;
        }

        KnownTypeValueHelper.TryGetMinimaxCountWarning(value, in parse);

        return parsedAll;
    }

    /// <inheritdoc />
    ISpecDynamicValue ISpecPropertyType<int>.CreateValue(int value) => SpecDynamicValue.Int32(value, this);
}

internal sealed class UnresolvedListSpecPropertyType :
    IEquatable<UnresolvedListSpecPropertyType?>,
    ISecondPassSpecPropertyType,
    IListTypeSpecPropertyType,
    IDisposable
{
    public ISecondPassSpecPropertyType InnerType { get; }
    ISpecPropertyType? IListTypeSpecPropertyType.GetInnerType() => InnerType;
    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

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