using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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

    /// <summary>
    /// Skips any properties starting with '$'.
    /// </summary>
    public static bool ShouldSkipAdditionalProperty([NotNullWhen(false)] string? key)
    {
        return string.IsNullOrEmpty(key) || key![0] == '$';
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
    
    public static bool TryReadGenericValue(in JsonElement reader, out object? obj)
    {
        obj = null;
        switch (reader.ValueKind)
        {
            case JsonValueKind.Null:
                return true;

            case JsonValueKind.True:
            case JsonValueKind.False:
                obj = reader.GetBoolean() ? BoxedPrimitives.True : BoxedPrimitives.False;
                return true;

            case JsonValueKind.Number:
                if (reader.TryGetInt32(out int int32))
                    obj = int32 switch
                    {
                        -1 => BoxedPrimitives.I4M1,
                        0 => BoxedPrimitives.I40,
                        1 => BoxedPrimitives.I41,
                        _ => int32
                    };
                else if (reader.TryGetUInt32(out uint uint32))
                    obj = uint32 switch
                    {
                        0 => BoxedPrimitives.U40,
                        _ => uint32
                    };
                else if (reader.TryGetInt64(out long int64))
                    obj = int64 switch
                    {
                        0 => BoxedPrimitives.I80,
                        _ => int64
                    };
                else if (reader.TryGetUInt64(out ulong uint64))
                    obj = uint64 switch
                    {
                        0 => BoxedPrimitives.U80,
                        _ => uint64
                    };
                else if (reader.TryGetDouble(out double dbl))
                    obj = uint64 switch
                    {
                        0 => BoxedPrimitives.R80,
                        _ => dbl
                    };
                else
                    return false;

                return true;

            case JsonValueKind.String:
                DateTime dt;
                // dont change this to TryGetGuid
                if (reader.TryGetGuid(out Guid guid))
                    obj = guid;
                else if (reader.TryGetDateTimeOffset(out DateTimeOffset dto))
                {
                    if (dto.Offset == TimeSpan.Zero && reader.TryGetDateTime(out dt))
                        obj = dt;
                    else
                        obj = dto;
                }
                else if (reader.TryGetDateTime(out dt))
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

            case JsonValueKind.Array:
                ArrayList? list = null;
                Type? listType = null;
                bool hasNonNullDifferingTypeValue = false;
                foreach (JsonElement e in reader.EnumerateArray())
                {
                    if (!TryReadGenericValue(in e, out object? element))
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
                        else
                        {
                            hasNonNullDifferingTypeValue = true;
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

    public static void WriteAdditionalProperties<T>(Utf8JsonWriter writer, T value, JsonSerializerOptions options) where T : IAdditionalPropertyProvider
    {
        OneOrMore<KeyValuePair<string, object?>> additionalProperties = value.AdditionalProperties;

        if (additionalProperties.IsNull)
            return;

        foreach (KeyValuePair<string, object?> kvp in additionalProperties)
        {
            writer.WritePropertyName(kvp.Key);
            WriteGenericValue(writer, kvp.Value);
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

        if (reader.TokenType != JsonTokenType.String || (reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length) != 32)
            return false;
        
        string str = reader.GetString()!;
        return Guid.TryParse(str, out guid);
    }

    public static bool TryGetGuid(in JsonElement element, out Guid guid)
    {
        // doesn't support non-dashed GUID format
        if (element.TryGetGuid(out guid))
            return true;

        if (element.ValueKind != JsonValueKind.String)
            return false;
        
        string str = element.GetString()!;
        return Guid.TryParse(str, out guid);
    }

    /// <summary>
    /// Attempts to read a value of the given type from JSON.
    /// </summary>
    /// <typeparam name="TTo">The type of value to read. Supports <see cref="EquatableArray{T}"/> and <see cref="DictionaryPair{TElementType}"/>.</typeparam>
    /// <param name="reader">The reader to read data from.</param>
    /// <param name="value">The read value.</param>
    [SkipLocalsInit]
    public static bool TryReadGenericValue<TTo>(ref Utf8JsonReader reader, out Optional<TTo> value) where TTo : IEquatable<TTo>
    {
        value = Optional<TTo>.Null;
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                value = Optional<TTo>.Null;
                return true;

            case JsonTokenType.True:
            case JsonTokenType.False:
                if (typeof(TTo) == typeof(bool))
                {
                    value = MathMatrix.As<bool, TTo>(reader.TokenType == JsonTokenType.True);
                    return true;
                }

                break;

            case JsonTokenType.Number:
                if (typeof(TTo) == typeof(int))
                {
                    if (!reader.TryGetInt32(out int v))
                        return false;

                    value = Unsafe.As<int, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(float))
                {
                    if (!reader.TryGetSingle(out float v))
                        return false;

                    value = Unsafe.As<float, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(long))
                {
                    if (!reader.TryGetInt64(out long v))
                        return false;

                    value = Unsafe.As<long, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(short))
                {
                    if (!reader.TryGetInt16(out short v))
                        return false;

                    value = Unsafe.As<short, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(sbyte))
                {
                    if (!reader.TryGetSByte(out sbyte v))
                        return false;

                    value = Unsafe.As<sbyte, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(uint))
                {
                    if (!reader.TryGetUInt32(out uint v))
                        return false;

                    value = Unsafe.As<uint, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(ulong))
                {
                    if (!reader.TryGetUInt64(out ulong v))
                        return false;

                    value = Unsafe.As<ulong, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(double))
                {
                    if (!reader.TryGetDouble(out double v))
                        return false;

                    value = Unsafe.As<double, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(ushort))
                {
                    if (!reader.TryGetUInt16(out ushort v))
                        return false;

                    value = Unsafe.As<ushort, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(GuidOrId))
                {
                    if (!reader.TryGetUInt16(out ushort v))
                        return false;

                    value = MathMatrix.As<GuidOrId, TTo>(new GuidOrId(v));
                    return true;
                }
                if (typeof(TTo) == typeof(byte))
                {
                    if (!reader.TryGetByte(out byte v))
                        return false;

                    value = Unsafe.As<byte, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(decimal))
                {
                    if (!reader.TryGetDecimal(out decimal v))
                        return false;

                    value = Unsafe.As<decimal, TTo>(ref v);
                    return true;
                }
                if (typeof(TTo) == typeof(char))
                {
                    if (!reader.TryGetUInt16(out ushort v))
                        return false;

                    value = MathMatrix.As<char, TTo>((char)v);
                    return true;
                }

                break;

            case JsonTokenType.String:
                if (typeof(TTo) == typeof(string))
                {
                    value = MathMatrix.As<string, TTo>(reader.GetString()!);
                    return true;
                }

                if (typeof(TTo) == typeof(Guid))
                {
                    if (!TryGetGuid(ref reader, out Guid guid))
                        return false;

                    value = Unsafe.As<Guid, TTo>(ref guid);
                    return true;
                }

                if (typeof(TTo) == typeof(GuidOrId))
                {
                    if (!TryGetGuid(ref reader, out Guid guid))
                    {
                        string str = reader.GetString()!;
                        if (!GuidOrId.TryParse(str, out GuidOrId id))
                            return false;

                        value = Unsafe.As<GuidOrId, TTo>(ref id);
                        return true;
                    }

                    value = MathMatrix.As<GuidOrId, TTo>(new GuidOrId(guid));
                    return true;
                }

                if (typeof(TTo) == typeof(DateTime))
                {
                    if (!reader.TryGetDateTime(out DateTime dateTime))
                        return false;

                    value = Unsafe.As<DateTime, TTo>(ref dateTime);
                    return true;
                }

                if (typeof(TTo) == typeof(DateTimeOffset))
                {
                    if (!reader.TryGetDateTimeOffset(out DateTimeOffset dateTime))
                        return false;

                    value = Unsafe.As<DateTimeOffset, TTo>(ref dateTime);
                    return true;
                }

                if (typeof(TTo) == typeof(TimeSpan))
                {
                    string str = reader.GetString()!;
                    if (!TimeSpan.TryParse(str, CultureInfo.InvariantCulture, out TimeSpan timeSpan))
                        return false;

                    value = Unsafe.As<TimeSpan, TTo>(ref timeSpan);
                    return true;
                }

                if (typeof(TTo) == typeof(IPv4Filter))
                {
                    string str = reader.GetString()!;
                    if (!IPv4Filter.TryParse(str, out IPv4Filter timeSpan))
                        return false;

                    value = Unsafe.As<IPv4Filter, TTo>(ref timeSpan);
                    return true;
                }

                if (typeof(TTo) == typeof(BundleReference))
                {
                    string str = reader.GetString()!;
                    if (!BundleReference.TryParse(str, out BundleReference timeSpan))
                        return false;

                    value = Unsafe.As<BundleReference, TTo>(ref timeSpan);
                    return true;
                }

                if (typeof(TTo) == typeof(char))
                {
                    string str = reader.GetString()!;
                    if (str.Length != 1)
                        return false;

                    value = MathMatrix.As<char, TTo>(str[0]);
                    return true;
                }

                if (typeof(TTo) == typeof(Color32))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseColorHex(str, out Color32 color, allowAlpha: true))
                        return false;

                    value = Unsafe.As<Color32, TTo>(ref color);
                    return true;
                }

                if (typeof(TTo) == typeof(Color))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseColorHex(str, out Color32 color, allowAlpha: true))
                        return false;

                    value = MathMatrix.As<Color, TTo>(color);
                    return true;
                }

                if (typeof(TTo) == typeof(Vector2))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseVector2Components(str, out Vector2 v2))
                        return false;

                    value = MathMatrix.As<Vector2, TTo>(v2);
                    return true;
                }

                if (typeof(TTo) == typeof(Vector3))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseVector3Components(str, out Vector3 v3))
                        return false;

                    value = MathMatrix.As<Vector3, TTo>(v3);
                    return true;
                }

                if (typeof(TTo) == typeof(Vector4))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseVector4Components(str, out Vector4 v4))
                        return false;

                    value = MathMatrix.As<Vector4, TTo>(v4);
                    return true;
                }

                if (typeof(TTo) == typeof(QualifiedType))
                {
                    string str = reader.GetString()!;
                    value = MathMatrix.As<QualifiedType, TTo>(new QualifiedType(str));
                    return true;
                }

                if (typeof(TTo) == typeof(QualifiedOrAliasedType))
                {
                    string str = reader.GetString()!;
                    value = MathMatrix.As<QualifiedOrAliasedType, TTo>(QualifiedOrAliasedType.FromType(str));
                    return true;
                }

                break;

            case JsonTokenType.StartObject:
                if (typeof(TTo).GetGenericTypeDefinition() == typeof(DictionaryPair<>))
                {
                    DictionaryPairParseCache<TTo>.ReaderDelegate(ref reader, out TTo array);
                    value = array;
                    return true;
                }

                break;

            case JsonTokenType.StartArray:
                if (typeof(TTo).GetGenericTypeDefinition() == typeof(EquatableArray<>))
                {
                    ArrayParseCache<TTo>.ReaderDelegate(ref reader, out TTo array);
                    value = array;
                    return true;
                }

                break;
        }

        return false;
    }


    [SkipLocalsInit]
    public static bool TryReadGenericValue<TTo>(in JsonElement json, out Optional<TTo> value) where TTo : IEquatable<TTo>
    {
        if (json.ValueKind == JsonValueKind.Null)
        {
            value = Optional<TTo>.Null;
            return true;
        }

        if (json.ValueKind == JsonValueKind.Number)
        {
            if (typeof(TTo) == typeof(int))
            {
                bool s = json.TryGetInt32(out int v);
                value = Unsafe.As<int, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(uint))
            {
                bool s = json.TryGetUInt32(out uint v);
                value = Unsafe.As<uint, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(long))
            {
                bool s = json.TryGetInt64(out long v);
                value = Unsafe.As<long, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(ulong))
            {
                bool s = json.TryGetUInt64(out ulong v);
                value = Unsafe.As<ulong, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(short))
            {
                bool s = json.TryGetInt16(out short v);
                value = Unsafe.As<short, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(ushort))
            {
                bool s = json.TryGetUInt16(out ushort v);
                value = Unsafe.As<ushort, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(GuidOrId))
            {
                bool s = json.TryGetUInt16(out ushort v);
                value = MathMatrix.As<GuidOrId, TTo>(new GuidOrId(v));
                return s;
            }
            if (typeof(TTo) == typeof(sbyte))
            {
                bool s = json.TryGetSByte(out sbyte v);
                value = Unsafe.As<sbyte, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(byte))
            {
                bool s = json.TryGetByte(out byte v);
                value = Unsafe.As<byte, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(float))
            {
                bool s = json.TryGetSingle(out float v);
                value = Unsafe.As<float, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(double))
            {
                bool s = json.TryGetDouble(out double v);
                value = Unsafe.As<double, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(decimal))
            {
                bool s = json.TryGetDecimal(out decimal v);
                value = Unsafe.As<decimal, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(char))
            {
                bool s = json.TryGetUInt16(out ushort v);
                value = MathMatrix.As<char, TTo>((char)v);
                return s;
            }
            if (typeof(TTo) == typeof(bool))
            {
                bool s = json.TryGetDouble(out double d);
                value = MathMatrix.As<bool, TTo>(d != 0);
                return s;
            }
        }
        else if (json.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            bool v = json.ValueKind == JsonValueKind.True;
            if (typeof(TTo) == typeof(bool))
            {
                value = MathMatrix.As<bool, TTo>(v);
                return true;
            }
            if (typeof(TTo) == typeof(int))
            {
                value = MathMatrix.As<int, TTo>(v ? 1 : 0);
                return true;
            }
            if (typeof(TTo) == typeof(uint))
            {
                value = MathMatrix.As<uint, TTo>(v ? 1u : 0u);
                return true;
            }
            if (typeof(TTo) == typeof(long))
            {
                value = MathMatrix.As<long, TTo>(v ? 1L : 0L);
                return true;
            }
            if (typeof(TTo) == typeof(ulong))
            {
                value = MathMatrix.As<ulong, TTo>(v ? 1ul : 0ul);
                return true;
            }
            if (typeof(TTo) == typeof(short))
            {
                value = MathMatrix.As<short, TTo>(v ? (short)1 : (short)0);
                return true;
            }
            if (typeof(TTo) == typeof(ushort))
            {
                value = MathMatrix.As<ushort, TTo>(v ? (ushort)1 : (ushort)0);
                return true;
            }
            if (typeof(TTo) == typeof(GuidOrId))
            {
                value = MathMatrix.As<GuidOrId, TTo>(new GuidOrId(v ? (ushort)1 : (ushort)0));
                return true;
            }
            if (typeof(TTo) == typeof(sbyte))
            {
                value = MathMatrix.As<sbyte, TTo>(v ? (sbyte)1 : (sbyte)0);
                return true;
            }
            if (typeof(TTo) == typeof(byte))
            {
                value = MathMatrix.As<byte, TTo>(v ? (byte)1 : (byte)0);
                return true;
            }
            if (typeof(TTo) == typeof(float))
            {
                value = MathMatrix.As<float, TTo>(v ? 1f : 0f);
                return true;
            }
            if (typeof(TTo) == typeof(double))
            {
                value = MathMatrix.As<double, TTo>(v ? 1d : 0d);
                return true;
            }
            if (typeof(TTo) == typeof(decimal))
            {
                value = MathMatrix.As<decimal, TTo>(v ? decimal.One : decimal.Zero);
                return true;
            }
            if (typeof(TTo) == typeof(char))
            {
                value = MathMatrix.As<char, TTo>(v ? '1' : '0');
                return true;
            }
            if (typeof(TTo) == typeof(string))
            {
                value = MathMatrix.As<string, TTo>(v ? "true" : "false");
                return true;
            }
        }

        if (typeof(TTo) == typeof(string))
        {
            value = MathMatrix.As<string, TTo>(json.ValueKind == JsonValueKind.String ? json.GetString()! : json.GetRawText());
            return true;
        }

        if (json.ValueKind == JsonValueKind.String)
        {
            string str = json.GetString()!;
            if (typeof(TTo) == typeof(Guid))
            {
                bool s = Guid.TryParse(str, out Guid v);
                value = Unsafe.As<Guid, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(GuidOrId))
            {
                bool s = GuidOrId.TryParse(str, out GuidOrId v);
                value = Unsafe.As<GuidOrId, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(BundleReference))
            {
                bool s = BundleReference.TryParse(str, out BundleReference v);
                value = Unsafe.As<BundleReference, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(QualifiedType))
            {
                value = MathMatrix.As<QualifiedType, TTo>(new QualifiedType(str, isCaseInsensitive: true));
                return true;
            }
            if (typeof(TTo) == typeof(QualifiedOrAliasedType))
            {
                value = MathMatrix.As<QualifiedOrAliasedType, TTo>(new QualifiedType(str, isCaseInsensitive: true));
                return true;
            }
            if (typeof(TTo) == typeof(DateTime))
            {
                bool s = json.TryGetDateTime(out DateTime dateTime);
                value = Unsafe.As<DateTime, TTo>(ref dateTime);
                return s;
            }
            if (typeof(TTo) == typeof(DateTimeOffset))
            {
                bool s = json.TryGetDateTimeOffset(out DateTimeOffset dateTime);
                value = Unsafe.As<DateTimeOffset, TTo>(ref dateTime);
                return s;
            }
            if (typeof(TTo) == typeof(TimeSpan))
            {
                bool s = TimeSpan.TryParse(str, out TimeSpan v);
                value = Unsafe.As<TimeSpan, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(IPv4Filter))
            {
                bool s = IPv4Filter.TryParse(str, out IPv4Filter v);
                value = Unsafe.As<IPv4Filter, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(Color32))
            {
                bool s = KnownTypeValueHelper.TryParseColorHex(str, out Color32 v, allowAlpha: true);
                value = Unsafe.As<Color32, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(Color))
            {
                bool s = KnownTypeValueHelper.TryParseColorHex(str, out Color32 v, allowAlpha: true);
                value = MathMatrix.As<Color, TTo>(v);
                return s;
            }
            if (typeof(TTo) == typeof(Vector2))
            {
                bool s = KnownTypeValueHelper.TryParseVector2Components(str, out Vector2 v);
                value = Unsafe.As<Vector2, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(Vector3))
            {
                bool s = KnownTypeValueHelper.TryParseVector3Components(str, out Vector3 v);
                value = Unsafe.As<Vector3, TTo>(ref v);
                return s;
            }
            if (typeof(TTo) == typeof(Vector4))
            {
                bool s = KnownTypeValueHelper.TryParseVector4Components(str, out Vector4 v);
                value = Unsafe.As<Vector4, TTo>(ref v);
                return s;
            }

            value = Optional<TTo>.Null;
            return false;
        }

        if (json.ValueKind == JsonValueKind.Object)
        {
            if (typeof(TTo).GetGenericTypeDefinition() == typeof(DictionaryPair<>))
            {
                DictionaryPairParseCache<TTo>.ElementDelegate(in json, out TTo array);
                value = array;
                return true;
            }
        }
        else if (json.ValueKind == JsonValueKind.Array)
        {
            if (typeof(TTo).GetGenericTypeDefinition() == typeof(EquatableArray<>))
            {
                ArrayParseCache<TTo>.ElementDelegate(in json, out TTo array);
                value = array;
                return true;
            }
        }

        value = Optional<TTo>.Null;
        return false;
    }


