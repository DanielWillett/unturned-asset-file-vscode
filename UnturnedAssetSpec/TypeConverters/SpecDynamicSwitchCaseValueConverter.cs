using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Json;

public class SpecDynamicSwitchCaseValueConverter : JsonConverter<SpecDynamicSwitchCaseValue?>
{
    private static readonly JsonEncodedText AndProperty = JsonEncodedText.Encode("And");
    private static readonly JsonEncodedText OrProperty = JsonEncodedText.Encode("Or");
    private static readonly JsonEncodedText ValueProperty = JsonEncodedText.Encode("Value");
    private static readonly JsonEncodedText WhenProperty = JsonEncodedText.Encode("When");
    private static readonly JsonEncodedText CasesProperty = JsonEncodedText.Encode("Cases");
    private static readonly JsonEncodedText CaseProperty = JsonEncodedText.Encode("Case");

    public static SpecDynamicSwitchCaseValue? ReadCase(ref Utf8JsonReader reader, JsonSerializerOptions? options, ISpecPropertyType? expectedType, bool expandLists)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} while reading SpecDynamicSwitchCaseValue.");
        }

        SpecCondition whenCondition = default;
        SpecDynamicSwitchCaseOperation operation = (SpecDynamicSwitchCaseOperation)(-1);
        ISpecDynamicValue? value = null;
        OneOrMore<SpecDynamicSwitchCaseOrCondition> conditions = OneOrMore<SpecDynamicSwitchCaseOrCondition>.Null;
        OneOrMore<SpecDynamicSwitchCaseValue> cases = OneOrMore<SpecDynamicSwitchCaseValue>.Null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                reader.Skip();

            int property;

            if (reader.ValueTextEquals(AndProperty.EncodedUtf8Bytes))
            {
                property = 0;
            }
            else if (reader.ValueTextEquals(OrProperty.EncodedUtf8Bytes))
            {
                property = 1;
            }
            else if (reader.ValueTextEquals(CasesProperty.EncodedUtf8Bytes))
            {
                property = 2;
            }
            else if (reader.ValueTextEquals(CaseProperty.EncodedUtf8Bytes))
            {
                property = 3;
            }
            else if (reader.ValueTextEquals(WhenProperty.EncodedUtf8Bytes))
            {
                property = 4;
            }
            else if (reader.ValueTextEquals(ValueProperty.EncodedUtf8Bytes))
            {
                property = 5;
            }
            else
            {
                property = -1;
            }

            if (!reader.Read() || reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray or JsonTokenType.PropertyName or JsonTokenType.Comment)
            {
                if (property != -1)
                    throw new JsonException($"Failed to read SpecDynamicSwitchCaseValue property {reader.GetString()}.");

                reader.Skip();
                continue;
            }

            if (property == -1)
            {
                reader.Skip();
                continue;
            }

            if (property < 4 && (!cases.IsNull || !conditions.IsNull))
                throw new JsonException("Inconsistant operation of SpecDynamicSwitchCaseValue, includes multiple condition lists.");

            switch (property)
            {
                case 0: // And
                    operation = SpecDynamicSwitchCaseOperation.And;
                    conditions = ReadConditions(ref reader, options, expectedType, expandLists);
                    break;

                case 1: // Or
                    operation = SpecDynamicSwitchCaseOperation.Or;
                    conditions = ReadConditions(ref reader, options, expectedType, expandLists);
                    break;

                case 2: // Cases
                    operation = SpecDynamicSwitchCaseOperation.When;
                    value = null;
                    cases = ReadCases(ref reader, options, expectedType, expandLists);
                    break;

                case 3: // Case
                    if (operation != SpecDynamicSwitchCaseOperation.When)
                        operation = SpecDynamicSwitchCaseOperation.And;
                    SpecDynamicSwitchCaseValue @case = ReadCase(ref reader, options, expectedType, expandLists)!;
                    conditions = new SpecDynamicSwitchCaseOrCondition(@case);
                    cases = @case;
                    break;

                case 4: // When
                    operation = SpecDynamicSwitchCaseOperation.When;
                    whenCondition = SpecConditionConverter.ReadCondition(ref reader, options);
                    value = null;
                    break;

                case 5: // Value
                    if (operation != SpecDynamicSwitchCaseOperation.When)
                        value = SpecDynamicValue.Read(ref reader, options, expandLists, expectedType: expectedType);
                    else
                        reader.Skip();
                    break;
            }
        }

        if (operation is not SpecDynamicSwitchCaseOperation.And and not SpecDynamicSwitchCaseOperation.Or and not SpecDynamicSwitchCaseOperation.When)
            operation = SpecDynamicSwitchCaseOperation.And;
        
        if (operation != SpecDynamicSwitchCaseOperation.When)
        {
            if (value == null)
                throw new JsonException("Missing 'Value' while reading SpecDynamicSwitchCaseValue.");

            return new SpecDynamicSwitchCaseValue(operation, value, conditions);
        }

        if (cases.IsNull)
        {
            throw new JsonException("Missing 'Cases' or 'Case' property while reading SpecDynamicSwitchCaseValue as a conditional switch (with a 'When' property).");
        }

        return new SpecDynamicSwitchCaseValue(whenCondition, expectedType, cases);
    }

    private static SpecDynamicSwitchCaseOrCondition ReadCondition(
        ref Utf8JsonReader reader,
        JsonSerializerOptions? options,
        ISpecPropertyType? expectedType,
        bool expandLists
    )
    {
        Utf8JsonReader readerCopy = reader;
        try
        {
            return new SpecDynamicSwitchCaseOrCondition(
                SpecConditionConverter.ReadCondition(ref reader, options)
            );
        }
        catch (JsonException)
        {
            reader = readerCopy;
        }

        return new SpecDynamicSwitchCaseOrCondition(
            ReadCase(ref reader, options, expectedType, expandLists)
        );
    }

    private static OneOrMore<SpecDynamicSwitchCaseOrCondition> ReadConditions(
        ref Utf8JsonReader reader,
        JsonSerializerOptions? options,
        ISpecPropertyType? expectedType,
        bool expandLists)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            return ReadCondition(ref reader, options, expectedType, expandLists);
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} reading condition or switch case.");
        }

        OneOrMore<SpecDynamicSwitchCaseOrCondition> collection = OneOrMore<SpecDynamicSwitchCaseOrCondition>.Null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            SpecDynamicSwitchCaseOrCondition condition = ReadCondition(ref reader, options, expectedType, expandLists);
            if (condition.IsNull)
                continue;
            
            collection = collection.Add(condition);
        }

        return collection;
    }

    private static OneOrMore<SpecDynamicSwitchCaseValue> ReadCases(
        ref Utf8JsonReader reader,
        JsonSerializerOptions? options,
        ISpecPropertyType? expectedType,
        bool expandLists)
    {
        if (reader.TokenType == JsonTokenType.StartObject)
        {
            SpecDynamicSwitchCaseValue? condition = ReadCase(ref reader, options, expectedType, expandLists);
            return condition ?? OneOrMore<SpecDynamicSwitchCaseValue>.Null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} reading condition or switch case.");
        }

        OneOrMore<SpecDynamicSwitchCaseValue> collection = OneOrMore<SpecDynamicSwitchCaseValue>.Null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            SpecDynamicSwitchCaseValue? @case = ReadCase(ref reader, options, expectedType, expandLists);
            if (@case == null)
                continue;

            collection = collection.Add(@case);
        }

        return collection;
    }

    public static void WriteCase(Utf8JsonWriter writer, SpecDynamicSwitchCaseValue? value, JsonSerializerOptions? options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        if (value.Operation == SpecDynamicSwitchCaseOperation.When)
        {
            writer.WritePropertyName(WhenProperty);
            SpecConditionConverter.Write(writer, value.WhenCondition);

            SpecDynamicSwitchValue @switch = (SpecDynamicSwitchValue)value.Value;
            if (@switch.HasCases)
            {
                writer.WritePropertyName(CasesProperty);
                @switch.WriteToJsonWriter(writer, options);
            }
        }
        else
        {
            if (value.HasConditions)
            {
                writer.WritePropertyName(value.Operation == SpecDynamicSwitchCaseOperation.And ? AndProperty : OrProperty);
                writer.WriteStartArray();
                foreach (SpecDynamicSwitchCaseOrCondition condition in value.Conditions)
                {
                    condition.WriteToJsonWriter(writer, options);
                }
                writer.WriteEndArray();
            }

            writer.WritePropertyName(ValueProperty);
            value.Value.WriteToJsonWriter(writer, options);
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public override SpecDynamicSwitchCaseValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadCase(ref reader, options, null, false);
    }


    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecDynamicSwitchCaseValue? value, JsonSerializerOptions options)
    {
        WriteCase(writer, value, options);
    }
}