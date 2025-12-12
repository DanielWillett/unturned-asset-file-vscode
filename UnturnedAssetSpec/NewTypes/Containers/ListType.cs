using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.NewTypes;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Factory class for the <see cref="ListType{TCountType,TElementType}"/> type.
/// </summary>
public static class ListType
{
    public const string TypeId = "List";

    /// <summary>
    /// Create a new list type.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <typeparam name="TCountType">The data-type of the count of the list.</typeparam>
    /// <param name="args">Parameters for how the list should be parsed.</param>
    /// <param name="subType">The element type of the list.</param>
    public static ListType<TCountType, TElementType> Create<TCountType, TElementType>(
        ListTypeArgs<TCountType, TElementType> args,
        IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
        where TCountType : unmanaged, IConvertible, IComparable<TCountType>, IEquatable<TCountType>
    {
        return new ListType<TCountType, TElementType>(args, subType);
    }

    /// <summary>
    /// Create a new list type that uses an integer as the count type.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <param name="args">Parameters for how the list should be parsed.</param>
    /// <param name="subType">The element type of the list.</param>
    public static ListType<int, TElementType> Create<TElementType>(
        ListTypeArgs<int, TElementType> args,
        IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new ListType<int, TElementType>(args, subType);
    }

    /// <summary>
    /// Create a new list type that parses a modern list syntax with no special rules.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <param name="subType">The element type of the list.</param>
    public static ListType<int, TElementType> Create<TElementType>(IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new ListType<int, TElementType>(new ListTypeArgs<int, TElementType>
        {
            Mode = ListMode.ModernList
        }, subType);
    }
}

/// <summary>
/// A container type which allows multiple <see cref="TElementType"/> sub-elements.
/// </summary>
/// <remarks>Use the factory methods in <see cref="ListType"/> to create a list type.</remarks>
/// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
/// <typeparam name="TCountType">The data-type of the count of the list.</typeparam>
public class ListType<TCountType, TElementType>
    : BaseType<EquatableArray<TElementType>, ListType<TCountType, TElementType>>, ITypeParser<EquatableArray<TElementType>>
    where TElementType : IEquatable<TElementType>
    where TCountType : unmanaged, IConvertible, IComparable<TCountType>, IEquatable<TCountType>
{
    private readonly ListTypeArgs<TCountType, TElementType> _args;
    private readonly IType<TElementType> _subType;
    private readonly ITypeConverter<TCountType>? _countConverter;
    private readonly int? _minCount;
    private readonly int? _maxCount;

    public override string Id => ListType.TypeId;

    public override string DisplayName { get; }

    public override ITypeParser<EquatableArray<TElementType>> Parser => this;

    /// <summary>
    /// Use the factory methods in <see cref="ListType"/> to create a list type.
    /// </summary>
    internal ListType(ListTypeArgs<TCountType, TElementType> args, IType<TElementType> subType)
    {
        _args = args;
        _subType = subType;
        DisplayName = string.Format(Properties.Resources.Type_Name_List_Generic, subType.DisplayName);

        if ((args.Mode & ListMode.Legacy) != 0)
        {
            _countConverter = Parsing.TypeConverters.Get<TCountType>();
        }

        try
        {
            _minCount = _args.MinimumCount?.ToInt32(CultureInfo.InvariantCulture);
            if (_minCount is <= 0)
                _minCount = null;
        }
        catch (OverflowException)
        {
            _minCount = null;
        }

        try
        {
            _maxCount = _args.MaximumCount?.ToInt32(CultureInfo.InvariantCulture);
            if (_maxCount is < 0)
                _maxCount = null;
        }
        catch (OverflowException)
        {
            _maxCount = null;
        }
    }

    protected override bool Equals(ListType<TCountType, TElementType> other)
    {
        return _args.Equals(in other._args);
    }

    private void CheckCount(int ct, ref TypeParserArgs<EquatableArray<TElementType>> args)
    {
        if (ct < _minCount)
        {
            args.DiagnosticSink?.UNT1024_Less(ref args, args.ParentNode, _minCount.Value);
        }

        if (ct > _maxCount)
        {
            args.DiagnosticSink?.UNT1024_More(ref args, args.ParentNode, _maxCount.Value);
        }
    }

    public bool TryParse(ref TypeParserArgs<EquatableArray<TElementType>> args, in FileEvaluationContext ctx, out Optional<EquatableArray<TElementType>> value)
    {
        value = Optional<EquatableArray<TElementType>>.Null;
        bool legacy = (_args.Mode & ListMode.Legacy) != 0;
        bool modern = (_args.Mode & ListMode.Modern) != 0;
        if (!modern && !legacy) modern = true;

        if (legacy && (_args.Mode & ListMode.LegacySingle) == ListMode.LegacySingle)
        {
            string? singlePropertyName = _args.LegacySingleKey;
            
            if (string.IsNullOrEmpty(singlePropertyName))
                singlePropertyName = _args.LegacySingularKey;

            if (string.IsNullOrEmpty(singlePropertyName))
            {
                if (args.ParentNode is IPropertySourceNode property)
                {
                    singlePropertyName = property.Key;
                }
                else
                {
                    singlePropertyName = ctx.Self.Key;
                }
            }

            if (args.ParentNode is not IPropertySourceNode { Parent: IDictionarySourceNode dictionary }
                || !dictionary.TryGetProperty(singlePropertyName!, out IPropertySourceNode? singularProperty))
            {
                // todo
            }
        }

        bool allFailed = true;
        switch (args.ValueNode)
        {
            // null (no value)
            default:
                if (!modern)
                    args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                else
                    args.DiagnosticSink?.UNT2004_NoList(ref args, args.ParentNode);

                return false;

            // wrong type of value
            case IDictionarySourceNode dictionaryNode:
                if (!modern)
                    args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref args, dictionaryNode, this);
                else
                    args.DiagnosticSink?.UNT2004_DictionaryInsteadOfList(ref args, dictionaryNode, this);

                return false;

            case IListSourceNode listNode:
                if ((_args.Mode & ListMode.ModernList) != ListMode.ModernList)
                {
                    args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, listNode, this);
                    return false;
                }

                CheckCount(listNode.Count, ref args);
                if (listNode.Count == 0)
                {
                    value = EquatableArray<TElementType>.Empty;
                    return true;
                }

                TElementType?[] array = new TElementType?[listNode.Count];
                int index = 0;
                ImmutableArray<ISourceNode> values = listNode.Children;
                for (int i = 0; i < values.Length; ++i)
                {
                    ISourceNode node = values[i];
                    if (node is not IAnyValueSourceNode v)
                        continue;

                    args.CreateSubTypeParserArgs(out TypeParserArgs<TElementType> parseArgs, v, listNode, _subType);

                    if (!_subType.Parser.TryParse(ref parseArgs, in ctx, out Optional<TElementType> elementType) || !elementType.HasValue)
                    {
                        if (!parseArgs.ShouldIgnoreFailureDiagnostic)
                        {
                            args.DiagnosticSink?.UNT2004_Generic(ref args, v.ToString(), _subType);
                        }
                    }
                    else
                    {
                        array[index] = elementType.Value;
                        ++index;
                        allFailed = false;
                    }
                }

                value = new EquatableArray<TElementType>(array!);
                return !allFailed;

            case IValueSourceNode valueNode:
                bool couldBeModernSingle = modern && (_args.Mode & ListMode.ModernSingle) == ListMode.ModernSingle;
                if (!legacy && !couldBeModernSingle)
                {
                    args.DiagnosticSink?.UNT2004_ValueInsteadOfList(ref args, valueNode, this);
                    return false;
                }



                break;

        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(1745437037, _args.GetHashCode());
}

/// <summary>
/// Parameters for <see cref="ListType{TCountType,TElementType}"/> types.
/// </summary>
/// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
/// <typeparam name="TCountType">The data-type of the count of the list.</typeparam>
public readonly struct ListTypeArgs<TCountType, TElementType>
    where TElementType : IEquatable<TElementType>
    where TCountType : unmanaged, IConvertible, IComparable<TCountType>, IEquatable<TCountType>
{
    /// <summary>
    /// The type of lists to parse.
    /// </summary>
    public required ListMode Mode { get; init; }

    /// <summary>
    /// Minimum number of elements in the list (inclusive).
    /// </summary>
    public TCountType? MinimumCount { get; init; }

    /// <summary>
    /// Maximum number of elements in the list (inclusive).
    /// </summary>
    public TCountType? MaximumCount { get; init; }

    /// <summary>
    /// The key used for legacy list elements. Ex 'Condition'_0 for 'Conditions'.
    /// </summary>
    public string? LegacySingularKey { get; init; }

    /// <summary>
    /// The key used for the legacy single list. Ex 'Blade_ID' 4356 for 'Blade_IDs'. Defaults to <see cref="LegacySingularKey"/>.
    /// </summary>
    public string? LegacySingleKey { get; init; }

    public bool Equals(in ListTypeArgs<TCountType, TElementType> other)
    {
        return other.Mode == Mode
               && string.Equals(other.LegacySingularKey, LegacySingularKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(other.LegacySingleKey, LegacySingleKey, StringComparison.OrdinalIgnoreCase)
               && EqualityComparer<TCountType?>.Default.Equals(other.MinimumCount, MinimumCount)
               && EqualityComparer<TCountType?>.Default.Equals(other.MaximumCount, MaximumCount)
               ;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mode, LegacySingularKey, LegacySingleKey, MinimumCount, MaximumCount);
    }
}

/// <summary>
/// Describes how <see cref="ListType{TCountType,TElementType}"/> values can be parsed.
/// </summary>
[Flags]
public enum ListMode
{
    /// <summary>
    /// Whether or not modern properties can be parsed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    Modern = 1,

    /// <summary>
    /// Whether or not legacy properties can be parsed.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    Legacy = 2,
    
    /// <summary>
    /// Whether or not single legacy lists can be parsed.
    /// </summary>
    LegacySingle = 4 | Legacy,

    /// <summary>
    /// Whether or not normal legacy lists can be parsed.
    /// </summary>
    LegacyList = 8 | Legacy,

    /// <summary>
    /// Whether or not single modern lists can be parsed.
    /// </summary>
    ModernSingle = 16 | Modern,

    /// <summary>
    /// Whether or not normal modern lists can be parsed.
    /// </summary>
    ModernList = 32 | Modern
}