using System;
using System.Globalization;
using System.IO;
using System.IO.Hashing;
using System.Runtime.InteropServices;
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

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

    /// <summary>
    /// Checks whether a path has the given extension.
    /// </summary>
    /// <param name="path">The original file path.</param>
    /// <param name="ext">The extension. Works whether or not the period is included.</param>
    public static bool IsExtension(ReadOnlySpan<char> path, string ext)
    {
        if (ext.Length == 0)
            return false;

        ReadOnlySpan<char> extension = GetExtension(path);

        if (ext[0] == '.')
        {
            return extension.Equals(ext, PathComparison);
        }
        
        if (extension.Length == 0)
        {
            return false;
        }

        return extension.Slice(1).Equals(ext, PathComparison);
    }

    /// <summary>
    /// Checks whether a path has the given file name.
    /// </summary>
    /// <param name="path">The original file path.</param>
    /// <param name="fileName">The file name.</param>
    public static bool IsFileName(string path, string fileName)
    {
        if (fileName.Length == 0)
            return false;

        int offset = path.Length;
        int start = offset - fileName.Length;
        if (start < 0)
            return false;

        ReadOnlySpan<char> correspondingSection = path.AsSpan(start);
        if (!correspondingSection.Equals(fileName, PathComparison))
        {
            return false;
        }

        if (start == 0)
            return true;

        char prevChar = path[start - 1];
        return prevChar == Path.DirectorySeparatorChar || prevChar == Path.AltDirectorySeparatorChar;
    }

    /// <summary>
    /// Checks whether a <paramref name="path"/> is within the given <paramref name="directory"/>.
    /// Expects that both paths are absolute and normalized.
    /// </summary>
    /// <remarks>Effectively the same as <c><paramref name="path"/>.StartsWith(<paramref name="directory"/>, <see cref="PathComparison"/>)</c> but handles the following edge case:
    /// <code>"Folder1/Folder23".StartsWith("Folder1/Folder2")</code>
    /// </remarks>
    public static bool Contains(string directory, string path)
    {
        if (directory.Length == 0 || directory.Length > path.Length)
            return false;

        char lastChar = directory[^1];
        // trim trailing slash
        if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar)
            return Contains(directory.AsSpan(0, directory.Length - 1), path);

        if (!path.StartsWith(directory, PathComparison))
            return false;

        if (path.Length == directory.Length)
            return true;

        lastChar = path[directory.Length];
        return lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar;
    }

    /// <inheritdoc cref="Contains(string,string)"/>
    public static bool Contains(ReadOnlySpan<char> directory, ReadOnlySpan<char> path)
    {
        if (directory.Length == 0 || directory.Length > path.Length)
            return false;

        // trim trailing slash
        char lastChar = directory[^1];
        if (lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar)
        {
            directory = directory[..^1];
        }

        if (!path.StartsWith(directory, PathComparison))
            return false;

        if (path.Length == directory.Length)
            return true;

        lastChar = path[directory.Length];
        return lastChar == Path.DirectorySeparatorChar || lastChar == Path.AltDirectorySeparatorChar;
    }

    /// <summary>
    /// Replaces all instances of <see cref="Path.DirectorySeparatorChar"/> with '<c>/</c>'.
    /// </summary>
    /// <remarks>No-op if <see cref="Path.DirectorySeparatorChar"/> is already '<c>/</c>'.</remarks>
    public static string ReplaceWithUnixSeparators(string path)
    {
        if (Path.DirectorySeparatorChar == '/')
            return path;

#if !NETCOREAPP2_1_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
        // see span implementation
        if (path.Length > 256)
            return path.Replace(Path.DirectorySeparatorChar, '/');
#endif

        return ReplaceWithUnixSeparators(path.AsSpan());
    }

    /// <inheritdoc cref="ReplaceWithUnixSeparators(string)"/>
    public static string ReplaceWithUnixSeparators(ReadOnlySpan<char> path)
    {
        if (Path.DirectorySeparatorChar == '/')
            return path.ToString();
#if NET9_0_OR_GREATER
        return string.Create(path.Length, path, static (span, state) =>
        {
            ReplaceToSpan(state, span, Path.DirectorySeparatorChar, '/');
        });
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        unsafe
        {
            UnixPathState state;
            state.Span = &path;
            return string.Create(path.Length, state, static (span, state) =>
            {
                ReplaceToSpan(*state.Span, span, Path.DirectorySeparatorChar, '/');
            });
        }
#else
        if (path.Length > 256)
        {
            // avoid stack overflow (results in a ~512B maximum stackalloc)
            return path.ToString().Replace(Path.DirectorySeparatorChar, '/');
        }

        Span<char> buffer = stackalloc char[path.Length];
        ReplaceToSpan(path, buffer, Path.DirectorySeparatorChar, '/');
        return buffer.ToString();
#endif
    }

    private static void ReplaceToSpan(ReadOnlySpan<char> src, Span<char> dst, char from, char to)
    {
        src.CopyTo(dst);
        int index = -1;
        while (index + 1 < src.Length)
        {
            int startIndex = index + 1;
            int newIndex = src.Slice(startIndex).IndexOf(from);
            if (newIndex < 0)
                break;
            newIndex += startIndex;
            dst[newIndex] = to;
            index = newIndex;
        }
    }

