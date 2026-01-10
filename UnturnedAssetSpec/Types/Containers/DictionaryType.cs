using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Factory class for the <see cref="DictionaryType{TKeyType,TValueType}"/> type.
/// </summary>
public sealed class DictionaryType : ITypeFactory
{
    public const string TypeId = "Dictionary";

    /// <summary>
    /// Factory used to create <see cref="DictionaryType{TKeyType,TValueType}"/> values from JSON.
    /// </summary>
    public static ITypeFactory Factory { get; } = new DictionaryType();

    private DictionaryType() { }
    static DictionaryType() { }

    /// <summary>
    /// Create a new dictionary type.
    /// </summary>
    /// <typeparam name="TValueType">The type of values to read from the dictionary.</typeparam>
    /// <param name="args">Parameters for how the list should be parsed.</param>
    /// <param name="valueType">The value type of the dictionary.</param>
    public static DictionaryType<string, TValueType> Create<TValueType>(
        DictionaryTypeArgs<TValueType> args,
        IType<TValueType> valueType)
        where TValueType : IEquatable<TValueType>
    {
        return new DictionaryType<string, TValueType>(args, StringType.Instance, valueType);
    }

    /// <summary>
    /// Create a new dictionary type.
    /// </summary>
    /// <typeparam name="TKeyType">The type of keys to read from the dictionary.</typeparam>
    /// <typeparam name="TValueType">The type of values to read from the dictionary.</typeparam>
    /// <param name="args">Parameters for how the list should be parsed.</param>
    /// <param name="keyType">The key type of the dictionary.</param>
    /// <param name="valueType">The value type of the dictionary.</param>
    public static DictionaryType<TKeyType, TValueType> Create<TKeyType, TValueType>(
        DictionaryTypeArgs<TValueType> args,
        IType<TKeyType> keyType,
        IType<TValueType> valueType)
        where TKeyType : IEquatable<TKeyType>
        where TValueType : IEquatable<TValueType>
    {
        return new DictionaryType<TKeyType, TValueType>(args, keyType, valueType);
    }

