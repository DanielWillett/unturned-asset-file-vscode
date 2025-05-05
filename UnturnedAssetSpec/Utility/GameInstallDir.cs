using System;
using System.IO;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

public readonly struct GameInstallDir : IEquatable<GameInstallDir>
{
    public string BaseFolder { get; }
    public string? WorkshopFolder { get; }

    public GameInstallDir(string baseFolder, string? workshopFolder)
    {
        BaseFolder = baseFolder;
        WorkshopFolder = workshopFolder;
    }

    public string GetFile(string relativePath)
    {
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