#if !NET9_0_OR_GREATER && (NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER)
    private unsafe struct UnixPathState
    {
        public ReadOnlySpan<char>* Span;
    }
#endif

    /// <summary>
    /// Removes the trailing extension and period.
    /// <para>
    /// Example: <c>"Folder\File.txt"</c> -> <c>"Folder\File"</c>.
    /// </para>
    /// <remarks>Only works with strings that use the current platform's path separator.</remarks>
    /// </summary>
    public static ReadOnlySpan<char> RemoveExtension(ReadOnlySpan<char> path)
    {
        int extIndex = path.LastIndexOf('.');
        int fileNameIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (extIndex >= 0 && extIndex > fileNameIndex)
        {
            path = path.Slice(0, extIndex);
        }

        return path;
    }

    /// <summary>
    /// Gets the extension of a file, including the period.
    /// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    public static ReadOnlySpan<char> GetExtension(ReadOnlySpan<char> path)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return Path.GetExtension(path);
#else
        int extIndex = path.LastIndexOf('.');
        int fileNameIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (Path.AltDirectorySeparatorChar != Path.DirectorySeparatorChar)
        {
            int altIndex = path.LastIndexOf(Path.AltDirectorySeparatorChar);
            fileNameIndex = Math.Max(fileNameIndex, altIndex);
        }

        if (extIndex >= 0 && extIndex > fileNameIndex)
        {
            return path.Slice(extIndex);
        }

        return ReadOnlySpan<char>.Empty;
#endif
    }

    /// <summary>
    /// Gets the name of a file or directory.
    /// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    public static ReadOnlySpan<char> GetFileName(ReadOnlySpan<char> path)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return Path.GetFileName(path);
#else
        int fileNameIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (Path.AltDirectorySeparatorChar != Path.DirectorySeparatorChar)
        {
            int altIndex = path.LastIndexOf(Path.AltDirectorySeparatorChar);
            fileNameIndex = Math.Max(fileNameIndex, altIndex);
        }

        return fileNameIndex >= 0 && fileNameIndex + 1 < path.Length ? path.Slice(fileNameIndex + 1) : ReadOnlySpan<char>.Empty;
