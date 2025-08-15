using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class JsonHelper
{
#if EMIT
    private static Func<JsonDocument, ReadOnlyMemory<byte>>? _jsonDocumentUtf8JsonDataGetter;
#endif
    private static FieldInfo? _jsonDocumentUtf8JsonDataField;

    static JsonHelper()
    {
        try
        {
            _jsonDocumentUtf8JsonDataField = typeof(JsonDocument).GetField(
                "_utf8Json",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly
            );

#if EMIT
            if (_jsonDocumentUtf8JsonDataField == null)
                return;

            DynamicMethod method = new DynamicMethod("GetJsonDocumentUtf8",
                MethodAttributes.Static | MethodAttributes.Public,
                CallingConventions.Standard,
                typeof(ReadOnlyMemory<byte>),
                [ typeof(JsonDocument) ],
                typeof(JsonHelper),
                skipVisibility: true
            );

            ILGenerator emit = method.GetILGenerator(13);

            emit.Emit(OpCodes.Ldarg_0);
            emit.Emit(OpCodes.Ldfld, _jsonDocumentUtf8JsonDataField);
            emit.Emit(OpCodes.Ret);

            _jsonDocumentUtf8JsonDataGetter = (Func<JsonDocument, ReadOnlyMemory<byte>>)method.CreateDelegate(typeof(Func<JsonDocument, ReadOnlyMemory<byte>>));
#endif
        }
        catch { /* ignored */ }
    }

    public static Utf8JsonReader CreateUtf8JsonReader(JsonDocument document, JsonReaderOptions options)
    {
        ReadOnlyMemory<byte> mem;

        while (true)
        {
#if EMIT
            if (_jsonDocumentUtf8JsonDataGetter != null)
            {
                try
                {
                    mem = _jsonDocumentUtf8JsonDataGetter(document);
                    break;
                }
                catch
                {
                    _jsonDocumentUtf8JsonDataGetter = null;
                }
            }
            else
#endif
            if (_jsonDocumentUtf8JsonDataField != null)
            {
                try
                {
                    mem = (ReadOnlyMemory<byte>)_jsonDocumentUtf8JsonDataField.GetValue(document);
                    break;
                }
                catch
                {
                    _jsonDocumentUtf8JsonDataField = null;
                }
            }
            else
            {
                using MemoryStream ms = new MemoryStream(1024);
                using Utf8JsonWriter writer = new Utf8JsonWriter(ms);
                document.WriteTo(writer);
                writer.Flush();

                ms.TryGetBuffer(out ArraySegment<byte> seg);
                mem = seg.AsMemory();
                break;
            }
        }

        return new Utf8JsonReader(mem.Span, options);
    }

    private static readonly object TrueBox = true;
    private static readonly object FalseBox = false;
    private static readonly object ZeroBox = 0;
    private static readonly object OneBox = 1;
    private static readonly object NegativeOneBox = -1;

    public static bool TryReadGenericValue(ref Utf8JsonReader reader, out object? obj)
    {
        obj = null;
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return true;

            case JsonTokenType.True:
            case JsonTokenType.False:
                obj = reader.GetBoolean() ? TrueBox : FalseBox;
                return true;

            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int int32))
                    obj = int32 switch
                    {
                        -1 => NegativeOneBox,
                        0 => ZeroBox,
                        1 => OneBox,
                        _ => int32
                    };
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
                {
                    string str = reader.GetString();
                    if (Guid.TryParse(str, out guid))
                        obj = guid;
                    else
                        obj = str;
                }
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

    public static void WriteGenericValue(Utf8JsonWriter writer, object? value)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else if (value is Array array)
        {
            writer.WriteStartArray();
            for (int i = 0; i < array.Length; ++i)
            {
                WriteGenericValue(writer, array.GetValue(i));
            }
            writer.WriteEndArray();
        }
        else if (value is IConvertible c)
        {
            TypeCode typeCode = c.GetTypeCode();
            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case (TypeCode)17:
                default:
                    writer.WriteNullValue();
                    break;
                case TypeCode.Boolean:
                    writer.WriteBooleanValue((bool)value);
                    break;
                case TypeCode.Char:
                    writer.WriteStringValue(value.ToString());
                    break;
                case TypeCode.SByte:
                    writer.WriteNumberValue((sbyte)value);
                    break;
                case TypeCode.Byte:
                    writer.WriteNumberValue((byte)value);
                    break;
                case TypeCode.Int16:
                    writer.WriteNumberValue((short)value);
                    break;
                case TypeCode.UInt16:
                    writer.WriteNumberValue((ushort)value);
                    break;
                case TypeCode.Int32:
                    writer.WriteNumberValue((int)value);
                    break;
                case TypeCode.UInt32:
                    writer.WriteNumberValue((uint)value);
                    break;
                case TypeCode.Int64:
                    writer.WriteNumberValue((long)value);
                    break;
                case TypeCode.UInt64:
                    writer.WriteNumberValue((ulong)value);
                    break;
                case TypeCode.Single:
                    writer.WriteNumberValue((float)value);
                    break;
                case TypeCode.Double:
                    writer.WriteNumberValue((double)value);
                    break;
                case TypeCode.Decimal:
                    writer.WriteNumberValue((decimal)value);
                    break;
                case TypeCode.DateTime:
                    writer.WriteStringValue((DateTime)value);
                    break;
                case TypeCode.String:
                    writer.WriteStringValue((string)value);
                    break;
            }
        }
        else
        {
            switch (value)
            {
                case Guid g:
                    writer.WriteStringValue(g);
                    break;

                case DateTimeOffset dt:
                    writer.WriteStringValue(dt);
                    break;

                default:
                    writer.WriteStringValue(value.ToString());
                    break;
            }
        }
    }

    public static object? Deserialize(ref Utf8JsonReader reader, Type type, JsonSerializerOptions? options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (type != typeof(Guid))
        {
            return JsonSerializer.Deserialize(ref reader, type, options);
        }

        if (TryGetGuid(ref reader, out Guid guid))
        {
            return guid;
        }

        // will error
        return reader.GetGuid();
    }

    public static bool TryGetGuid(ref Utf8JsonReader reader, out Guid guid)
    {
        // doesn't support non-dashed GUID format
        if (reader.TryGetGuid(out guid))
            return true;

        if (reader.TokenType != JsonTokenType.String)
            return false;
        
        string str = reader.GetString()!;
        return Guid.TryParse(str, out guid);
    }
}
