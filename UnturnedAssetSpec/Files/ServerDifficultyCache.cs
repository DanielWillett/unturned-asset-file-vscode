using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Caches the difficulty pertaining to files.
/// </summary>
public readonly struct ServerDifficultyCache
{
    private readonly ConcurrentDictionary<string, ServerDifficulty> _cache;
    private readonly HashSet<string> _files;

    private ServerDifficultyCache(ConcurrentDictionary<string, ServerDifficulty> cache)
    {
        _cache = cache;
        _files = new HashSet<string>(OSPathHelper.PathComparer);
    }

    public static ServerDifficultyCache Create()
    {
        return new ServerDifficultyCache(new ConcurrentDictionary<string, ServerDifficulty>(OSPathHelper.PathComparer));
    }

    public bool RemoveCachedFile(string oldFilePath)
    {
        if (oldFilePath.EndsWith("Commands.dat", OSPathHelper.PathComparison))
        {
            return _cache.TryRemove(oldFilePath, out _);
        }
#if NETSTANDARD2_1_OR_GREATER
        ReadOnlySpan<char> dirName = Path.GetDirectoryName(oldFilePath.AsSpan());
        if (dirName.IsEmpty)
            return false;
#else
        string? dirName = Path.GetDirectoryName(oldFilePath);
        if (string.IsNullOrEmpty(dirName))
            return false;
#endif
        lock (_files)
        {
            foreach (string configFile in _files)
            {
                if (configFile.StartsWith(dirName, OSPathHelper.PathComparison)
                    && configFile.Length > dirName.Length
                    && (configFile[dirName.Length] == Path.DirectorySeparatorChar || configFile[dirName.Length] == Path.AltDirectorySeparatorChar))
                {
                    return false;
                }
            }

            string cmd = GetCommandFilePath(oldFilePath);
            return !_files.Contains(cmd) && _cache.TryRemove(cmd, out _);
        }
    }

    public void HandleCommandFileUpdated(string commandFilePath)
    {
        string? dir = Path.GetDirectoryName(commandFilePath);
        if (dir == null)
            return;

        foreach (string path in _cache.Keys)
        {
            if (!path.StartsWith(dir, OSPathHelper.PathComparison))
                continue;

            _cache.TryRemove(path, out _);
        }
    }

    public bool TryGetDifficulty(string filePath, out ServerDifficulty difficulty)
    {
        if (filePath.EndsWith("Config_EasyDifficulty.txt", OSPathHelper.PathComparison))
        {
            difficulty = ServerDifficulty.Easy;
            return true;
        }

        if (filePath.EndsWith("Config_NormalDifficulty.txt", OSPathHelper.PathComparison))
        {
            difficulty = ServerDifficulty.Normal;
            return true;
        }

        if (filePath.EndsWith("Config_HardDifficulty.txt", OSPathHelper.PathComparison))
        {
            difficulty = ServerDifficulty.Hard;
            return true;
        }

        // note: small issue here...
        // The check from Commands.dat for the Mode command uses the localization values
        // for ModeEasy/Normal/Hard instead of hard-coded which makes this imperfect.

        string commandFilePath;
        if (filePath.EndsWith("Commands.dat", OSPathHelper.PathComparison))
        {
            if (_cache.TryGetValue(filePath, out difficulty))
                return true;
            commandFilePath = filePath;
        }
        else
        {
            lock (_files)
            {
                _files.Add(filePath);
            }
            commandFilePath = GetCommandFilePath(filePath);
            if (_cache.TryGetValue(commandFilePath, out difficulty))
                return true;
        }

        if (!File.Exists(commandFilePath))
        {
            return false;
        }

        difficulty = ServerDifficulty.Normal;

        lock (_cache)
        {
            using FileStream fs = new FileStream(commandFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, FileOptions.SequentialScan);
            using StreamReader sr = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, 1024, leaveOpen: true);
            while (sr.ReadLine() is { } line)
            {
                if (!line.StartsWith("Mode", StringComparison.OrdinalIgnoreCase) || line.Length <= 5)
                    continue;

                ReadOnlySpan<char> command = line.AsSpan(5);
                if (command.Equals("Easy", StringComparison.OrdinalIgnoreCase))
                    difficulty = ServerDifficulty.Easy;
                else if (command.Equals("Normal", StringComparison.OrdinalIgnoreCase))
                    difficulty = ServerDifficulty.Normal;
                else if (command.Equals("Hard", StringComparison.OrdinalIgnoreCase))
                    difficulty = ServerDifficulty.Hard;
            }
        }

        _cache[filePath] = difficulty;
        return true;
    }

    private static string GetCommandFilePath(string filePath)
    {
#if NETSTANDARD2_1_OR_GREATER
        ReadOnlySpan<char> dirName = Path.GetDirectoryName(filePath.AsSpan());
        string commandFilePath = Path.Join(dirName, "Server", "Commands.dat");
#else
        string? dirName = Path.GetDirectoryName(filePath);
        string commandFilePath = Path.Combine(dirName ?? string.Empty, "Server", "Commands.dat");
#endif
        return commandFilePath;
    }
}