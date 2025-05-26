using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class TypeDictionaryConverter<TValue> : JsonConverter<Dictionary<QualifiedType, TValue>?>
{
    /// <inheritdoc />
    public override Dictionary<QualifiedType, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Unexpected token {reader.TokenType} reading Dictionary<QualifiedType, {typeof(TValue)}>.");

        Dictionary<QualifiedType, TValue> dict = new Dictionary<QualifiedType, TValue>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token {reader.TokenType} reading Dictionary<QualifiedType, {typeof(TValue)}>.");

            QualifiedType key = new QualifiedType(reader.GetString()!);
            if (!reader.Read())
                break;

            TValue val = JsonSerializer.Deserialize<TValue>(ref reader, options);
            dict.Add(key, val);
        }

        return dict;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Dictionary<QualifiedType, TValue>? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        foreach (KeyValuePair<QualifiedType, TValue> kvp in value)
        {
            writer.WritePropertyName(kvp.Key.Type);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }
}