#endif
    }

    /// <summary>
    /// Gets the name of a directory or file without it's extension.
    /// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
    public static ReadOnlySpan<char> GetFileNameWithoutExtension(ReadOnlySpan<char> path)
    {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        return Path.GetFileNameWithoutExtension(path);
#else
        ReadOnlySpan<char> fileName = GetFileName(path);
        int extStart = fileName.LastIndexOf('.');
        return extStart >= 0 ? fileName[..extStart] : fileName;
#endif
    }

    /// <summary>
    /// Removes the trailing file/directory name.
    /// <para>
    /// Examples:<br/>
    /// <c>"Folder\File.txt"</c> -> <c>"Folder"</c><br/>
    /// <c>"File.txt"</c> -> <c>""</c><br/>
    /// <c>"./File.txt"</c> -> <c>""</c>
    /// </para>
    /// <remarks>Only works with strings that use the current platform's path separator.</remarks>
    /// </summary>
    /// <returns>The original path with the file name removed. An empty span if the file path doesn't have a directory.</returns>
    public static ReadOnlySpan<char> GetDirectoryName(ReadOnlySpan<char> path)
    {
        int fileNameIndex = path.LastIndexOf(Path.DirectorySeparatorChar);
        if (fileNameIndex >= 0)
        {
            path = path.Slice(0, fileNameIndex);
            if (path.Length == 1 && path[0] == '.')
            {
                return ReadOnlySpan<char>.Empty;
            }
        }
        else
        {
            path = ReadOnlySpan<char>.Empty;
        }

        return path;
    }

    /// <summary>
    /// Combines a root folder with the given path, converting Unix directory separators to the current platform's directory separator.
    /// </summary>
    /// <param name="baseFolder">The base folder which should already be using the correct separators.</param>
    /// <param name="path">The relative path that may not be using the correct separators.</param>
    /// <returns>The two paths combined.</returns>
    public static string CombineWithBaseFolderAndFixDirectorySeparators(ReadOnlySpan<char> baseFolder, ReadOnlySpan<char> path)
    {
        TrimSeparatorsFromCombiningPaths(ref baseFolder, ref path);

        int length = baseFolder.Length + 1 + path.Length;
#if NET9_0_OR_GREATER
        CombinePathState state;
        state.Base = baseFolder;
        state.Span = path;
        return string.Create(length, state, static (span, state) =>
        {
            Core(span, state.Base, state.Span);
        });
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        unsafe
        {
            CombinePathState state;
            state.Base = &baseFolder;
            state.Span = &path;
            return string.Create(length, state, static (span, state) =>
            {
                Core(span, *state.Base, *state.Span);
            });
        }
#else
        if (length > 256)
        {
            return Path.GetFullPath(Path.Combine(baseFolder.ToString(), path.ToString()));
        }

        Span<char> buffer = stackalloc char[length];
        Core(buffer, baseFolder, path);
        return buffer.ToString();
#endif

        static void Core(Span<char> buffer, ReadOnlySpan<char> baseFolder, ReadOnlySpan<char> path)
        {
            baseFolder.CopyTo(buffer);
            int index = baseFolder.Length;
            buffer[index] = Path.DirectorySeparatorChar;
            ++index;
            if (Path.DirectorySeparatorChar != '/')
            {
                ReplaceToSpan(path, buffer[index..], '/', Path.DirectorySeparatorChar);
            }
            else
            {
                path.CopyTo(buffer[index..]);
            }
        }
    }

    private static void TrimSeparatorsFromCombiningPaths(ref ReadOnlySpan<char> baseFolder, ref ReadOnlySpan<char> path)
    {
        if (!baseFolder.IsEmpty)
        {
            char last = baseFolder[^1];
            if (last == Path.DirectorySeparatorChar || last == '/')
            {
                baseFolder = baseFolder[..^1];
            }
        }

        if (!path.IsEmpty)
        {
            char last = path[0];
            if (last == Path.DirectorySeparatorChar || last == '/')
            {
                path = path[1..];
            }
        }
    }

#if NET9_0_OR_GREATER
    private ref struct CombinePathState
    {
        public ReadOnlySpan<char> Base;
        public ReadOnlySpan<char> Span;
    }
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    private unsafe struct CombinePathState
    {
        public ReadOnlySpan<char>* Base;
        public ReadOnlySpan<char>* Span;
    }
#endif

    /// <summary>
    /// Combines a folder to a file name, concatenating text to the end (usually a file extension).
    /// </summary>
    /// <param name="baseFolder">The first part of the path. Can be empty.</param>
    /// <param name="path">The second part of the path.</param>
    /// <param name="concatToFileName">Text to append to the end of <paramref name="path"/> before combining. Usually an extension.</param>
    /// <param name="insertBeforeExtension">Whether or not to insert <paramref name="concatToFileName"/> before <paramref name="path"/>'s extension.</param>
    public static string CombineAndConcat(
        ReadOnlySpan<char> baseFolder,
        ReadOnlySpan<char> path,
        ReadOnlySpan<char> concatToFileName,
        bool insertBeforeExtension = false)
    {
        ReadOnlySpan<char> tail = ReadOnlySpan<char>.Empty;
        
        if (insertBeforeExtension)
        {
            ReadOnlySpan<char> old = path;
            path = RemoveExtension(path);
            tail = old.Slice(path.Length);
        }

        TrimSeparatorsFromCombiningPaths(ref baseFolder, ref path);

        int length = baseFolder.Length + 1 + path.Length + concatToFileName.Length + tail.Length;

#if NET9_0_OR_GREATER
        CombinePathAndConcatState state;
        state.Base = baseFolder;
        state.Span = path;
        state.Concat = concatToFileName;
        state.Tail = tail;
        return string.Create(length, state, static (span, state) =>
        {
            Core(span, state.Base, state.Span, state.Concat, state.Tail);
        });
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        unsafe
        {
            CombinePathAndConcatState state;
            state.Base = &baseFolder;
            state.Span = &path;
            state.Concat = &concatToFileName;
            state.Tail = &tail;
            return string.Create(length, state, static (span, state) =>
            {
                Core(span, *state.Base, *state.Span, *state.Concat, *state.Tail);
            });
        }
#else
        if (length > 256)
        {
            char[] buffer = new char[length];
            Core(buffer, baseFolder, path, concatToFileName, tail);
            return new string(buffer);
        }
        else
        {
            Span<char> buffer = stackalloc char[length];
            Core(buffer, baseFolder, path, concatToFileName, tail);
            return buffer.ToString();
        }
#endif

        static void Core(Span<char> buffer, ReadOnlySpan<char> baseFolder, ReadOnlySpan<char> path, ReadOnlySpan<char> concatToFileName, ReadOnlySpan<char> tail)
        {
            int index = baseFolder.Length;
            if (!baseFolder.IsEmpty)
            {
                baseFolder.CopyTo(buffer);
                buffer[index] = Path.DirectorySeparatorChar;
                ++index;
            }
            if (Path.DirectorySeparatorChar != '/')
            {
                ReplaceToSpan(path, buffer[index..], '/', Path.DirectorySeparatorChar);
            }
            else
            {
                path.CopyTo(buffer[index..]);
            }

            index += path.Length;

            concatToFileName.CopyTo(buffer[index..]);
            if (tail.IsEmpty)
                return;

            index += concatToFileName.Length;
            tail.CopyTo(buffer[index..]);
        }
    }

