using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class AssetSpecTypeConverter : JsonConverter<AssetSpecType?>
{
    private static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("Type");
    private static readonly JsonEncodedText CategoryProperty = JsonEncodedText.Encode("Category");
    private static readonly JsonEncodedText DisplayNameProperty = JsonEncodedText.Encode("DisplayName");
    private static readonly JsonEncodedText ParentProperty = JsonEncodedText.Encode("Parent");
    private static readonly JsonEncodedText DocsProperty = JsonEncodedText.Encode("Docs");
    private static readonly JsonEncodedText VanillaIdLimitProperty = JsonEncodedText.Encode("VanillaIdLimit");
    private static readonly JsonEncodedText RequireIdProperty = JsonEncodedText.Encode("RequireId");
    private static readonly JsonEncodedText PropertiesProperty = JsonEncodedText.Encode("Properties");
    private static readonly JsonEncodedText LocalizationProperty = JsonEncodedText.Encode("Localization");
    private static readonly JsonEncodedText BundleAssetsProperty = JsonEncodedText.Encode("BundleAssets");
    private static readonly JsonEncodedText TypesProperty = JsonEncodedText.Encode("Types");
    private static readonly JsonEncodedText VersionProperty = JsonEncodedText.Encode("Version");

    private static readonly JsonEncodedText[] Properties =
    [
        TypeProperty,            // 0
        CategoryProperty,        // 1
        DisplayNameProperty,     // 2
        ParentProperty,          // 3
        DocsProperty,            // 4
        VanillaIdLimitProperty,  // 5
        RequireIdProperty,       // 6
        PropertiesProperty,      // 7
        LocalizationProperty,    // 8
        BundleAssetsProperty,    // 9
        TypesProperty,           // 10
        VersionProperty          // 11
    ];

    /// <inheritdoc />
    public override AssetSpecType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadFile(ref reader, options);
    }

    public static AssetSpecType? ReadFile(ref Utf8JsonReader reader, JsonSerializerOptions? options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        AssetSpecType type = new AssetSpecType();

        bool hadParent = false;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token {reader.TokenType} reading AssetSpecType.");

            int propType = -1;
            for (int i = 0; i < Properties.Length; ++i)
            {
                if (!reader.ValueTextEquals(Properties[i].EncodedUtf8Bytes))
                    continue;

                propType = i;
                break;
            }

            string? key = null;
            if (propType == -1)
            {
                key = reader.GetString();
            }

            if (!reader.Read() || reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray or JsonTokenType.PropertyName or JsonTokenType.Comment)
            {
                if (propType != -1)
                    throw new JsonException($"Failed to read AssetSpecType property {Properties[propType].ToString()}.");

                continue;
            }

            switch (propType)
            {
                case -1:
                    // extra properties
                    if (JsonHelper.TryReadGenericValue(ref reader, out object? extraValue))
                    {
                        type.AdditionalProperties = type.AdditionalProperties.Add(new KeyValuePair<string, object?>(key!, extraValue));
                    }
                    break;

                case 0: // Type
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    type.Type = new QualifiedType(reader.GetString());
                    break;

                case 1: // Category
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    string? str = reader.GetString();
                    if (str == null)
                    {
                        type.Category = AssetCategory.None;
                    }
                    else if (!AssetCategory.TryParse(str, out EnumSpecTypeValue category))
                    {
                        throw new JsonException($"Invalid category \"{str}\" reading AssetSpecType.\"{CategoryProperty.ToString()}\".");
                    }
                    else
                    {
                        type.Category = category;
                    }
                    break;

                case 2: // DisplayName
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    type.DisplayName = reader.GetString();
                    break;

                case 3: // Parent
                    hadParent = true;
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        type.Parent = QualifiedType.None;
                        break;
                    }
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    type.Parent = new QualifiedType(reader.GetString());
                    break;

                case 4: // Docs
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    type.Docs = reader.GetString();
                    break;

                case 5: // VanillaIdLimit
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        type.VanillaIdLimit = 0;
                        break;
                    }

                    if (reader.TokenType is not JsonTokenType.Number)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    if (!reader.TryGetUInt16(out ushort id))
                        throw new JsonException($"Invalid ID reading AssetSpecType.\"{VanillaIdLimitProperty.ToString()}\".");

                    type.VanillaIdLimit = id;
                    break;

                case 6: // RequireId
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    type.RequireId = reader.TokenType == JsonTokenType.True;
                    break;

                case 7: // Properties
                    type.Properties = ReadPropertyList(ref reader, options, type, in PropertiesProperty);
                    break;

                case 8: // Localization
                    type.LocalizationProperties = ReadPropertyList(ref reader, options, type, in LocalizationProperty);
                    break;

                case 9: // BundleAssets
                    //info.BundleAssets = ReadPropertyList(ref reader, options, in LocalizationProperty);
                    reader.Skip();
                    break;

                case 10: // Types
                    type.Types = ReadTypeList(ref reader, options, type);
                    break;

                case 11: // Version
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    type.Version = reader.TokenType == JsonTokenType.String ? new Version(reader.GetString()!) : null;
                    break;
            }
        }

        if (type.Type.IsNull)
            throw new JsonException($"AssetSpecType missing \"{TypeProperty.ToString()}\" property.");
        if (string.IsNullOrEmpty(type.DisplayName))
            throw new JsonException($"AssetSpecType missing \"{DisplayNameProperty.ToString()}\" property.");
        if (!hadParent)
            throw new JsonException($"AssetSpecType missing \"{ParentProperty.ToString()}\" property.");

        type.Properties ??= Array.Empty<SpecProperty>();
        type.LocalizationProperties ??= Array.Empty<SpecProperty>();
        type.BundleAssets ??= Array.Empty<SpecBundleAsset>();
        type.Types ??= Array.Empty<ISpecType>();

        type.DisplayName ??= QualifiedType.ExtractTypeName(type.Type.Type.AsSpan()).ToString();

        return type;
    }

    private static void ThrowUnexpectedToken(JsonTokenType tokenType, int propType)
    {
        ThrowUnexpectedToken(tokenType, Properties[propType].ToString());
    }
    private static void ThrowUnexpectedToken(JsonTokenType tokenType, string property)
    {
        throw new JsonException($"Unexpected token {tokenType} reading AssetSpecType.\"{property}\".");
    }

    private static ISpecType[] ReadTypeList(ref Utf8JsonReader reader, JsonSerializerOptions? options, AssetSpecType type)
    {
        List<ISpecType> list = new List<ISpecType>(4);
        int index = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            try
            {
                ISpecType? t = SpecTypeConverter.ReadType(ref reader, options);
                if (t != null)
                {
                    t.Owner = type;
                    list.Add(t);
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error reading AssetSpecType.\"{TypesProperty.ToString()}\"[{index}].", ex);
            }

            ++index;
        }

        return list.ToArray();
    }

    private static SpecProperty[] ReadPropertyList(ref Utf8JsonReader reader, JsonSerializerOptions? options, AssetSpecType type, in JsonEncodedText propertyName)
    {
        List<SpecProperty> list = new List<SpecProperty>(32);
        int index = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            try
            {
                SpecProperty? prop = SpecPropertyConverter.ReadProperty(ref reader, options);
                if (prop != null)
                {
                    prop.Owner = type;
                    list.Add(prop);
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException($"Error reading AssetSpecType.\"{propertyName.ToString()}\"[{index}].", ex);
            }

            ++index;
        }

        return list.ToArray();
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, AssetSpecType? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        writer.WriteString(TypeProperty, value.Type.Type);

        if (value.Category != AssetCategory.None)
        {
            writer.WriteString(CategoryProperty, value.Category.Value);
        }

        if (!string.IsNullOrEmpty(value.DisplayName))
        {
            writer.WriteString(DisplayNameProperty, value.DisplayName);
        }
        else
        {
            writer.WriteString(DisplayNameProperty, value.Type.Type);
        }

        writer.WriteString(ParentProperty, value.Parent.Type);

        if (!string.IsNullOrEmpty(value.Docs))
        {
            writer.WriteString(DocsProperty, value.Docs);
        }

        if (value.VanillaIdLimit != 0)
        {
            writer.WriteNumber(VanillaIdLimitProperty, value.VanillaIdLimit);
        }

        if (value.RequireId)
        {
            writer.WriteBoolean(RequireIdProperty, true);
        }

        if (value.Version != null)
        {
            writer.WriteString(VersionProperty, value.Version.ToString());
        }

        JsonHelper.WriteAdditionalProperties(writer, value, options);

        writer.WriteStartArray(PropertiesProperty);
        
        if (value.Properties != null)
        {
            foreach (SpecProperty? property in value.Properties)
            {
                if (property?.Key == null)
                    continue;

                if (!property.IsOverride)
                {
                    SpecPropertyConverter.WriteProperty(writer, property, options);
                }
            }
        }

        writer.WriteEndArray();

        writer.WriteStartArray(LocalizationProperty);
        
        if (value.LocalizationProperties != null)
        {
            foreach (SpecProperty? localizationProperty in value.LocalizationProperties)
            {
                if (localizationProperty?.Key == null)
                    continue;

                if (ReferenceEquals(localizationProperty.Owner, value))
                {
                    SpecPropertyConverter.WriteProperty(writer, localizationProperty, options);
                }
            }
        }

        writer.WriteEndArray();

        writer.WriteStartArray(BundleAssetsProperty);
        
        //if (value.BundleAssets != null)
        //{
        //    foreach (SpecBundleAsset? bundleAsset in value.BundleAssets)
        //    {
        //        if (bundleAsset == null)
        //            continue;
        //
        //        BundleAssetConverter.Write(writer, bundleAsset, options);
        //    }
        //}

        writer.WriteEndArray();

        writer.WriteStartArray(TypesProperty);
        
        if (value.Types != null)
        {
            foreach (ISpecType? type in value.Types)
            {
                if (type == null)
                    continue;
        
                SpecTypeConverter.WriteType(writer, type, options);
            }
        }

        writer.WriteEndArray();


        writer.WriteEndObject();
    }
}
