using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Utility to find the install directory for Steam games.
/// </summary>
public class InstallDirUtility
{
    private readonly string _gameName;
    private readonly string _gameId;

    /// <summary>
    /// Picks out the install directory from the library file as the last match
    /// </summary>
    private readonly Regex _libraryVcfFindPathRegex;

    private readonly Action<string> _logMethod;
    private GameInstallDir _installDirectory;

    /// <summary>
    /// Set an explicit directory to use instead of automatically finding it.
    /// </summary>
    public GameInstallDir OverrideInstallDirectory
    {
        get;
        set
        {
            field = value;
            _installDirectory = OverrideInstallDirectory;
        }
    }

    /// <summary>
    /// The cached install directory, only available after the first time <see cref="TryGetInstallDirectory"/> is ran.
    /// </summary>
    /// <exception cref="DirectoryNotFoundException">Could not locate the game's installation directory.</exception>
    public GameInstallDir InstallDirectory
    {
        get
        {
            if (_installDirectory.BaseFolder == null && !TryGetInstallDirectory(out _installDirectory))
            {
                throw new DirectoryNotFoundException($"Failed to locate the {_gameName} installation directory.");
            }

            return _installDirectory;
        }
    }

    public InstallDirUtility(string gameName, string gameId)
    {
        _gameName = gameName;
        _gameId = gameId;
        _logMethod = Log;
        _libraryVcfFindPathRegex = new Regex(
            $"""\"path\"\s*\"([^\n\r]*)\"(?=.*\"{_gameId}\")""",
            RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled
        );
    }

    /// <summary>
    /// Marks the install directory as possibly changed and requires it be re-fetched next time it's needed.
    /// </summary>
    public void InvalidateInstallDirectory()
    {
        _installDirectory = default;
    }

    protected virtual void Log(string error)
    {
        Console.Write("InstallDirUtility >> ");
        Console.WriteLine(error);
    }

    /// <summary>
    /// Attempts to automatically locate the installation location of the game and workshop folders.
    /// </summary>
    public bool TryGetInstallDirectory(out GameInstallDir installDir)
    {
        if (_installDirectory.BaseFolder != null)
        {
            installDir = _installDirectory;
            return true;
        }

        installDir = default;

        if (_gameName == "\0" || _gameId == "\0")
            return false;

        string libraryFilePath;
        bool isUnix = false;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!WindowsInstallDirUtility.TryFindSteamInstallDirectory(out libraryFilePath, _logMethod))
            {
                return false;
            }
        }
        else if (!UnixInstallDirUtility.TryFindSteamInstallDirectory(out libraryFilePath, _logMethod))
        {
            return false;
        }
        else
        {
            isUnix = true;
        }

        MatchCollection matches = _libraryVcfFindPathRegex.Matches(File.ReadAllText(libraryFilePath));
        if (matches.Count == 0 || matches[matches.Count - 1].Groups.Count <= 1)
        {
            Log($"Failed to match {_gameName} installation in: \"{libraryFilePath}\".");
            return false;
        }

        string libraryDir = matches[matches.Count - 1].Groups[1].Value;
        if (!isUnix)
        {
            libraryDir = libraryDir.Replace(@"\\", @"\");
        }
        if (!Directory.Exists(libraryDir))
        {
            Log($"Library \"{libraryDir}\" has been removed.");
            return false;
        }

        string gameInstallDir = Path.Combine(libraryDir, "steamapps", "common", _gameName);
        if (!Directory.Exists(gameInstallDir))
        {
            if (!isUnix)
            {
                Log($"{_gameName} installation at \"{gameInstallDir}\" has been removed.");
                return false;
            }

            gameInstallDir = Path.Combine(libraryDir, "SteamApps", "common", _gameName);
            if (!Directory.Exists(gameInstallDir))
            {
                Log($"{_gameName} installation at \"{gameInstallDir}\" has been removed.");
                return false;
            }
        }

        string? workshopInstallDir = Path.Combine(libraryDir, "steamapps", "workshop", "content", _gameId);
        if (!Directory.Exists(workshopInstallDir))
        {
            if (isUnix)
            {
                workshopInstallDir = Path.Combine(libraryDir, "SteamApps", "workshop", "content", _gameId);
                if (!Directory.Exists(workshopInstallDir))
                {
                    Log($"{_gameName} workshop directory at \"{workshopInstallDir}\" is missing.");
                    workshopInstallDir = null;
                }
            }
            else
            {
                Log($"{_gameName} workshop directory at \"{workshopInstallDir}\" is missing.");
                workshopInstallDir = null;
            }
        }

        installDir = new GameInstallDir(gameInstallDir, workshopInstallDir);
        _installDirectory = installDir;
        return true;
    }
}

