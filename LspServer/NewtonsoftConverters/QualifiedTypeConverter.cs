using DanielWillett.UnturnedDataFileLspServer.Data;
using Newtonsoft.Json;

namespace DanielWillett.UnturnedDataFileLspServer.NewtonsoftConverters;

public sealed class QualifiedTypeConverter : JsonConverter<QualifiedType>
{
    /// <inheritdoc />
    public override QualifiedType ReadJson(JsonReader reader, Type objectType, QualifiedType existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return default;

        if (reader.TokenType != JsonToken.String)
            throw new JsonException($"Unexpected token {reader.TokenType} while parsing QualifiedType.");

        return new QualifiedType((string)reader.Value!);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, QualifiedType value, JsonSerializer serializer)
    {
        if (value.IsNull)
            writer.WriteNull();
        else
            writer.WriteValue(value.Type);
    }
}