    IType ITypeFactory.CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec, IDatSpecificationObject owner, string context)
    {
        ValueTypeVisitor v;
        v.Result = null;
        v.Spec = spec;
        v.Owner = owner;
        v.Context = context;
        v.Json = typeDefinition;
        if (typeDefinition.TryGetProperty("ValueType"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
        {
            IType elementType = spec.ReadType(in element, owner, context);
            elementType.Visit(ref v);
        }
        else
        {
            throw new JsonException(
                string.Format(
                    Resources.JsonException_RequiredTypePropertyMissing,
                    "ValueType",
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

    private struct ValueTypeVisitor : ITypeVisitor
    {
        public IType? Result;
        public JsonElement Json;
        public IDatSpecificationReadContext Spec;
        public IDatSpecificationObject Owner;
        public string Context;

        public void Accept<TValueType>(IType<TValueType> type) where TValueType : IEquatable<TValueType>
        {
            KeyTypeVisitor<TValueType> v;
            v.Result = null;
            v.Spec = Spec;
            v.Owner = Owner;
            v.Context = Context;
            v.Json = Json;
            v.SubType = type;
            if (Json.TryGetProperty("KeyType"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
            {
                IType countType = Spec.ReadType(in element, Owner, Context);
                countType.Visit(ref v);
            }
            else
            {
                v.Accept(StringType.Instance);
            }

            Result = v.Result;
        }
    }
    private struct KeyTypeVisitor<TValueType> : ITypeVisitor where TValueType : IEquatable<TValueType>
    {
        public IType? Result;
        public JsonElement Json;
        public IDatSpecificationReadContext Spec;
        public IDatSpecificationObject Owner;
        public IType<TValueType> SubType;
        public string Context;

        public void Accept<TKeyType>(IType<TKeyType> type) where TKeyType : IEquatable<TKeyType>
        {
            Optional<int> minValue = Optional<int>.Null, maxValue = Optional<int>.Null;
            if (Json.TryGetProperty("MinimumCount"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
            {
                TypeConverterParseArgs<int> parseArgs = new TypeConverterParseArgs<int>(Int32Type.Instance);
                if (!TypeConverters.Int32.TryReadJson(in Json, out minValue, ref parseArgs))
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
                if (!TypeConverters.Int32.TryReadJson(in Json, out minValue, ref parseArgs))
                    throw new JsonException(string.Format(
                            Resources.JsonException_FailedToParseValue,
                            Int32Type.TypeId,
                            Context.Length != 0 ? $"{Owner.FullName}.{Context}.MaximumCount" : $"{Owner.FullName}.MaximumCount"
                        )
                    );
            }

            IValue? defaultValue = null;
            if (Json.TryGetProperty("DefaultValue"u8, out element))
            {
                defaultValue = Spec.ReadValue(in element, type, Owner, Context.Length == 0 ? "DefaultValue" : $"{Context}.DefaultValue");
            }

            bool requireKeyType = Json.TryGetProperty("RequireKeyType"u8, out element)
                               && element.ValueKind != JsonValueKind.Null
                               && element.GetBoolean();

            Result = new DictionaryType<TKeyType, TValueType>(new DictionaryTypeArgs<TValueType>
            {
                MinimumCount = minValue.AsNullable(),
                MaximumCount = maxValue.AsNullable(),
                DefaultValue = defaultValue,
                RequireKeyType = requireKeyType
            }, type, SubType);
        }
    }
}

/// <summary>
/// A container type which allows an arbitrary modern dictionary of keys to <see cref="TValueType"/> values.
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="IType{TValue}"/> ElementType</c> - The type of values in the dictionary - required.</item>
///     <item><c><see cref="IType{TValue}"/> KeyType</c> - Type used to parse keys. Defaults to <see cref="StringType"/>.</item>
///     <item><c><see cref="int"/> MinimumCount</c> - Minimum number of keys (inclusive).</item>
///     <item><c><see cref="int"/> MaximumCount</c> - Maximum number of keys (inclusive).</item>
///     <item><c><see cref="IValue{TValue}"/> DefaultValue</c> - Default value for a key with no value.</item>
///     <item><c><see cref="bool"/> RequireKeyType</c> - Whether or not it's an error to have a key that isn't a valid value of <c>KeyType</c>. Defaults to <see langword="false"/>.</item>
/// </list>
/// </para>
/// </summary>
/// <remarks>Use the factory methods in <see cref="DictionaryType"/> to create a list type.</remarks>
/// <typeparam name="TKeyType">The type of value to read from the keys of the dictionary. Defaults to <see cref="string"/>.</typeparam>
/// <typeparam name="TValueType">The type of value to read from the values of the dictionary.</typeparam>
public class DictionaryType<TKeyType, TValueType>
    : BaseType<EquatableArray<DictionaryPair<TValueType>>, DictionaryType<TKeyType, TValueType>>, ITypeParser<EquatableArray<DictionaryPair<TValueType>>>, IReferencingType
    where TKeyType : IEquatable<TKeyType>
    where TValueType : IEquatable<TValueType>
{
    private readonly DictionaryTypeArgs<TValueType> _args;
    private readonly IType<TKeyType> _keyType;
    private readonly IType<TValueType> _valueType;
    private readonly int? _minCount;
    private readonly int? _maxCount;

    public override string Id => DictionaryType.TypeId;

    public override string DisplayName { get; }

    public override ITypeParser<EquatableArray<DictionaryPair<TValueType>>> Parser => this;

    /// <inheritdoc />
    public OneOrMore<IType> ReferencedTypes
    {
        get
        {
            if (field.IsNull)
            {
                field = new OneOrMore<IType>([ _keyType, _valueType ]);
            }

            return field;
        }
    }

    /// <summary>
    /// Use the factory methods in <see cref="DictionaryType"/> to create a list type.
    /// </summary>
    internal DictionaryType(DictionaryTypeArgs<TValueType> args, IType<TKeyType> keyType, IType<TValueType> valueType)
    {
        _args = args;
        _valueType = valueType;
        _keyType = keyType;
        DisplayName = _keyType.Equals(StringType.Instance)
                          ? string.Format(Resources.Type_Name_Dictionary_Generic_String, valueType.DisplayName)
                          : string.Format(Resources.Type_Name_Dictionary_Generic, keyType.DisplayName, valueType.DisplayName);

        _minCount = args.MinimumCount;
        if (_minCount is <= 0)
            _minCount = null;

        _maxCount = args.MaximumCount;
        if (_maxCount is < 0)
            _maxCount = null;
    }

    #region JSON

    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        WriteTypeName(writer);
        writer.WritePropertyName("ElementType"u8);
        _valueType.WriteToJson(writer, options);

        if (_minCount is > 0)
            writer.WriteNumber("MinimumCount"u8, _minCount.Value);

        if (_maxCount is > 0)
            writer.WriteNumber("MaximumCount"u8, _maxCount.Value);

        if (_args.DefaultValue != null)
        {
            writer.WritePropertyName("DefaultValue"u8);
            _args.DefaultValue.WriteToJson(writer, options);
        }

        if (!_keyType.Equals(StringType.Instance))
        {
            writer.WritePropertyName("KeyType"u8);
            _keyType.WriteToJson(writer, options);
        }

        if (_args.RequireKeyType)
            writer.WriteBoolean("RequireKeyType"u8, true);
        
        writer.WriteEndObject();
    }

    public bool TryReadValueFromJson(in JsonElement json, out Optional<EquatableArray<DictionaryPair<TValueType>>> value, IType<EquatableArray<DictionaryPair<TValueType>>> valueType)
    {
        value = Optional<EquatableArray<DictionaryPair<TValueType>>>.Null;
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = EquatableArray<DictionaryPair<TValueType>>.Empty;
                return true;

            case JsonValueKind.Array:
                Optional<TValueType> o;
                int len = json.GetArrayLength();
                DictionaryPair<TValueType>[] arr = new DictionaryPair<TValueType>[len];
                for (int i = 0; i < len; ++i)
                {
                    JsonElement element = json[i];
                    if (element.ValueKind != JsonValueKind.Object
                        || !element.TryGetProperty("Key"u8, out JsonElement keyElement)
                        || keyElement.ValueKind != JsonValueKind.String
                        || !element.TryGetProperty("Value"u8, out JsonElement valueElement)
                        || !_valueType.Parser.TryReadValueFromJson(in valueElement, out o, _valueType)
                        || !o.TryGetValueOrNull(out TValueType? parsedValue))
                    {
                        return false;
                    }

                    arr[i] = new DictionaryPair<TValueType>(keyElement.GetString(), parsedValue);
                }

                value = new EquatableArray<DictionaryPair<TValueType>>(arr);
                return true;

            case JsonValueKind.Object:
                JsonElement.ObjectEnumerator objectEnumerator = json.EnumerateObject();
                List<DictionaryPair<TValueType>> list = new List<DictionaryPair<TValueType>>();
                foreach (JsonProperty property in objectEnumerator)
                {
                    JsonElement v = property.Value;
                    if (!_valueType.Parser.TryReadValueFromJson(in v, out o, _valueType)
                     || !o.TryGetValueOrNull(out TValueType? parsedValue))
                    {
                        return false;
                    }

                    list.Add(new DictionaryPair<TValueType>(property.Name, parsedValue));
                }

                value = new EquatableArray<DictionaryPair<TValueType>>(list);
                return true;

            default:
                return false;
        }
    }

    public void WriteValueToJson(Utf8JsonWriter writer, EquatableArray<DictionaryPair<TValueType>> value, IType<EquatableArray<DictionaryPair<TValueType>>> valueType, JsonSerializerOptions options)
    {
        if (value.Array == null || value.Array.Length == 0)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (DictionaryPair<TValueType> kvp in value.Array)
        {
            if (kvp.Key == null)
                continue;

            writer.WriteStartObject();

            writer.WriteString("Key"u8, kvp.Key);

            writer.WritePropertyName("Value"u8);
            TValueType? v = kvp.Value;
            if (v == null)
            {
                writer.WriteNullValue();
            }
            else
            {
                _valueType.Parser.WriteValueToJson(writer, v, _valueType, options);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    #endregion

    protected override bool Equals(DictionaryType<TKeyType, TValueType> other)
    {
        return _valueType.Equals(other._valueType) && _keyType.Equals(other._keyType) && _args.Equals(in other._args);
    }

    private void CheckCount(int ct, ref TypeParserArgs<EquatableArray<DictionaryPair<TValueType>>> args)
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

    public bool TryParse(ref TypeParserArgs<EquatableArray<DictionaryPair<TValueType>>> args, in FileEvaluationContext ctx, out Optional<EquatableArray<DictionaryPair<TValueType>>> value)
    {
        value = Optional<EquatableArray<DictionaryPair<TValueType>>>.Null;
        
        switch (args.ValueNode)
        {
            // null (no value)
            default:
                args.DiagnosticSink?.UNT2004_NoDictionary(ref args, args.ParentNode);
                break;

            case IDictionarySourceNode dictionaryNode:
                ImmutableArray<ISourceNode> nodes = dictionaryNode.Children;
                int count = dictionaryNode.Count;
                CheckCount(count, ref args);
                if (count == 0)
                {
                    value = EquatableArray<DictionaryPair<TValueType>>.Empty;
                    return true;
                }

                DictionaryPair<TValueType>[] array = new DictionaryPair<TValueType>[count];
                bool allPassed = true;
                int index = 0;
                foreach (ISourceNode node in nodes)
                {
                    if (node is not IPropertySourceNode property)
                        continue;

                    string key = property.Key;
                    IAnyValueSourceNode? valueNode = property.Value;

                    args.ReferencedPropertySink?.AcceptReferencedProperty(property);

                    if (args.DiagnosticSink != null
                        && _keyType != null
                        && _args.RequireKeyType
                        && !TryParseKey(property, ref args, in ctx, out _))
                    {
                        allPassed = false;
                    }

                    args.CreateSubTypeParserArgs(
                         out TypeParserArgs<TValueType> parseArgs,
                         valueNode,
                         property,
                         _valueType,
                         LegacyExpansionFilter.Modern
                    );

                    if (!_valueType.Parser.TryParse(ref parseArgs, in ctx, out Optional<TValueType> parsedValue)
                        || !parsedValue.TryGetValueOrNull(out TValueType? actualValue))
                    {
                        allPassed = false;
                        array[index] = new DictionaryPair<TValueType>(key, default);
                        ++index;
                        continue;
                    }

                    array[index] = new DictionaryPair<TValueType>(key, actualValue);
                    ++index;
                }

                value = new EquatableArray<DictionaryPair<TValueType>>(array, index);
                return allPassed;

            case IListSourceNode listNode:
                args.DiagnosticSink?.UNT2004_ListInsteadOfDictionary(ref args, listNode, this);
                break;

            case IValueSourceNode valueNode:
                args.DiagnosticSink?.UNT2004_ValueInsteadOfDictionary(ref args, valueNode, this);
                break;
        }

        return false;
    }

    private bool TryParseKey(
        IPropertySourceNode property,
        ref TypeParserArgs<EquatableArray<DictionaryPair<TValueType>>> args,
        in FileEvaluationContext ctx,
        out Optional<TKeyType> parsedKey
    )
    {
        string key = property.Key;
        if (_keyType.Equals(StringType.Instance))
        {
            parsedKey = Unsafe.As<string, TKeyType>(ref key);
            return true;
        }

        ITypeParser<TKeyType> parser = _keyType.Parser;
        if (parser is TypeConverterParser<TKeyType> { CanUseTypeConverterDirectly: true } typeConverterParser)
        {
            ITypeConverter<TKeyType> converter = typeConverterParser.TypeConverter;
            args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<TKeyType> parseArgs, _keyType);

            parseArgs.ValueRange = property.Range;
            parseArgs.TextAsString = key;

            if (converter.TryParse(key, ref parseArgs, out TKeyType? keyValue))
            {
                parsedKey = keyValue;
                return true;
            }

            parseArgs.DiagnosticSink?.UNT2004_Generic(ref parseArgs, key, _keyType);
            args.ShouldIgnoreFailureDiagnostic = true;
        }
        else
        {
            AnySourceNodeProperties props = default;
            props.Range = property.Range;
            props.Index = property.Index;
            props.ChildIndex = property.ChildIndex;
            props.Depth = property.Depth;
            props.FirstCharacterIndex = property.FirstCharacterIndex;
            props.LastCharacterIndex = property.LastCharacterIndex;
            ValueNode fakeNode = ValueNode.Create(key, property.KeyIsQuoted, Comment.None, in props);

            args.CreateSubTypeParserArgs(out TypeParserArgs<TKeyType> parseArgs, fakeNode, args.ParentNode, _keyType, ctx.PropertyContext switch
            {
                PropertyResolutionContext.Modern => LegacyExpansionFilter.Modern,
                _ => LegacyExpansionFilter.Legacy
            });

            if (parser.TryParse(ref parseArgs, in ctx, out parsedKey))
            {
                return true;
            }

            args.ShouldIgnoreFailureDiagnostic |= parseArgs.ShouldIgnoreFailureDiagnostic;
        }

        parsedKey = Optional<TKeyType>.Null;
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(762457717, _valueType, _keyType, _args);
    }
}

/// <summary>
/// Parameters for <see cref="DictionaryType{TKeyType,TValueType}"/> types.
/// </summary>
/// <typeparam name="TValueType">The type of values to read from the elements of the list.</typeparam>
public readonly struct DictionaryTypeArgs<TValueType>
    where TValueType : IEquatable<TValueType>
{
    /// <summary>
    /// Minimum number of elements in the list (inclusive).
    /// </summary>
    public int? MinimumCount { get; init; }

    /// <summary>
    /// Maximum number of elements in the list (inclusive).
    /// </summary>
    public int? MaximumCount { get; init; }

    /// <summary>
    /// The default value for missing legacy list elements.
    /// </summary>
    public IValue? DefaultValue { get; init; }

    /// <summary>
    /// Whether or not it's an error to have a key that isn't part of the key type.
    /// </summary>
    public bool RequireKeyType { get; init; }

    public bool Equals(in DictionaryTypeArgs<TValueType> other)
    {
        return MinimumCount.Equals(other.MinimumCount)
               && MaximumCount.Equals(other.MaximumCount)
               && RequireKeyType == other.RequireKeyType
               && (DefaultValue?.Equals(other.DefaultValue) ?? other.DefaultValue == null);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MinimumCount, MaximumCount, RequireKeyType, DefaultValue);
    }
}