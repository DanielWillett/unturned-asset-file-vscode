using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecTypeConverter : JsonConverter<ISpecType?>
{
    /// <inheritdoc />
    public override ISpecType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        reader.Skip();
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ISpecType? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }


    }
}
