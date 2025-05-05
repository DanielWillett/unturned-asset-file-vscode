using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecPropertyConverter : JsonConverter<SpecProperty>
{
    private readonly SpecPropertyContext _context;
    public SpecPropertyConverter() : this(SpecPropertyContext.Property) { }
    public SpecPropertyConverter(SpecPropertyContext context)
    {
        if (_context is not SpecPropertyContext.Property and not SpecPropertyContext.Localization)
            throw new ArgumentOutOfRangeException(nameof(context));

        _context = context;
    }

    /// <inheritdoc />
    public override SpecProperty Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecProperty value, JsonSerializerOptions options)
    {

    }
}