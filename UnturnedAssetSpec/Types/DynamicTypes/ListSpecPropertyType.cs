using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

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
    public Type ValueType => typeof(string);

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