file static class UnixInstallDirUtility
{
    private static string[]? _linuxInstallPaths;

    // ReSharper disable once EmptyConstructor (beforefieldinit)
    static UnixInstallDirUtility() { }

    private static void CheckLinuxInstallPaths()
    {
        // steam seems to install in various paths on Linux depending on how it was installed
        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _linuxInstallPaths =
        [
            Path.Combine(home, ".local/share/Steam"),
            Path.Combine(home, ".steam"),
            Path.Combine(home, ".steam/steam"),
            Path.Combine(home, "Steam"),
            Path.Combine(home, "snap/steam"),
            Path.Combine(home, ".var/app/com.valvesoftware.Steam/.steam")
        ];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
#if NET5_0_OR_GREATER
    [UnsupportedOSPlatform("windows")]
#endif
    public static bool TryFindSteamInstallDirectory(out string libraryVcf, Action<string>? log)
    {
        libraryVcf = null!;

        // MacOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            const string defaultLocation = "Library/Application Support/Steam";

            string steamDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                defaultLocation
            );

            return CheckUnixSteamDir(steamDir, ref libraryVcf, log, true);
        }
        
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            log?.Invoke($"Platform not supported: {RuntimeInformation.OSDescription}.");
            return false;
        }

        // Linux
        if (_linuxInstallPaths == null)
        {
            CheckLinuxInstallPaths();
        }

        foreach (string dir in _linuxInstallPaths!)
        {
            if (CheckUnixSteamDir(dir, ref libraryVcf, log, false))
            {
                return true;
            }
        }

        log?.Invoke($"Steam directory not found in any of the following paths: \"{string.Join("\", \"", _linuxInstallPaths)}\". Automatic discovery is not supported on Linux, consider manually changing the install directory.");
        return false;
    }

    private static bool CheckUnixSteamDir(string steamDir, ref string libraryVcf, Action<string>? log, bool logDirNotFound)
    {
        if (!Directory.Exists(steamDir))
        {
            if (logDirNotFound)
                log?.Invoke($"Steam directory not found in \"{steamDir}\". Automatic discovery is not supported on MacOS, consider manually changing the install directory.");
            return false;
        }

        string libraryFilePath = steamDir + "/steamapps/libraryfolders.vdf";

        if (!File.Exists(libraryFilePath))
        {
            libraryFilePath = steamDir + "/SteamApps/libraryfolders.vdf";
            if (!File.Exists(libraryFilePath))
            {
                log?.Invoke($"Failed to recognize Steam directory: \"{steamDir}\" because the library configuration file at \"{libraryFilePath}\" was missing.");
                return false;
            }
        }

        libraryVcf = libraryFilePath;
        return true;
    }
}

file static class WindowsInstallDirUtility
{
    // ReSharper disable once EmptyConstructor (beforefieldinit)
    static WindowsInstallDirUtility() { }

    [MethodImpl(MethodImplOptions.NoInlining)]
#if NET5_0_OR_GREATER
    [SupportedOSPlatform("windows")]
#endif
    public static bool TryFindSteamInstallDirectory(out string libraryVcf, Action<string>? log)
    {
        libraryVcf = null!;

        string? steamDir;
        try
        {
            steamDir = (Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Valve\Steam", "InstallPath", null)
                        ?? Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null))
                ?.ToString();
        }
        catch (Exception ex)
        {
            log?.Invoke("Failed to access the registry.");
            log?.Invoke(ex.ToString());
            return false;
        }

        if (steamDir == null)
        {
            string defaultDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam");
            steamDir = defaultDir;
            log?.Invoke($"Failed to find Steam directory in registry, falling back to {defaultDir}.");
        }

        if (!Directory.Exists(steamDir))
        {
            log?.Invoke($"Steam directory \"{steamDir}\" was removed.");
            return false;
        }

        string libraryFilePath = Path.Combine(steamDir, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libraryFilePath))
        {
            log?.Invoke($"Failed to recognize Steam directory: \"{steamDir}\" because the library configuration file at \"{libraryFilePath}\" was missing.");
            return false;
        }

        libraryVcf = libraryFilePath;
        return true;
    }
}