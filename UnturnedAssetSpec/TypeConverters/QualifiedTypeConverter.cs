using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public sealed class QualifiedTypeConverter : JsonConverter<QualifiedType>
{
    /// <inheritdoc />
    public override QualifiedType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Unexpected token {reader.TokenType} parsing QualifiedType.");

        return new QualifiedType(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, QualifiedType value, JsonSerializerOptions options)
    {
        if (value.IsNull)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.Type);
    }
}
