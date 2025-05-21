using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecPropertyConverter : JsonConverter<SpecProperty?>
{
    private static readonly JsonEncodedText KeyProperty = JsonEncodedText.Encode("Key");

    private static readonly JsonEncodedText HideInheritedProperty = JsonEncodedText.Encode("HideInherited");

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
        HideInheritedProperty               // 28
    ];


    public SpecPropertyContext Context { get; }

    public SpecPropertyConverter() : this(SpecPropertyContext.Unspecified) { }
    public SpecPropertyConverter(SpecPropertyContext context)
    {
        if (context is not SpecPropertyContext.Property and not SpecPropertyContext.Localization and not SpecPropertyContext.Unspecified)
            throw new ArgumentOutOfRangeException(nameof(context));

        Context = context;
    }

    /// <inheritdoc />
    public override SpecProperty? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
                    Type = KnownTypes.String
                };
            }

            throw new JsonException($"Unexpected token {reader.TokenType} reading SpecProperty.");
        }

        bool isHidingInherited = false;
        string? typeStr = null, elementTypeStr = null;
        OneOrMore<string> specialTypes = OneOrMore<string>.Null;
        SpecProperty property = new SpecProperty { Key = null!, Type = null! };

        Utf8JsonReader defaultValueReader = default,
            includedDefaultValueReader = default;

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
                            property.RequiredCondition = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty, expectedType: KnownTypes.Boolean);
                            break;

                        case JsonTokenType.StartObject:
                            property.RequiredCondition = SpecDynamicValue.Read(ref reader, options, expectedType: KnownTypes.Boolean);
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

                default:
                    reader.Skip();
                    break;

            }
        }

        if (typeStr == null)
        {
            // todo hide child exception
            throw new JsonException($"Missing {TypeProperty.ToString()} property while reading SpecProperty.");
        }

        ISpecPropertyType? propertyType = KnownTypes.GetType(typeStr, property, elementTypeStr, specialTypes);
        propertyType ??= new UnresolvedSpecPropertyType(typeStr);

        if (defaultValueReader.TokenType != JsonTokenType.None && propertyType is not UnresolvedSpecPropertyType)
        {
            property.DefaultValue = ReadDefaultValue(ref defaultValueReader, options, propertyType);
        }

        if (includedDefaultValueReader.TokenType != JsonTokenType.None && propertyType is not UnresolvedSpecPropertyType)
        {
            property.IncludedDefaultValue = ReadDefaultValue(ref defaultValueReader, options, propertyType);
        }

        return null;
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

    private static ISpecDynamicValue ReadDefaultValue(
        ref Utf8JsonReader reader, JsonSerializerOptions? options, ISpecPropertyType? expectedPropertyType
    )
    {
        return SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowSwitch, expectedPropertyType);
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

    }
}