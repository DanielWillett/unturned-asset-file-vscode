using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public sealed class Color32Converter : JsonConverter<Color32>
{
    public override Color32 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Invalid token: {reader.TokenType}.");

        if (!KnownTypeValueHelper.TryParseColorHex(reader.GetString().AsSpan(), out Color32 c32, true))
            throw new JsonException("Invalid color.");

        return c32;
    }

    public override void Write(Utf8JsonWriter writer, Color32 value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}