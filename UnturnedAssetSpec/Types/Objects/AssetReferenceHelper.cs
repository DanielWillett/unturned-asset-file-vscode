using System;
using System.Text;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

internal static class AssetReferenceHelper
{
    internal static string GetDisplayName(OneOrMore<QualifiedType> baseTypes, string displayName, string displayNameFormattable, bool isAsset = true)
    {
        if (baseTypes.IsNull
            || baseTypes.IsSingle
                && (
                    isAsset && (baseTypes.Value == QualifiedType.AssetBaseType || baseTypes.Value == QualifiedType.AssetBaseType)
                    || (QualifiedType.ExtractParts(baseTypes.Value.Type, out ReadOnlySpan<char> fullTypeName, out _) && fullTypeName.Equals("System.Object", StringComparison.OrdinalIgnoreCase))
                )
            )
        {
            return displayName;
        }

        if (baseTypes.IsSingle)
        {
            return string.Format(displayNameFormattable, baseTypes.Value.GetTypeName());
        }
        if (baseTypes.Length == 2)
        {
            return string.Format(displayNameFormattable, baseTypes.Values[0].GetTypeName() + " or " + baseTypes.Values[1].GetTypeName());
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < baseTypes.Length; i++)
        {
            QualifiedType type = baseTypes[i];
            if (i != 0)
                sb.Append(", ");
            if (i == baseTypes.Length - 1)
                sb.Append("or ");

            sb.Append(type.GetTypeName());
        }

        return sb.ToString();
    }

    internal static void ReadCommonJsonProperties(in JsonElement typeDefinition, out OneOrMore<QualifiedType> baseTypes, out bool allowThis, out bool preventSelfRef, out bool isDefault)
    {
        if (typeDefinition.ValueKind != JsonValueKind.Object)
        {
            baseTypes = OneOrMore<QualifiedType>.Null;
            allowThis = false;
            preventSelfRef = false;
            isDefault = true;
        }
        else
        {
            if (typeDefinition.TryGetProperty("AssetType"u8, out JsonElement element)
                && element.ValueKind != JsonValueKind.Null)
            {
                baseTypes = new OneOrMore<QualifiedType>(new QualifiedType(element.GetString()!, isCaseInsensitive: true));
            }
            else if (typeDefinition.TryGetProperty("AssetTypes"u8, out element)
                     && element.ValueKind != JsonValueKind.Null)
            {
                int len = element.GetArrayLength();
                QualifiedType[] arr = new QualifiedType[len];
                for (int i = 0; i < len; ++i)
                {
                    arr[i] = new QualifiedType(element[i].GetString()!, isCaseInsensitive: true);
                }

                baseTypes = new OneOrMore<QualifiedType>(arr);
            }
            else
            {
                baseTypes = OneOrMore<QualifiedType>.Null;
            }

            allowThis = typeDefinition.TryGetProperty("SupportsThis"u8, out element)
                        && element.ValueKind != JsonValueKind.Null
                        && element.GetBoolean();

            preventSelfRef = typeDefinition.TryGetProperty("PreventSelfReference"u8, out element)
                             && element.ValueKind != JsonValueKind.Null
                             && element.GetBoolean();

            isDefault = baseTypes.IsNull && !allowThis && !preventSelfRef;
        }
    }

    internal static void WriteCommonJsonProperties(Utf8JsonWriter writer, OneOrMore<QualifiedType> baseTypes, bool supportsThis, bool preventSelfReference)
    {
        if (!baseTypes.IsNull)
        {
            if (baseTypes.IsSingle)
            {
                writer.WriteString("AssetType"u8, baseTypes[0].Type);
            }
            else
            {
                writer.WritePropertyName("AssetTypes"u8);
                writer.WriteStartArray();
                foreach (QualifiedType t in baseTypes)
                    writer.WriteStringValue(t.Type);
                writer.WriteEndArray();
            }
        }

        if (supportsThis)
            writer.WriteBoolean("SupportsThis"u8, true);

        if (preventSelfReference)
            writer.WriteBoolean("PreventSelfReference"u8, true);
    }

    internal static AssetCategoryValue GetDefaultCategory(OneOrMore<QualifiedType> baseTypes, IDatSpecificationReadContext database)
    {
        AssetCategoryValue category = AssetCategoryValue.None;

        foreach (QualifiedType type in baseTypes)
        {
            int c = AssetCategory.GetCategoryFromType(type, database.Information);
            if (c == -1)
                continue;

            if (c == 0 || category.Index != 0 && category.Index != c)
            {
                return AssetCategoryValue.None;
            }

            category = new AssetCategoryValue(c);
        }

        return category;
    }
}
