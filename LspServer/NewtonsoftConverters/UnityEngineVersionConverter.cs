using DanielWillett.UnturnedDataFileLspServer.Data;
using Newtonsoft.Json;

namespace DanielWillett.UnturnedDataFileLspServer.NewtonsoftConverters;

public sealed class UnityEngineVersionConverter : JsonConverter<UnityEngineVersion>
{
    /// <inheritdoc />
    public override UnityEngineVersion ReadJson(JsonReader reader, Type objectType, UnityEngineVersion existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return default;

        if (reader.TokenType != JsonToken.String)
            throw new JsonException($"Unexpected token {reader.TokenType} while parsing UnityEngineVersion.");

        return UnityEngineVersion.Parse((string)reader.Value!);
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, UnityEngineVersion value, JsonSerializer serializer)
    {
        if (value.Status == null)
            writer.WriteNull();
        else
            writer.WriteValue(value.ToString());
    }
}
