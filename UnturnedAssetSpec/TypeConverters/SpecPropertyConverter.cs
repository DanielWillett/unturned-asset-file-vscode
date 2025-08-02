using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecPropertyConverter : JsonConverter<SpecProperty?>
{
    private static readonly JsonEncodedText KeyProperty = JsonEncodedText.Encode("Key");
    private static readonly JsonEncodedText SingleKeyOverrideProperty = JsonEncodedText.Encode("SingleKeyOverride");
    private static readonly JsonEncodedText KeyIsRegexProperty = JsonEncodedText.Encode("KeyIsRegex");
    private static readonly JsonEncodedText KeyGroupsProperty = JsonEncodedText.Encode("KeyGroups");
    private static readonly JsonEncodedText KeyGroupsRegexGroupProperty = JsonEncodedText.Encode("RegexGroup");
    private static readonly JsonEncodedText KeyGroupsNameProperty = JsonEncodedText.Encode("Name");
    private static readonly JsonEncodedText FileCrossRefProperty = JsonEncodedText.Encode("FileCrossRef");
    private static readonly JsonEncodedText CountForRegexGroupProperty = JsonEncodedText.Encode("CountForRegexGroup");
    private static readonly JsonEncodedText ValueRegexGroupReferenceProperty = JsonEncodedText.Encode("ValueRegexGroupReference");
    private static readonly JsonEncodedText AliasesProperty = JsonEncodedText.Encode("Aliases");
    private static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("Type");
    private static readonly JsonEncodedText SubtypeSwitchProperty = JsonEncodedText.Encode("SubtypeSwitch");
    private static readonly JsonEncodedText ElementTypeProperty = JsonEncodedText.Encode("ElementType");
    private static readonly JsonEncodedText SpecialTypesProperty = JsonEncodedText.Encode("SpecialTypes");
    private static readonly JsonEncodedText RequiredProperty = JsonEncodedText.Encode("Required");
    private static readonly JsonEncodedText CanBeInMetadataProperty = JsonEncodedText.Encode("CanBeInMetadata");
    private static readonly JsonEncodedText DefaultValueProperty = JsonEncodedText.Encode("DefaultValue");
    private static readonly JsonEncodedText IncludedDefaultValueProperty = JsonEncodedText.Encode("IncludedDefaultValue");
    private static readonly JsonEncodedText DescriptionProperty = JsonEncodedText.Encode("Description");
    private static readonly JsonEncodedText VariableProperty = JsonEncodedText.Encode("Variable");
    private static readonly JsonEncodedText DocsProperty = JsonEncodedText.Encode("Docs");
    private static readonly JsonEncodedText MarkdownProperty = JsonEncodedText.Encode("Markdown");
    private static readonly JsonEncodedText MinimumProperty = JsonEncodedText.Encode("Minimum");
    private static readonly JsonEncodedText MaximumProperty = JsonEncodedText.Encode("Maximum");
    private static readonly JsonEncodedText MinimumExclusiveProperty = JsonEncodedText.Encode("MinimumExclusive");
    private static readonly JsonEncodedText MaximumExclusiveProperty = JsonEncodedText.Encode("MaximumExclusive");
    private static readonly JsonEncodedText ExceptProperty = JsonEncodedText.Encode("Except");
    private static readonly JsonEncodedText ExclusiveWithProperty = JsonEncodedText.Encode("ExclusiveWith");
    private static readonly JsonEncodedText InclusiveWithProperty = JsonEncodedText.Encode("InclusiveWith");
    private static readonly JsonEncodedText DeprecatedProperty = JsonEncodedText.Encode("Deprecated");
    private static readonly JsonEncodedText PriorityProperty = JsonEncodedText.Encode("Priority");
    private static readonly JsonEncodedText SimilarGroupingProperty = JsonEncodedText.Encode("SimilarGrouping");

    private static readonly JsonEncodedText HideInheritedProperty = JsonEncodedText.Encode("HideInherited");

    private static readonly JsonEncodedText[] Properties =
    [
        KeyProperty,                        // 0
        SingleKeyOverrideProperty,          // 1
        KeyIsRegexProperty,                 // 2
        KeyGroupsProperty,                  // 3
        FileCrossRefProperty,               // 4
        CountForRegexGroupProperty,         // 5
        ValueRegexGroupReferenceProperty,   // 6
        AliasesProperty,                    // 7
        TypeProperty,                       // 8
        SubtypeSwitchProperty,              // 9
        ElementTypeProperty,                // 10
        SpecialTypesProperty,               // 11
        RequiredProperty,                   // 12
        CanBeInMetadataProperty,            // 13
        DefaultValueProperty,               // 14
        IncludedDefaultValueProperty,       // 15
        DescriptionProperty,                // 16
        VariableProperty,                   // 17
        DocsProperty,                       // 18
        MarkdownProperty,                   // 19
        MinimumProperty,                    // 20
        MaximumProperty,                    // 21
        MinimumExclusiveProperty,           // 22
        MaximumExclusiveProperty,           // 23
        ExceptProperty,                     // 24
        ExclusiveWithProperty,              // 25
        InclusiveWithProperty,              // 26
        DeprecatedProperty,                 // 27
        HideInheritedProperty,              // 28
        PriorityProperty,                   // 29
        SimilarGroupingProperty             // 30
    ];

    /// <inheritdoc />
    public override SpecProperty? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadProperty(ref reader, options);
    }
    public static SpecProperty? ReadProperty(ref Utf8JsonReader reader, JsonSerializerOptions? options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new SpecProperty
                {
                    Key = reader.GetString(),
                    Type = new PropertyTypeOrSwitch(KnownTypes.String)
                };
            }

            throw new JsonException($"Unexpected token {reader.TokenType} reading SpecProperty.");
        }

        bool isHidingInherited = false;
        string? typeStr = null, elementTypeStr = null;
        OneOrMore<string> specialTypes = OneOrMore<string>.Null;
        SpecProperty property = new SpecProperty { Key = null!, Type = default };

        Utf8JsonReader
            defaultValueReader = default,
            includedDefaultValueReader = defaultValueReader,
            minimumValue = defaultValueReader,
            maximumValue = defaultValueReader,
            exceptValue = defaultValueReader,
            typeSwitch = defaultValueReader;

        bool minimumIsExclusive = false,
            maximumIsExclusive = false;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                if (reader.TokenType == JsonTokenType.Comment)
                    continue;

                throw new JsonException($"Unexpected token {reader.TokenType} reading SpecProperty.");
            }

            int propType = -1;
            for (int i = 0; i < Properties.Length; ++i)
            {
                ref JsonEncodedText t = ref Properties[i];
                if (!reader.ValueTextEquals(t.EncodedUtf8Bytes))
                    continue;

                propType = i;
                break;
            }

            if (!reader.Read() || reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray or JsonTokenType.PropertyName or JsonTokenType.Comment)
            {
                if (propType != -1)
                    throw new JsonException($"Failed to read SpecProperty property {Properties[propType].ToString()}.");

                reader.Skip();
                continue;
            }
            
            switch (propType)
            {
                case 0: // Key
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Key = reader.GetString();
                    break;

                case 1: // SingleKeyOverride
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.SingleKeyOverride = reader.GetString();
                    break;

                case 2: // KeyIsRegex
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.KeyIsRegex = reader.TokenType == JsonTokenType.True;
                    break;

                case 3: // KeyGroups
                    ReadKeyGroups(ref reader, property);
                    break;

                case 4: // FileCrossRef
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.FileCrossRef = reader.GetString();
                    break;

                case 5: // CountForRegexGroup
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.CountForRegexGroup = reader.GetString();
                    break;

                case 6: // ValueRegexGroupReference
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.ValueRegexGroupReference = reader.GetString();
                    break;

                case 7: // Aliases
                    property.Aliases = ReadStringArray(ref reader, in AliasesProperty, true);
                    break;

                case 8: // Type
                    if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.StartObject)
                    {
                        typeSwitch = reader;
                        break;
                    }

                    typeSwitch = default;
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);

                    typeStr = reader.GetString();
                    break;
                
                case 9: // SubtypeSwitch
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.SubtypeSwitch = reader.GetString();
                    break;

                case 10: // ElementType
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    elementTypeStr = reader.GetString();
                    break;

                case 11: // SpecialTypes
                    specialTypes = ReadStringArray(ref reader, in SpecialTypesProperty, true);
                    break;

                case 12: // Required
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.True:
                            property.RequiredCondition = SpecDynamicValue.True;
                            break;

                        case JsonTokenType.False:
                            property.RequiredCondition = SpecDynamicValue.False;
                            break;

                        case JsonTokenType.String:
                            try
                            {
                                property.RequiredCondition = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty, expectedType: KnownTypes.Boolean);
                            }
                            catch (Exception ex)
                            {
                                throw new JsonException($"Failed to read property \"{RequiredProperty.ToString()}\" while reading SpecProperty.", ex);
                            }
                            break;

                        case JsonTokenType.StartArray:
                        case JsonTokenType.StartObject:
                            try
                            {
                                property.RequiredCondition = SpecDynamicValue.Read(ref reader, options, expectedType: KnownTypes.Boolean);
                            }
                            catch (Exception ex)
                            {
                                throw new JsonException($"Failed to read property \"{RequiredProperty.ToString()}\" while reading SpecProperty.", ex);
                            }
                            break;

                        default:
                            ThrowUnexpectedToken(reader.TokenType, propType);
                            break;
                    }

                    break;

                case 13: // CanBeInMetadata
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.CanBeInMetadata = reader.TokenType == JsonTokenType.True;
                    break;

                case 14: // DefaultValue
                    defaultValueReader = reader;
                    reader.Skip();
                    break;

                case 15: // IncludedDefaultValue
                    includedDefaultValueReader = reader;
                    reader.Skip();
                    break;

                case 16: // Description
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Description = reader.GetString();
                    break;

                case 17: // Variable
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Variable = reader.GetString();
                    break;

                case 18: // Docs
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Docs = reader.GetString();
                    break;

                case 19: // Markdown
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Markdown = reader.GetString();
                    break;

                case 20: // Minimum
                    minimumValue = reader.TokenType == JsonTokenType.Null ? default : reader;
                    minimumIsExclusive = false;
                    reader.Skip();
                    break;

                case 21: // Maximum
                    maximumValue = reader.TokenType == JsonTokenType.Null ? default : reader;
                    maximumIsExclusive = false;
                    reader.Skip();
                    break;

                case 22: // MinimumExclusive
                    minimumValue = reader.TokenType == JsonTokenType.Null ? default : reader;
                    minimumIsExclusive = true;
                    reader.Skip();
                    break;

                case 23: // MaximumExclusive
                    maximumValue = reader.TokenType == JsonTokenType.Null ? default : reader;
                    maximumIsExclusive = true;
                    reader.Skip();
                    break;

                case 24: // Except
                    exceptValue = reader;
                    reader.Skip();
                    break;

                case 25: // ExclusiveWith
                    property.ExclusiveProperties = InclusionConditionConverter.ReadCondition(ref reader, options);
                    break;

                case 26: // InclusiveWith
                    property.InclusiveProperties = InclusionConditionConverter.ReadCondition(ref reader, options);
                    break;

                case 27: // Deprecated
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Deprecated = reader.TokenType == JsonTokenType.True;
                    break;

                case 28: // HideInherited
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    isHidingInherited = reader.TokenType == JsonTokenType.True;
                    break;

                case 29: // Priority
                    if (reader.TokenType == JsonTokenType.Null)
                    {
                        property.Priority = 0;
                        break;
                    }

                    int priority = 0;
                    if (reader.TokenType is not JsonTokenType.Number || !reader.TryGetInt32(out priority))
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.Priority = priority;
                    break;

                case 30: // SimilarGrouping
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.SimilarGrouping = reader.GetString();
                    break;

                default:
                    reader.Skip();
                    break;

            }
        }

        if (typeStr == null && typeSwitch.TokenType == JsonTokenType.None)
        {
            if (isHidingInherited)
            {
                property.Type = new PropertyTypeOrSwitch(HideInheritedPropertyType.Instance);
                return property;
            }

            throw new JsonException($"Missing {TypeProperty.ToString()} property while reading SpecProperty.");
        }

        PropertyTypeOrSwitch propertyType;
        if (typeSwitch.TokenType != JsonTokenType.None)
        {
            propertyType = new PropertyTypeOrSwitch(ReadTypeSwitch(ref typeSwitch, options, in TypeProperty));
        }
        else
        {
            ISpecPropertyType? pt = KnownTypes.GetType(typeStr!, elementTypeStr, specialTypes);
            pt ??= new UnresolvedSpecPropertyType(typeStr!);

            propertyType = new PropertyTypeOrSwitch(pt);
        }

        property.Type = propertyType;

        if (defaultValueReader.TokenType != JsonTokenType.None)
        {
            property.DefaultValue = ReadValue(propertyType, ref defaultValueReader, SpecDynamicValueContext.AllowSwitch, options, in DefaultValueProperty);
        }

        if (includedDefaultValueReader.TokenType != JsonTokenType.None)
        {
            property.IncludedDefaultValue = ReadValue(propertyType, ref includedDefaultValueReader, SpecDynamicValueContext.AllowSwitch, options, in IncludedDefaultValueProperty);
        }

        if (minimumValue.TokenType != JsonTokenType.None)
        {
            property.MinimumValue = ReadValue(
                propertyType,
                ref minimumValue,
                SpecDynamicValueContext.AllowSwitch,
                options,
                minimumIsExclusive ? MinimumExclusiveProperty : MinimumProperty
            );
            property.ExceptionsAreWhitelist = true;
        }

        if (maximumValue.TokenType != JsonTokenType.None)
        {
            property.MaximumValue = ReadValue(
                propertyType,
                ref maximumValue,
                SpecDynamicValueContext.AllowSwitch,
                options,
                maximumIsExclusive ? MaximumExclusiveProperty : MaximumProperty
            );
            property.ExceptionsAreWhitelist = true;
        }

        if (exceptValue.TokenType != JsonTokenType.None)
        {
            if (exceptValue.TokenType != JsonTokenType.StartArray)
            {
                property.Exceptions = new OneOrMore<ISpecDynamicValue>(
                    ReadValue(
                        propertyType,
                        ref exceptValue,
                        SpecDynamicValueContext.AllowSwitch,
                        options,
                        in ExceptProperty
                    )
                );
            }
            else
            {
                Utf8JsonReader rTemp = exceptValue;
                if (rTemp.Read())
                {
                    switch (rTemp.TokenType)
                    {
                        case JsonTokenType.EndArray:
                            property.Exceptions = OneOrMore<ISpecDynamicValue>.Null;
                            break;

                        case JsonTokenType.StartObject:
                            property.Exceptions = new OneOrMore<ISpecDynamicValue>(ReadValue(
                                propertyType,
                                ref exceptValue,
                                SpecDynamicValueContext.AllowSwitch,
                                options,
                                in ExceptProperty
                            ));
                            break;

                        default:
                            property.Exceptions = ReadExceptionList(propertyType, ref rTemp, options);
                            break;
                    }
                }
            }
        }
        else
        {
            property.Exceptions = OneOrMore<ISpecDynamicValue>.Null;
        }

        return property;
    }

    private static OneOrMore<ISpecDynamicValue> ReadExceptionList(
        PropertyTypeOrSwitch propertyType,
        ref Utf8JsonReader reader,
        JsonSerializerOptions? options)
    {
        OneOrMore<ISpecDynamicValue> list = OneOrMore<ISpecDynamicValue>.Null;

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            if (!reader.Read() || reader.TokenType == JsonTokenType.EndArray)
                return list;
        }
            
        do
        {
            list = list.Add(ReadValue(
                propertyType,
                ref reader,
                SpecDynamicValueContext.AllowSwitch,
                options,
                in ExceptProperty)
            );
        }
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray);

        return list;
    }

    private static SpecDynamicSwitchValue ReadTypeSwitch(
        ref Utf8JsonReader reader,
        JsonSerializerOptions? options,
        in JsonEncodedText property)
    {
        try
        {
            SpecDynamicSwitchValue? vals = SpecDynamicSwitchValueConverter.ReadSwitch(ref reader, options, new PropertyTypeOrSwitch(SpecPropertyTypeType.Instance));
            return vals ?? throw new JsonException($"Failed to read property \"{property.ToString()}\" while reading SpecProperty, null value.");
        }
        catch (Exception ex)
        {
            throw new JsonException($"Failed to read property \"{property.ToString()}\" while reading SpecProperty.", ex);
        }
    }

    private static ISpecDynamicValue ReadValue(
        PropertyTypeOrSwitch propertyType,
        ref Utf8JsonReader reader,
        SpecDynamicValueContext context,
        JsonSerializerOptions? options,
        in JsonEncodedText property)
    {
        if (propertyType is { IsSwitch: false, Type: UnresolvedSpecPropertyType unresolvedSpecPropertyType })
        {
            return new UnresolvedDynamicValue(
                unresolvedSpecPropertyType,
                JsonDocument.ParseValue(ref reader),
                options,
                $"SpecProperty.\"{property.ToString()}\"",
                context
            );
        }

        try
        {
            return SpecDynamicValue.Read(ref reader, options, context, propertyType);
        }
        catch (Exception ex)
        {
            throw new JsonException($"Failed to read property \"{property.ToString()}\" while reading SpecProperty.", ex);
        }
    }

    private static void ReadKeyGroups(ref Utf8JsonReader reader, SpecProperty property)
    {
        OneOrMore<RegexKeyGroup> array = OneOrMore<RegexKeyGroup>.Null;
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                property.KeyGroups = array;
                return;
            }

            ThrowUnexpectedToken(reader.TokenType, KeyGroupsProperty.ToString());
        }

        int index = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                ThrowUnexpectedToken(reader.TokenType, KeyGroupsProperty.ToString());

            string? name = null;
            int group = -1;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    ThrowUnexpectedToken(reader.TokenType, KeyGroupsProperty.ToString());

                int prop = -1;
                if (reader.ValueTextEquals(KeyGroupsRegexGroupProperty.EncodedUtf8Bytes))
                {
                    prop = 0;
                }
                else if (reader.ValueTextEquals(KeyGroupsNameProperty.EncodedUtf8Bytes))
                {
                    prop = 1;
                }

                if (!reader.Read())
                {
                    if (prop != -1)
                        throw new JsonException($"Failed to read SpecProperty.KeyGroups[{index}] property {(prop == 0 ? KeyGroupsRegexGroupProperty : KeyGroupsNameProperty).ToString()}.");

                    reader.Skip();
                    continue;
                }

                if (prop == 0)
                {
                    if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int groupNum) || groupNum < 0)
                    {
                        throw new JsonException($"Failed to read SpecProperty.KeyGroups[{index}] property {KeyGroupsRegexGroupProperty.ToString()}, expected an integer value.");
                    }

                    group = groupNum;
                }
                else
                {
                    if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { Length: > 0 } nameStr)
                    {
                        throw new JsonException($"Failed to read SpecProperty.KeyGroups[{index}] property {KeyGroupsNameProperty.ToString()}, expected a string value.");
                    }

                    name = nameStr;
                }
            }

            if (name == null && group == -1)
                throw new JsonException($"Failed to read SpecProperty.KeyGroups[{index}], missing \"{KeyGroupsNameProperty.ToString()}\", \"{KeyGroupsRegexGroupProperty.ToString()}\".");
            if (name == null)
                throw new JsonException($"Failed to read SpecProperty.KeyGroups[{index}], missing \"{KeyGroupsNameProperty.ToString()}\".");
            if (group == -1)
                throw new JsonException($"Failed to read SpecProperty.KeyGroups[{index}], missing \"{KeyGroupsRegexGroupProperty.ToString()}\".");
            
            array = array.Add(new RegexKeyGroup(group, name));
            ++index;
        }

        property.KeyGroups = array;
    }

    private static OneOrMore<string> ReadStringArray(ref Utf8JsonReader reader, in JsonEncodedText text, bool allowOne)
    {
        if (allowOne && reader.TokenType == JsonTokenType.String)
        {
            string str = reader.GetString();
            return string.IsNullOrEmpty(str) ? OneOrMore<string>.Null : str;
        }

        if (reader.TokenType == JsonTokenType.Null)
            return OneOrMore<string>.Null;

        if (reader.TokenType != JsonTokenType.StartArray)
            ThrowUnexpectedToken(reader.TokenType, text.ToString());

        OneOrMore<string> array = OneOrMore<string>.Null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.String)
                ThrowUnexpectedToken(reader.TokenType, $"{text.ToString()}[{array.Length - 1}]");

            string? alias = reader.GetString();
            if (string.IsNullOrEmpty(alias))
                continue;

            array = array.Add(alias);
        }

        return array;
    }

    private static void ThrowUnexpectedToken(JsonTokenType tokenType, int propType)
    {
        ThrowUnexpectedToken(tokenType, Properties[propType].ToString());
    }
    private static void ThrowUnexpectedToken(JsonTokenType tokenType, string property)
    {
        throw new JsonException($"Unexpected token {tokenType} reading SpecProperty.\"{property}\".");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecProperty? value, JsonSerializerOptions options)
    {
        WriteProperty(writer, value, options);
    }

    public static void WriteProperty(Utf8JsonWriter writer, SpecProperty? property, JsonSerializerOptions options)
    {
        if (property == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (property.IsHidden)
        {
            writer.WriteStartObject();

            writer.WriteString(KeyProperty, property.Key);
            writer.WriteBoolean(HideInheritedProperty, true);

            writer.WriteEndObject();
            return;
        }

        writer.WriteStartObject();

        writer.WriteString(KeyProperty, property.Key);

        ISpecPropertyType? propType = property.Type.Type;
        if (propType != null)
        {
            writer.WriteString(TypeProperty, propType.Type);
            if (propType is IElementTypeSpecPropertyType { ElementType: { Length: > 0 } elementType })
            {
                writer.WriteString(ElementTypeProperty, elementType);
            }
            if (propType is ISpecialTypesSpecPropertyType specialTypes)
            {
                bool hasHeader = false;
                foreach (string? type in specialTypes.SpecialTypes)
                {
                    if (string.IsNullOrEmpty(type))
                        continue;

                    if (!hasHeader)
                    {
                        writer.WriteStartArray(SpecialTypesProperty);
                        hasHeader = true;
                    }

                    writer.WriteStringValue(type);
                }

                if (hasHeader)
                    writer.WriteEndArray();
            }
        }
        else if (property.Type.TypeSwitch != null)
        {
            writer.WritePropertyName(TypeProperty);
            property.Type.TypeSwitch.WriteToJsonWriter(writer, options);
        }

        if (property.Description != null)
        {
            writer.WriteString(DescriptionProperty, property.Description);
        }
        
        if (property.Markdown != null)
        {
            writer.WriteString(MarkdownProperty, property.Markdown);
        }
        
        if (property.Docs != null)
        {
            writer.WriteString(DocsProperty, property.Docs);
        }
        
        if (property.RequiredCondition != null)
        {
            writer.WritePropertyName(RequiredProperty);
            property.RequiredCondition.WriteToJsonWriter(writer, options);
        }

        if (property.CanBeInMetadata)
        {
            writer.WriteBoolean(CanBeInMetadataProperty, true);
        }

        if (property.DefaultValue != null)
        {
            writer.WritePropertyName(DefaultValueProperty);
            property.DefaultValue.WriteToJsonWriter(writer, options);
        }
        if (property.IncludedDefaultValue != null)
        {
            writer.WritePropertyName(IncludedDefaultValueProperty);
            property.IncludedDefaultValue.WriteToJsonWriter(writer, options);
        }

        if (property.SingleKeyOverride != null)
        {
            writer.WriteString(SingleKeyOverrideProperty, property.SingleKeyOverride);
        }

        if (property.KeyIsRegex)
        {
            writer.WriteBoolean(KeyIsRegexProperty, true);

            writer.WriteStartArray(KeyGroupsProperty);

            foreach (RegexKeyGroup grp in property.KeyGroups)
            {
                writer.WriteStartObject();

                writer.WriteString(KeyGroupsNameProperty, grp.Name);
                writer.WriteNumber(KeyGroupsRegexGroupProperty, grp.Group);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
        
        if (property.Priority != 0)
        {
            writer.WriteNumber(PriorityProperty, property.Priority);
        }

        if (property.SimilarGrouping != null)
        {
            writer.WriteString(SimilarGroupingProperty, property.SimilarGrouping);
        }

        if (property.Deprecated)
        {
            writer.WriteBoolean(DeprecatedProperty, true);
        }

        if (property.FileCrossRef != null)
        {
            writer.WriteString(FileCrossRefProperty, property.FileCrossRef);
        }

        if (property.CountForRegexGroup != null)
        {
            writer.WriteString(CountForRegexGroupProperty, property.CountForRegexGroup);
        }

        if (property.ValueRegexGroupReference != null)
        {
            writer.WriteString(ValueRegexGroupReferenceProperty, property.ValueRegexGroupReference);
        }

        if (!property.Aliases.IsNull)
        {
            writer.WriteStartArray(AliasesProperty);
            foreach (string alias in property.Aliases)
            {
                writer.WriteStringValue(alias);
            }
            writer.WriteEndArray();
        }

        if (property.SubtypeSwitch != null)
        {
            writer.WriteString(SubtypeSwitchProperty, property.SubtypeSwitch);
        }

        if (property.ExceptionsAreWhitelist)
        {
            if (property.MinimumValue != null)
            {
                writer.WritePropertyName(property.IsMinimumValueExclusive ? MinimumExclusiveProperty : MinimumProperty);
                property.MinimumValue.WriteToJsonWriter(writer, options);
            }
            if (property.MaximumValue != null)
            {
                writer.WritePropertyName(property.IsMaximumValueExclusive ? MaximumExclusiveProperty : MaximumProperty);
                property.MaximumValue.WriteToJsonWriter(writer, options);
            }
        }

        if (!property.Exceptions.IsNull)
        {
            writer.WriteStartArray(ExceptProperty);

            foreach (ISpecDynamicValue exception in property.Exceptions)
            {
                exception.WriteToJsonWriter(writer, options);
            }

            writer.WriteEndArray();
        }

        if (property.InclusiveProperties != null)
        {
            writer.WritePropertyName(InclusiveWithProperty);
            InclusionConditionConverter.WriteCondition(writer, property.InclusiveProperties, options);
        }
        if (property.ExclusiveProperties != null)
        {
            writer.WritePropertyName(ExclusiveWithProperty);
            InclusionConditionConverter.WriteCondition(writer, property.ExclusiveProperties, options);
        }

        if (property.Variable != null)
        {
            writer.WriteString(VariableProperty, property.Variable);
        }

        writer.WriteEndObject();
    }
}

internal sealed class SpecPropertyTypeType : ISpecPropertyType<ISpecPropertyType>, IStringParseableSpecPropertyType
{
    public static SpecPropertyTypeType Instance { get; } = new SpecPropertyTypeType();
    static SpecPropertyTypeType() { }
    private SpecPropertyTypeType() { }

    /// <inheritdoc />
    public string Type => "Type";

    /// <inheritdoc />
    public Type ValueType => typeof(ISpecPropertyType);

    /// <inheritdoc />
    public string DisplayName => "Type";

    /// <inheritdoc />
    public SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Class;

    /// <inheritdoc />
    public ISpecPropertyType<TValue>? As<TValue>() where TValue : IEquatable<TValue> => this as ISpecPropertyType<TValue>;

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecDynamicValue value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType? other) => other is SpecPropertyTypeType;

    /// <inheritdoc />
    public bool Equals(ISpecPropertyType<ISpecPropertyType>? other) => other is SpecPropertyTypeType;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is SpecPropertyTypeType;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <inheritdoc />
    public bool TryParseValue(in SpecPropertyTypeParseContext parse, out ISpecPropertyType? value)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        stringValue ??= span.ToString();

        ISpecPropertyType? t = KnownTypes.GetType(stringValue);
        if (t != null)
        {
            dynamicValue = new SpecDynamicConcreteValue<ISpecPropertyType>(t, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }
}