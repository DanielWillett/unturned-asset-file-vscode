using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
public class SpecConditionConverter : JsonConverter<SpecCondition>
{
    /// <inheritdoc />
    public override SpecCondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => TODO_IMPLEMENT_ME;

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecCondition value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName();
    }
}
