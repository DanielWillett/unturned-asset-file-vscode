using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Factory class for the <see cref="CommaDelimitedStringType{TElementType}"/> type.
/// </summary>
public sealed class CommaDelimitedStringType : ITypeFactory
{
    public const string TypeId = "CommaDelimitedString";

    internal const StringSplitOptions DefaultSplitOptions = StringSplitOptions.RemoveEmptyEntries
#if NET5_0_OR_GREATER
                                                            | StringSplitOptions.TrimEntries;
#else
                                                            | (StringSplitOptions)FallbackStringSplitOptions.TrimEntries;
#endif
#if !NET5_0_OR_GREATER
    internal enum FallbackStringSplitOptions
    {
        // netstandard/framework does not have TrimEntries so we have to add a polyfill for it.
        // It was added in .NET 5.
        None = 0,
        RemoveEmptyEntries = 1,
        TrimEntries = 2
    }
#endif

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
    /// <param name="minCount">Minimum number of items that must be in the list, inclusive.</param>
    /// <param name="maxCount">Maximum number of items that must be in the list, inclusive.</param>
    /// <param name="subType">The element type of the list.</param>
    public static CommaDelimitedStringType<TElementType> Create<TElementType>(
        int? minCount, int? maxCount,
        IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new CommaDelimitedStringType<TElementType>(minCount, maxCount, DefaultSplitOptions, subType);
    }

    /// <summary>
    /// Create a new comma-delimted list type.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <param name="minCount">Minimum number of items that must be in the list, inclusive.</param>
    /// <param name="maxCount">Maximum number of items that must be in the list, inclusive.</param>
    /// <param name="splitOptions">Options used for splitting the text using <see cref="string.Split(string, StringSplitOptions)"/>.</param>
    /// <param name="subType">The element type of the list.</param>
    public static CommaDelimitedStringType<TElementType> Create<TElementType>(
        int? minCount, int? maxCount, StringSplitOptions splitOptions,
        IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new CommaDelimitedStringType<TElementType>(minCount, maxCount, splitOptions, subType);
    }

