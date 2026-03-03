using System;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class LazyUtil
{
    /// <summary>
    /// Creates a <see cref="Lazy{T}"/> instance with a constant value (platform-independent).
    /// </summary>
    public static Lazy<T> CreatePredefinedInstance<T>(T value)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
        Lazy<T> lazy = new Lazy<T>(value);
#else
        Lazy<T> lazy = new Lazy<T>(() => value, LazyThreadSafetyMode.None);
        // instantiate it now to avoid thread-safety issues but also prevent needing a lock later
        _ = lazy.Value;
#endif
        return lazy;
    }
}
