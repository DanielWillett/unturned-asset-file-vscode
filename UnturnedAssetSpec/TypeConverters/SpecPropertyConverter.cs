using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    private static readonly JsonEncodedText DefaultValueSwitchProperty = JsonEncodedText.Encode("DefaultValueSwitch");
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


    private readonly SpecPropertyContext _context;
    public SpecPropertyConverter() : this(SpecPropertyContext.Property) { }
    public SpecPropertyConverter(SpecPropertyContext context)
    {
        if (context is not SpecPropertyContext.Property and not SpecPropertyContext.Localization)
            throw new ArgumentOutOfRangeException(nameof(context));

        _context = context;
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
        SpecProperty property = new SpecProperty();

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Unexpected token {reader.TokenType} reading SpecProperty.");
            }


        }

        return null;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, SpecProperty? value, JsonSerializerOptions options)
    {

    }
}