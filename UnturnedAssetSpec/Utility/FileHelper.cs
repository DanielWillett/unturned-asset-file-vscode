using System;
using System.IO;
using System.Runtime.InteropServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal static class FileHelper
{
    // wtf microsoft
    private static readonly DateTime FileNotExistsWriteTimeReturnValue
        = new DateTime(1601, 01, 01, 00, 00, 00);

    public static DateTime GetLastWriteTimeUTCSafe(string file, DateTime defaultValue)
    {
        DateTime dt;
        try
        {
            dt = File.GetLastWriteTimeUtc(file);
        }
        catch
        {
            return defaultValue;
        }

        return dt == FileNotExistsWriteTimeReturnValue ? defaultValue : dt;
    }

    public static bool HasSpaceToStoreBytesAt(string fileLocation, long byteCount)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileLocation = Path.GetPathRoot(fileLocation);
        }

        return new DriveInfo(fileLocation).AvailableFreeSpace >= byteCount;
    }
}