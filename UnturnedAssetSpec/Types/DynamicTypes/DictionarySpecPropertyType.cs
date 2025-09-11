using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

// todo: support for KeyEnumType, KeyAllowExtraValues
public sealed class DictionarySpecPropertyType<TElementType> :
    BaseSpecPropertyType<EquatableArray<DictionaryPair<TElementType>>>,
    ISpecPropertyType<EquatableArray<DictionaryPair<TElementType>>>,
    IEquatable<DictionarySpecPropertyType<TElementType>?>,
    IElementTypeSpecPropertyType
    where TElementType : IEquatable<TElementType>
{
    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "Dictionary";

    /// <inheritdoc />
    public Type ValueType => typeof(EquatableArray<TElementType>);

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    public ISpecPropertyType<TElementType> InnerType { get; }

    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public DictionarySpecPropertyType(ISpecPropertyType<TElementType> innerType)
    {
        InnerType = innerType;
        DisplayName = "Dictionary of " + innerType.DisplayName;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        if (!TryParseValue(in parse, out EquatableArray<DictionaryPair<TElementType>> val))
        {
            value = null!;
            return false;
        }

        value = new SpecDynamicConcreteValue<EquatableArray<DictionaryPair<TElementType>>>(val, this);
        return true;
    }

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out EquatableArray<DictionaryPair<TElementType>> value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileDictionaryValueNode dictNode)
        {
            return FailedToParse(in parse, out value);
        }

        EquatableArray<DictionaryPair<TElementType>> eqArray = new EquatableArray<DictionaryPair<TElementType>>(dictNode.Pairs.Count);

        bool parsedAll = true;
        int index = 0;
        foreach (AssetFileKeyValuePairNode node in dictNode.Pairs)
        {
            string key = node.Key.Value;
            if (node.Value == null)
            {
                if (parse.HasDiagnostics)
                {
                    DatDiagnosticMessage message = new DatDiagnosticMessage
                    {
                        Diagnostic = DatDiagnostics.UNT1005,
                        Message = string.Format(DiagnosticResources.UNT1005, key),
                        Range = node.Range
                    };

                    parse.Log(message);
                }

                parsedAll = false;
                continue;
            }

            if (!TryParseElement(node.Value, dictNode, in parse, out TElementType element))
            {
                parsedAll = false;
            }
            else
            {
                eqArray.Array[index] = new DictionaryPair<TElementType>(key, element);
            }

            ++index;
        }

        if (index < eqArray.Array.Length)
        {
            eqArray = new EquatableArray<DictionaryPair<TElementType>>(eqArray.Array, index);
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
    public bool Equals(DictionarySpecPropertyType<TElementType>? other) => other != null && InnerType.Equals(other.InnerType);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is DictionarySpecPropertyType<TElementType> t && Equals(t);

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<EquatableArray<DictionaryPair<TElementType>>>? other) => other is DictionarySpecPropertyType<TElementType> t && Equals(t);

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}

/// <summary>
/// A key-value-pair used by <see cref="DictionarySpecPropertyType{TElementType}"/>.
/// </summary>
/// <typeparam name="TElementType">The value type.</typeparam>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct DictionaryPair<TElementType> : IEquatable<DictionaryPair<TElementType>> where TElementType : IEquatable<TElementType>
{
    public string Key { get; }
    public TElementType Value { get; }

    public DictionaryPair(string key, TElementType value)
    {
        Key = key;
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(DictionaryPair<TElementType> other)
    {
        return string.Equals(other.Key, Key, StringComparison.OrdinalIgnoreCase) && (Value == null ? other.Value == null : Value.Equals(other.Value));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DictionaryPair<TElementType> pair && Equals(pair);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (Key == null)
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        int hc = StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
        return Value == null ? hc : (hc ^ Value.GetHashCode());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"({Key}, {Value})";
    }
}

internal sealed class UnresolvedDictionarySpecPropertyType :
    IEquatable<UnresolvedDictionarySpecPropertyType?>,
    ISecondPassSpecPropertyType,
    IDisposable
{
    public ISecondPassSpecPropertyType InnerType { get; }

    public string DisplayName => "Dictionary";
    public string Type => "Dictionary";
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    public Type ValueType => throw new NotSupportedException();

    public UnresolvedDictionarySpecPropertyType(ISecondPassSpecPropertyType innerType)
    {
        InnerType = innerType ?? throw new ArgumentNullException(nameof(innerType));
    }

    public bool Equals(UnresolvedDictionarySpecPropertyType? other) => other != null && InnerType.Equals(other.InnerType);
    public bool Equals(ISpecPropertyType? other) => other is UnresolvedDictionarySpecPropertyType l && Equals(l);
    public override bool Equals(object? obj) => obj is UnresolvedDictionarySpecPropertyType l && Equals(l);
    public override int GetHashCode() => InnerType.GetHashCode();
    public override string ToString() => $"Unresolved Dictionary of {InnerType.Type}";
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
        return KnownTypes.Dictionary(InnerType.Transform(property, database, assetFile));
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }
}