#if NET9_0_OR_GREATER
    private ref struct CombinePathAndConcatState
    {
        public ReadOnlySpan<char> Base;
        public ReadOnlySpan<char> Span;
        public ReadOnlySpan<char> Concat;
        public ReadOnlySpan<char> Tail;
    }
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    private unsafe struct CombinePathAndConcatState
    {
        public ReadOnlySpan<char>* Base;
        public ReadOnlySpan<char>* Span;
        public ReadOnlySpan<char>* Concat;
        public ReadOnlySpan<char>* Tail;
    }
#endif

    internal static int CreateStablePathHash(ReadOnlySpan<char> path)
    {
        if (!IsCaseInsensitive)
        {
            return DoXxHash32(MemoryMarshal.Cast<char, byte>(path));
        }

        Span<byte> data = stackalloc byte[path.Length];
        for (int i = 0; i < path.Length; ++i)
        {
            char c = path[i];
            if (c > byte.MaxValue)
            {
                // non-ascii
                string p = path.ToString().ToLowerInvariant();
                return DoXxHash32(MemoryMarshal.Cast<char, byte>(p.AsSpan()));
            }

            if (c is >= 'A' and <= 'Z')
                data[i] = (byte)(c + 32);
            else
                data[i] = (byte)c;
        }

        return DoXxHash32(data);

        static int DoXxHash32(ReadOnlySpan<byte> data)
        {
            Span<byte> hash = stackalloc byte[4];
            XxHash32.Hash(data, hash);
            return MemoryMarshal.Read<int>(hash);
        }
    }


    public static string CombineWithUnixSeparators(ReadOnlySpan<char> p1, ReadOnlySpan<char> p2)
    {
        TrimSeparatorsFromCombiningPaths(ref p1, ref p2);

        if (p1.IsEmpty)
        {
            return p2.ToString();
        }

        if (p2.IsEmpty)
        {
            return p1.ToString();
        }

        int length = p1.Length + 1 + p2.Length;

#if NET9_0_OR_GREATER
        CombineWithUnixSeparatorsState state;
        state.P1 = p1;
        state.P2 = p2;
        return string.Create(length, state, static (span, state) =>
        {
            Core(span, state.P1, state.P2);
        });
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
        unsafe
        {
            CombineWithUnixSeparatorsState state;
            state.P1 = &p1;
            state.P2 = &p2;
            return string.Create(length, state, static (span, state) =>
            {
                Core(span, *state.P1, *state.P2);
            });
        }
#else
        if (length > 256)
        {
            char[] buffer = new char[length];
            Core(buffer, p1, p2);
            return new string(buffer);
        }
        else
        {
            Span<char> buffer = stackalloc char[length];
            Core(buffer, p1, p2);
            return buffer.ToString();
        }
#endif

        static void Core(Span<char> buffer, ReadOnlySpan<char> p1, ReadOnlySpan<char> p2)
        {
            ReplaceToSpan(p1, buffer, '\\', '/');
            buffer[p1.Length] = '/';
            ReplaceToSpan(p2, buffer.Slice(p1.Length + 1), '\\', '/');
        }
    }

#if NET9_0_OR_GREATER
    private ref struct CombineWithUnixSeparatorsState
    {
        public ReadOnlySpan<char> P1;
        public ReadOnlySpan<char> P2;
    }
#elif NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
    private unsafe struct CombineWithUnixSeparatorsState
    {
        public ReadOnlySpan<char>* P1;
        public ReadOnlySpan<char>* P2;
    }
#endif
}
