using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class AssetTypeInformationConverter : JsonConverter<AssetTypeInformation?>
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

    private static readonly SpecPropertyConverter PropertyConverter = new SpecPropertyConverter(context: SpecPropertyContext.Property);
    private static readonly SpecPropertyConverter LocalizationConverter = new SpecPropertyConverter(context: SpecPropertyContext.Localization);
    private static readonly SpecBundleAssetConverter BundleAssetConverter = new SpecBundleAssetConverter();
    private static readonly SpecTypeConverter SpecTypeConverter = new SpecTypeConverter();

    /// <inheritdoc />
    public override AssetTypeInformation? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        AssetTypeInformation info = new AssetTypeInformation();
        bool hadParent = false;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token {reader.TokenType} reading AssetTypeInformation.");

            if (reader.ValueTextEquals(TypeProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.Type = new QualifiedType(reader.GetString());
            }
            else if (reader.ValueTextEquals(CategoryProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.Category = reader.GetString() ?? "NONE";
            }
            else if (reader.ValueTextEquals(DisplayNameProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.DisplayName = reader.GetString();
            }
            else if (reader.ValueTextEquals(ParentProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.Parent = reader.TokenType == JsonTokenType.Null ? default : new QualifiedType(reader.GetString());
                hadParent = true;
            }
            else if (reader.ValueTextEquals(DocsProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.Docs = reader.GetString();
            }
            else if (reader.ValueTextEquals(VanillaIdLimitProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.VanillaIdLimit = reader.TokenType == JsonTokenType.Null ? (ushort)0 : reader.GetUInt16();
            }
            else if (reader.ValueTextEquals(RequireIdProperty.EncodedUtf8Bytes))
            {
                DoRead(ref reader);
                info.RequireId = reader.TokenType == JsonTokenType.True;
            }
            else
            {
                SpecPropertyContext ctx;
                if (reader.ValueTextEquals(PropertiesProperty.EncodedUtf8Bytes))
                    ctx = SpecPropertyContext.Property;
                else if (reader.ValueTextEquals(LocalizationProperty.EncodedUtf8Bytes))
                    ctx = SpecPropertyContext.Localization;
                // todo else if (reader.ValueTextEquals(BundleAssetsProperty.EncodedUtf8Bytes))
                // todo     ctx = SpecPropertyContext.BundleAsset;
                // todo else if (reader.ValueTextEquals(TypesProperty.EncodedUtf8Bytes))
                // todo     ctx = SpecPropertyContext.Type;
                else
                {
                    DoRead(ref reader);
                    reader.Skip();
                    continue;
                }

                DoRead(ref reader);
                if (reader.TokenType != JsonTokenType.StartArray)
                {
                    if (reader.TokenType == JsonTokenType.Null)
                        continue;
                    
                    throw new JsonException($"Unexpected token {reader.TokenType} reading list in AssetTypeInformation.");
                }

                switch (ctx)
                {
                    case SpecPropertyContext.Property:
                        List<SpecProperty> properties = new List<SpecProperty>(16);
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            SpecProperty? property = PropertyConverter.Read(ref reader, typeof(SpecProperty), options);
                            if (property is { Key: not null, Type: not null })
                                properties.Add(property);
                        }

                        properties.Capacity = properties.Count;
                        info.Properties = properties;
                        break;

                    case SpecPropertyContext.Localization:
                        properties = new List<SpecProperty>(4);
                        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                        {
                            SpecProperty? property = LocalizationConverter.Read(ref reader, typeof(SpecProperty), options);
                            if (property is { Key: not null, Type: not null })
                                properties.Add(property);
                        }

                        properties.Capacity = properties.Count;
                        info.LocalizationProperties = properties;
                        break;

                    // todo case SpecPropertyContext.BundleAsset:
                    // todo     List<SpecBundleAsset> assets = new List<SpecBundleAsset>(16);
                    // todo     while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    // todo     {
                    // todo         SpecBundleAsset? asset = (SpecBundleAsset?)BundleAssetConverter.Read(ref reader, typeof(SpecBundleAsset), options);
                    // todo         if (asset is { Key: not null, Type: not null })
                    // todo             assets.Add(asset);
                    // todo     }
                    // todo 
                    // todo     assets.Capacity = assets.Count;
                    // todo     info.BundleAssets = assets;
                    // todo     break;
                    // todo 
                    // todo case SpecPropertyContext.Type:
                    // todo     List<ISpecType> types = new List<ISpecType>(8);
                    // todo     while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    // todo     {
                    // todo         ISpecType? type = SpecTypeConverter.Read(ref reader, typeof(ISpecType), options);
                    // todo         if (type != null)
                    // todo             types.Add(type);
                    // todo     }
                    // todo 
                    // todo     types.Capacity = types.Count;
                    // todo     info.Types = types;
                    // todo     break;
                }
            }
        }

        if (info.Type.IsNull)
            throw new JsonException("AssetTypeInformation missing \"Type\" property.");
        if (string.IsNullOrEmpty(info.DisplayName))
            throw new JsonException("AssetTypeInformation missing \"DisplayName\" property.");
        if (!hadParent)
            throw new JsonException("AssetTypeInformation missing \"Parent\" property.");

        info.Properties ??= new List<SpecProperty>(0);
        info.LocalizationProperties ??= new List<SpecProperty>(0);
        info.BundleAssets ??= new List<SpecBundleAsset>(0);
        info.Types ??= new List<ISpecType>(0);

        info.DisplayName ??= QualifiedType.ExtractTypeName(info.Type.Type.AsSpan()).ToString();

        return info;
    }

    private static void DoRead(ref Utf8JsonReader reader)
    {
        if (!reader.Read())
            throw new JsonException("Expected value after property name in AssetTypeInformation converter.");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, AssetTypeInformation? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        writer.WriteString(TypeProperty, value.Type.Type);

        if (!string.IsNullOrEmpty(value.Category) && !string.Equals(value.Category, "NONE", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteString(CategoryProperty, value.Category);
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

        if (value.VanillaIdLimit is > 0 and < 65535)
        {
            writer.WriteNumber(VanillaIdLimitProperty, value.VanillaIdLimit);
        }

        if (value.RequireId)
        {
            writer.WriteBoolean(RequireIdProperty, true);
        }

        writer.WriteStartArray(PropertiesProperty);
        
        if (value.Properties != null)
        {
            foreach (SpecProperty? property in value.Properties)
            {
                if (property?.Key == null || property.Type == null)
                    continue;

                PropertyConverter.Write(writer, property, options);
            }
        }

        writer.WriteEndArray();

        writer.WriteStartArray(LocalizationProperty);
        
        if (value.LocalizationProperties != null)
        {
            foreach (SpecProperty? localizationProperty in value.LocalizationProperties)
            {
                if (localizationProperty?.Key == null || localizationProperty.Type == null)
                    continue;

                LocalizationConverter.Write(writer, localizationProperty, options);
            }
        }

        writer.WriteEndArray();

        writer.WriteStartArray(BundleAssetsProperty);
        
        if (value.BundleAssets != null)
        {
            foreach (SpecBundleAsset? bundleAsset in value.BundleAssets)
            {
                if (bundleAsset == null)
                    continue;

                BundleAssetConverter.Write(writer, bundleAsset, options);
            }
        }

        writer.WriteEndArray();

        writer.WriteStartArray(TypesProperty);
        
        if (value.Types != null)
        {
            foreach (ISpecType? type in value.Types)
            {
                if (type == null)
                    continue;

                SpecTypeConverter.Write(writer, type, options);
            }
        }

        writer.WriteEndArray();


        writer.WriteEndObject();
    }
}
