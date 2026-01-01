using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class CollectionHelper
{
    public static bool ContainsSameElements<T>(T[] arr1, T[] arr2, IEqualityComparer<T>? comparer = null)
    {
        if (arr1.Length == 0 && arr2.Length == 0)
            return true;
        if (arr1.Length == 0 || arr2.Length == 0)
            return false;

        comparer ??= EqualityComparer<T>.Default;

        if (arr1.Length > arr2.Length)
        {
            (arr1, arr2) = (arr2, arr1);
        }

        LightweightBitArray arr2Bits = new LightweightBitArray(arr2.Length);
        int hits = 0;
        for (int i = 0; i < arr1.Length; ++i)
        {
            T v1 = arr1[i];
            bool contains = false;
            for (int j = i; j < arr2.Length; ++j)
            {
                if (arr2Bits[j])
                    continue;

                T v2 = arr2[i];
                if (!comparer.Equals(v1, v2))
                    continue;

                contains = true;
                arr2Bits[j] = true;
                ++hits;
                break;
            }

            if (contains)
                continue;

            for (int j = 0; j < i; ++j)
            {
                if (arr2Bits[j])
                    continue;

                T v2 = arr2[i];
                if (!comparer.Equals(v1, v2))
                    continue;

                contains = true;
                arr2Bits[j] = true;
                ++hits;
                break;
            }

            if (!contains)
                return false;
        }

        return hits == arr2.Length;
    }

    public static bool ContainsSameElements<T>(IList<T> list1, IList<T> list2, IEqualityComparer<T>? comparer = null)
    {
        int c1 = list1.Count;
        int c2 = list2.Count;
        if (c1 == 0 && c2 == 0)
            return true;
        if (c1 == 0 || c2 == 0)
            return false;

        comparer ??= EqualityComparer<T>.Default;

        if (c1 > c2)
        {
            (list1, list2) = (list2, list1);
            (c1, c2) = (c2, c1);
        }

        LightweightBitArray arr2Bits = new LightweightBitArray(c2);
        int hits = 0;
        for (int i = 0; i < c1; ++i)
        {
            T v1 = list1[i];
            bool contains = false;
            for (int j = i; j < c2; ++j)
            {
                T v2 = list2[i];
                if (arr2Bits[j] || !comparer.Equals(v1, v2))
                    continue;

                contains = true;
                arr2Bits[j] = true;
                ++hits;
                break;
            }

            if (contains)
                continue;

            for (int j = 0; j < i; ++j)
            {
                T v2 = list2[i];
                if (arr2Bits[j] || !comparer.Equals(v1, v2))
                    continue;

                contains = true;
                arr2Bits[j] = true;
                ++hits;
                break;
            }

            if (!contains)
                return false;
        }

        return hits == c2;
    }
}
