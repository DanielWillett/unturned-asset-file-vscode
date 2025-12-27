using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using Newtonsoft.Json;

namespace DanielWillett.UnturnedDataFileLspServer.NewtonsoftConverters;

public sealed class BundleReferenceConverter : JsonConverter<BundleReference>
{
    /// <inheritdoc />
    public override BundleReference ReadJson(JsonReader reader, Type objectType, BundleReference existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        switch (reader.TokenType)
        {
            case JsonToken.Null:
                return default;

            case JsonToken.String:
                if (!KnownTypeValueHelper.TryParseMasterBundleReference((string)reader.Value!, out string name, out string path))
                {
                    throw new JsonException("Unable to parse a bundle reference from a string.");
                }

                return new BundleReference(name, path, BundleReferenceKind.Unspecified);

            case JsonToken.StartObject:

                string? valName = null, valPath = null;
                BundleReferenceKind type = BundleReferenceKind.Unspecified;

                while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new JsonException($"Unexpected token {reader.TokenType} while parsing BundleReference.");

                    int propType;
                    string? property = (string?)reader.Value;
                    if (string.Equals("Name", property, StringComparison.Ordinal))
                    {
                        propType = 0;
                    }
                    else if (string.Equals("Path", property, StringComparison.Ordinal))
                    {
                        propType = 1;
                    }
                    else if (string.Equals("MasterBundle", property, StringComparison.Ordinal))
                    {
                        propType = 2;
                    }
                    else if (string.Equals("AssetPath", property, StringComparison.Ordinal))
                    {
                        propType = 3;
                    }
                    else
                    {
                        propType = -1;
                    }


                    if (!reader.Read() || reader.TokenType is JsonToken.EndObject or JsonToken.EndArray or JsonToken.PropertyName or JsonToken.Comment)
                    {
                        if (propType != -1)
                            throw new JsonException($"Failed to read BundleReference property {property}.");

                        continue;
                    }

                    if (propType == -1)
                    {
                        reader.Skip();
                        continue;
                    }

                    if (reader.TokenType is not JsonToken.String and not JsonToken.Null)
                        throw new JsonException($"Unexpected token {reader.TokenType} while parsing BundleReference property \"{property}\".");

                    switch (propType)
                    {
                        case 0: // Name
                            if (type is not BundleReferenceKind.Unspecified and not BundleReferenceKind.ContentReference)
                                type = BundleReferenceKind.Unspecified;
                            else
                                type = BundleReferenceKind.ContentReference;

                            valName = (string?)reader.Value;
                            break;

                        case 1: // Path
                            if (type is not BundleReferenceKind.Unspecified and not BundleReferenceKind.ContentReference)
                                type = BundleReferenceKind.Unspecified;
                            else
                                type = BundleReferenceKind.ContentReference;

                            valPath = (string?)reader.Value;
                            break;

                        case 2: // MasterBundle
                            if (type is not BundleReferenceKind.Unspecified and not BundleReferenceKind.MasterBundleReference)
                                type = BundleReferenceKind.Unspecified;
                            else
                                type = BundleReferenceKind.MasterBundleReference;

                            valName = (string?)reader.Value;
                            break;

                        case 3: // AssetPath
                            if (type is not BundleReferenceKind.Unspecified and not BundleReferenceKind.MasterBundleReference)
                                type = BundleReferenceKind.Unspecified;
                            else
                                type = BundleReferenceKind.MasterBundleReference;

                            valPath = (string?)reader.Value;
                            break;
                    }
                }

                if (valPath == null)
                    throw new JsonException("Missing asset path (\"AssetPath\" or \"Path\") while parsing BundleReference.");

                return new BundleReference(valName ?? string.Empty, valPath, type);

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} while parsing BundleReference.");
        }
    }

    /// <inheritdoc />
    public override void WriteJson(JsonWriter writer, BundleReference value, JsonSerializer serializer)
    {
        if (value.Name == null || value.Path == null)
            writer.WriteNull();
        else if (value.Name.Length == 0)
            writer.WriteValue(value.Path);
        else
            writer.WriteValue(value.Name + ":" + value.Path);
    }

}