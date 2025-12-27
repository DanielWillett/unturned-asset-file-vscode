using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Factory class for the <see cref="ListType{TCountType,TElementType}"/> type.
/// </summary>
public sealed class ListType : ITypeFactory
{
    public const string TypeId = "List";

    /// <summary>
    /// Factory used to create <see cref="ListType{TCountType,TElementType}"/> values from JSON.
    /// </summary>
    public static ITypeFactory Factory { get; } = new ListType();

    private ListType() { }
    static ListType() { }

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

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context)
    {
        ElementTypeVisitor v;
        v.Result = null;
        v.Spec = spec;
        v.Owner = owner;
        v.Context = context;
        v.Json = typeDefinition;
        if (typeDefinition.TryGetProperty("ElementType"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
        {
            IType elementType = spec.ReadType(in element, owner, context);
            elementType.Visit(ref v);
        }
        else
        {
            throw new JsonException(
                string.Format(
                    Resources.JsonException_RequiredTypePropertyMissing,
                    "ElementType",
                    "List",
                    context.Length != 0 ? $"{owner.FullName}.{context}" : owner.FullName
                )
            );
        }

        return v.Result ?? throw new JsonException(
            string.Format(
                Resources.JsonException_FailedToParseValue,
                "IType<> (invalid type)",
                context.Length != 0 ? $"{owner.FullName}.{context}" : $"{owner.FullName}"
            )
        );
    }

    private struct ElementTypeVisitor : ITypeVisitor
    {
        public IType? Result;
        public JsonElement Json;
        public IDatSpecificationReadContext Spec;
        public IDatSpecificationObject Owner;
        public string Context;

        public void Accept<TElementType>(IType<TElementType> type) where TElementType : IEquatable<TElementType>
        {
            CountTypeVisitor<TElementType> v;
            v.Result = null;
            v.Spec = Spec;
            v.Owner = Owner;
            v.Context = Context;
            v.Json = Json;
            v.SubType = type;
            if (Json.TryGetProperty("CountType"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
            {
                IType countType = Spec.ReadType(in element, Owner, Context);
                countType.Visit(ref v);
            }
            else
            {
                v.Accept(Int32Type.Instance);
            }

            Result = v.Result;
        }
    }
    private struct CountTypeVisitor<TElementType> : ITypeVisitor where TElementType : IEquatable<TElementType>
    {
        public IType? Result;
        public JsonElement Json;
        public IDatSpecificationReadContext Spec;
        public IDatSpecificationObject Owner;
        public IType<TElementType> SubType;
        public string Context;

        public void Accept<TCountType>(IType<TCountType> type) where TCountType : IEquatable<TCountType>
        {
            ListMode mode = ListMode.ModernList;

            if (Json.TryGetProperty("Mode"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
            {
                if (!Enum.TryParse(element.GetString(), out mode) || (mode & (ListMode.Modern | ListMode.Legacy)) == 0)
                {
                    throw new JsonException(
                        string.Format(
                            Resources.JsonException_FailedToParseEnum,
                            nameof(ListMode),
                            element.GetString(),
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.Mode" : $"{Owner.FullName}.Mode"
                        )
                    );
                }
            }


            Optional<TCountType> minValue = Optional<TCountType>.Null, maxValue = Optional<TCountType>.Null;
            if (Json.TryGetProperty("MinimumCount"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                TypeConverterParseArgs<TCountType> parseArgs = new TypeConverterParseArgs<TCountType>(type);
                if (!TypeConverters.Get<TCountType>().TryReadJson(in Json, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            type.Id,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MinimumCount" : $"{Owner.FullName}.MinimumCount"
                        )
                    );
            }
            
            if (Json.TryGetProperty("MaximumCount"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                TypeConverterParseArgs<TCountType> parseArgs = new TypeConverterParseArgs<TCountType>(type);
                if (!TypeConverters.Get<TCountType>().TryReadJson(in Json, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            type.Id,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MaximumCount" : $"{Owner.FullName}.MaximumCount"
                        )
                    );
            }

            IValue<TElementType>? defaultValue = null;
            if (Json.TryGetProperty("LegacyDefaultElementTypeValue"u8, out element))
            {
                defaultValue = Spec.ReadValue(in element, SubType, Owner, Context.Length == 0 ? "LegacyDefaultElementTypeValue" : $"{Context}.LegacyDefaultElementTypeValue");
            }

            string? legacySingleKey = null, legacySingularKey = null;
            if (Json.TryGetProperty("LegacySingleKey"u8, out element) && element.ValueKind != JsonValueKind.Null)
                legacySingleKey = element.GetString();
            if (Json.TryGetProperty("LegacySingularKey"u8, out element) && element.ValueKind != JsonValueKind.Null)
                legacySingularKey = element.GetString();

            bool skipUnderscoreInLegacyKey = false;
            if (Json.TryGetProperty("SkipUnderscoreInLegacyKey"u8, out element) && element.ValueKind != JsonValueKind.Null)
                skipUnderscoreInLegacyKey = element.GetBoolean();

            Result = new ListType<TCountType, TElementType>(new ListTypeArgs<TCountType, TElementType>
            {
                Mode = mode,
                MinimumCount = minValue,
                MaximumCount = maxValue,
                LegacyDefaultElementTypeValue = defaultValue,
                LegacySingleKey = legacySingleKey,
                LegacySingularKey = legacySingularKey,
                SkipUnderscoreInLegacyKey = skipUnderscoreInLegacyKey
            }, SubType);
        }
    }
}

/// <summary>
/// A container type which allows multiple <see cref="TElementType"/> sub-elements.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="IType{TElementType}"/> ElementType</c> - The type of elements in the list - required.</item>
///     <item><c><see cref="IType{TCountType}"/> CountType</c> - The type of number to parse for legacy list counts. Defaults to <see cref="Int32Type"/>.</item>
///     <item><c><see cref="ListMode"/> Mode</c> - What kinds of lists to parse (bitwise flag). Defaults to <see cref="ListMode.ModernList"/>.</item>
///     <item><c><typeparamref name="TCountType"/> MinimumCount</c> - Minimum number of items (inclusive).</item>
///     <item><c><typeparamref name="TCountType"/> MaximumCount</c> - Maximum number of items (inclusive).</item>
///     <item><c><see cref="IValue{TElementType}"/> LegacyDefaultElementTypeValue</c> - Default value for undefined legacy elements or elements without values.</item>
///     <item><c><see cref="string"/> LegacySingleKey</c> - Key for the single property when <see cref="ListMode.LegacySingle"/> is included. Defaults to <c>LegacySingularKey</c>.</item>
///     <item><c><see cref="string"/> LegacySingularKey</c> - Base key for elements in legacy lists. For example, 'Condition', for Conditions, Condition_0, etc.</item>
///     <item><c><see cref="string"/> SkipUnderscoreInLegacyKey</c> - Indicates that the '_' should not be used to separate <c>LegacySingularKey</c> and the index. Defaults to <see langword="false"/>.</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>Use the factory methods in <see cref="ListType"/> to create a list type.</remarks>
/// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
/// <typeparam name="TCountType">The data-type of the count of the list.</typeparam>
public class ListType<TCountType, TElementType>
    : BaseType<EquatableArray<TElementType>, ListType<TCountType, TElementType>>, ITypeParser<EquatableArray<TElementType>>
    where TElementType : IEquatable<TElementType>
    where TCountType : IEquatable<TCountType>
{
    private readonly ListTypeArgs<TCountType, TElementType> _args;
    private readonly IType<TElementType> _subType;
    private readonly ITypeConverter<TCountType>? _countConverter;
    private readonly IType<TCountType>? _countType;
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
        DisplayName = string.Format(Resources.Type_Name_List_Generic, subType.DisplayName);

        ITypeConverter<TCountType> countConverter = TypeConverters.Get<TCountType>();
        if ((args.Mode & ListMode.Legacy) != 0)
        {
            _countType = CommonTypes.GetIntegerType<TCountType>();
            _countConverter = countConverter;
        }

        countConverter.TryConvertTo(_args.MinimumCount, out Optional<int> newMinCount);
        countConverter.TryConvertTo(_args.MaximumCount, out Optional<int> newMaxCount);
        _minCount = newMinCount.AsNullable();
        if (_minCount is <= 0)
            _minCount = null;

        _maxCount = newMaxCount.AsNullable();
        if (_maxCount is < 0)
            _maxCount = null;
    }

    #region JSON

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        WriteTypeName(writer);
        writer.WritePropertyName("ElementType"u8);
        _subType.WriteToJson(writer, options);

        if (_countType != null && !Int32Type.Instance.Equals(_countType))
        {
            writer.WritePropertyName("CountType"u8);
            _countType.WriteToJson(writer, options);
        }

        if (_minCount is > 0)
            writer.WriteNumber("MinimumCount"u8, _minCount.Value);

        if (_maxCount is > 0)
            writer.WriteNumber("MaximumCount"u8, _maxCount.Value);

        if (_args.Mode != ListMode.ModernList)
            writer.WriteString("Mode"u8, _args.Mode.ToString());

        if (_args.LegacySingularKey != null)
            writer.WriteString("LegacySingularKey"u8, _args.LegacySingularKey);

        if (_args.LegacySingleKey != null)
            writer.WriteString("LegacySingleKey"u8, _args.LegacySingleKey);

        if (_args.SkipUnderscoreInLegacyKey)
            writer.WriteBoolean("SkipUnderscoreInLegacyKey"u8, _args.SkipUnderscoreInLegacyKey);

        if (_args.LegacyDefaultElementTypeValue != null)
        {
            writer.WritePropertyName("LegacyDefaultElementTypeValue"u8);
            _args.LegacyDefaultElementTypeValue.WriteToJson(writer);
        }
    }

    public bool TryReadValueFromJson(in JsonElement json, out Optional<EquatableArray<TElementType>> value, IType<EquatableArray<TElementType>> valueType)
    {
        value = Optional<EquatableArray<TElementType>>.Null;
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = EquatableArray<TElementType>.Empty;
                return true;

            case JsonValueKind.Array:
                Optional<TElementType> o;
                int len = json.GetArrayLength();
                TElementType[] arr = new TElementType[len];
                for (int i = 0; i < len; ++i)
                {
                    JsonElement element = json[i];
                    if (!_subType.Parser.TryReadValueFromJson(in element, out o, _subType) || !o.HasValue)
                    {
                        return false;
                    }

                    arr[i] = o.Value;
                }

                value = new EquatableArray<TElementType>(arr);
                return true;

            default:
                if (!_subType.Parser.TryReadValueFromJson(in json, out o, _subType) || !o.HasValue)
                {
                    return false;
                }
                value = new EquatableArray<TElementType>(new TElementType[] { o.Value });
                return true;
        }
    }

    public void WriteValueToJson(Utf8JsonWriter writer, EquatableArray<TElementType> value, IType<EquatableArray<TElementType>> valueType)
    {
        if (value.Array == null || value.Array.Length == 0)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (TElementType element in value.Array)
        {
            if (element == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _subType.Parser.WriteValueToJson(writer, element, _subType);
            }
        }

        writer.WriteEndArray();
    }

    #endregion

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
        bool legacy = (_args.Mode & ListMode.Legacy) != 0 && args.KeyFilter != LegacyExpansionFilter.Modern;
        bool modern = (_args.Mode & ListMode.Modern) != 0 && args.KeyFilter != LegacyExpansionFilter.Legacy;
        if (!modern && !legacy) modern = true;

        TElementType?[] array;

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

            if (args.ParentNode is IPropertySourceNode { Parent: IDictionarySourceNode dictionary }
                && dictionary.TryGetProperty(singlePropertyName, out IPropertySourceNode? singularProperty))
            {
                args.ReferencedPropertySink?.AcceptReferencedProperty(singularProperty);

                args.CreateSubTypeParserArgs(out TypeParserArgs<TElementType> parseArgs, singularProperty.Value, args.ParentNode, _subType, LegacyExpansionFilter.Modern);

                if (!_subType.Parser.TryParse(ref parseArgs, in ctx, out Optional<TElementType> element))
                {
                    if (!parseArgs.ShouldIgnoreFailureDiagnostic)
                        args.DiagnosticSink?.UNT2004_Generic(ref args, singularProperty.Value == null ? "-" : singularProperty.Value.ToString()!, _subType);
                }
                else if (!element.HasValue)
                {
                    // value = null;
                    return true;
                }
                else
                {
                    array = new TElementType[1];
                    array[0] = element.Value;
                    value = new EquatableArray<TElementType>(array!);
                    return true;
                }
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

                break;

            // wrong type of value
            case IDictionarySourceNode dictionaryNode:
                if (!modern)
                    args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref args, dictionaryNode, this);
                else
                    args.DiagnosticSink?.UNT2004_DictionaryInsteadOfList(ref args, dictionaryNode, this);

                break;

            case IListSourceNode listNode:
                if ((_args.Mode & ListMode.ModernList) != ListMode.ModernList)
                {
                    args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, listNode, this);
                    break;
                }

                CheckCount(listNode.Count, ref args);
                if (listNode.Count == 0)
                {
                    value = EquatableArray<TElementType>.Empty;
                    return true;
                }

                array = new TElementType?[listNode.Count];
                int index = 0;
                ImmutableArray<ISourceNode> values = listNode.Children;
                for (int i = 0; i < values.Length; ++i)
                {
                    ISourceNode node = values[i];
                    if (node is not IAnyValueSourceNode v)
                        continue;

                    args.CreateSubTypeParserArgs(out TypeParserArgs<TElementType> elementParseArgs, v, listNode, _subType, LegacyExpansionFilter.Modern);

                    if (!_subType.Parser.TryParse(ref elementParseArgs, in ctx, out Optional<TElementType> elementType) || !elementType.HasValue)
                    {
                        if (!elementParseArgs.ShouldIgnoreFailureDiagnostic)
                        {
                            args.DiagnosticSink?.UNT2004_Generic(ref args, v.ToString()!, _subType);
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
                bool couldBeModernSingle = modern && (_args.Mode & ListMode.ModernSingle) == ListMode.ModernSingle && args.KeyFilter != LegacyExpansionFilter.Legacy;
                bool couldBeLegacyCount = legacy && (_args.Mode & ListMode.LegacyList) == ListMode.LegacyList && args.KeyFilter != LegacyExpansionFilter.Modern;
                if (!couldBeLegacyCount && !couldBeModernSingle)
                {
                    args.DiagnosticSink?.UNT2004_ValueInsteadOfList(ref args, valueNode, this);
                    break;
                }

                if (couldBeLegacyCount && _countConverter != null && _countType != null)
                {
                    args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<TCountType> parseArgs, _countType, valueNode.Value);
                    bool success = _countConverter.TryParse(valueNode.Value.AsSpan(), ref parseArgs, out TCountType? countValue);
                    int count = 0;
                    if (success)
                    {
                        success = _countConverter.TryConvertTo(new Optional<TCountType>(countValue!), out Optional<int> newCount) && newCount.HasValue;
                        count = newCount.Value;
                    }

                    if (success)
                    {
                        CheckCount(count, ref args);

                        if (args.ParentNode is not IPropertySourceNode { Parent: IDictionarySourceNode dictionary })
                        {
                            if (!couldBeModernSingle)
                                args.DiagnosticSink?.UNT2004_Generic(ref args, valueNode.Value, _countType);
                        }
                        else
                        {
                            string? singularPropertyName = _args.LegacySingularKey;

                            if (string.IsNullOrEmpty(singularPropertyName))
                            {
                                if (args.ParentNode is IPropertySourceNode property)
                                {
                                    singularPropertyName = property.Key;
                                }
                                else
                                {
                                    singularPropertyName = ctx.Self.Key;
                                }
                            }

                            array = new TElementType?[count];
                            allFailed = true;
                            for (int i = 0; i < count; ++i)
                            {
                                string newKey = CreateLegacyKey(singularPropertyName, i);
                                bool needsDefault = false;
                                if (!dictionary.TryGetProperty(newKey, out IPropertySourceNode? property))
                                {
                                    args.DiagnosticSink?.UNT1007(ref args, valueNode, newKey);
                                    needsDefault = true;
                                }
                                else
                                {
                                    args.ReferencedPropertySink?.AcceptReferencedProperty(property);
                                    args.CreateSubTypeParserArgs(out TypeParserArgs<TElementType> elementParseArgs, property.Value, property, _subType, LegacyExpansionFilter.Modern);

                                    if (!_subType.Parser.TryParse(ref elementParseArgs, in ctx, out Optional<TElementType> elementType) || !elementType.HasValue)
                                    {
                                        if (!elementParseArgs.ShouldIgnoreFailureDiagnostic)
                                        {
                                            args.DiagnosticSink?.UNT2004_Generic(ref args, property.Value == null ? "-" : property.Value.ToString()!, _subType);
                                        }

                                        needsDefault = true;
                                    }
                                    else
                                    {
                                        array[i] = elementType.Value;
                                        allFailed = false;
                                    }
                                }

                                if (needsDefault
                                    && _args.LegacyDefaultElementTypeValue != null
                                    && _args.LegacyDefaultElementTypeValue.TryEvaluateValue(out Optional<TElementType> elementValue, in ctx))
                                {
                                    array[i] = elementValue.Value;
                                }
                            }

                            value = new EquatableArray<TElementType>(array!);
                            return !allFailed;
                        }
                    }
                    else if (!couldBeModernSingle && !parseArgs.ShouldIgnoreFailureDiagnostic)
                    {
                        args.DiagnosticSink?.UNT2004_Generic(ref args, valueNode.Value, _countType);
                    }
                }

                if (couldBeModernSingle)
                {
                    args.CreateSubTypeParserArgs(out TypeParserArgs<TElementType> parseArgs, args.ValueNode, args.ParentNode, _subType, LegacyExpansionFilter.Modern);

                    if (!_subType.Parser.TryParse(ref parseArgs, in ctx, out Optional<TElementType> element))
                    {
                        if (!parseArgs.ShouldIgnoreFailureDiagnostic)
                            args.DiagnosticSink?.UNT2004_Generic(ref args, valueNode.Value, _subType);
                    }
                    else if (!element.HasValue)
                    {
                        // value = null;
                        return true;
                    }
                    else
                    {
                        array = new TElementType[1];
                        array[0] = element.Value;
                        value = new EquatableArray<TElementType>(array!);
                        return true;
                    }
                }

                break;
        }

        return false;
    }

    private string CreateLegacyKey(string baseKey, int i)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        int l = baseKey.Length + StringHelper.CountDigits(i);
        CreateLegacyKeyState state;
        state.BaseKey = baseKey;
        state.Index = i;
        if (_args.SkipUnderscoreInLegacyKey)
        {
            return string.Create(l, state, static (span, state) =>
            {
                state.BaseKey.AsSpan().CopyTo(span);
                state.Index.TryFormat(span.Slice(state.BaseKey.Length), out _, provider: CultureInfo.InvariantCulture);
            });
        }

        ++l;
        return string.Create(l, state, static (span, state) =>
        {
            state.BaseKey.AsSpan().CopyTo(span);
            span[state.BaseKey.Length] = '_';
            state.Index.TryFormat(span.Slice(state.BaseKey.Length + 1), out _, provider: CultureInfo.InvariantCulture);
        });
#else
        if (_args.SkipUnderscoreInLegacyKey)
        {
            return baseKey + i.ToString(CultureInfo.InvariantCulture);
        }

        return baseKey + "_" + i.ToString(CultureInfo.InvariantCulture);
#endif
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    private struct CreateLegacyKeyState
    {
        public string BaseKey;
        public int Index;
    }
#endif

    public override int GetHashCode() => HashCode.Combine(1745437037, _args.GetHashCode());
}

/// <summary>
/// Parameters for <see cref="ListType{TCountType,TElementType}"/> types.
/// </summary>
/// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
/// <typeparam name="TCountType">The data-type of the count of the list.</typeparam>
public readonly struct ListTypeArgs<TCountType, TElementType>
    where TElementType : IEquatable<TElementType>
    where TCountType : IEquatable<TCountType>
{
    /// <summary>
    /// The type of lists to parse.
    /// </summary>
    public required ListMode Mode { get; init; }

    /// <summary>
    /// Minimum number of elements in the list (inclusive).
    /// </summary>
    public Optional<TCountType> MinimumCount { get; init; }

    /// <summary>
    /// Maximum number of elements in the list (inclusive).
    /// </summary>
    public Optional<TCountType> MaximumCount { get; init; }

    /// <summary>
    /// The key used for legacy list elements. Ex 'Condition'_0 for 'Conditions'.
    /// </summary>
    public string? LegacySingularKey { get; init; }

    /// <summary>
    /// The key used for the legacy single list. Ex 'Blade_ID' 4356 for 'Blade_IDs'. Defaults to <see cref="LegacySingularKey"/>.
    /// </summary>
    public string? LegacySingleKey { get; init; }

    /// <summary>
    /// If <see langword="true"/>, skips the '_' separator when creating legacy element keys.
    /// </summary>
    public bool SkipUnderscoreInLegacyKey { get; init; }

    /// <summary>
    /// The default value for missing legacy list elements.
    /// </summary>
    public IValue<TElementType>? LegacyDefaultElementTypeValue { get; init; }

    public bool Equals(in ListTypeArgs<TCountType, TElementType> other)
    {
        return other.Mode == Mode
               && string.Equals(other.LegacySingularKey, LegacySingularKey, StringComparison.OrdinalIgnoreCase)
               && string.Equals(other.LegacySingleKey, LegacySingleKey, StringComparison.OrdinalIgnoreCase)
               && MinimumCount.Equals(other.MinimumCount)
               && MaximumCount.Equals(other.MaximumCount)
               && SkipUnderscoreInLegacyKey == other.SkipUnderscoreInLegacyKey
               && Equals(LegacyDefaultElementTypeValue, other.LegacyDefaultElementTypeValue)
               ;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mode, LegacySingularKey, LegacySingleKey, MinimumCount, MaximumCount, SkipUnderscoreInLegacyKey, LegacyDefaultElementTypeValue);
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