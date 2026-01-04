using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

partial class SpecificationFileReader
{
    private DatType? FindExistingType(string typeName, in JsonElement arrayRoot, int minIndex, ImmutableDictionary<QualifiedType, DatType>.Builder typeDictionary, DatFileType file)
    {
        QualifiedType qt = new QualifiedType(typeName, true);
        if (typeDictionary.TryGetValue(qt, out DatType? type))
        {
            return type;
        }

        if (file.Parent != null && file.Parent.Types.TryGetValue(qt, out type))
        {
            return type;
        }

        int typeCount = arrayRoot.GetArrayLength();
        for (int i = minIndex; i < typeCount; ++i)
        {
            JsonElement root = arrayRoot[i];
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("Type"u8, out JsonElement element)
                || element.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            if (element.ValueEquals(typeName.AsSpan()))
            {
                return ReadTypeFirstPass(in arrayRoot, i, typeDictionary, file);
            }
        }

        return null;
    }

    private DatType ReadTypeFirstPass(in JsonElement arrayRoot, int index, ImmutableDictionary<QualifiedType, DatType>.Builder typeDictionary, DatFileType file)
    {
        JsonElement root = arrayRoot[index];
        QualifiedType fileType = file.TypeName;
        AssertValueKind(in root, fileType, JsonValueKind.Object);

        if (!root.TryGetProperty("Type"u8, out JsonElement element) || element.ValueKind != JsonValueKind.String)
        {
            throw new JsonException(string.Format(Resources.JsonException_TypeMissingTypeName, $"{fileType.GetFullTypeName()}.Types[{index}]"));
        }

        QualifiedType typeName = new QualifiedType(element.GetString()!, true);

        if (typeDictionary.TryGetValue(typeName, out DatType? existing))
        {
            return existing;
        }

        string displayName;
        if (!root.TryGetProperty("DisplayName"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            displayName = typeName.GetTypeName();
        }
        else
        {
            displayName = element.GetString()!;
        }

        DatType parsedType;
        if (root.TryGetProperty("Values"u8, out JsonElement enumValues))
        {
            AssertValueKind(in enumValues, fileType, JsonValueKind.Array);
            bool isFlags = root.TryGetProperty("IsFlags"u8, out element) && element.ValueKind != JsonValueKind.Null && element.GetBoolean();

            DatEnumType enumType = DatType.CreateEnumType(typeName, isFlags, root, file);
            enumType.DisplayNameIntl = displayName;

            typeDictionary[typeName] = parsedType = enumType;

            int valueCount = enumValues.GetArrayLength();

            ImmutableArray<DatEnumValue>.Builder values = ImmutableArray.CreateBuilder<DatEnumValue>(valueCount);
            for (int i = 0; i < valueCount; ++i)
            {
                JsonElement enumValue = enumValues[i];
                switch (enumValue.ValueKind)
                {
                    case JsonValueKind.String:
                        string? value = enumValue.GetString();
                        if (string.IsNullOrEmpty(value))
                            throw new JsonException(string.Format(Resources.JsonException_EnumMissingValue, $"{typeName.GetFullTypeName()}[{i}]"));
                        if (isFlags)
                            throw new JsonException(string.Format(Resources.JsonException_FlagEnumMissingNumericValue, typeName.GetFullTypeName() + "." + value));
                        values.Add(new DatEnumValue(value, values.Count, enumType, default));
                        break;

                    case JsonValueKind.Object:
                        values.Add(ReadDatEnumValueFromObject(in enumValue, isFlags, i, enumType));
                        break;

                    default:
                        throw new JsonException(string.Format(Resources.JsonException_InvalidJsonToken, enumValue.ValueKind, $"{fileType.Type}/{typeName.GetFullTypeName()}.Values[{i}]"));
                }
            }

            enumType.Values = values.MoveToImmutable();
        }
        else
        {
            DatTypeWithProperties? parentType = null;
            if (root.TryGetProperty("Parent"u8, out element))
            {
                string? parentTypeName = element.GetString();
                if (parentTypeName != null && !string.Equals(parentTypeName, typeName.Type, StringComparison.OrdinalIgnoreCase))
                {
                    parentType = FindExistingType(parentTypeName, in arrayRoot, index + 1, typeDictionary, file) as DatTypeWithProperties;
                    if (parentType == null)
                    {
                        throw new JsonException(string.Format(Resources.JsonException_ParentTypeNotFound, parentTypeName, typeName.GetFullTypeName()), $"Types[{index}].Parent", null, null);
                    }
                }
            }

            DatCustomType customType = DatType.CreateCustomType(typeName, root, parentType, file);
            customType.DisplayNameIntl = displayName;

            typeDictionary[typeName] = parsedType = customType;

            // AutoGeneratedKeys
            if (root.TryGetProperty("AutoGeneratedKeys"u8, out element) && element.ValueKind != JsonValueKind.Null)
                customType.AutoGeneratedKeys = element.GetBoolean();

            // OverridableProperties
            if (root.TryGetProperty("OverridableProperties"u8, out element) && element.ValueKind != JsonValueKind.Null)
                customType.OverridableProperties = element.GetBoolean();

            // todo: properties, localization
        }


        // Docs
        if (root.TryGetProperty("Docs"u8, out element))
            parsedType.Docs = element.GetString();

        // StringParseableType
        if (root.TryGetProperty("StringParseableType"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            parsedType.StringParseableType = Type.GetType(element.GetString()!, throwOnError: true, ignoreCase: false);
        }

        // Version
        if (root.TryGetProperty("Version"u8, out element) && element.ValueKind != JsonValueKind.Null)
            parsedType.Version = Version.Parse(element.GetString()!);


        return parsedType;
    }

    private static DatEnumValue ReadDatEnumValueFromObject(in JsonElement root, bool isFlags, int i, DatEnumType enumType)
    {
        // Value
        string? value = null;
        if (root.TryGetProperty("Value"u8, out JsonElement element))
            value = element.GetString();

        if (string.IsNullOrEmpty(value))
            throw new JsonException(string.Format(Resources.JsonException_EnumMissingValue, $"{enumType.TypeName.GetFullTypeName()}[{i}]"));

        long? numericValue = null;
        if (root.TryGetProperty("NumericValue"u8, out element) && element.ValueKind != JsonValueKind.Null)
        {
            if (element.TryGetInt64(out long l))
                numericValue = l;
            else
                numericValue = unchecked((long)element.GetUInt64());
        }

        if (isFlags && !numericValue.HasValue)
            throw new JsonException(string.Format(Resources.JsonException_FlagEnumMissingNumericValue, enumType.TypeName.GetFullTypeName() + "." + value));

        DatEnumValue parsedValue = isFlags
            ? DatFlagEnumValue.Create(value, i, (DatFlagEnumType)enumType, numericValue!.Value, root)
            : DatEnumValue.Create(value, i, enumType, root);

        if (!isFlags)
            parsedValue.NumericValue = numericValue;

        // Casing
        if (root.TryGetProperty("Casing"u8, out element))
            parsedValue.CasingValue = element.GetString();

        // CorrespondingType
        if (root.TryGetProperty("CorrespondingType"u8, out element) && element.ValueKind != JsonValueKind.Null)
            parsedValue.CorrespondingType = new QualifiedType(element.GetString()!, false);

        // RequiredBaseType
        if (root.TryGetProperty("RequiredBaseType"u8, out element) && element.ValueKind != JsonValueKind.Null)
            parsedValue.RequiredBaseType = new QualifiedType(element.GetString()!, false);

        // Description
        if (root.TryGetProperty("Description"u8, out element))
            parsedValue.Description = element.GetString();

        // Abbreviation
        if (root.TryGetProperty("Abbreviation"u8, out element))
            parsedValue.Abbreviation = element.GetString();

        // Deprecated
        if (root.TryGetProperty("Deprecated"u8, out element) && element.ValueKind != JsonValueKind.Null)
            parsedValue.Deprecated = element.GetBoolean();

        // Docs
        if (root.TryGetProperty("Docs"u8, out element))
            parsedValue.Docs = element.GetString();

        // Version
        if (root.TryGetProperty("Version"u8, out element) && element.ValueKind != JsonValueKind.Null)
            parsedValue.Version = Version.Parse(element.GetString()!);

        return parsedValue;
    }
}
