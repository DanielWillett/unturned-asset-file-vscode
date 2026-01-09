using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Factory class for the <see cref="CommaDelimitedStringType{TElementType}"/> type.
/// </summary>
public sealed class CommaDelimitedStringType : ITypeFactory
{
    public const string TypeId = "CommaDelimitedString";

    /// <summary>
    /// Factory used to create <see cref="CommaDelimitedStringType{TElementType}"/> values from JSON.
    /// </summary>
    public static ITypeFactory Factory { get; } = new CommaDelimitedStringType();

    private CommaDelimitedStringType() { }
    static CommaDelimitedStringType() { }

    /// <summary>
    /// Create a new comma-delimted list type.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <typeparam name="TCountType">The data-type of the count of the list.</typeparam>
    /// <param name="args">Parameters for how the list should be parsed.</param>
    /// <param name="subType">The element type of the list.</param>
    public static CommaDelimitedStringType<TElementType> Create<TElementType>(
        int? minCount, int? maxCount,
        IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new CommaDelimitedStringType<TElementType>(minCount, maxCount, subType);
    }

    /// <summary>
    /// Create a new comma-delimted list type that parses a modern list syntax with no special rules.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <param name="subType">The element type of the list.</param>
    public static CommaDelimitedStringType<TElementType> Create<TElementType>(IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new CommaDelimitedStringType<TElementType>(null, null, subType);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context)
    {
        ElementTypeVisitor v;
        v.Result = null;
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
                    "CommaDelimitedString",
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
        public IDatSpecificationObject Owner;
        public string Context;

        public void Accept<TElementType>(IType<TElementType> type) where TElementType : IEquatable<TElementType>
        {
            Optional<int> minValue = Optional<int>.Null, maxValue = Optional<int>.Null;
            if (Json.TryGetProperty("MinimumCount"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
            {
                TypeConverterParseArgs<int> parseArgs = new TypeConverterParseArgs<int>(Int32Type.Instance);
                if (!TypeConverters.Get<int>().TryReadJson(in Json, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            type.Id,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MinimumCount" : $"{Owner.FullName}.MinimumCount"
                        )
                    );
            }

            if (Json.TryGetProperty("MaximumCount"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                TypeConverterParseArgs<int> parseArgs = new TypeConverterParseArgs<int>(Int32Type.Instance);
                if (!TypeConverters.Get<int>().TryReadJson(in Json, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            type.Id,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MaximumCount" : $"{Owner.FullName}.MaximumCount"
                        )
                    );
            }

            Result = new CommaDelimitedStringType<TElementType>(minValue.AsNullable(), maxValue.AsNullable(), type);
        }
    }
}

/// <summary>
/// A container type which allows multiple <see cref="TElementType"/> sub-elements separated by commas in a string.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="IType{TElementType}"/> ElementType</c> - The type of elements in the list - required.</item>
///     <item><c><see cref="int"/> MinimumCount</c> - Minimum number of items (inclusive).</item>
///     <item><c><see cref="int"/> MaximumCount</c> - Maximum number of items (inclusive).</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>Use the factory methods in <see cref="CommaDelimitedStringType"/> to create a list type.</remarks>
/// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
public class CommaDelimitedStringType<TElementType>
    : BaseType<EquatableArray<TElementType>, CommaDelimitedStringType<TElementType>>, ITypeParser<EquatableArray<TElementType>>, IReferencingType
    where TElementType : IEquatable<TElementType>
{
    private readonly IType<TElementType> _subType;
    private readonly int? _minCount;
    private readonly int? _maxCount;
    private readonly StringSplitOptions _splitOptions;

    public override string Id => CommaDelimitedStringType.TypeId;

    public override string DisplayName { get; }

    public override ITypeParser<EquatableArray<TElementType>> Parser => this;

    /// <inheritdoc />
    public OneOrMore<IType> ReferencedTypes => new OneOrMore<IType>(_subType);

    /// <summary>
    /// Use the factory methods in <see cref="CommaDelimitedStringType"/> to create a list type.
    /// </summary>
    internal CommaDelimitedStringType(int? minCount, int? maxCount, StringSplitOptions splitOptions, IType<TElementType> subType)
    {
        _subType = subType;
        DisplayName = string.Format(Resources.Type_Name_CommaDelimitedString_Generic, subType.DisplayName);

        _splitOptions = splitOptions;

        _minCount = minCount;
        if (_minCount is <= 0)
            _minCount = null;

        _maxCount = maxCount;
        if (_maxCount is < 0)
            _maxCount = null;
    }

    #region JSON

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        WriteTypeName(writer);
        writer.WritePropertyName("ElementType"u8);
        _subType.WriteToJson(writer, options);

        if (_minCount is > 0)
            writer.WriteNumber("MinimumCount"u8, _minCount.Value);

        if (_maxCount is > 0)
            writer.WriteNumber("MaximumCount"u8, _maxCount.Value);
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

    public void WriteValueToJson(Utf8JsonWriter writer, EquatableArray<TElementType> value, IType<EquatableArray<TElementType>> valueType, JsonSerializerOptions options)
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
                _subType.Parser.WriteValueToJson(writer, element, _subType, options);
            }
        }

        writer.WriteEndArray();
    }

    #endregion

    protected override bool Equals(CommaDelimitedStringType<TElementType> other)
    {
        return other._subType.Equals(_subType) && other._minCount == _minCount && other._maxCount == _maxCount;
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
        bool legacy = (_args.Mode & CommaDelimitedStringMode.Legacy) != 0 && args.KeyFilter != LegacyExpansionFilter.Modern;
        bool modern = (_args.Mode & CommaDelimitedStringMode.Modern) != 0 && args.KeyFilter != LegacyExpansionFilter.Legacy;
        if (!modern && !legacy) modern = true;

        TElementType?[] array;

        if (legacy && (_args.Mode & CommaDelimitedStringMode.LegacySingle) == CommaDelimitedStringMode.LegacySingle)
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
                args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                break;

            // wrong type of value
            case IDictionarySourceNode dictionaryNode:
                args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref args, dictionaryNode, this);
                break;

            case IListSourceNode listNode:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, listNode, this);
                break;

            case IValueSourceNode valueNode:
                string val = valueNode.Value;
                string[] values = val.Split(',', StringSplitOptions.None)
                break;
        }

        return false;
    }

    private struct DefaultValueVisitor : IValueVisitor
    {
        public TElementType?[] Array;
        public int Index;

        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!value.HasValue)
                return;

            if (typeof(TValue) == typeof(TElementType))
            {
                Array[Index] = Unsafe.As<TValue, TElementType>(ref Unsafe.AsRef(in value.Value));
                return;
            }

            ConvertVisitor<TElementType> converter;
            converter.IsNull = false;
            converter.WasSuccessful = false;
            converter.Result = default;
            converter.Accept(value.Value);

            if (!converter.WasSuccessful)
                return;

            Array[Index] = converter.Result;
        }
    }

    public override int GetHashCode() => HashCode.Combine(611860609, _subType, _minCount, _maxCount);
}