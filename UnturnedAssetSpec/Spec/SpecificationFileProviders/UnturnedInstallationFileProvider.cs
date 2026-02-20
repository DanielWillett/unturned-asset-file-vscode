using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Pulls files from an available Unturned installation.
/// </summary>
public sealed class UnturnedInstallationFileProvider : ISpecificationFileProvider
{
    private readonly InstallDirUtility? _installDir;
    private readonly ILogger<UnturnedInstallationFileProvider> _logger;
    public int Priority => 3;
    public bool IsEnabled => _installDir != null;

    public UnturnedInstallationFileProvider(InstallDirUtility? installDir, ILogger<UnturnedInstallationFileProvider> logger)
    {
        _installDir = installDir;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public async Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        if (file is not KnownConfigurationFile.GameStatus
                and not KnownConfigurationFile.CharacterLocalization
                and not KnownConfigurationFile.InventoryLocalization
                and not KnownConfigurationFile.SkillsLocalization
            || _installDir == null
            || !_installDir.TryGetInstallDirectory(out GameInstallDir installDir))
        {
            return false;
        }

        string? statusPath = installDir.GetFile(file switch
        {
            KnownConfigurationFile.GameStatus => "Status.json",
            KnownConfigurationFile.CharacterLocalization => @"Localization\English\Menu\Survivors\MenuSurvivorsCharacter.dat",
            KnownConfigurationFile.InventoryLocalization => @"Localization\English\Player\PlayerDashboardInventory.dat",
            KnownConfigurationFile.SkillsLocalization => @"Localization\English\Player\PlayerDashboardSkills.dat",
            _ => null
        });

        if (File.Exists(statusPath))
        {
            try
            {
                using FileStream fs = new FileStream(statusPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.SequentialScan);
                await action(fs, state, token).ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, Resources.Log_FailedToParseResource, statusPath);
            }
        }

        return false;
    }
}