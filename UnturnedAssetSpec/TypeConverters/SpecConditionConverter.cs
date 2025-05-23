using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecConditionConverter : JsonConverter<SpecCondition>
{
    private static readonly JsonEncodedText[] Operations =
    [
        JsonEncodedText.Encode("lt"),               // LessThan
        JsonEncodedText.Encode("gt"),               // GreaterThan
        JsonEncodedText.Encode("lte"),              // LessThanOrEqual
        JsonEncodedText.Encode("gte"),              // GreaterThanOrEqual
        JsonEncodedText.Encode("eq"),               // Equal
        JsonEncodedText.Encode("neq"),              // NotEqual
        JsonEncodedText.Encode("neq-i"),            // NotEqualCaseInsensitive
        JsonEncodedText.Encode("contains"),         // Containing
        JsonEncodedText.Encode("starts with"),      // StartingWith
        JsonEncodedText.Encode("ends with"),        // EndingWith
        JsonEncodedText.Encode("matches"),          // Matching
        JsonEncodedText.Encode("contains-i"),       // ContainingCaseInsensitive
        JsonEncodedText.Encode("eq-i"),             // EqualCaseInsensitive
        JsonEncodedText.Encode("starts with-i"),    // StartingWithCaseInsensitive
        JsonEncodedText.Encode("ends with-i"),      // EndingWithCaseInsensitive
        JsonEncodedText.Encode("assignable-to"),    // AssignableTo
        JsonEncodedText.Encode("assignable-from"),  // AssignableFrom
        JsonEncodedText.Encode("included"),         // Included
        JsonEncodedText.Encode("is-type")           // ReferenceIsOfType
    ];

    private static readonly JsonEncodedText VariableProperty = JsonEncodedText.Encode("Variable");
    private static readonly JsonEncodedText OperationProperty = JsonEncodedText.Encode("Operation");
    private static readonly JsonEncodedText ComparandProperty = JsonEncodedText.Encode("Comparand");

    /// <inheritdoc />
    public override SpecCondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadCondition(ref reader, options);
    }

    public static SpecCondition ReadCondition(ref Utf8JsonReader reader, JsonSerializerOptions? options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartObject:
                break;

            case JsonTokenType.True:
                // always true
                return new SpecCondition(SpecDynamicValue.True, ConditionOperation.Equal, true);

            case JsonTokenType.False:
            case JsonTokenType.Null:
                // always false
                return new SpecCondition(SpecDynamicValue.False, ConditionOperation.Equal, true);

            case JsonTokenType.String:
                // check if property by this name is true
                return new SpecCondition(SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty, KnownTypes.Boolean), ConditionOperation.Equal, true);

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} while reading SpecCondition.");
        }

        ISpecDynamicValue? variable = null;
        ConditionOperation? operation = null;
        bool hasComparand = false;
        object? comparand = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                reader.Skip();

            if (reader.ValueTextEquals(VariableProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read())
                    throw new JsonException("Failed to read 'Variable' in SpecCondition.");

                variable = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty);
            }
            else if (reader.ValueTextEquals(OperationProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read() || reader.TokenType != JsonTokenType.String)
                    throw new JsonException("Failed to read 'Operation' in SpecCondition.");

                for (int i = 0; i < Operations.Length; ++i)
                {
                    if (!reader.ValueTextEquals(Operations[i].EncodedUtf8Bytes))
                        continue;

                    operation = (ConditionOperation)i;
                    break;
                }

                if (!operation.HasValue)
                    throw new JsonException($"Unknown operation '{reader.GetString()}' while reading SpecCondition.");
            }
            else if (reader.ValueTextEquals(ComparandProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read() || !JsonHelper.TryReadGenericValue(ref reader, out object? obj))
                {
                    throw new JsonException("Failed to read 'Comparand' in SpecCondition.");
                }

                comparand = obj;
                hasComparand = true;
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        if (variable == null)
            throw new JsonException("Missing 'Variable' while reading SpecCondition.");

        if (!operation.HasValue)
            throw new JsonException("Missing 'Operation' while reading SpecCondition.");

        if (!hasComparand)
            throw new JsonException("Missing 'Comparand' while reading SpecCondition.");

        return new SpecCondition(variable, operation.Value, comparand);
    }

    public static void Write(Utf8JsonWriter writer, SpecCondition value)
    {
        if (ReferenceEquals(SpecDynamicValue.True, value.Variable) && value is { Operation: ConditionOperation.Equal, Comparand: true })
        {
            writer.WriteBooleanValue(true);
            return;
        }
        if (ReferenceEquals(SpecDynamicValue.False, value.Variable) && value is { Operation: ConditionOperation.Equal, Comparand: true })
        {
            writer.WriteBooleanValue(false);
            return;
        }
        if (value.Variable is PropertyRef p && value is { Operation: ConditionOperation.Equal, Comparand: true })
        {
            writer.WriteStringValue(p.ToString());
            return;
        }

        writer.WriteStartObject();

        writer.WriteString(VariableProperty, value.Variable.ToString());
        writer.WriteString(OperationProperty, Operations[(int)value.Operation]);

        writer.WritePropertyName(ComparandProperty);

        JsonHelper.WriteGenericValue(writer, value.Comparand);

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecCondition value, JsonSerializerOptions options)
    {
        Write(writer, value);
    }
}