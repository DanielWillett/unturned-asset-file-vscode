using System;
using System.Collections;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class JsonHelper
{
    public static bool TryReadGenericValue(ref Utf8JsonReader reader, out object? obj)
    {
        obj = null;
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return true;

            case JsonTokenType.True:
            case JsonTokenType.False:
                obj = reader.GetBoolean();
                return true;

            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int int32))
                    obj = int32;
                else if (reader.TryGetUInt32(out uint uint32))
                    obj = uint32;
                else if (reader.TryGetInt64(out long int64))
                    obj = int64;
                else if (reader.TryGetUInt64(out ulong uint64))
                    obj = uint64;
                else if (reader.TryGetDouble(out double dbl))
                    obj = dbl;
                else
                    return false;

                return true;

            case JsonTokenType.String:
                if (reader.TryGetGuid(out Guid guid))
                    obj = guid;
                else if (reader.TryGetDateTimeOffset(out DateTimeOffset dto))
                    obj = dto;
                else if (reader.TryGetDateTime(out DateTime dt))
                    obj = dt;
                else
                    obj = reader.GetString();
                return true;

            case JsonTokenType.StartArray:
                ArrayList? list = null;
                Type? listType = null;
                bool hasNonNullDifferingTypeValue = false;
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (!TryReadGenericValue(ref reader, out object? element))
                        return false;

                    if (listType == null)
                    {
                        listType = element == null ? typeof(object) : element.GetType();
                    }
                    else if (listType == typeof(object) && element != null && !hasNonNullDifferingTypeValue)
                    {
                        Type elementType = element.GetType();
                        if (!elementType.IsValueType)
                        {
                            listType = elementType;
                        }
                    }
                    else if (element != null && !listType.IsInstanceOfType(element) || element == null && listType.IsValueType)
                    {
                        listType = typeof(object);
                        hasNonNullDifferingTypeValue = true;
                    }

                    (list ??= new ArrayList()).Add(element);
                }

                if (list == null || listType == null)
                    return false;

                obj = list.ToArray(listType);
                break;
        }

        obj = null;
        return false;
    }
}
