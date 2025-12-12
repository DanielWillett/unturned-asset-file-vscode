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
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

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
    
    public static bool TryReadGenericValue(ref Utf8JsonReader reader, out object? obj)
    {
        obj = null;
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return true;

            case JsonTokenType.True:
            case JsonTokenType.False:
                obj = reader.GetBoolean() ? BoxedPrimitives.True : BoxedPrimitives.False;
                return true;

            case JsonTokenType.Number:
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

            case JsonTokenType.String:
                if (TryGetGuid(ref reader, out Guid guid))
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
                    value = SpecDynamicExpressionTreeValueHelpers.As<bool, TTo>(reader.TokenType == JsonTokenType.True);
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

                    value = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId(v));
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

                    value = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>((char)v);
                    return true;
                }

                break;

            case JsonTokenType.String:
                if (typeof(TTo) == typeof(string))
                {
                    value = SpecDynamicExpressionTreeValueHelpers.As<string, TTo>(reader.GetString()!);
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

                    value = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TTo>(new GuidOrId(guid));
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

                    value = SpecDynamicExpressionTreeValueHelpers.As<char, TTo>(str[0]);
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

                    value = SpecDynamicExpressionTreeValueHelpers.As<Color, TTo>(color);
                    return true;
                }

                if (typeof(TTo) == typeof(Vector2))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseVector2Components(str, out Vector2 v2))
                        return false;

                    value = SpecDynamicExpressionTreeValueHelpers.As<Vector2, TTo>(v2);
                    return true;
                }

                if (typeof(TTo) == typeof(Vector3))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseVector3Components(str, out Vector3 v3))
                        return false;

                    value = SpecDynamicExpressionTreeValueHelpers.As<Vector3, TTo>(v3);
                    return true;
                }

                if (typeof(TTo) == typeof(Vector4))
                {
                    string str = reader.GetString()!;
                    if (!KnownTypeValueHelper.TryParseVector4Components(str, out Vector4 v4))
                        return false;

                    value = SpecDynamicExpressionTreeValueHelpers.As<Vector4, TTo>(v4);
                    return true;
                }

                if (typeof(TTo) == typeof(QualifiedType))
                {
                    string str = reader.GetString()!;
                    value = SpecDynamicExpressionTreeValueHelpers.As<QualifiedType, TTo>(new QualifiedType(str));
                    return true;
                }

                if (typeof(TTo) == typeof(QualifiedOrAliasedType))
                {
                    string str = reader.GetString()!;
                    value = SpecDynamicExpressionTreeValueHelpers.As<QualifiedOrAliasedType, TTo>(QualifiedOrAliasedType.FromType(str));
                    return true;
                }

                break;

            case JsonTokenType.StartObject:
                if (typeof(TTo).GetGenericTypeDefinition() == typeof(DictionaryPair<>))
                {
                    DictionaryPairParseCache<TTo>.Delegate(ref reader, out TTo array);
                    value = array;
                    return true;
                }

                break;

            case JsonTokenType.StartArray:
                if (typeof(TTo).GetGenericTypeDefinition() == typeof(EquatableArray<>))
                {
                    ArrayParseCache<TTo>.Delegate(ref reader, out TTo array);
                    value = array;
                    return true;
                }

                break;
        }

        return false;
    }


#pragma warning disable IDE0051 // Used implicitly
    private static bool TryReadGenericEquatableArrayElements<TElementType>(ref Utf8JsonReader reader, out EquatableArray<TElementType> array) where TElementType : IEquatable<TElementType>
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

    private static readonly JsonEncodedText DictionaryPairKeyProperty = JsonEncodedText.Encode("Key");
    private static readonly JsonEncodedText DictionaryPairValueProperty = JsonEncodedText.Encode("Value");

    private static bool TryReadDictionaryPair<TElementType>(ref Utf8JsonReader reader, out DictionaryPair<TElementType> pair) where TElementType : IEquatable<TElementType>
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

                if (!TryReadGenericValue<TElementType>(ref reader, out Optional<TElementType> elementType) || !elementType.HasValue)
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
#pragma warning restore IDE0051

    private static class ArrayParseCache<TArrayType>
    {
        public delegate bool TryReadGenericTypeArrayElement(ref Utf8JsonReader reader, out TArrayType arrayType);

        public static readonly TryReadGenericTypeArrayElement Delegate;
        static ArrayParseCache()
        {
            Type elementType = typeof(TArrayType).GetGenericArguments()[0];
            MethodInfo method = typeof(JsonHelper)
                .GetMethod("TryReadGenericEquatableArrayElements", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException("Method not found: JsonHelper.TryReadGenericEquatableArrayElements");

            Delegate = (TryReadGenericTypeArrayElement)method.MakeGenericMethod(elementType).CreateDelegate(typeof(TryReadGenericTypeArrayElement));
        }
    }

    private static class DictionaryPairParseCache<TDictionaryPairType>
    {
        public delegate bool TryReadDictionaryPair(ref Utf8JsonReader reader, out TDictionaryPairType arrayType);

        public static readonly TryReadDictionaryPair Delegate;
        static DictionaryPairParseCache()
        {
            Type elementType = typeof(TDictionaryPairType).GetGenericArguments()[0];
            MethodInfo method = typeof(JsonHelper)
                .GetMethod("TryReadDictionaryPair", BindingFlags.NonPublic | BindingFlags.Static)
                ?? throw new MissingMethodException("Method not found: JsonHelper.TryReadDictionaryPair");

            Delegate = (TryReadDictionaryPair)method.MakeGenericMethod(elementType).CreateDelegate(typeof(TryReadDictionaryPair));
        }
    }
}
