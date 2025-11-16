using System;
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
}
