using DanielWillett.UnturnedDataFileLspServer.Data;
using Newtonsoft.Json;
using System.Globalization;

namespace DanielWillett.UnturnedDataFileLspServer.NewtonsoftConverters;

[JsonConverter(typeof(GuidOrId))]
public sealed class GuidOrIdConverter : JsonConverter<GuidOrId>
{
    private static readonly string EmptyGuid = Guid.Empty.ToString("D");

    /// <inheritdoc />
    public override GuidOrId ReadJson(JsonReader reader, Type objectType, GuidOrId existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Null:
                return GuidOrId.Empty;

            case JsonToken.Integer:
            case JsonToken.Float:
                if (reader.Value is long l and >= ushort.MinValue and <= ushort.MaxValue)
                    return l == 0 ? GuidOrId.Empty : new GuidOrId((ushort)l);
                if (reader.Value is int i and >= ushort.MinValue and <= ushort.MaxValue)
                    return i == 0 ? GuidOrId.Empty : new GuidOrId((ushort)i);

                throw new JsonException("Expected ID to be an integer within the range 0-65535 while parsing GuidOrId.");

            case JsonToken.String:
            case JsonToken.Bytes:
            case JsonToken.Date:

                string? str = reader.Value as string ?? reader.Value?.ToString();
                if (str == null)
                    throw new JsonException("Expected string to be a valid GUID or integer (0-65535) while parsing GuidOrId.");

                if (string.Equals(str, EmptyGuid))
                    return GuidOrId.Empty;

                if (Guid.TryParse(str, CultureInfo.InvariantCulture, out Guid guid))
                    return new GuidOrId(guid);

                if (ushort.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out ushort id))
                    return id == 0 ? GuidOrId.Empty : new GuidOrId(id);

                throw new JsonException("Expected string to be a valid GUID or integer (0-65535) while parsing GuidOrId.");

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} while parsing GuidOrId.");
        }
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, GuidOrId value, JsonSerializer serializer)
    {
        if (value.IsNull)
            writer.WriteValue(EmptyGuid);
        else if (value.IsId)
            writer.WriteValue(value.Id);
        else
            writer.WriteValue(value.Guid);
    }
}