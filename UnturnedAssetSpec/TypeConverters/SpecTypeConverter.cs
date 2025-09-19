using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public sealed class SpecTypeConverter : JsonConverter<ISpecType?>
{
    private static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("Type");
    private static readonly JsonEncodedText DisplayNameProperty = JsonEncodedText.Encode("DisplayName");
    private static readonly JsonEncodedText IsLegacyExpandedTypeProperty = JsonEncodedText.Encode("IsLegacyExpandedType");
    private static readonly JsonEncodedText DocsProperty = JsonEncodedText.Encode("Docs");
    private static readonly JsonEncodedText ParentProperty = JsonEncodedText.Encode("Parent");
    private static readonly JsonEncodedText ValuesProperty = JsonEncodedText.Encode("Values");
    private static readonly JsonEncodedText PropertiesProperty = JsonEncodedText.Encode("Properties");
    private static readonly JsonEncodedText LocalizationProperty = JsonEncodedText.Encode("Localization");
    private static readonly JsonEncodedText StringParseableTypeProperty = JsonEncodedText.Encode("StringParseableType");
    private static readonly JsonEncodedText VersionProperty = JsonEncodedText.Encode("Version");
    private static readonly JsonEncodedText IsFlagsProperty = JsonEncodedText.Encode("IsFlags");

    private static readonly JsonEncodedText[] Properties =
    [
        TypeProperty,                   // 0
        DisplayNameProperty,            // 1
        IsLegacyExpandedTypeProperty,   // 2
        DocsProperty,                   // 3
        ParentProperty,                 // 4
        ValuesProperty,                 // 5
        PropertiesProperty,             // 6
        LocalizationProperty,           // 7
        StringParseableTypeProperty,    // 8
        VersionProperty,                // 9
        IsFlagsProperty                 // 10
    ];

    private static readonly JsonEncodedText ValueEnumProperty = JsonEncodedText.Encode("Value");
    private static readonly JsonEncodedText CasingEnumProperty = JsonEncodedText.Encode("Casing");
    private static readonly JsonEncodedText CorrespondingTypeEnumProperty = JsonEncodedText.Encode("CorrespondingType");
    private static readonly JsonEncodedText RequiredBaseTypeEnumProperty = JsonEncodedText.Encode("RequiredBaseType");
    private static readonly JsonEncodedText DescriptionEnumProperty = JsonEncodedText.Encode("Description");
    private static readonly JsonEncodedText DeprecatedEnumProperty = JsonEncodedText.Encode("Deprecated");
    private static readonly JsonEncodedText DocsEnumProperty = JsonEncodedText.Encode("Docs");
    private static readonly JsonEncodedText VersionEnumProperty = JsonEncodedText.Encode("Version");
    private static readonly JsonEncodedText NumericValueEnumProperty = JsonEncodedText.Encode("NumericValue");

    private static readonly JsonEncodedText[] EnumProperties =
    [
        ValueEnumProperty,             // 0
        CasingEnumProperty,            // 1
        CorrespondingTypeEnumProperty, // 2
        RequiredBaseTypeEnumProperty,  // 3
        DescriptionEnumProperty,       // 4
        DeprecatedEnumProperty,        // 5
        DocsEnumProperty,              // 6
        VersionEnumProperty,           // 7
        NumericValueEnumProperty       // 8
    ];

    /// <inheritdoc />
    public override ISpecType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadType(ref reader, options);
    }

    public static ISpecType? ReadType(ref Utf8JsonReader reader, JsonSerializerOptions? options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} while reading ISpecType.");
        }

        string? type = null, displayName = null, docs = null, parent = null;

        bool isExpanded = false, isFlags = false;
        string? stringParseableType = null;

        List<EnumValueInfo>? values = null;
        SpecProperty[]? properties = null, localProperties = null;

        OneOrMore<KeyValuePair<string, object?>> extraData = OneOrMore<KeyValuePair<string, object?>>.Null;

        Version? version = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token {reader.TokenType} reading ISpecType.");

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
                    throw new JsonException($"Failed to read ISpecType property {Properties[propType].ToString()}.");

                continue;
            }

            switch (propType)
            {
                case -1:
                    // extra properties
                    if (!JsonHelper.ShouldSkipAdditionalProperty(key) && JsonHelper.TryReadGenericValue(ref reader, out object? extraValue))
                    {
                        extraData = extraData.Add(new KeyValuePair<string, object?>(key, extraValue));
                    }
                    break;

                case 0: // Type
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    type = reader.GetString();
                    break;

                case 1: // DisplayName
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    displayName = reader.GetString();
                    break;

                case 2: // IsLegacyExpandedType
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    isExpanded = reader.TokenType == JsonTokenType.True;
                    break;

                case 3: // Docs
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    docs = reader.GetString();
                    break;

                case 4: // Parent
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    parent = reader.GetString();
                    break;

                case 5: // Values
                    if (reader.TokenType == JsonTokenType.Null)
                        continue;

                    if (properties != null || localProperties != null)
                        throw new JsonException("ISpecType contains properties for both an enum and custom type.");
                    if (reader.TokenType is not JsonTokenType.StartArray)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    values = ReadValueList(ref reader, options, type);
                    break;

                case 6: // Properties
                    if (reader.TokenType == JsonTokenType.Null)
                        continue;

                    if (values != null)
                        throw new JsonException("ISpecType contains properties for both an enum and custom type.");
                    if (reader.TokenType is not JsonTokenType.StartArray)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    properties = ReadPropertyList(ref reader, options, type, PropertiesProperty, SpecPropertyContext.Property);
                    break;

                case 7: // Localization
                    if (reader.TokenType == JsonTokenType.Null)
                        continue;

                    if (values != null)
                        throw new JsonException("ISpecType contains properties for both an enum and custom type.");
                    if (reader.TokenType is not JsonTokenType.StartArray)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    localProperties = ReadPropertyList(ref reader, options, type, LocalizationProperty, SpecPropertyContext.Localization);
                    break;

                case 8: // StringParseableType
                    if (values != null)
                        throw new JsonException("ISpecType contains properties for both an enum and custom type.");
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    stringParseableType = reader.GetString();
                    break;

                case 9: // Version
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    version = reader.TokenType == JsonTokenType.String ? new Version(reader.GetString()!) : null;
                    break;

                case 10: // IsFlags
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    isFlags = reader.TokenType == JsonTokenType.True;
                    break;
            }
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new JsonException($"Missing property \"{TypeProperty.ToString()}\" while reading ISpecType.");
        }

        QualifiedType qt = new QualifiedType(type!);

        if (string.IsNullOrWhiteSpace(displayName))
            displayName = qt.GetTypeName();

        if (values != null)
        {
            if (parent != null)
            {
                throw new JsonException($"ISpecType enum types can not define a value for \"{TypeProperty.ToString()}\".");
            }
            if (isExpanded)
            {
                throw new JsonException($"ISpecType enum types can not define a value for \"{IsLegacyExpandedTypeProperty.ToString()}\".");
            }

            EnumSpecTypeValue[] createdValues = new EnumSpecTypeValue[values.Count];
            EnumSpecType enumType = new EnumSpecType
            {
                Values = createdValues,
                DisplayName = displayName!,
                Docs = docs,
                Type = qt,
                AdditionalProperties = extraData,
                Version = version,
                IsFlags = isFlags
            };

            for (int i = 0; i < createdValues.Length; ++i)
            {
                EnumValueInfo valueInfo = values[i];
                if (valueInfo.Casing != null && !string.Equals(valueInfo.Value, valueInfo.Casing, StringComparison.OrdinalIgnoreCase))
                    throw new JsonException($"ISpecType enum type \"{type}\" value \"{valueInfo.Value}\" has a mismatched casing: \"{valueInfo.Casing}\".");

                createdValues[i] = new EnumSpecTypeValue
                {
                    Index = i,
                    Value = valueInfo.Value,
                    Casing = valueInfo.Casing ?? valueInfo.Value,
                    Type = enumType,
                    CorrespondingType = valueInfo.CorrespondingType,
                    Deprecated = valueInfo.Deprecated,
                    Description = valueInfo.Description,
                    RequiredBaseType = valueInfo.RequiredBaseType,
                    AdditionalProperties = valueInfo.Properties,
                    Docs = valueInfo.Docs,
                    Version = valueInfo.Version,
                    NumericValue = valueInfo.NumericValue
                };
            }

            return enumType;
        }

        properties ??= Array.Empty<SpecProperty>();
        localProperties ??= Array.Empty<SpecProperty>();

        CustomSpecType customType = new CustomSpecType
        {
            Type = qt,
            DisplayName = displayName ?? qt.GetTypeName(),
            Parent = parent == null ? QualifiedType.None : new QualifiedType(parent),
            Properties = properties,
            LocalizationProperties = localProperties,
            Docs = docs,
            AdditionalProperties = extraData,
            IsLegacyExpandedType = isExpanded,
            StringParsableType = stringParseableType,
            Version = version
        };

        foreach (SpecProperty prop in properties)
            prop.Owner = customType;
        foreach (SpecProperty prop in localProperties)
            prop.Owner = customType;

        return customType;
    }

    private static EnumValueInfo ReadEnumValueInfo(ref Utf8JsonReader reader, JsonSerializerOptions? options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new EnumValueInfo(reader.GetString(), null, QualifiedType.None, QualifiedType.None, null, false, null, OneOrMore<KeyValuePair<string, object?>>.Null, null, null);
        }

        if (reader.TokenType == JsonTokenType.Null)
            return default;

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} while reading enum value.");
        }

        string? value = null, casing = null, description = null, docs = null, corrType = null, baseType = null;
        bool deprecated = false;

        Version? version = null;

        long? numericValue = null;

        OneOrMore<KeyValuePair<string, object?>> extraData = OneOrMore<KeyValuePair<string, object?>>.Null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException($"Unexpected token {reader.TokenType} reading enum value.");

            int propType = -1;
            for (int i = 0; i < EnumProperties.Length; ++i)
            {
                if (!reader.ValueTextEquals(EnumProperties[i].EncodedUtf8Bytes))
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
                    throw new JsonException($"Failed to read enum value property {EnumProperties[propType].ToString()}.");

                continue;
            }

            switch (propType)
            {
                case -1:
                    // extra properties
                    if (!JsonHelper.ShouldSkipAdditionalProperty(key) && JsonHelper.TryReadGenericValue(ref reader, out object? extraValue))
                    {
                        extraData = extraData.Add(new KeyValuePair<string, object?>(key, extraValue));
                    }
                    break;

                case 0: // Value
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    value = reader.GetString();
                    break;

                case 1: // Casing
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    casing = reader.GetString();
                    break;

                case 2: // CorrespondingType
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    corrType = reader.GetString();
                    break;

                case 3: // RequiredBaseType
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    baseType = reader.GetString();
                    break;

                case 4: // Description
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    description = reader.GetString();
                    break;

                case 5: // Deprecated
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    deprecated = reader.TokenType == JsonTokenType.True;
                    break;

                case 6: // Docs
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedTokenEnumValue(reader.TokenType, propType);
                    docs = reader.GetString();
                    break;

                case 7: // Version
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    version = reader.TokenType == JsonTokenType.String ? new Version(reader.GetString()!) : null;
                    break;

                case 8: // NumericValue
                    if (reader.TokenType is not JsonTokenType.Number and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    if (reader.TokenType == JsonTokenType.Null)
                        numericValue = null;
                    else
                    {
                        if (reader.TryGetInt64(out long l))
                            numericValue = l;
                        else if (reader.TryGetUInt64(out ulong ul))
                            numericValue = unchecked((long)ul);
                        else
                            ThrowUnexpectedToken(JsonTokenType.Number, propType);
                    }
                    break;
            }
        }

        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException($"Missing property \"{ValueEnumProperty.ToString()}\" while reading enum value.");
        }
        
        return new EnumValueInfo(value!, casing, baseType, corrType, description, deprecated, docs, extraData, version, numericValue);
    }

    private static List<EnumValueInfo> ReadValueList(ref Utf8JsonReader reader, JsonSerializerOptions? options, string? typeName)
    {
        List<EnumValueInfo> list = new List<EnumValueInfo>(12);
        int index = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            try
            {
                EnumValueInfo prop = ReadEnumValueInfo(ref reader, options);
                if (!string.IsNullOrWhiteSpace(prop.Value))
                {
                    list.Add(prop);
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException(typeName != null
                        ? $"Error reading ISpecType '{typeName}' \"{ValuesProperty.ToString()}\"[{index}]."
                        : $"Error reading ISpecType.\"{ValuesProperty.ToString()}\"[{index}].",
                    ex
                );
            }

            ++index;
        }

        return list;
    }

    private static SpecProperty[] ReadPropertyList(ref Utf8JsonReader reader, JsonSerializerOptions? options, string? typeName, in JsonEncodedText propertyName, SpecPropertyContext context)
    {
        List<SpecProperty> list = new List<SpecProperty>(16);
        int index = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            try
            {
                SpecProperty? prop = SpecPropertyConverter.ReadProperty(ref reader, options);
                if (prop != null)
                {
                    prop.Context = context;
                    list.Add(prop);
                }
            }
            catch (JsonException ex)
            {
                throw new JsonException(typeName != null
                    ? $"Error reading ISpecType '{typeName}' \"{propertyName.ToString()}\"[{index}]."
                    : $"Error reading ISpecType.\"{propertyName.ToString()}\"[{index}].",
                    ex
                );
            }

            ++index;
        }

        return list.ToArray();
    }

    private static void ThrowUnexpectedToken(JsonTokenType tokenType, int propType)
    {
        ThrowUnexpectedToken(tokenType, Properties[propType].ToString());
    }
    private static void ThrowUnexpectedTokenEnumValue(JsonTokenType tokenType, int propType)
    {
        ThrowUnexpectedToken(tokenType, EnumProperties[propType].ToString());
    }
    private static void ThrowUnexpectedToken(JsonTokenType tokenType, string property)
    {
        throw new JsonException($"Unexpected token {tokenType} reading ISpecType.\"{property}\".");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ISpecType? value, JsonSerializerOptions options)
    {
        WriteType(writer, value, options);
    }

    public static void WriteType(Utf8JsonWriter writer, ISpecType? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        writer.WriteString(TypeProperty, value.Type.Type);
        writer.WriteString(DisplayNameProperty, value.DisplayName);

        if (value.Docs != null)
            writer.WriteString(DocsProperty, value.Docs);

        if (value.Version != null)
            writer.WriteString(VersionProperty, value.Version.ToString());

        switch (value)
        {
            case EnumSpecType enumType:
                JsonHelper.WriteAdditionalProperties(writer, value, options);

                writer.WritePropertyName(ValuesProperty);
                writer.WriteStartArray();

                for (int i = 0; i < enumType.Values.Length; i++)
                {
                    ref EnumSpecTypeValue property = ref enumType.Values[i];
                    if (property.CanBeWrittenAsString)
                    {
                        writer.WriteStringValue(property.Value);
                        continue;
                    }

                    writer.WriteStartObject();

                    writer.WriteString(ValueEnumProperty, property.Value);
                    if (!string.Equals(property.Value, property.Casing, StringComparison.Ordinal))
                    {
                        writer.WriteString(CasingEnumProperty, property.Casing);
                    }

                    if (!property.CorrespondingType.IsNull)
                        writer.WriteString(CorrespondingTypeEnumProperty, property.CorrespondingType);
                
                    if (!property.RequiredBaseType.IsNull)
                        writer.WriteString(CorrespondingTypeEnumProperty, property.RequiredBaseType);
                
                    if (property.Description != null)
                        writer.WriteString(DescriptionEnumProperty, property.Description);
                    if (property.Docs != null)
                        writer.WriteString(DocsEnumProperty, property.Docs);

                    if (property.Deprecated)
                        writer.WriteBoolean(DeprecatedEnumProperty, true);

                    if (property.Version != null)
                        writer.WriteString(VersionProperty, property.Version.ToString());

                    if (property.NumericValue.HasValue)
                        writer.WriteNumber(NumericValueEnumProperty, property.NumericValue.Value);

                    JsonHelper.WriteAdditionalProperties(writer, property, options);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                break;
            
            case CustomSpecType customType:
                if (!value.Parent.IsNull)
                    writer.WriteString(ParentProperty, value.Parent.Type);

                if (customType.StringParsableType != null)
                    writer.WriteString(StringParseableTypeProperty, customType.StringParsableType);

                if (customType.IsLegacyExpandedType)
                    writer.WriteBoolean(IsLegacyExpandedTypeProperty, true);

                JsonHelper.WriteAdditionalProperties(writer, value, options);

                writer.WritePropertyName(PropertiesProperty);
                writer.WriteStartArray();

                foreach (SpecProperty property in customType.Properties)
                {
                    if (!property.IsOverride)
                    {
                        SpecPropertyConverter.WriteProperty(writer, property, options);
                    }
                }

                writer.WriteEndArray();

                if (customType.LocalizationProperties.Length > 0)
                {
                    writer.WritePropertyName(LocalizationProperty);
                    writer.WriteStartArray();

                    foreach (SpecProperty property in customType.LocalizationProperties)
                    {
                        if (!property.IsOverride)
                        {
                            SpecPropertyConverter.WriteProperty(writer, property, options);
                        }
                    }

                    writer.WriteEndArray();
                }

                break;
        }
        writer.WriteEndObject();
    }


    private record struct EnumValueInfo(
        string Value,
        string? Casing,
        QualifiedType RequiredBaseType,
        QualifiedType CorrespondingType,
        string? Description,
        bool Deprecated,
        string? Docs,
        OneOrMore<KeyValuePair<string, object?>> Properties,
        Version? Version,
        long? NumericValue
    );
}