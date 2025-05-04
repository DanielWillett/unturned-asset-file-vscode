using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public sealed class UnityEngineVersionConverter : JsonConverter<UnityEngineVersion>
{
    /// <inheritdoc />
    public override UnityEngineVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Unexpected token {reader.TokenType} parsing UnityEngineVersion.");

        return UnityEngineVersion.Parse(reader.GetString()!);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, UnityEngineVersion value, JsonSerializerOptions options)
    {
        if (value.Status == null)
            writer.WriteNullValue();
        else
            writer.WriteStringValue(value.ToString());
    }
}
