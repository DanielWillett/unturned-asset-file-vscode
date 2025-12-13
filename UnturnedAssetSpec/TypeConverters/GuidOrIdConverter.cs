using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Json;

public sealed class GuidOrIdConverter : JsonConverter<GuidOrId>
{
    private static readonly JsonEncodedText EmptyGuid;
    static GuidOrIdConverter()
    {
        EmptyGuid = JsonEncodedText.Encode(Guid.Empty.ToString("D"));
    }

    /// <inheritdoc />
    public override GuidOrId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return GuidOrId.Empty;

            case JsonTokenType.Number:
                if (reader.TryGetUInt16(out ushort id))
                    return id == 0 ? GuidOrId.Empty : new GuidOrId(id);

                throw new JsonException("Expected ID to be an integer within the range 0-65535 while parsing GuidOrId.");

            case JsonTokenType.String:
                if (reader.ValueTextEquals(EmptyGuid.EncodedUtf8Bytes))
                    return GuidOrId.Empty;

                if (JsonHelper.TryGetGuid(ref reader, out Guid guid))
                    return new GuidOrId(guid);

                if (ushort.TryParse(reader.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out id))
                    return id == 0 ? GuidOrId.Empty : new GuidOrId(id);

                throw new JsonException("Expected string to be a valid GUID or integer (0-65535) while parsing GuidOrId.");

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} while parsing GuidOrId.");
        }
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, GuidOrId value, JsonSerializerOptions options)
    {
        if (value.IsNull)
            writer.WriteStringValue(EmptyGuid);
        else if (value.IsId)
            writer.WriteNumberValue(value.Id);
        else
            writer.WriteStringValue(value.Guid);
    }
}
