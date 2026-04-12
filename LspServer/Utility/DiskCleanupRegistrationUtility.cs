using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Utility;

internal class DiskCleanupRegistrationUtility
{
    public const string EnvVarDisableDiskCleanupHandler = "UNTURNED_ASSET_SPEC_NO_DISK_CLEANUP";

    private const string DataVersionValueName = "LspServer_CacheDataVersion";
    private const int CurrentVersion = 1;
    private const string RegistrationName = "Unturned Data File Language Server";
    private const string KeyPath = $@"Software\Microsoft\Windows\CurrentVersion\Explorer\VolumeCaches\{RegistrationName}";

    private readonly ILogger<DiskCleanupRegistrationUtility> _logger;
    private readonly EnvironmentCache _cache;

    public DiskCleanupRegistrationUtility(ILogger<DiskCleanupRegistrationUtility> logger, EnvironmentCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    [SupportedOSPlatform("windows")]
    internal bool IsNeeded()
    {
        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(KeyPath);

        int version = 0;

        object? obj = key?.GetValue(DataVersionValueName);
        if (obj is IConvertible conv)
        {
            try
            {
                version = conv.ToInt32(CultureInfo.InvariantCulture);
            }
            catch
            {
                version = 0;
            }
        }

        if (version != CurrentVersion)
            return true;

        string? folderValue = key?.GetValue("Folder")?.ToString();
        string? csidlValue = key?.GetValue("CSIDL")?.ToString();

        GetFolderValues(out bool useCsidl, out string folder);
        return useCsidl == (csidlValue != null) && string.Equals(folder, folderValue, OSPathHelper.PathComparison);
    }

    private void GetFolderValues(out bool useCsidl, out string folder)
    {
        string rootFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        useCsidl = OSPathHelper.Contains(rootFolder, _cache.RootDirectory);
        folder = !useCsidl ? _cache.RootDirectory : Path.GetRelativePath(rootFolder, _cache.RootDirectory);
    } 
    

    internal enum RegisterDiskCleanupHandlerResult { Success, Failure = 1, AlreadyRegistered = -1, RequiresPermission = 2 }

    [SupportedOSPlatform("windows")]
    internal RegisterDiskCleanupHandlerResult RegisterDiskCleanupHandler(bool hasPermission, out string? command)
    {
        if (!IsNeeded())
        {
            command = null;
            return RegisterDiskCleanupHandlerResult.AlreadyRegistered;
        }

        bool isElevated;
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        string? location;
        try
        {
            location = Assembly.GetExecutingAssembly().Location;
        }
        catch
        {
            location = null;
        }

        // CSIDL_COMMON_APPDATA
        const string csidl = "dword:00000023";
        GetFolderValues(out bool useCsidl, out string folder);

        // DDEVCF_DOSUBDIRS | DDEVCF_REMOVEHIDDEN | DDEVCF_DONTSHOWIFZERO
        const string flags = "dword:00000031";

        string tempPath = Path.Combine(Path.GetDirectoryName(location)!, "disk-cleanup-handler.reg");
        using (StreamWriter writer = new StreamWriter(tempPath, Encoding.ASCII, new FileStreamOptions
        {
            Access = FileAccess.Write,
            Share = FileShare.ReadWrite,
            Mode = FileMode.Create,
            Options = FileOptions.SequentialScan
        }))
        {
            // DataDrivenCleaner
            // https://learn.microsoft.com/en-us/windows/win32/lwef/disk-cleanup#using-the-datadrivencleaner-object
            writer.WriteLine("Windows Registry Editor Version 5.00");
            writer.WriteLine();
            writer.WriteLine($@"[HKEY_LOCAL_MACHINE\{KeyPath}]");
            writer.WriteLine("@=\"{C0E13E61-0CC6-11d1-BBB6-0060978B2AE6}\"");
            if (useCsidl)
            {
                writer.WriteLine($"\"CSIDL\"={csidl}");
            }

            writer.WriteLine($"\"Folder\"={GetRegExpandSzFormat(folder)}");
            writer.WriteLine($"\"FileList\"={GetRegExpandSzFormat(@"*.toc|*.unity3d")}");
            writer.WriteLine($"\"Description\"={GetRegExpandSzFormat(Properties.Resources.WindowsDiskCleanup_Description)}");
            writer.WriteLine($"\"Display\"={GetRegExpandSzFormat(Properties.Resources.WindowsDiskCleanup_Display)}");
            writer.WriteLine($"\"Flags\"={flags}");
            if (File.Exists(location))
            {
                writer.WriteLine($"\"IconPath\"={GetRegExpandSzFormat(location + ",0")}");
            }

            writer.WriteLine("\"Priority\"=dword:00000000");
            writer.WriteLine($"\"{DataVersionValueName}\"=dword:{CurrentVersion:x8}");
        }

        string args = $"import \"{tempPath}\"";

        string windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.System);
        string regExePath = Path.Combine(windowsPath, "reg.exe");
        if (!File.Exists(regExePath))
        {
            _logger.LogWarning("Unable to find reg.exe tool at {0} while trying to register windows disk cleanup handler.", regExePath);
            command = null;
            return RegisterDiskCleanupHandlerResult.Failure;
        }

        regExePath = $"\"{regExePath}\"";

        command = regExePath + " " + args;

        if (!hasPermission && !isElevated)
        {
            return RegisterDiskCleanupHandlerResult.RequiresPermission;
        }

        _logger.LogInformation(
            "The following command is about to run: '{0}', which requires elevated permissions. A prompt will appear soon.",
            command
        );

        /*
         * Note this will prompt with a 'Registry Editor wants to modify your PC' message.
         */
        using Process? p = Process.Start(new ProcessStartInfo(regExePath, args)
        {
            // run with elevated privileges
            Verb = "runas",
            UseShellExecute = true
        });

        if (p == null)
        {
            _logger.LogWarning("Unable to start reg.exe tool while trying to register windows disk cleanup handler.");
            return RegisterDiskCleanupHandlerResult.Failure;
        }

        if (!p.WaitForExit(TimeSpan.FromSeconds(5)))
        {
            _logger.LogWarning("Timeout (5s) waiting for reg.exe tool while trying to register windows disk cleanup handler.");
            try
            {
                p.Kill();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to stop reg.exe tool after timeout.");
            }
            return RegisterDiskCleanupHandlerResult.Failure;
        }

        if (p.ExitCode != 0)
        {
            _logger.LogWarning(
                "reg.exe tool failed (code {0}) while trying to register windows disk cleanup handler. It's possible permission was denied.",
                "0x" + p.ExitCode.ToString("x8")
            );
            return RegisterDiskCleanupHandlerResult.Failure;
        }

        _logger.LogInformation("Successfully registered cache folder for disk cleanup.");
        return RegisterDiskCleanupHandlerResult.Success;
    }

    private static string GetRegExpandSzFormat(string str)
    {
        ReadOnlySpan<byte> bytes = MemoryMarshal.Cast<char, byte>(str.AsSpan());
        return string.Create(12 + (bytes.Length * 3), bytes, (span, bytes) =>
        {
            "hex(2):".AsSpan().CopyTo(span);
            int index = 7;
            for (int i = 0; i < bytes.Length; ++i)
            {
                WriteByte(bytes[i], span, ref index);
                span[index] = ',';
                ++index;
            }
            "00,00".AsSpan().CopyTo(span.Slice(index));
        });

        static void WriteByte(byte b, Span<char> span, ref int index)
        {
            int nibl = (b & 0xF0) >> 4;
            span[index] = nibl > 9 ? (char)(nibl + 87) : (char)(nibl + 48);
            nibl = b & 0xF;
            span[index + 1] = nibl > 9 ? (char)(nibl + 87) : (char)(nibl + 48);
            index += 2;
        }
    }
}
