using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal readonly struct FileTypeInfo : IEquatable<FileTypeInfo>
{
    public readonly bool IsAsset;
    public readonly bool IsLocalization;

    /// <summary>
    /// Corresponding asset path if this is a localization file.
    /// </summary>
    public readonly string? AssetPath;

    public FileTypeInfo(ReadOnlySpan<char> fullName)
    {
        ReadOnlySpan<char> fileName = Path.GetFileName(fullName);

        if (fileName.Equals("Asset.dat", OSPathHelper.PathComparison))
        {
            IsAsset = true;
            return;
        }

        int extStartIndex = fileName.LastIndexOf('.');
        ReadOnlySpan<char> fnWithoutExt = extStartIndex >= 0 ? fileName[..extStartIndex] : fileName;
        if (fnWithoutExt.IsEmpty)
        {
            // file name is '**/.dat' or '**/.asset'
            return;
        }

        ReadOnlySpan<char> fullDirectoryName = Path.GetDirectoryName(fullName);
        ReadOnlySpan<char> directoryName = Path.GetFileName(fullDirectoryName);
        ReadOnlySpan<char> ext = fileName[extStartIndex..];

        // **/*.asset
        if (ext.Equals(".asset", StringComparison.OrdinalIgnoreCase))
        {
            IsAsset = true;
            return;
        }

        if (!ext.Equals(".dat", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Folder/Folder.dat
        if (!directoryName.IsEmpty && directoryName.Equals(fnWithoutExt, OSPathHelper.PathComparison))
        {
            IsAsset = true;
            return;
        }

        if (!char.IsUpper(fnWithoutExt[0]))
        {
            // all languages start with a capital letter
            return;
        }

        // Folder/English.dat (with Folder.dat)
        string datAsset = string.Concat(directoryName, ".dat");
        datAsset = Path.Join(fullDirectoryName, datAsset);
        if (File.Exists(datAsset))
        {
            IsLocalization = true;
            AssetPath = datAsset;
            return;
        }

        // Folder/English.dat (with Folder.asset)
        string assetAsset = string.Concat(directoryName, ".asset");
        assetAsset = Path.Join(fullDirectoryName, assetAsset);
        if (File.Exists(assetAsset))
        {
            IsLocalization = true;
            AssetPath = assetAsset;
            return;
        }

        string assetDatPath = Path.Join(fullDirectoryName, "Asset.dat");
        if (File.Exists(assetDatPath))
        {
            IsLocalization = true;
            AssetPath = assetDatPath;
        }
    }

    /// <inheritdoc />
    public bool Equals(FileTypeInfo other) => Equals(in other);

    /// <inheritdoc cref="IEquatable{T}.Equals" />
    public bool Equals(in FileTypeInfo other)
    {
        return IsAsset == other.IsAsset
               && IsLocalization == other.IsLocalization
               && (!IsLocalization || string.Equals(AssetPath, other.AssetPath, OSPathHelper.PathComparison));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is FileTypeInfo other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(
            IsAsset,
            IsLocalization,
            !IsLocalization || AssetPath == null ? 0 : OSPathHelper.PathComparer.GetHashCode(AssetPath)
        );
    }
}
