using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public sealed class BundleReferenceConverter : JsonConverter<BundleReference>
{
    private static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("Name");
    private static readonly JsonEncodedText PathProperty = JsonEncodedText.Encode("Path");

    private static readonly JsonEncodedText MasterBundleProperty = JsonEncodedText.Encode("MasterBundle");
    private static readonly JsonEncodedText AssetPathProperty = JsonEncodedText.Encode("AssetPath");

    /// <inheritdoc />
    public override BundleReference Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return default;

            case JsonTokenType.String:
                if (!KnownTypeValueHelper.TryParseMasterBundleReference(reader.GetString(), out string name, out string path))
                {
                    throw new JsonException("Unable to parse a bundle reference from a string.");
                }

                return new BundleReference(name, path, MasterBundleReferenceType.Unspecified);

            case JsonTokenType.StartObject:

                string? valName = null, valPath = null;
                MasterBundleReferenceType type = MasterBundleReferenceType.Unspecified;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException($"Unexpected token {reader.TokenType} while parsing BundleReference.");

                    int propType;
                    JsonEncodedText property;
                    if (reader.ValueTextEquals(NameProperty.EncodedUtf8Bytes))
                    {
                        propType = 0;
                        property = NameProperty;
                    }
                    else if (reader.ValueTextEquals(PathProperty.EncodedUtf8Bytes))
                    {
                        propType = 1;
                        property = PathProperty;
                    }
                    else if (reader.ValueTextEquals(MasterBundleProperty.EncodedUtf8Bytes))
                    {
                        propType = 2;
                        property = MasterBundleProperty;
                    }
                    else if (reader.ValueTextEquals(AssetPathProperty.EncodedUtf8Bytes))
                    {
                        propType = 3;
                        property = AssetPathProperty;
                    }
                    else
                    {
                        propType = -1;
                        property = default;
                    }


                    if (!reader.Read() || reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray or JsonTokenType.PropertyName or JsonTokenType.Comment)
                    {
                        if (propType != -1)
                            throw new JsonException($"Failed to read BundleReference property {property.ToString()}.");

                        continue;
                    }

                    if (propType == -1)
                    {
                        reader.Skip();
                        continue;
                    }

                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        throw new JsonException($"Unexpected token {reader.TokenType} while parsing BundleReference property \"{property.ToString()}\".");

                    switch (propType)
                    {
                        case 0: // Name
                            if (type is not MasterBundleReferenceType.Unspecified and not MasterBundleReferenceType.ContentReference)
                                type = MasterBundleReferenceType.Unspecified;
                            else
                                type = MasterBundleReferenceType.ContentReference;

                            valName = reader.GetString();
                            break;

                        case 1: // Path
                            if (type is not MasterBundleReferenceType.Unspecified and not MasterBundleReferenceType.ContentReference)
                                type = MasterBundleReferenceType.Unspecified;
                            else
                                type = MasterBundleReferenceType.ContentReference;

                            valPath = reader.GetString();
                            break;

                        case 2: // MasterBundle
                            if (type is not MasterBundleReferenceType.Unspecified and not MasterBundleReferenceType.MasterBundleReference)
                                type = MasterBundleReferenceType.Unspecified;
                            else
                                type = MasterBundleReferenceType.MasterBundleReference;

                            valName = reader.GetString();
                            break;

                        case 3: // AssetPath
                            if (type is not MasterBundleReferenceType.Unspecified and not MasterBundleReferenceType.MasterBundleReference)
                                type = MasterBundleReferenceType.Unspecified;
                            else
                                type = MasterBundleReferenceType.MasterBundleReference;

                            valPath = reader.GetString();
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
    public override void Write(Utf8JsonWriter writer, BundleReference value, JsonSerializerOptions options)
    {
        if (value.Name == null || value.Path == null)
            writer.WriteNullValue();
        else if (value.Name.Length == 0)
            writer.WriteStringValue(value.Path);
        else
            writer.WriteStringValue(value.Name + ":" + value.Path);
    }
}