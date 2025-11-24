using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A dictionary with strings as the keys and <typeparamref name="TElementType"/> as the values.
/// <para>Example: <c>LevelAsset.Skillset_Loadouts</c></para>
/// <code>
/// Prop
/// {
///     Key1 1
///     Key2 -4
/// }
/// </code>
/// <para>
/// Has support for the <c>KeyEnumType</c> property which enables enum auto-completion for keys.
/// When using that property, <c>KeyAllowExtraValues</c> can be used to indicate that invalid enum values shouldn't raise warnings.
/// </para>
/// <para>
/// Also supports the <c>MinimumCount</c> and <c>MaximumCount</c> properties for property count limits.
/// </para>
/// </summary>
// todo: support for KeyEnumType, KeyAllowExtraValues
public sealed class DictionarySpecPropertyType<TElementType> :
    BaseSpecPropertyType<DictionarySpecPropertyType<TElementType>, EquatableArray<DictionaryPair<TElementType>>>,
    ISpecPropertyType<EquatableArray<DictionaryPair<TElementType>>>,
    IEquatable<DictionarySpecPropertyType<TElementType>?>,
    IDictionaryTypeSpecPropertyType
    where TElementType : IEquatable<TElementType>
{
    private readonly IAssetSpecDatabase _database;

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "Dictionary";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Struct;

    public ISpecPropertyType<TElementType> InnerType { get; }
    ISpecPropertyType IDictionaryTypeSpecPropertyType.GetInnerType(IAssetSpecDatabase database) => InnerType;

    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public override int GetHashCode()
    {
        return 68 ^ InnerType.GetHashCode();
    }

    public DictionarySpecPropertyType(IAssetSpecDatabase database, ISpecPropertyType<TElementType> innerType)
    {
        _database = database.ResolveFacade();
        InnerType = innerType;
        DisplayName = "Dictionary of " + innerType.DisplayName;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out EquatableArray<DictionaryPair<TElementType>> value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IDictionarySourceNode dictNode)
        {
            return FailedToParse(in parse, out value);
        }

        ImmutableArray<ISourceNode> children = dictNode.Children;
        EquatableArray<DictionaryPair<TElementType>> eqArray = new EquatableArray<DictionaryPair<TElementType>>(children.Length);

        bool parsedAll = true;
        int index = 0;
        foreach (ISourceNode node in children)
        {
            if (node is not IPropertySourceNode property)
                continue;

            string key = property.Key;
            IAnyValueSourceNode? val = property.Value;
            if (val == null)
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

            if (!TryParseElement(val, dictNode, in parse, out TElementType element))
            {
                parsedAll = false;
            }
            else
            {
                eqArray.Array[index] = new DictionaryPair<TElementType>(key, element);
            }

            ++index;
        }

        KnownTypeValueHelper.TryGetMinimaxCountWarning(children.Length, in parse);

        if (index < eqArray.Array.Length)
        {
            eqArray = new EquatableArray<DictionaryPair<TElementType>>(eqArray.Array, index);
        }

        value = eqArray;
        return parsedAll;
    }

    private bool TryParseElement(IAnyValueSourceNode node, ISourceNode? parent, in SpecPropertyTypeParseContext parse, out TElementType element)
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
}

/// <summary>
/// A case-insensitive key-value-pair used by <see cref="DictionarySpecPropertyType{TElementType}"/>.
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
    IDictionaryTypeSpecPropertyType,
    IDisposable
{
    private readonly IAssetSpecDatabase _database;
    public ISecondPassSpecPropertyType InnerType { get; }
    ISpecPropertyType IDictionaryTypeSpecPropertyType.GetInnerType(IAssetSpecDatabase database) => InnerType;
    string IElementTypeSpecPropertyType.ElementType => InnerType.Type;

    public string DisplayName => "Dictionary";
    public string Type => "Dictionary";
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;
    public Type ValueType => throw new NotSupportedException();

    public UnresolvedDictionarySpecPropertyType(IAssetSpecDatabase database, ISecondPassSpecPropertyType innerType)
    {
        _database = database;
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
        return KnownTypes.Dictionary(_database, InnerType.Transform(property, database, assetFile));
    }

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) { }
}