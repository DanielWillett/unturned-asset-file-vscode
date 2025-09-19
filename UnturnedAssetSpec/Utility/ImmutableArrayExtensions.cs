using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class ImmutableArrayExtensions
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] UnsafeThaw<T>(this ImmutableArray<T> array)
    {
        return array.IsDefaultOrEmpty ? Array.Empty<T>() : Unsafe.As<ImmutableArray<T>, T[]>(ref array);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ImmutableArray<T> UnsafeFreeze<T>(this T[]? array)
    {
        return array is not { Length: > 0 } ? ImmutableArray<T>.Empty : Unsafe.As<T[], ImmutableArray<T>>(ref array);
    }
}
