using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

public class SpecPropertyConverter : JsonConverter<SpecProperty?>
{
    private static readonly JsonEncodedText KeyProperty = JsonEncodedText.Encode("Key");
    private static readonly JsonEncodedText SingleKeyOverrideProperty = JsonEncodedText.Encode("SingleKeyOverride");
    private static readonly JsonEncodedText TemplateProperty = JsonEncodedText.Encode("Template");
    private static readonly JsonEncodedText TemplateGroupsProperty = JsonEncodedText.Encode("TemplateGroups");
    private static readonly JsonEncodedText TemplateGroupsNameProperty = JsonEncodedText.Encode("Name");
    private static readonly JsonEncodedText TemplateGroupsUseValueOfProperty = JsonEncodedText.Encode("UseValueOf");
    private static readonly JsonEncodedText FileCrossRefProperty = JsonEncodedText.Encode("FileCrossRef");
    private static readonly JsonEncodedText CountForTemplateGroupProperty = JsonEncodedText.Encode("CountForTemplateGroup");
    private static readonly JsonEncodedText ValueTemplateGroupReferenceProperty = JsonEncodedText.Encode("ValueTemplateGroupReference");
    private static readonly JsonEncodedText AliasesProperty = JsonEncodedText.Encode("Aliases");
    private static readonly JsonEncodedText AliasesAliasProperty = JsonEncodedText.Encode("Alias");
    private static readonly JsonEncodedText AliasesLegacyExpansionFilterProperty = JsonEncodedText.Encode("LegacyExpansionFilter");
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
    private static readonly JsonEncodedText ExperimentalProperty = JsonEncodedText.Encode("Experimental");
    private static readonly JsonEncodedText ListReferenceProperty = JsonEncodedText.Encode("ListReference");
    private static readonly JsonEncodedText TemplateGroupUniqueValueProperty = JsonEncodedText.Encode("TemplateGroupUniqueValue");
    private static readonly JsonEncodedText KeyLegacyExpansionFilterProperty = JsonEncodedText.Encode("KeyLegacyExpansionFilter");

    private static readonly JsonEncodedText HideInheritedProperty = JsonEncodedText.Encode("HideInherited");

    private static readonly JsonEncodedText[] Properties =
    [
        KeyProperty,                        // 0
        SingleKeyOverrideProperty,          // 1
        TemplateProperty,                   // 2
        TemplateGroupsProperty,             // 3
        FileCrossRefProperty,               // 4
        CountForTemplateGroupProperty,      // 5
        ValueTemplateGroupReferenceProperty,// 6
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
        ExperimentalProperty,               // 30
        ListReferenceProperty,              // 31
        TemplateGroupUniqueValueProperty,   // 32
        KeyLegacyExpansionFilterProperty    // 33
    ];

