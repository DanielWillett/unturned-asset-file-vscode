using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Json;

public class SpecDynamicSwitchValueConverter : JsonConverter<SpecDynamicSwitchValue?>
{
    public static SpecDynamicSwitchValue? ReadSwitch(ref Utf8JsonReader reader, JsonSerializerOptions? options, PropertyTypeOrSwitch expectedType, bool expandLists)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} while reading SpecDynamicSwitchValue.");
        }

        List<SpecDynamicSwitchCaseValue> buffer = new List<SpecDynamicSwitchCaseValue>(8);
        ISpecPropertyType? endType = expectedType.Type;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            ISpecPropertyType? type = endType;
            Utf8JsonReader r2 = reader;
            SpecDynamicSwitchCaseValue? @case = SpecDynamicSwitchCaseValueConverter.ReadCase(ref reader, options, type, expandLists);
            if (@case == null)
                continue;

            if (type == null
                && expectedType.IsSwitch
                && expectedType.TypeSwitch.TryEvaluateMatchingSwitchCase(buffer, @case, out SpecDynamicSwitchCaseValue? matchingCase)
                && matchingCase.Value is SpecDynamicConcreteValue<ISpecPropertyType> { Value: { } caseType })
            {
                @case = SpecDynamicSwitchCaseValueConverter.ReadCase(ref r2, options, caseType, expandLists);
                if (@case == null)
                    continue;
            }

            buffer.Add(@case);
        }

        return new SpecDynamicSwitchValue(endType, new OneOrMore<SpecDynamicSwitchCaseValue>(buffer));
    }

    public static void WriteSwitch(Utf8JsonWriter writer, SpecDynamicSwitchValue? value, JsonSerializerOptions? options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();

        foreach (SpecDynamicSwitchCaseValue @case in value.Cases)
        {
            SpecDynamicSwitchCaseValueConverter.WriteCase(writer, @case, options);
        }

        writer.WriteEndArray();
    }

    /// <inheritdoc />
    public override SpecDynamicSwitchValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadSwitch(ref reader, options, default, false);
    }


    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecDynamicSwitchValue? value, JsonSerializerOptions options)
    {
        WriteSwitch(writer, value, options);
    }
}