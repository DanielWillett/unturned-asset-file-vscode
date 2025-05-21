using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecDynamicSwitchValueConverter : JsonConverter<SpecDynamicSwitchValue?>
{
    public static SpecDynamicSwitchValue? ReadSwitch(ref Utf8JsonReader reader, JsonSerializerOptions? options, ISpecPropertyType? expectedType)
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
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            SpecDynamicSwitchCaseValue? @case = SpecDynamicSwitchCaseValueConverter.ReadCase(ref reader, options, expectedType);
            if (@case == null)
                continue;

            buffer.Add(@case);
        }

        return new SpecDynamicSwitchValue(expectedType, new OneOrMore<SpecDynamicSwitchCaseValue>(buffer));
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
        return ReadSwitch(ref reader, options, null);
    }


    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecDynamicSwitchValue? value, JsonSerializerOptions options)
    {
        WriteSwitch(writer, value, options);
    }
}