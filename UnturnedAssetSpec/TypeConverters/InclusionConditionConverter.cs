using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class InclusionConditionConverter : JsonConverter<InclusionCondition?>
{
    private static readonly JsonEncodedText KeyProperty = JsonEncodedText.Encode("Key");
    private static readonly JsonEncodedText ValueProperty = JsonEncodedText.Encode("Value");
    private static readonly JsonEncodedText ConditionProperty = JsonEncodedText.Encode("Condition");

    /// <inheritdoc />
    public override InclusionCondition? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadCondition(ref reader, options, false);
    }

    public static InclusionCondition? ReadCondition(ref Utf8JsonReader reader, JsonSerializerOptions? options, bool isInclusive)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string propertyName = reader.GetString();
                return new InclusionCondition(
                    new OneOrMore<PropertyRef>(
                        new PropertyRef(propertyName.AsSpan(), propertyName)
                    )
                );
            }

            throw new JsonException($"Unexpected token {reader.TokenType} reading InclusionCondition.");
        }

        OneOrMore<PropertyRef> properties = OneOrMore<PropertyRef>.Null;
        OneOrMore<InclusionConditionProperty> conditions = OneOrMore<InclusionConditionProperty>.Null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType is JsonTokenType.Comment or JsonTokenType.Null)
                continue;

            if (reader.TokenType == JsonTokenType.String)
            {
                if (!conditions.IsNull)
                    throw new JsonException("Can not combine property names and conditions while reading an InclusionCondition.");

                string str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str))
                    continue;

                properties = properties.Add(str.Length > 1 && str.StartsWith("@", StringComparison.Ordinal)
                    ? new PropertyRef(str.AsSpan(1), null)
                    : new PropertyRef(str.AsSpan(), str)
                );
                continue;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException($"Unexpected token {reader.TokenType} reading InclusionCondition.");

            if (!properties.IsNull)
                throw new JsonException("Can not combine property names and conditions while reading an InclusionCondition.");

            SpecDynamicSwitchCaseOrCondition cond = default;
            string? key = null;
            object? value = "*";

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    if (reader.TokenType == JsonTokenType.Comment)
                        continue;

                    throw new JsonException($"Unexpected token {reader.TokenType} reading InclusionCondition.");
                }

                int propType;
                JsonEncodedText property;
                if (reader.ValueTextEquals(KeyProperty.EncodedUtf8Bytes))
                {
                    propType = 0;
                    property = KeyProperty;
                }
                else if (reader.ValueTextEquals(ValueProperty.EncodedUtf8Bytes))
                {
                    propType = 1;
                    property = ValueProperty;
                }
                else if (reader.ValueTextEquals(ConditionProperty.EncodedUtf8Bytes))
                {
                    propType = 2;
                    property = ConditionProperty;
                }
                else
                {
                    propType = -1;
                    property = default;
                }

                if (!reader.Read() || reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray or JsonTokenType.PropertyName or JsonTokenType.Comment)
                {
                    if (propType != -1)
                        throw new JsonException($"Failed to read InclusionCondition property {property.ToString()}.");

                    reader.Skip();
                    continue;
                }

                switch (propType)
                {
                    case 0: // Key
                        if (reader.TokenType is not JsonTokenType.String)
                            ThrowUnexpectedToken(reader.TokenType, propType);
                        key = reader.GetString();
                        break;

                    case 1: // Value
                        if (!JsonHelper.TryReadGenericValue(ref reader, out value))
                            ThrowUnexpectedToken(reader.TokenType, propType);
                        break;

                    case 2: // Condition
                        if (reader.TokenType == JsonTokenType.Null)
                        {
                            cond = default;
                            break;
                        }

                        if (reader.TokenType is not JsonTokenType.StartObject)
                            ThrowUnexpectedToken(reader.TokenType, propType);

                        try
                        {
                            Utf8JsonReader readerCopy = reader;
                            cond = new SpecDynamicSwitchCaseOrCondition(
                                SpecConditionConverter.ReadCondition(ref readerCopy, options)
                            );
                            reader = readerCopy;
                        }
                        catch (JsonException)
                        {
                            cond = new SpecDynamicSwitchCaseOrCondition(
                                SpecDynamicSwitchCaseValueConverter.ReadCase(ref reader, options, KnownTypes.Boolean, false)
                            );
                        }

                        break;

                    default:
                        reader.Skip();
                        break;
                }
            }

            InclusionConditionProperty prop = new InclusionConditionProperty(isInclusive, new PropertyRef(key.AsSpan(), key), value, cond);
            conditions = conditions.Add(prop);
        }

        return conditions.Length > 0 ? new InclusionCondition(conditions) : new InclusionCondition(properties);
    }

    private static void ThrowUnexpectedToken(JsonTokenType tokenType, int propType)
    {
        ThrowUnexpectedToken(tokenType, (propType switch
        {
            0 => KeyProperty,
            1 => ValueProperty,
            _ => ConditionProperty
        }).ToString());
    }
    private static void ThrowUnexpectedToken(JsonTokenType tokenType, string property)
    {
        throw new JsonException($"Unexpected token {tokenType} reading InclusionConditionProperty.\"{property}\".");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, InclusionCondition? value, JsonSerializerOptions options)
    {
        WriteCondition(writer, value, options);
    }

    public static void WriteCondition(Utf8JsonWriter writer, InclusionCondition? property, JsonSerializerOptions options)
    {
        if (property == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        if (property.Properties.IsNull)
        {
            foreach (PropertyRef str in property.PropertyNames)
            {
                writer.WriteStringValue(str.ToString());
            }
        }
        else
        {
            foreach (InclusionConditionProperty prop in property.Properties)
            {
                writer.WriteStartObject();

                writer.WriteString(KeyProperty, prop.PropertyName.ToString());

                if (prop.Value is not string str || !str.Equals("*", StringComparison.Ordinal))
                {
                    writer.WritePropertyName(ValueProperty);
                    JsonHelper.WriteGenericValue(writer, prop.Value);
                }

                if (!prop.Condition.IsNull)
                {
                    writer.WritePropertyName(ConditionProperty);
                    prop.Condition.WriteToJsonWriter(writer, options);
                }

                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray();
    }
}