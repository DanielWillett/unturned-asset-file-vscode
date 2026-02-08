using System;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class SetOperationsHelper
{
    /// <summary>
    /// Checks whether or not a set starts with the given value.
    /// </summary>
    /// <typeparam name="TSet">The element type of the array.</typeparam>
    /// <typeparam name="TValue">The type of value to check for.</typeparam>
    /// <param name="set">The array that should start with <paramref name="value"/>.</param>
    /// <param name="value">The value that should be the first element in <paramref name="set"/>.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    public static bool StartsWith<TSet, TValue>(EquatableArray<TSet> set, TValue? value, bool caseInsensitive)
        where TSet : IEquatable<TSet>
        where TValue : IEquatable<TValue>
    {
        TSet?[]? setArray = set.Array;
        if (setArray == null || setArray.Length == 0)
        {
            return false;
        }

        EqualityVisitor<TSet> visitor = default;
        visitor.Value = setArray[0];
        visitor.IsNull = visitor.Value == null;
        visitor.CaseInsensitive = caseInsensitive;

        visitor.Accept(value);

        return visitor.IsEqual;
    }

    /// <summary>
    /// Checks whether or not a set starts with the given set.
    /// </summary>
    /// <typeparam name="TSupersetElement">The element type of the array.</typeparam>
    /// <typeparam name="TSubsetElement">The element type of set to check for at the beginning of the set.</typeparam>
    /// <param name="superset">The array that should start with <paramref name="subset"/>.</param>
    /// <param name="subset">The values that should be the first elements in <paramref name="superset"/>.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    public static bool StartsWith<TSupersetElement, TSubsetElement>(EquatableArray<TSupersetElement> superset, EquatableArray<TSubsetElement> subset, bool caseInsensitive)
        where TSupersetElement : IEquatable<TSupersetElement>
        where TSubsetElement : IEquatable<TSubsetElement>
    {
        TSubsetElement?[]? subsetArray = subset.Array;
        TSupersetElement?[]? supersetArray = superset.Array;
        if (subsetArray == null || subsetArray.Length == 0)
            return true;
        
        if (supersetArray == null || supersetArray.Length < subsetArray.Length)
            return false;
        
        EqualityVisitor<TSupersetElement> visitor = default;
        visitor.CaseInsensitive = caseInsensitive;
        for (int i = 0; i < subsetArray.Length; ++i)
        {
            visitor.Value = supersetArray[i];
            visitor.IsNull = visitor.Value != null;
            visitor.IsEqual = false;

            visitor.Accept(subsetArray[i]);

            if (!visitor.IsEqual)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether or not a set ends with the given value.
    /// </summary>
    /// <typeparam name="TSet">The element type of the array.</typeparam>
    /// <typeparam name="TValue">The type of value to check for.</typeparam>
    /// <param name="set">The array that should end with <paramref name="value"/>.</param>
    /// <param name="value">The value that should be the last element in <paramref name="set"/>.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    public static bool EndsWith<TSet, TValue>(EquatableArray<TSet> set, TValue? value, bool caseInsensitive)
        where TSet : IEquatable<TSet>
        where TValue : IEquatable<TValue>
    {
        TSet?[]? setArray = set.Array;
        if (setArray == null || setArray.Length == 0)
        {
            return false;
        }

        EqualityVisitor<TSet> visitor = default;
        visitor.Value = setArray[^1];
        visitor.IsNull = visitor.Value == null;
        visitor.CaseInsensitive = caseInsensitive;

        visitor.Accept(value);

        return visitor.IsEqual;
    }

    /// <summary>
    /// Checks whether or not a set ends with the given set.
    /// </summary>
    /// <typeparam name="TSupersetElement">The element type of the array.</typeparam>
    /// <typeparam name="TSubsetElement">The element type of set to check for at the end of the set.</typeparam>
    /// <param name="superset">The array that should end with <paramref name="subset"/>.</param>
    /// <param name="subset">The values that should be the last elements in <paramref name="superset"/>.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    public static bool EndsWith<TSupersetElement, TSubsetElement>(EquatableArray<TSupersetElement> superset, EquatableArray<TSubsetElement> subset, bool caseInsensitive)
        where TSupersetElement : IEquatable<TSupersetElement>
        where TSubsetElement : IEquatable<TSubsetElement>
    {
        TSubsetElement?[]? subsetArray = subset.Array;
        TSupersetElement?[]? supersetArray = superset.Array;
        if (subsetArray == null || subsetArray.Length == 0)
            return true;
        
        if (supersetArray == null || supersetArray.Length < subsetArray.Length)
            return false;
        
        EqualityVisitor<TSupersetElement> visitor = default;
        visitor.CaseInsensitive = caseInsensitive;
        int diff = supersetArray.Length - subsetArray.Length;
        for (int i = subsetArray.Length - 1; i >= 0; ++i)
        {
            visitor.Value = supersetArray[i + diff];
            visitor.IsNull = visitor.Value != null;
            visitor.IsEqual = false;

            visitor.Accept(subsetArray[i]);

            if (!visitor.IsEqual)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if an array (<paramref name="superset"/>) contains all elements in another array (<paramref name="subset"/>).
    /// </summary>
    /// <typeparam name="TSuperset">The element type of the superset array.</typeparam>
    /// <typeparam name="TSubset">The elemnet type of the subset array.</typeparam>
    /// <param name="superset">The array that should contain elements from <paramref name="subset"/>.</param>
    /// <param name="subset">The array that defines all elements that must be in <paramref name="superset"/>. If the subset contains multiple of the same value, that value must be in the superset that many times.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    public static bool ContainsAll<TSuperset, TSubset>(EquatableArray<TSuperset> superset, EquatableArray<TSubset> subset, bool caseInsensitive)
        where TSuperset : IEquatable<TSuperset>
        where TSubset : IEquatable<TSubset>
    {
        TSubset[] subsetArray = subset.Array;
        TSuperset[] supersetArray = superset.Array;
        if (subsetArray == null || subsetArray.Length == 0)
            return true;

        if (supersetArray == null || supersetArray.Length == 0)
            return false;

        LightweightBitArray array = new LightweightBitArray(supersetArray.Length);
        if (typeof(TSuperset) == typeof(TSubset))
        {
            foreach (TSubset check in subsetArray)
            {
                int index = IndexOf<TSuperset>(supersetArray, Unsafe.As<TSubset, TSuperset>(ref Unsafe.AsRef(in check)), caseInsensitive, ref array);
                if (index < 0)
                    return false;
            }
        }
        else
        {
            foreach (TSubset check in subsetArray)
            {
                int index = IndexOf(supersetArray, check, caseInsensitive, ref array);
                if (index < 0)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if an array (<paramref name="superset"/>) contains a <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TSuperset">The element type of the superset array.</typeparam>
    /// <typeparam name="TValue">The type of value being checked for.</typeparam>
    /// <param name="superset">The array that should contain elements from <paramref name="subset"/>.</param>
    /// <param name="value">The value that must exist in the <paramref name="superset"/>.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    public static bool Contains<TSuperset, TValue>(EquatableArray<TSuperset> superset, TValue? value, bool caseInsensitive)
        where TSuperset : IEquatable<TSuperset>
        where TValue : IEquatable<TValue>
    {
        return IndexOf(superset, value, caseInsensitive) >= 0;
    }

    /// <summary>
    /// Finds the first index of a <paramref name="value"/> in an array (<paramref name="superset"/>).
    /// </summary>
    /// <typeparam name="TSuperset">The element type of the superset array.</typeparam>
    /// <typeparam name="TValue">The type of value being checked for.</typeparam>
    /// <param name="superset">The array that should contain elements from <paramref name="subset"/>.</param>
    /// <param name="value">The value that must exist in the <paramref name="superset"/>.</param>
    /// <param name="caseInsensitive">Whether or not comparisons should be case-insensitive.</param>
    /// <returns>The index of <paramref name="value"/> in <paramref name="superset"/>, or <c>-1</c> if it's not found.</returns>
    public static int IndexOf<TSuperset, TValue>(EquatableArray<TSuperset> superset, TValue? value, bool caseInsensitive)
        where TSuperset : IEquatable<TSuperset>
        where TValue : IEquatable<TValue>
    {
        TSuperset[] supersetArray = superset.Array;
        if (supersetArray == null || supersetArray.Length == 0)
            return -1;

        LightweightBitArray array = default;

        int index = typeof(TSuperset) == typeof(TValue)
            ? IndexOf<TSuperset>(supersetArray, Unsafe.As<TValue?, TSuperset?>(ref value), caseInsensitive, ref array)
            : IndexOf(supersetArray, value, caseInsensitive, ref array);

        return index;
    }

    private static int IndexOf<TSuperset, TValue>(TSuperset?[] array, TValue? value, bool caseInsensitive, ref LightweightBitArray mask)
        where TSuperset : IEquatable<TSuperset>
        where TValue : IEquatable<TValue>
    {
        if (!ConvertVisitor<TSuperset>.TryConvert(value, out TSuperset? v))
        {
            return -1;
        }

        return IndexOf<TSuperset>(array, v, caseInsensitive, ref mask);
    }

    private static int IndexOf<TValue>(TValue?[] array, TValue? value, bool caseInsensitive, ref LightweightBitArray mask)
    {
        StringComparison comparison = caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        if (typeof(TValue) == typeof(string))
        {
            string? v = Unsafe.As<TValue?, string?>(ref value);
            string?[] arr = Unsafe.As<TValue?[], string?[]>(ref array);
            for (int i = 0; i < arr.Length; ++i)
            {
                if (mask[i]) continue;
                if (string.Equals(arr[i], v, comparison))
                    return i;
            }

            return -1;
        }

        if (typeof(TValue) == typeof(char))
        {
            char v = Unsafe.As<TValue?, char>(ref value);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
            ReadOnlySpan<char> character = MemoryMarshal.CreateReadOnlySpan(ref v, 1);
#else
            Span<char> character = stackalloc char[1];
            character[0] = v;
#endif
            ReadOnlySpan<char> arr = (array as char[])!;
            for (int i = 0; i < arr.Length; ++i)
            {
                if (mask[i]) continue;
                ReadOnlySpan<char> singleSpan = arr.Slice(i, 1);
                if (singleSpan.CompareTo(character, comparison) == 0)
                    return i;
            }

            return -1;
        }

        int lastIndex = -1;
        do
        {
            int index = Array.IndexOf(array, value, lastIndex + 1);
            if (index < 0)
                return -1;

            if (!mask[index])
            {
                mask[index] = true;
                return index;
            }

            lastIndex = index;
        } while (lastIndex + 1 < array.Length);

        return -1;
    }
}