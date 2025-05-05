using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

public readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>> where T : IEquatable<T>
{
    public readonly T[] Array;

    public EquatableArray(T[] array)
    {
        Array = array;
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
        if (Array == null)
            return other.Array == null;
        if (other.Array == null)
            return false;

        T[] arr1 = Array;
        T[] arr2 = other.Array;
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
