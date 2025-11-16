using System;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Information about where the game is installed on the host PC.
/// </summary>
public readonly struct GameInstallDir : IEquatable<GameInstallDir>
{
    /// <summary>
    /// The root folder of the game ('U3DS' or 'Unturned').
    /// </summary>
    public string BaseFolder { get; }

    /// <summary>
    /// The workshop folder of the game ('304930').
    /// </summary>
    public string? WorkshopFolder { get; }

    public GameInstallDir(string baseFolder, string? workshopFolder)
    {
        BaseFolder = baseFolder;
        WorkshopFolder = workshopFolder;
    }

    /// <summary>
    /// Gets an absolute file path from a path relative to the game's root folder.
    /// </summary>
    public string GetFile(string relativePath)
    {
        if (Path.DirectorySeparatorChar != '\\')
        {
            relativePath = relativePath.Replace('\\', Path.DirectorySeparatorChar);
        }
        return Path.Combine(BaseFolder, relativePath);
    }

    /// <inheritdoc />
    public override string ToString() => BaseFolder;

    /// <inheritdoc />
    public bool Equals(GameInstallDir other) => BaseFolder == other.BaseFolder && WorkshopFolder == other.WorkshopFolder;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is GameInstallDir other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (BaseFolder.GetHashCode() * 397) ^ (WorkshopFolder != null ? WorkshopFolder.GetHashCode() : 0);
        }
    }
}