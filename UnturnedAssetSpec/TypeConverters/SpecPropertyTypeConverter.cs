using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecPropertyTypeConverter : JsonConverter<ISpecPropertyType>
{
    private readonly AssetSpecDatabase _specDatabase;

    public SpecPropertyTypeConverter(AssetSpecDatabase specDatabase)
    {
        _specDatabase = specDatabase;
    }

    /// <inheritdoc />
    public override ISpecPropertyType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException($"Unexpected token {reader.TokenType} while reading ISpecPropertyType.");

        string type = reader.GetString();
        if (string.IsNullOrWhiteSpace(type))
            throw new JsonException("Empty type while reading ISpecPropertyType.");

        return new UnresolvedSpecPropertyType(type);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ISpecPropertyType value, JsonSerializerOptions options)
    {
        return;
    }
}