    /// <summary>
    /// Create a new comma-delimted list type that parses a modern list syntax with no special rules.
    /// </summary>
    /// <typeparam name="TElementType">The type of values to read from the elements of the list.</typeparam>
    /// <param name="subType">The element type of the list.</param>
    public static CommaDelimitedStringType<TElementType> Create<TElementType>(IType<TElementType> subType)
        where TElementType : IEquatable<TElementType>
    {
        return new CommaDelimitedStringType<TElementType>(null, null, DefaultSplitOptions, subType);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec,
        DatProperty owner, string context)
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
                if (!TypeConverters.Get<int>().TryReadJson(in element, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            Int32Type.TypeId,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MinimumCount" : $"{Owner.FullName}.MinimumCount"
                        )
                    );
            }

            if (Json.TryGetProperty("MaximumCount"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                TypeConverterParseArgs<int> parseArgs = new TypeConverterParseArgs<int>(Int32Type.Instance);
                if (!TypeConverters.Get<int>().TryReadJson(in element, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            Int32Type.TypeId,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MaximumCount" : $"{Owner.FullName}.MaximumCount"
                        )
                    );
            }

            StringSplitOptions splitOptions = DefaultSplitOptions;

            if (Json.TryGetProperty("SplitOptions"u8, out element) && element.ValueKind != JsonValueKind.Null)
            {
                if (!Enum.TryParse(element.GetString(), out
#if NET5_0_OR_GREATER
                            splitOptions
#else
                            FallbackStringSplitOptions options
#endif
                    ))
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

#if !NET5_0_OR_GREATER
                splitOptions = (StringSplitOptions)options;
#endif
            }

            Result = new CommaDelimitedStringType<TElementType>(minValue.AsNullable(), maxValue.AsNullable(), splitOptions, type);
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
        writer.WriteStartObject();

        WriteTypeName(writer);
        writer.WritePropertyName("ElementType"u8);
        _subType.WriteToJson(writer, options);

        if (_minCount is > 0)
            writer.WriteNumber("MinimumCount"u8, _minCount.Value);

        if (_maxCount is > 0)
            writer.WriteNumber("MaximumCount"u8, _maxCount.Value);

        if (_splitOptions != CommaDelimitedStringType.DefaultSplitOptions)
        {
#if NET5_0_OR_GREATER
            writer.WriteString("SplitOptions"u8, _splitOptions.ToString());
#else
            writer.WriteString("SplitOptions"u8, ((CommaDelimitedStringType.FallbackStringSplitOptions)_splitOptions).ToString());
#endif
        }

        writer.WriteEndObject();
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
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.TryParseStringValueOnly(ref args, out IValueSourceNode? valueNode))
        {
            return false;
        }

        value = Optional<EquatableArray<TElementType>>.Null;
        bool trimEntries = (_splitOptions & (StringSplitOptions)2) != 0;


        bool allPassed = true;
        ReadOnlySpan<char> valueSpan = valueNode.Value.AsSpan();

        int maxElementCount = valueSpan.Count(',') + 1;
        Span<Range> ranges = stackalloc Range[maxElementCount];

        int segments = valueSpan.Split(ranges, ',', trimEntries, trimEntries, _splitOptions);
        CheckCount(segments, ref args);

        TElementType?[] array = new TElementType[segments];

        ITypeParser<TElementType> parser = _subType.Parser;

        FileRange valueNodeRange = valueNode.Range;
        if (parser is TypeConverterParser<TElementType> { CanUseTypeConverterDirectly: true } typeConverterParser)
        {
            ITypeConverter<TElementType> converter = typeConverterParser.TypeConverter;
            args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<TElementType> parseArgs, _subType);
            for (int i = 0; i < segments; ++i)
            {
                Range range = ranges[i];
                ReadOnlySpan<char> span = valueSpan[range];

                (int offset, int length) = range.GetOffsetAndLength(valueSpan.Length);

                parseArgs.ValueRange.Start.Character = valueNodeRange.Start.Character + offset;
                parseArgs.ValueRange.End.Character = valueNodeRange.Start.Character + offset + length - 1;
                parseArgs.TextAsString = segments == 1 && length == valueSpan.Length ? valueNode.Value : null;

                if (converter.TryParse(span, ref parseArgs, out array[i]))
                    continue;

                allPassed = false;
                if (parseArgs.DiagnosticSink == null)
                    break;
                if (parseArgs.ShouldIgnoreFailureDiagnostic)
                    continue;

                parseArgs.DiagnosticSink?.UNT2004_Generic(ref parseArgs, parseArgs.GetString(span), _subType);
                args.ShouldIgnoreFailureDiagnostic = true;
            }
        }
        else
        {
            AnySourceNodeProperties props = default;
            props.Range = valueNodeRange;
            props.Index = valueNode.Index;
            props.ChildIndex = valueNode.ChildIndex;
            props.Depth = valueNode.Depth;
            int valueNodeFirstCharacterIndex = valueNode.FirstCharacterIndex;
            int valueNodeLastCharacterIndex = valueNode.LastCharacterIndex;
            for (int i = 0; i < segments; ++i)
            {
                Range range = ranges[i];
                ReadOnlySpan<char> span = valueSpan[range];

                (int offset, int length) = range.GetOffsetAndLength(valueSpan.Length);

                props.Range.Start.Character = valueNodeRange.Start.Character + offset;
                props.Range.End.Character = valueNodeRange.Start.Character + offset + length - 1;

                props.FirstCharacterIndex = valueNodeFirstCharacterIndex + offset;
                props.LastCharacterIndex = valueNodeLastCharacterIndex + offset + length - 1;
                ValueNode fakeNode = ValueNode.Create(span.ToString(), false, Comment.None, in props);

                args.CreateSubTypeParserArgs(out TypeParserArgs<TElementType> parseArgs, fakeNode, args.ParentNode, _subType, PropertyResolutionContext.Unknown /* todo: ctx.PropertyContext */);

                if (parser.TryParse(ref parseArgs, in ctx, out Optional<TElementType> optionalValue)
                    && optionalValue.TryGetValueOrNull(out array[i]))
                {
                    continue;
                }

                allPassed = false;
                args.ShouldIgnoreFailureDiagnostic |= parseArgs.ShouldIgnoreFailureDiagnostic;
            }
        }

        return allPassed;
    }

    public override int GetHashCode() => HashCode.Combine(611860609, _subType, _minCount, _maxCount);
}