using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Case-sensitivity tools for file paths depending on the operating system.
/// </summary>
public static class OSPathHelper
{
    public static bool IsCaseInsensitive { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                                                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static StringComparer PathComparer => IsCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    public static StringComparison PathComparison => IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

#if NETSTANDARD2_1_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    public static ReadOnlySpan<char> GetFileName(string path)
    {
#if NETSTANDARD2_1_OR_GREATER
        return Path.GetFileName(path.AsSpan());
#else
        return Path.GetFileName(path);
#endif
    }
}
