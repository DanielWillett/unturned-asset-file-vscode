using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

[JsonConverter(typeof(EquatableArrayConverterFactory))]
public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    public readonly T[] Array;

    public static EquatableArray<T> Empty => new EquatableArray<T>(System.Array.Empty<T>());

    public EquatableArray(T[] array)
    {
        Array = array;
    }

    public EquatableArray(List<T> list)
    {
        Array = list.Count == 0 ? System.Array.Empty<T>() : list.ToArray();
    }

    public EquatableArray(T[] array, int length)
    {
        if (length == array.Length)
        {
            Array = array;
        }
        else
        {
            T[] newArray = new T[length];
            System.Array.Copy(array, newArray, Math.Min(array.Length, length));
            Array = newArray;
        }
    }

    public EquatableArray(int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException(nameof(length));

        Array = length == 0 ? System.Array.Empty<T>() : new T[length];
    }

    /// <inheritdoc />
    public bool Equals(EquatableArray<T> other)
    {
        return EquatableArray.EqualsEquatable(Array, other.Array);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is EquatableArray<T> equatableArray && Equals(equatableArray);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (Array.Length == 0)
            return 0;

        int hash = Array.Length << 16;
        for (int i = 0; i < Array.Length; ++i)
        {
            T value = Array[i];
            int hashCode = value == null ? 0 : value.GetHashCode();
            hash ^= (hashCode << i) | (hashCode >> (32 - i));
        }

        return hash;
    }
}

public class EquatableArrayConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert is { IsGenericType: true, IsValueType: true }
               && typeToConvert.GetGenericTypeDefinition() == typeof(EquatableArray<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        return (JsonConverter)Activator.CreateInstance(typeof(EquatableArrayConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]));
    }
}

public sealed class EquatableArrayConverter<T> : JsonConverter<EquatableArray<T>> where T : IEquatable<T>
{
    private static JsonConverter<T>? GetConverter(JsonSerializerOptions options)
    {
        return options.GetConverter(typeof(T)) as JsonConverter<T>;
    }

    public override EquatableArray<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return default;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} when parsing EquatableArray<{typeof(T).FullName}>.");
        }

        List<T>? list = null;
        if (!reader.Read() || reader.TokenType == JsonTokenType.EndArray)
        {
            return EquatableArray<T>.Empty;
        }

        T? oneValue = default;
        bool hasOneValue = false;

        JsonConverter<T>? converter = GetConverter(options);

        do
        {
            T? value = converter == null
                ? JsonSerializer.Deserialize<T>(ref reader, options)
                : converter.Read(ref reader, typeof(T), options);
            if (!hasOneValue)
            {
                oneValue = value;
                hasOneValue = true;
            }
            else if (list == null)
            {
                list = new List<T>(16) { oneValue!, value };
            }
            else
            {
                list.Add(value);
            }
        } while (reader.Read() && reader.TokenType != JsonTokenType.EndArray);

        if (!hasOneValue)
        {
            return EquatableArray<T>.Empty;
        }

        if (list == null)
        {
            return new EquatableArray<T>(new T[] { oneValue! });
        }

        return new EquatableArray<T>(list);
    }

    public override void Write(Utf8JsonWriter writer, EquatableArray<T> value, JsonSerializerOptions options)
    {
        if (value.Array == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Array.Length == 0)
        {
            writer.WriteStartArray();
            writer.WriteEndArray();
            return;
        }


        writer.WriteStartArray();

        JsonConverter<T>? converter = GetConverter(options);

        if (converter == null)
        {
            foreach (T element in value.Array)
                JsonSerializer.Serialize(writer, element, options);
        }
        else
        {
            foreach (T element in value.Array)
                converter.Write(writer, element, options);
        }

        writer.WriteEndArray();
    }
}

public static class EquatableArray
{
    public static bool Equals<T>(T[]? arr1, T[]? arr2)
    {
        if (arr1 == null)
            return arr2 == null;
        if (arr2 == null)
            return false;

        if (arr1.Length != arr2.Length)
            return false;

        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        for (int i = 0; i < arr1.Length; ++i)
        {
            T val = arr1[i];
            T val2 = arr2[i];
            if (val == null)
            {
                if (val2 != null)
                    return false;
                continue;
            }
            if (val2 == null)
            {
                return false;
            }

            if (!comparer.Equals(val, val2))
            {
                return false;
            }
        }

        return true;
    }

    public static bool EqualsEquatable<T>(T[]? arr1, T[]? arr2) where T : IEquatable<T>
    {
        if (arr1 == null)
            return arr2 == null;
        if (arr2 == null)
            return false;

        if (arr1.Length != arr2.Length)
            return false;

        for (int i = 0; i < arr1.Length; ++i)
        {
            T val = arr1[i];
            T val2 = arr2[i];
            if (val == null)
            {
                if (val2 != null)
                    return false;
                continue;
            }
            if (val2 == null)
            {
                return false;
            }

            if (!val.Equals(val2))
            {
                return false;
            }
        }

        return true;
    }
}