    /// <inheritdoc />
    public override SpecProperty? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return ReadProperty(ref reader, options);
    }

    [SkipLocalsInit]
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

        OneOrMore<KeyValuePair<string, object?>> extraData = OneOrMore<KeyValuePair<string, object?>>.Null;

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

            string? key = null;
            if (propType == -1)
            {
                key = reader.GetString();
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
                case -1:
                    // extra properties
                    if (!JsonHelper.ShouldSkipAdditionalProperty(key) && JsonHelper.TryReadGenericValue(ref reader, out object? extraValue))
                    {
                        extraData = extraData.Add(new KeyValuePair<string, object?>(key, extraValue));
                    }
                    break;

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

                case 2: // Template
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.IsTemplate = reader.TokenType == JsonTokenType.True;
                    break;

                case 3: // TemplateGroups
                    ReadTemplateGroups(ref reader, property);
                    break;

                case 4: // FileCrossRef
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.FileCrossRef = reader.GetString();
                    break;

                case 5: // CountForTemplateGroup
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.CountForTemplateGroup = reader.GetString();
                    break;

                case 6: // ValueTemplateGroupReference
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    if (!SpecDynamicValue.TryParse(reader.GetString(), SpecDynamicValueContext.AssumeDataRef, null, out ISpecDynamicValue value)
                        || value is not TemplateGroupsDataRef dr
                        || dr.Index == -1)
                    {
                        throw new JsonException($"Unable to parse dataref when reading SpecProperty.\"{Properties[propType].ToString()}\".");
                    }

                    property.ValueTemplateGroupReference = dr;
                    break;

                case 7: // Aliases
                    property.Aliases = ReadAliases(ref reader);
                    break;

                case 8: // Type
                    if (reader.TokenType is JsonTokenType.StartArray or JsonTokenType.StartObject)
                    {
                        typeSwitch = reader;
                        reader.Skip();
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
                    property.Description = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowSwitch, KnownTypes.String);
                    break;

                case 17: // Variable
                    property.Variable = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowSwitch, KnownTypes.String);
                    break;

                case 18: // Docs
                    property.Docs = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowSwitch, KnownTypes.String);
                    break;

                case 19: // Markdown
                    property.Markdown = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowSwitch, KnownTypes.String);
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
                    property.ExclusiveProperties = InclusionConditionConverter.ReadCondition(ref reader, options, false);
                    break;

                case 26: // InclusiveWith
                    property.InclusiveProperties = InclusionConditionConverter.ReadCondition(ref reader, options, true);
                    break;

                case 27: // Deprecated
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.True:
                            property.Deprecated = SpecDynamicValue.True;
                            break;

                        case JsonTokenType.False:
                            property.Deprecated = SpecDynamicValue.False;
                            break;

                        case JsonTokenType.String:
                            property.Deprecated = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty, KnownTypes.Boolean);
                            break;

                        case JsonTokenType.StartObject:
                            property.Deprecated = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowCondition | SpecDynamicValueContext.AllowSwitchCase, KnownTypes.Boolean);
                            break;

                        default:
                            ThrowUnexpectedToken(reader.TokenType, propType);
                            break;
                    }
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

                case 30: // Experimental
                    switch (reader.TokenType)
                    {
                        case JsonTokenType.True:
                            property.Experimental = SpecDynamicValue.True;
                            break;

                        case JsonTokenType.False:
                            property.Experimental = SpecDynamicValue.False;
                            break;

                        case JsonTokenType.String:
                            property.Experimental = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AssumeProperty, KnownTypes.Boolean);
                            break;

                        case JsonTokenType.StartObject:
                            property.Experimental = SpecDynamicValue.Read(ref reader, options, SpecDynamicValueContext.AllowCondition | SpecDynamicValueContext.AllowSwitchCase, KnownTypes.Boolean);
                            break;

                        default:
                            ThrowUnexpectedToken(reader.TokenType, propType);
                            break;
                    }
                    break;

                case 31: // ListReference
                    if (reader.TokenType is not JsonTokenType.String and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.ListReference = reader.GetString();
                    break;

                case 32: // TemplateGroupUniqueValue
                    if (reader.TokenType is not JsonTokenType.True and not JsonTokenType.False and not JsonTokenType.Null)
                        ThrowUnexpectedToken(reader.TokenType, propType);
                    property.TemplateGroupUniqueValue = reader.TokenType == JsonTokenType.True;
                    break;

                case 33: // KeyLegacyExpansionFilter
                    property.KeyLegacyExpansionFilter = ReadLegacyExpansionFilter(ref reader, in KeyLegacyExpansionFilterProperty);
                    break;
            }
        }

        if (string.IsNullOrEmpty(property.Key) || !property.IsTemplate)
        {
            property.IsTemplate = false;
            property.TemplateGroupUniqueValue = false;
            property.TemplateGroups = OneOrMore<TemplateGroup>.Null;
        }
        else if (property.Key.Equals("#This.Key", StringComparison.OrdinalIgnoreCase))
        {
            property.KeyIsLegacySelfRef = true;
            property.Key = string.Empty;
        }
        else
        {
            property.CreateTemplateProcessors();
        }

        if (typeStr == null && typeSwitch.TokenType == JsonTokenType.None)
        {
            if (isHidingInherited)
            {
                property.Type = new PropertyTypeOrSwitch(HideInheritedPropertyType.Instance);
                return property;
            }

            throw new JsonException($"Missing \"{TypeProperty.ToString()}\" property while reading SpecProperty.");
        }

        if (property.Key == null)
        {
            throw new JsonException($"Missing \"{KeyProperty.ToString()}\" property while reading SpecProperty.");
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
                minimumIsExclusive ? MinimumExclusiveProperty : MinimumProperty,
                numericFallback: true
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
                maximumIsExclusive ? MaximumExclusiveProperty : MaximumProperty,
                numericFallback: true
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

        property.AdditionalProperties = extraData;

        return property;
    }

    private static LegacyExpansionFilter ReadLegacyExpansionFilter(ref Utf8JsonReader reader, in JsonEncodedText prop, Func<int, string>? getPropStr = null, int index = -1)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return LegacyExpansionFilter.Either;

        if (reader.TokenType != JsonTokenType.String)
            ThrowUnexpectedToken(reader.TokenType, prop.ToString());

        string str = reader.GetString();
        if (str.Equals(nameof(LegacyExpansionFilter.Legacy), StringComparison.OrdinalIgnoreCase))
            return LegacyExpansionFilter.Legacy;
        if (str.Equals(nameof(LegacyExpansionFilter.Modern), StringComparison.OrdinalIgnoreCase))
            return LegacyExpansionFilter.Modern;

        throw new JsonException($"Invalid LegacyExpansionFilter value in property {getPropStr?.Invoke(index) ?? prop.ToString()}.");
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
        in JsonEncodedText property,
        // minimums and maximums can be used to indicate min/max string length as well, which is what this is for
        bool numericFallback = false)
    {
        if (propertyType is { IsSwitch: false, Type: UnresolvedSpecPropertyType unresolvedSpecPropertyType })
        {
            if (numericFallback && reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int v))
            {
                return SpecDynamicValue.Int32(v);
            }

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
            if (numericFallback && reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int v))
            {
                return SpecDynamicValue.Int32(v);
            }

            throw new JsonException($"Failed to read property \"{property.ToString()}\" while reading SpecProperty.", ex);
        }
    }

    private static void ReadTemplateGroups(ref Utf8JsonReader reader, SpecProperty property)
    {
        OneOrMore<TemplateGroup> array = OneOrMore<TemplateGroup>.Null;
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                property.TemplateGroups = array;
                return;
            }

            ThrowUnexpectedToken(reader.TokenType, TemplateGroupsProperty.ToString());
        }

        int index = 0;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            string? name;
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.String:
                        name = reader.GetString();
                        array = array.Add(new TemplateGroup(array.Length + 1, name));
                        continue;

                    case JsonTokenType.Null:
                        continue;

                    default:
                        ThrowUnexpectedToken(reader.TokenType, TemplateGroupsProperty.ToString());
                        break;
                }
            }

            name = null;
            string? useValueOf = null;

            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    ThrowUnexpectedToken(reader.TokenType, TemplateGroupsProperty.ToString());

                int prop = -1;
                if (reader.ValueTextEquals(TemplateGroupsNameProperty.EncodedUtf8Bytes))
                {
                    prop = 0;
                }
                else if (reader.ValueTextEquals(TemplateGroupsUseValueOfProperty.EncodedUtf8Bytes))
                {
                    prop = 1;
                }

                if (!reader.Read())
                {
                    if (prop != -1)
                        throw new JsonException($"Failed to read SpecProperty.TemplateGroups[{index}] property {(prop == 0 ? TemplateGroupsNameProperty : TemplateGroupsUseValueOfProperty).ToString()}.");

                    reader.Skip();
                    continue;
                }

                switch (prop)
                {
                    case 0: // Name
                        if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { Length: > 0 } nameStr)
                        {
                            throw new JsonException($"Failed to read SpecProperty.TemplateGroups[{index}] property {TemplateGroupsNameProperty.ToString()}, expected a string value.");
                        }

                        name = nameStr;
                        break;

                    case 1: // UseValueOf
                        if (reader.TokenType != JsonTokenType.String || reader.GetString() is not { Length: > 0 } useValueOfStr)
                        {
                            throw new JsonException($"Failed to read SpecProperty.TemplateGroups[{index}] property {TemplateGroupsUseValueOfProperty.ToString()}, expected a string value.");
                        }

                        useValueOf = useValueOfStr;
                        break;
                }
            }

            if (name == null)
                throw new JsonException($"Failed to read SpecProperty.TemplateGroups[{index}], missing \"{TemplateGroupsNameProperty.ToString()}\".");
            
            array = array.Add(new TemplateGroup(array.Length + 1, name, useValueOf));
            ++index;
        }

        property.TemplateGroups = array;
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

    private static OneOrMore<Alias> ReadAliases(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                string str = reader.GetString();
                return string.IsNullOrEmpty(str) ? OneOrMore<Alias>.Null : new OneOrMore<Alias>(new Alias(str));

            case JsonTokenType.StartObject:
                return new OneOrMore<Alias>(ReadAliasObject(ref reader, 0));

            case JsonTokenType.Null:
                return OneOrMore<Alias>.Null;

            case JsonTokenType.StartArray: break;

            default:
                ThrowUnexpectedToken(reader.TokenType, AliasesProperty.ToString());
                break;
        }

        OneOrMore<Alias> array = OneOrMore<Alias>.Null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                Alias a = ReadAliasObject(ref reader, array.Length);
                if (string.IsNullOrEmpty(a.Value))
                    continue;

                array = array.Add(a);
                continue;
            }

            if (reader.TokenType != JsonTokenType.String)
                ThrowUnexpectedToken(reader.TokenType, $"{AliasesProperty.ToString()}[{array.Length}]");

            string? alias = reader.GetString();
            if (string.IsNullOrEmpty(alias))
                continue;

            array = array.Add(new Alias(alias));
        }

        return array;

        static Alias ReadAliasObject(ref Utf8JsonReader reader, int index)
        {
            string? alias = null;
            LegacyExpansionFilter filter = LegacyExpansionFilter.Either;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType != JsonTokenType.PropertyName)
                    ThrowUnexpectedToken(JsonTokenType.PropertyName, AliasesProperty + "[" + index + "]");

                reader.Read();

                if (reader.ValueTextEquals(AliasesAliasProperty.EncodedUtf8Bytes))
                {
                    if (reader.TokenType is not JsonTokenType.String)
                        ThrowUnexpectedToken(reader.TokenType, AliasesProperty + "[" + index + "]." + AliasesAliasProperty);

                    alias = reader.GetString();
                }
                else if (reader.ValueTextEquals(AliasesLegacyExpansionFilterProperty.EncodedUtf8Bytes))
                {
                    filter = ReadLegacyExpansionFilter(ref reader, in AliasesProperty, static index => AliasesProperty + "[" + index + "]." + AliasesLegacyExpansionFilterProperty, index);
                }
                else
                {
                    reader.Skip();
                }
            }

            return new Alias(alias ?? string.Empty, filter);
        }
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

        if (ReferenceEquals(property.Type.Type, HideInheritedPropertyType.Instance))
        {
            writer.WriteStartObject();

            writer.WriteString(KeyProperty, property.Key);
            writer.WriteBoolean(HideInheritedProperty, true);

            writer.WriteEndObject();
            return;
        }

        writer.WriteStartObject();

        string key = property.Key;
        if (property.KeyIsLegacySelfRef)
            key = "#This.Key";
        else if (property.IsTemplate)
            key = TemplateProcessor.EscapeKey(key, property.KeyTemplateProcessor);

        writer.WriteString(KeyProperty, key);

        if (property.KeyLegacyExpansionFilter is LegacyExpansionFilter.Legacy or LegacyExpansionFilter.Modern)
        {
            WriteLegacyExpansionFilter(writer, KeyLegacyExpansionFilterProperty, property.KeyLegacyExpansionFilter);
        }

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
            writer.WritePropertyName(DescriptionProperty);
            property.Description.WriteToJsonWriter(writer, options);
        }
        
        if (property.Markdown != null)
        {
            writer.WritePropertyName(MarkdownProperty);
            property.Markdown.WriteToJsonWriter(writer, options);
        }
        
        if (property.Docs != null)
        {
            writer.WritePropertyName(DocsProperty);
            property.Docs.WriteToJsonWriter(writer, options);
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

        if (property.IsTemplate)
        {
            writer.WriteBoolean(TemplateProperty, true);

            if (property.TemplateGroupUniqueValue)
                writer.WriteBoolean(TemplateGroupUniqueValueProperty, true);

            writer.WriteStartArray(TemplateGroupsProperty);

            OneOrMore<TemplateGroup> templateGroups = property.TemplateGroups;

            int last = 0;
            bool needsSorted = false;
            foreach (TemplateGroup grp in templateGroups)
            {
                if (grp.Group <= last)
                {
                    needsSorted = true;
                    break;
                }
            }

            if (needsSorted)
            {
                TemplateGroup[] group = templateGroups.ToArray();
                Array.Sort(group, (g1, g2) => g1.Group.CompareTo(g2.Group));
                templateGroups = new OneOrMore<TemplateGroup>(group);
            }

            last = 0;
            int index = 0;
            foreach (TemplateGroup grp in templateGroups)
            {
                if (grp.Group == last)
                    continue;

                last = grp.Group;

                ++index;
                while (index < last)
                {
                    ++index;
                    writer.WriteNullValue();
                }

                if (string.IsNullOrEmpty(grp.UseValueOf))
                {
                    writer.WriteStringValue(grp.Name);
                    continue;
                }

                writer.WriteStartObject();

                writer.WriteString(TemplateGroupsNameProperty, grp.Name);
                writer.WriteString(TemplateGroupsUseValueOfProperty, grp.UseValueOf);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
        
        if (property.Priority != 0)
        {
            writer.WriteNumber(PriorityProperty, property.Priority);
        }

        if (property.Deprecated != null && !property.Deprecated.Equals(SpecDynamicValue.False))
        {
            writer.WritePropertyName(DeprecatedProperty);
            property.Deprecated.WriteToJsonWriter(writer, options);
        }

        if (property.Experimental != null && !property.Experimental.Equals(SpecDynamicValue.False))
        {
            writer.WritePropertyName(ExperimentalProperty);
            property.Experimental.WriteToJsonWriter(writer, options);
        }

        if (property.FileCrossRef != null)
        {
            writer.WriteString(FileCrossRefProperty, property.FileCrossRef);
        }

        if (property.CountForTemplateGroup != null)
        {
            writer.WriteString(CountForTemplateGroupProperty, property.CountForTemplateGroup);
        }

        if (property.ValueTemplateGroupReference != null)
        {
            writer.WritePropertyName(ValueTemplateGroupReferenceProperty);
            property.ValueTemplateGroupReference.WriteToJsonWriter(writer, options);
        }
        if (property.ListReference != null)
        {
            writer.WriteString(ListReferenceProperty, property.ListReference);
        }

        if (!property.Aliases.IsNull)
        {
            writer.WriteStartArray(AliasesProperty);
            for (int i = 0; i < property.Aliases.Length; i++)
            {
                Alias alias = property.Aliases[i];
                string aliasKey = alias.Value;
                if (property.IsTemplate)
                    aliasKey = TemplateProcessor.EscapeKey(aliasKey, property.GetAliasTemplateProcessor(i));

                if (alias.Filter is LegacyExpansionFilter.Legacy or LegacyExpansionFilter.Modern)
                {
                    writer.WriteStartObject();

                    writer.WriteString(AliasesAliasProperty, aliasKey);
                    WriteLegacyExpansionFilter(writer, AliasesLegacyExpansionFilterProperty, alias.Filter);

                    writer.WriteEndObject();
                }
                else
                {
                    writer.WriteStringValue(aliasKey);
                }
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

        JsonHelper.WriteAdditionalProperties(writer, property, options);

        if (property.Variable != null)
        {
            writer.WritePropertyName(VariableProperty);
            property.Variable.WriteToJsonWriter(writer, options);
        }

        writer.WriteEndObject();
    }

    private static void WriteLegacyExpansionFilter(Utf8JsonWriter writer, JsonEncodedText propertyName, LegacyExpansionFilter filter)
    {
        writer.WriteString(propertyName, filter == LegacyExpansionFilter.Legacy ? nameof(LegacyExpansionFilter.Legacy) : nameof(LegacyExpansionFilter.Modern));
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

    /// <inheritdoc />
    public string? ToString(ISpecDynamicValue value) => value.AsConcrete<ISpecPropertyType>()?.Type;

    void ISpecPropertyType.Visit<TVisitor>(ref TVisitor visitor) => visitor.Visit(this);
}