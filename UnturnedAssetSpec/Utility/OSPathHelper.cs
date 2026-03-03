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

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    public static ReadOnlySpan<char> GetFileName(string path)
    {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return Path.GetFileName(path.AsSpan());
#else
        return Path.GetFileName(path);
#endif
    }

    /// <summary>
    /// Checks whether a path has the given extension.
    /// </summary>
    /// <param name="path">The original file path.</param>
    /// <param name="ext">The extension. Works whether or not the period is included.</param>
    public static bool IsExtension(string path, string ext)
    {
        if (ext.Length == 0)
            return false;

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        ReadOnlySpan<char> extension = Path.GetExtension(path.AsSpan());
#else
        string extension = Path.GetExtension(path);
#endif

        if (ext[0] == '.')
        {
            return extension.Equals(ext, PathComparison);
        }
        
        if (extension.Length == 0)
        {
            return false;
        }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
        return extension.Slice(1)
#else
        return extension.AsSpan(1)
#endif
            .Equals(ext, PathComparison);
    }
}