#pragma warning disable IDE0051 // Used implicitly
    private static bool TryReadGenericEquatableArrayElementsReader<TElementType>(ref Utf8JsonReader reader, out EquatableArray<TElementType> array) where TElementType : IEquatable<TElementType>
    {
        Utf8JsonReader readerCopy = reader;
        int elementCount = 0;
        while (readerCopy.Read() && readerCopy.TokenType != JsonTokenType.EndArray)
        {
            ++elementCount;
            readerCopy.Skip();
        }

        if (elementCount == 0)
        {
            array = EquatableArray<TElementType>.Empty;
            return true;
        }

        TElementType[] buffer = new TElementType[elementCount];
        elementCount = 0;
        bool failed = false;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (!TryReadGenericValue<TElementType>(ref reader, out Optional<TElementType> elementType) || !elementType.HasValue)
            {
                failed = true;
                reader.Skip();
            }
            else
            {
                buffer[elementCount] = elementType.Value;
            }

            ++elementCount;
        }

        array = new EquatableArray<TElementType>(buffer, elementCount);
        return !failed;
    }
    
    private static bool TryReadGenericEquatableArrayElementsElement<TElementType>(in JsonElement element, out EquatableArray<TElementType> array) where TElementType : IEquatable<TElementType>
    {
        int elementCount = element.GetArrayLength();
        if (elementCount == 0)
        {
            array = EquatableArray<TElementType>.Empty;
            return true;
        }

        TElementType[] buffer = new TElementType[elementCount];
        elementCount = 0;
        bool failed = false;
        for (int i = 0; i < elementCount; ++i)
        {
            JsonElement e = element[i];
            if (!TryReadGenericValue<TElementType>(in e, out Optional<TElementType> elementType) || !elementType.HasValue)
            {
                failed = true;
            }
            else
            {
                buffer[elementCount] = elementType.Value;
            }

            ++elementCount;
        }

        array = new EquatableArray<TElementType>(buffer, elementCount);
        return !failed;
    }

    private static readonly JsonEncodedText DictionaryPairKeyProperty = JsonEncodedText.Encode("Key");
    private static readonly JsonEncodedText DictionaryPairValueProperty = JsonEncodedText.Encode("Value");

    private static bool TryReadDictionaryPairReader<TElementType>(ref Utf8JsonReader reader, out DictionaryPair<TElementType> pair) where TElementType : IEquatable<TElementType>
    {
        string? key = null;
        TElementType? value = default;
        bool hasValue = false;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            if (reader.ValueTextEquals(DictionaryPairKeyProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read())
                    break;

                if (reader.TokenType != JsonTokenType.String)
                {
                    reader.Skip();
                    continue;
                }

                key = reader.GetString()!;
            }
            else if (reader.ValueTextEquals(DictionaryPairValueProperty.EncodedUtf8Bytes))
            {
                if (!reader.Read())
                    break;

                if (!TryReadGenericValue<TElementType>(ref reader, out Optional<TElementType> elementType))
                {
                    reader.Skip();
                }
                else if (elementType.HasValue)
                {
                    value = elementType.Value;
                    hasValue = true;
                }
            }
            else
            {
                reader.Read();
                reader.Skip();
            }
        }

        pair = new DictionaryPair<TElementType>(key!, value!);
        return hasValue && key != null;
    }

    private static bool TryReadDictionaryPairElement<TElementType>(in JsonElement element, out DictionaryPair<TElementType> pair) where TElementType : IEquatable<TElementType>
    {
        string? key = null;
        TElementType? value = default;

        if (element.TryGetProperty(DictionaryPairKeyProperty.EncodedUtf8Bytes, out JsonElement valueElement))
        {
            if (valueElement.ValueKind != JsonValueKind.String)
            {
                pair = default;
                return false;
            }

            key = valueElement.GetString()!;
        }

        if (element.TryGetProperty(DictionaryPairValueProperty.EncodedUtf8Bytes, out valueElement))
        {
            if (!TryReadGenericValue<TElementType>(in valueElement, out Optional<TElementType> elementType) || !elementType.HasValue)
            {
                pair = default;
                return false;
            }

            value = elementType.Value;
        }

        pair = new DictionaryPair<TElementType>(key!, value!);
        return key != null;
    }
