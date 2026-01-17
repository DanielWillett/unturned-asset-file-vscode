namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// The difficulty of a <see cref="T:SDG.Unturned.ConfigData"/> config file.
/// </summary>
public enum ServerDifficulty
{
    /// <summary>
    /// Corresponds to <see cref="F:SDG.Unturned.EGameMode.EASY"/>.
    /// </summary>
    Easy,

    /// <summary>
    /// Corresponds to <see cref="F:SDG.Unturned.EGameMode.NORMAL"/>.
    /// </summary>
    Normal,

    /// <summary>
    /// Corresponds to <see cref="F:SDG.Unturned.EGameMode.HARD"/>.
    /// </summary>
    Hard
}