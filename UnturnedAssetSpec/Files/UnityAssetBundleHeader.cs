using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;


/// <summary>
/// Utility to read the headers of UnityFS AssetBundle files.
/// </summary>
/// <remarks>Written with help from <see href="https://imbushuo.net/blog/archives/505/"/>.</remarks>
[DebuggerDisplay("{ToString(),nq}")]
public class UnityAssetBundleHeader
{
    public string FileSystem { get; }
    public uint FileVersion { get; }
    public string PlayerVersion { get; }
    public UnityEngineVersion EngineVersion { get; }
    public ulong Size { get; }

    public UnityAssetBundleHeader(string fileSystem, uint fileVersion, string playerVersion, UnityEngineVersion engineVersion, ulong size)
    {
        FileSystem = fileSystem;
        FileVersion = fileVersion;
        PlayerVersion = playerVersion;
        EngineVersion = engineVersion;
        Size = size;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{{{FileSystem}: v{FileVersion.ToString(CultureInfo.CurrentCulture)} ({PlayerVersion}, {EngineVersion.ToString()}): {Size.ToString("N0", CultureInfo.CurrentCulture)} B}}";
    }

    /// <summary>
    /// Reads an asset bundle header from a file.
    /// </summary>
    /// <exception cref="FormatException">Invalid or unsupported asset bundle header.</exception>
    public static UnityAssetBundleHeader FromFile(string file, out int bytesRead)
    {
        return FromStream(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 48, FileOptions.SequentialScan), out bytesRead, leaveOpen: false);
    }

    /// <summary>
    /// Reads an asset bundle header from a stream
    /// </summary>
    /// <exception cref="FormatException">Invalid or unsupported asset bundle header.</exception>
    public static UnityAssetBundleHeader FromStream(Stream stream, out int bytesRead, bool leaveOpen = true)
    {
        try
        {
            bytesRead = 0;
            // 55 6E 69 74 79 46 53 00
            byte[] buffer = new byte[48];

            int b;
            int read = 0;
            while ((b = stream.ReadByte()) != -1)
            {
                buffer[read] = (byte)b;
                ++read;
                if (b == 0)
                    break;
            }

            bytesRead += read;
            if (b == -1)
            {
                throw new FormatException("Stream run out.");
            }

            if (read <= 1)
            {
                throw new FormatException("Failed to find file system.");
            }

            string fileSystem = Encoding.ASCII.GetString(buffer, 0, read - 1);

            read = stream.Read(buffer, 0, sizeof(uint));
            bytesRead += read;

            if (read < sizeof(uint))
            {
                throw new FormatException("Stream run out.");
            }

            uint version = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(buffer, 0));

            read = 0;
            while ((b = stream.ReadByte()) != -1)
            {
                buffer[read] = (byte)b;
                ++read;
                if (b == 0)
                    break;
            }

            bytesRead += read;
            if (b == -1)
            {
                throw new FormatException("Stream run out.");
            }

            if (read <= 1)
            {
                throw new FormatException("Failed to find player version.");
            }

            string playerVersion = Encoding.ASCII.GetString(buffer, 0, read - 1);

            read = 0;
            while ((b = stream.ReadByte()) != -1)
            {
                buffer[read] = (byte)b;
                ++read;
                if (b == 0)
                    break;
            }

            bytesRead += read;
            if (b == -1)
            {
                throw new FormatException("Stream run out.");
            }

            if (read <= 1)
            {
                throw new FormatException("Failed to find editor version.");
            }

            UnityEngineVersion editorVersion = UnityEngineVersion.Parse(Encoding.ASCII.GetString(buffer, 0, read - 1));

            ulong size;
            if (version >= 6)
            {
                read = stream.Read(buffer, 0, sizeof(ulong));
                bytesRead += read;
                if (read < sizeof(ulong))
                {
                    throw new FormatException("Stream run out.");
                }

                size = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt64(buffer, 0));
            }
            else
            {
                read = stream.Read(buffer, 0, sizeof(uint));
                bytesRead += read;
                if (read < sizeof(uint))
                {
                    throw new FormatException("Stream run out.");
                }

                size = BinaryPrimitives.ReverseEndianness(BitConverter.ToUInt32(buffer, 0));
            }

            return new UnityAssetBundleHeader(fileSystem, version, playerVersion, editorVersion, size);
        }
        finally
        {
            if (!leaveOpen)
                stream.Dispose();
        }
    }

    /// <summary>
    /// Reads an asset bundle header from a binary string.
    /// </summary>
    /// <exception cref="FormatException">Invalid or unsupported asset bundle header.</exception>
    public static unsafe UnityAssetBundleHeader FromBinary(ReadOnlySpan<byte> binary, out int bytesRead)
    {
        int strEnd = 0;
        for (; strEnd < binary.Length && binary[strEnd] != '\0'; strEnd++) ;

        if (strEnd == binary.Length || strEnd == 0)
            throw new FormatException("Failed to find file system.");

        int fileSystemLength = strEnd;

        int index = fileSystemLength + 1;
        if (binary.Length <= 12)
            throw new FormatException("Buffer run out.");

        uint version = MemoryMarshal.Read<uint>(binary.Slice(index));
        if (BitConverter.IsLittleEndian)
            version = BinaryPrimitives.ReverseEndianness(version);

        index += sizeof(uint);

        strEnd = index;
        for (; strEnd < binary.Length && binary[strEnd] != '\0'; strEnd++) ;

        if (strEnd == binary.Length || index == strEnd)
            throw new FormatException("Failed to find player version.");

        int playerVersionStart = index, playerVersionLength = strEnd - index;
        index += playerVersionLength + 1;

        strEnd = index;
        for (; strEnd < binary.Length && binary[strEnd] != '\0'; strEnd++) ;

        if (strEnd == binary.Length || index == strEnd)
            throw new FormatException("Failed to find editor version.");

        int editorVersionStart = index, editorVersionLength = strEnd - index;
        index += editorVersionLength + 1;

        ulong size;
        if (version >= 6)
        {
            size = MemoryMarshal.Read<ulong>(binary.Slice(index));
            if (BitConverter.IsLittleEndian)
                size = BinaryPrimitives.ReverseEndianness(size);

            index += sizeof(ulong);
        }
        else
        {
            uint s = MemoryMarshal.Read<uint>(binary.Slice(index));
            if (BitConverter.IsLittleEndian)
                s = BinaryPrimitives.ReverseEndianness(s);

            size = s;
            index += sizeof(uint);
        }

        bytesRead = index;

        string fileSystem, playerVersion, editorVersion;
        fixed (byte* ptr = binary)
        {
            fileSystem = Encoding.ASCII.GetString(ptr, fileSystemLength);
            playerVersion = Encoding.ASCII.GetString(ptr + playerVersionStart, playerVersionLength);
            editorVersion = Encoding.ASCII.GetString(ptr + editorVersionStart, editorVersionLength);
        }

        return new UnityAssetBundleHeader(fileSystem, version, playerVersion, UnityEngineVersion.Parse(editorVersion), size);
    }
}