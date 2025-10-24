using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal readonly struct FileTypeInfo
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
        }
        else
        {
            ReadOnlySpan<char> fnWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            ReadOnlySpan<char> fullDirectoryName = Path.GetDirectoryName(fullName);
            ReadOnlySpan<char> directoryName = Path.GetFileName(fullDirectoryName);
            if (!directoryName.IsEmpty && directoryName.Equals(fnWithoutExt, OSPathHelper.PathComparison))
            {
                IsAsset = true;
                return;
            }

            ReadOnlySpan<char> ext = Path.GetExtension(fileName);
            if (ext.Equals(".asset", StringComparison.OrdinalIgnoreCase))
            {
                IsAsset = true;
                return;
            }

            string datAsset = string.Concat(directoryName, ".dat");
            datAsset = Path.Join(fullDirectoryName, datAsset);
            if (File.Exists(datAsset))
            {
                IsLocalization = true;
                AssetPath = datAsset;
            }

            string assetAsset = string.Concat(directoryName, ".asset");
            assetAsset = Path.Join(fullDirectoryName, assetAsset);
            if (File.Exists(assetAsset))
            {
                IsLocalization = true;
                AssetPath = assetAsset;
            }
        }
    }
}
