using System;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class PathHelper
{
    public static bool IsCaseInsensitive { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
                                                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

    public static StringComparer PathComparer => IsCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
}