#pragma warning restore IDE0051

    private static class ArrayParseCache<TArrayType>
    {
        public delegate bool TryReadGenericTypeArrayElementReader(ref Utf8JsonReader reader, out TArrayType arrayType);
        public delegate bool TryReadGenericTypeArrayElementElement(in JsonElement element, out TArrayType arrayType);

        public static readonly TryReadGenericTypeArrayElementReader ReaderDelegate;
        public static readonly TryReadGenericTypeArrayElementElement ElementDelegate;
        static ArrayParseCache()
        {
            Type elementType = typeof(TArrayType).GetGenericArguments()[0];
            MethodInfo readerMethod = typeof(JsonHelper)
                .GetMethod("TryReadGenericEquatableArrayElementsReader", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException("Method not found: JsonHelper.TryReadGenericEquatableArrayElementsReader");
            MethodInfo elementMethod = typeof(JsonHelper)
                .GetMethod("TryReadGenericEquatableArrayElementsElement", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException("Method not found: JsonHelper.TryReadGenericEquatableArrayElementsElement");
            
            Type[] args = [ elementType ];
            ReaderDelegate = (TryReadGenericTypeArrayElementReader)readerMethod.MakeGenericMethod(args).CreateDelegate(typeof(TryReadGenericTypeArrayElementReader));
            ElementDelegate = (TryReadGenericTypeArrayElementElement)readerMethod.MakeGenericMethod(args).CreateDelegate(typeof(TryReadGenericTypeArrayElementElement));
        }
    }

    private static class DictionaryPairParseCache<TDictionaryPairType>
    {
        public delegate bool TryReadDictionaryPairReader(ref Utf8JsonReader reader, out TDictionaryPairType dictionaryType);
        public delegate bool TryReadDictionaryPairElement(in JsonElement element, out TDictionaryPairType dictionaryType);

        public static readonly TryReadDictionaryPairReader ReaderDelegate;
        public static readonly TryReadDictionaryPairElement ElementDelegate;
        static DictionaryPairParseCache()
        {
            Type elementType = typeof(TDictionaryPairType).GetGenericArguments()[0];
            MethodInfo readerMethod = typeof(JsonHelper)
                .GetMethod("TryReadDictionaryPairReader", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException("Method not found: JsonHelper.TryReadDictionaryPairReader");
            MethodInfo elementMethod = typeof(JsonHelper)
                .GetMethod("TryReadDictionaryPairElement", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException("Method not found: JsonHelper.TryReadDictionaryPairElement");

            Type[] args = [ elementType ];
            ReaderDelegate = (TryReadDictionaryPairReader)readerMethod.MakeGenericMethod(args).CreateDelegate(typeof(TryReadDictionaryPairReader));
            ElementDelegate = (TryReadDictionaryPairElement)elementMethod.MakeGenericMethod(args).CreateDelegate(typeof(TryReadDictionaryPairElement));
        }
    }
}