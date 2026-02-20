using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Handles reading asset files from some source.
/// </summary>
public interface ISpecificationFileProvider
{
    /// <summary>
    /// The priority of this file provider.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Whether or not this file provider can be used.
    /// </summary>
    bool IsEnabled { get; }

    Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);

    Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);
}

public enum KnownConfigurationFile
{
    /// <summary>
    /// <c>Asset Spec/Assets.json</c> file.
    /// </summary>
    Assets,

    /// <summary>
    /// <c>Unturned/Status.json</c> file.
    /// </summary>
    GameStatus,

    /// <summary>
    /// <c>Unturned/Localization/English/Player/PlayerDashboardInventory.dat</c> file.
    /// </summary>
    InventoryLocalization,

    /// <summary>
    /// <c>Unturned/Localization/English/Menu/Survivors/MenuSurvivorsCharacter.dat</c> file.
    /// </summary>
    CharacterLocalization,

    /// <summary>
    /// <c>Unturned/Localization/English/Player/PlayerDashboardSkills.dat</c> file.
    /// </summary>
    SkillsLocalization
}