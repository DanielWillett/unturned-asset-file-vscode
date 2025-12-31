using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Back-ported utilities for <see cref="ImmutableArray{T}"/>.
/// </summary>
public static class ImmutableArrayExtensions
{
    /// <summary>
    /// Extracts the underlying array from an <see cref="ImmutableArray{T}"/>.
    /// </summary>
    /// <typeparam name="T">The array's element type.</typeparam>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] UnsafeThaw<T>(this ImmutableArray<T> array)
    {
        return array.IsDefaultOrEmpty ? Array.Empty<T>() : Unsafe.As<ImmutableArray<T>, T[]>(ref array);
    }

    /// <summary>
    /// Creates an <see cref="ImmutableArray{T}"/> from an array without copying.
    /// </summary>
    /// <typeparam name="T">The array's element type.</typeparam>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ImmutableArray<T> UnsafeFreeze<T>(this T[]? array)
    {
        return array is not { Length: > 0 } ? ImmutableArray<T>.Empty : Unsafe.As<T[], ImmutableArray<T>>(ref array);
    }

    /// <summary>
    /// Effeciently creates an <see cref="ImmutableArray{T}"/> from a <see cref="ImmutableArray{T}.Builder"/>.
    /// </summary>
    /// <typeparam name="T">The array's element type.</typeparam>
    public static ImmutableArray<T> MoveToImmutableOrCopy<T>(this ImmutableArray<T>.Builder builder)
    {
        return builder.Capacity == builder.Count ? builder.MoveToImmutable() : builder.ToImmutable();
    }

    /// <summary>
    /// Re-interprets one array as another type without creating a new backing array. Only works on reference types.
    /// </summary>
    public static ImmutableArray<TTo> UnsafeConvert<TFrom, TTo>(this ImmutableArray<TFrom> old) where TTo : class where TFrom : class, TTo
    {
        TFrom[] oldArray = old.UnsafeThaw();
        // ReSharper disable once CoVariantArrayConversion
        return ((TTo[])oldArray).UnsafeFreeze();
    }

    /// <summary>
    /// Run a binary search on an <see cref="ImmutableArray{T}"/> searching each element for a transformation value.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    /// <param name="array">The sorted array to search.</param>
    /// <param name="key">The value to search for.</param>
    /// <param name="converter">Lambda expression to convert from the element type to the actual value.</param>
    /// <param name="comparer">Comparer to use to compare values.</param>
    /// <returns>The index of the found value, or the two's complement NOT of the insertion index.</returns>
    public static int BinaryKeySearch<TKey, TElement>(this ImmutableArray<TElement> array, TKey key, Func<TElement, TKey> converter, IComparer<TKey> comparer)
    {
        int low = 0, high = array.Length - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            switch (comparer.Compare(key, converter(array[mid])))
            {
                case 0:
                    return mid;

                case > 0:
                    low = mid + 1;
                    break;

                default:
                    high = mid - 1;
                    break;
            }
        }

        return ~low;
    }
}
