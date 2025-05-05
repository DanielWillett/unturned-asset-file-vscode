using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecBundleAssetConverter : JsonConverter<SpecBundleAsset>
{
    /// <inheritdoc />
    public override SpecBundleAsset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        reader.Skip();
        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecBundleAsset value, JsonSerializerOptions options)
    {

    }
}