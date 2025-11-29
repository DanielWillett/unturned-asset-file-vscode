using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
public sealed class DictionarySpecPropertyType<TElementType> :
    BaseSpecPropertyType<DictionarySpecPropertyType<TElementType>, EquatableArray<DictionaryPair<TElementType>>>,
    ISpecPropertyType<EquatableArray<DictionaryPair<TElementType>>>,
    IEquatable<DictionarySpecPropertyType<TElementType>?>,
    IDictionaryTypeSpecPropertyType
    where TElementType : IEquatable<TElementType>
{
    private readonly IAssetSpecDatabase _database;
    private bool _hasKeyEnumType;
    private EnumSpecType? _keyEnumType;
    private bool _keyEnumTypeAllowExtraValues;

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

    [SkipLocalsInit]
    private EnumSpecType? TryGetKeyEnumType(SpecProperty property, AssetFileType fileType, out bool keyAllowExtraValues, ISourceNode? range = null, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        string? keyEnumTypeStr;
        if (_hasKeyEnumType)
        {
            keyAllowExtraValues = _keyEnumTypeAllowExtraValues;
            if (diagnostics != null
                && _keyEnumType == null
                && range != null
                && property.TryGetAdditionalProperty("KeyEnumType", out keyEnumTypeStr)
                && !string.IsNullOrEmpty(keyEnumTypeStr))
            {
                LogDiagnostic(diagnostics, keyEnumTypeStr, range!);
            }

            return _keyEnumType;
        }

        if (property.TryGetAdditionalProperty("KeyEnumType", out keyEnumTypeStr) && !string.IsNullOrEmpty(keyEnumTypeStr))
        {
            _keyEnumType = _database.FindType(keyEnumTypeStr, fileType) as EnumSpecType;
            if (diagnostics != null
                && range != null
                && _keyEnumType == null)
            {
                LogDiagnostic(diagnostics, keyEnumTypeStr, range!);
            }

            property.TryGetAdditionalProperty("KeyAllowExtraValues", out _keyEnumTypeAllowExtraValues);
            _hasKeyEnumType = true;
        }


        keyAllowExtraValues = _keyEnumTypeAllowExtraValues;
        return _keyEnumType;

        static void LogDiagnostic(ICollection<DatDiagnosticMessage> diagnostics, string keyEnumTypeStr, ISourceNode key)
        {
            diagnostics.Add(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2005,
                Message = string.Format(DiagnosticResources.UNT2005, keyEnumTypeStr),
                Range = key.Range
            });
        }
    }

    public Task<AutoCompleteResult[]> GetKeyAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context)
    {
        ISpecType? type = TryGetKeyEnumType(parameters.Property, parameters.FileType, out _);
        if (type is not EnumSpecType enumType)
            return AutoCompleteResult.NoneTask;

        return enumType.GetAutoCompleteResults(in parameters, in context);
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

        bool keyAllowsExtraValues = false;
        EnumSpecType? keyEnumType = parse.HasDiagnostics
            ? TryGetKeyEnumType(
                parse.EvaluationContext.Self,
                parse.FileType,
                out keyAllowsExtraValues,
                (ISourceNode?)(parse.Parent as IPropertySourceNode) ?? parse.Node,
                parse.Diagnostics
            )
            : null;

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

            if (keyEnumType != null && !keyAllowsExtraValues && !keyEnumType.TryParse(key, out int _))
            {
                parse.Log(new DatDiagnosticMessage
                {
                    Range = property.Range,
                    Diagnostic = DatDiagnostics.UNT1014,
                    Message = string.Format(DiagnosticResources.UNT1014_Specific, key, keyEnumType.DisplayName)
                });
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

    Task<AutoCompleteResult[]> IDictionaryTypeSpecPropertyType.GetKeyAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context)
    {
        throw new NotSupportedException();
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