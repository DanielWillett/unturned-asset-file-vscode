using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;

namespace DanielWillett.UnturnedDataFileLspServer.Utility;

/// <summary>
/// Utility for configuring file icons and associations on Windows.
/// </summary>
[SupportedOSPlatform("windows")]
internal class FileAssociationUtility
{
    private readonly ILogger<FileAssociationUtility> _logger;

    public const string AssetProgId = "UnturnedDat";
    public const string ProjectProgId = "UnturnedDatProject";

    public string? AssetCommand { get; set; }
    public string? ProjectCommand { get; set; }
    public int FileAssocVersion { get; set; } = 1;

    public FileAssociationUtility(ILogger<FileAssociationUtility> logger)
    {
        _logger = logger;
    }

    internal async Task<bool> AssociateFileTypesAsync(bool force)
    {
        const string dataVersionValueName = "LspServer_AssocDataVersion";

        int version = 0;
        RegistryKey? projIdKey = null;
        try
        {
            projIdKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{AssetProgId}");
            object? obj = projIdKey.GetValue(dataVersionValueName);
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
        }
        catch
        {
            version = 0;
        }

        try
        {
            if (version == FileAssocVersion && !force)
            {
                _logger.LogDebug("File associations are up to date (v{0}).", FileAssocVersion);
                return false;
            }

            (string ext, string progId)[] extensions =
            [
                (".dat", AssetProgId),
                (".asset", AssetProgId),
                (".udat", AssetProgId),
                (".udatproj", ProjectProgId)
            ];

            string regsvr32 = @"C:\Windows\SysWOW64\regsvr32.exe";
            if (!File.Exists(regsvr32))
            {
                regsvr32 = @"C:\Windows\System32\regsvr32.exe";
                if (!File.Exists(regsvr32))
                {
                    _logger.LogError(string.Format(Properties.Resources.Error_RegSvr32NotFound, regsvr32));
                    return false;
                }
            }

            foreach ((string ext, _) in extensions)
            {
                ClearProgramFileAssociation(ext);
                await Task.Yield();
            }

            int? exitCode = null;
            string args = $@"/n /s /i:user ""{Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, @"UnturnedAssetFileIconHandler.dll")}""";
            try
            {
                Process? process = Process.Start(new ProcessStartInfo(regsvr32, args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ErrorDialog = false
                });

                if (process == null)
                {
                    _logger.LogError(string.Format(Properties.Resources.Error_RegSvr32Exception, regsvr32 + " " + args, "?"));
                    return false;
                }

                if (!process.WaitForExit(TimeSpan.FromSeconds(15)))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, string.Format(Properties.Resources.Error_RegSvr32Timeout, regsvr32 + " " + args));
                        return false;
                    }

                    _logger.LogError(string.Format(Properties.Resources.Error_RegSvr32Timeout, regsvr32 + " " + args));
                }

                if (process.ExitCode != 0)
                {
                    _logger.LogError(string.Format(Properties.Resources.Error_RegSvr32Exception, regsvr32 + " " + args, process.ExitCode));
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Format(Properties.Resources.Error_RegSvr32Exception, regsvr32 + " " + args, exitCode?.ToString() ?? "?"));
                return false;
            }

            string? assetCommand = AssetCommand;
            string? projectCommand = ProjectCommand;

            if (assetCommand == null || projectCommand == null)
            {
                string? vscode = FindVsCodeLocation();
                if (vscode == null)
                {
                    string[] paths =
                    [
                        @"%LOCALAPPDATA%\Programs\Microsoft VS Code\Code.exe",
                        @"C:\Program Files\Microsoft VS Code\Code.exe",
                        @"C:\Program Files (x86)\Microsoft VS Code\Code.exe"
                    ];
                    vscode = Array.Find(paths, File.Exists);
                }

                if (vscode == null)
                {
                    _logger.LogError(Properties.Resources.Error_FailedToFindVscode);
                    vscode = "code";
                }

                if (vscode.IndexOf(' ') >= 0)
                    vscode = "\"" + vscode + "\"";

                assetCommand ??= vscode + " \"%1\"";
                projectCommand ??= vscode + " \"%1\\..\" \"%1\"";
            }

            CreateProgramFileAssociation(AssetProgId, assetCommand);
            CreateProgramFileAssociation(ProjectProgId, projectCommand);

            projIdKey?.SetValue(dataVersionValueName, FileAssocVersion, RegistryValueKind.DWord);

            return true;
        }
        finally
        {
            projIdKey?.Dispose();
        }
    }

    private void ClearProgramFileAssociation(string ext)
    {
        string key = $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{ext}\";

        try
        {
            Registry.CurrentUser.DeleteSubKey(key + "UserChoice", throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKey(key + @"UserChoiceLatest\ProgId", throwOnMissingSubKey: false);
            Registry.CurrentUser.DeleteSubKey(key + @"UserChoiceLatest", throwOnMissingSubKey: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, string.Format(Properties.Resources.Error_FileAssocFailedToClear, ext));
        }
    }

    private void CreateProgramFileAssociation(string progId, string cmd)
    {
        string key = $@"Software\Classes\{progId}\Shell\Open\Command";

        RegistryKey? cmdKey = null;
        try
        {
            cmdKey = Registry.CurrentUser.CreateSubKey(key);
            if (cmdKey == null)
            {
                _logger.LogError(string.Format(Properties.Resources.Error_FileAssocFailedToClear, progId));
                return;
            }

            cmdKey.SetValue(null, cmd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, string.Format(Properties.Resources.Error_FileAssocFailedToClear, progId));
        }
        finally
        {
            cmdKey?.Dispose();
        }
    }

    private string? FindVsCodeLocation()
    {
        try
        {
            Process[] processes = Process.GetProcessesByName("Code");
            string? fn = null;
            foreach (Process process in processes)
            {
                if (fn == null)
                {
                    ProcessModule? mainModule = process.MainModule;
                    if (mainModule == null)
                        continue;

                    fn = mainModule.FileName;
                    if (!fn.EndsWith(@"\Code.exe"))
                        fn = null;
                }

                process.Dispose();
            }

            return fn;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, Properties.Resources.Error_FailedToFindVscode);
        }

        return null;
    }
}
