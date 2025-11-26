using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecDynamicValueConverter : JsonConverter<ISpecDynamicValue>
{
    /// <inheritdoc />
    public override ISpecDynamicValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return SpecDynamicValue.ReadValue(ref reader, null, null!, false);
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ISpecDynamicValue value, JsonSerializerOptions options)
    {
        value.WriteToJsonWriter(writer, options);
    }
}