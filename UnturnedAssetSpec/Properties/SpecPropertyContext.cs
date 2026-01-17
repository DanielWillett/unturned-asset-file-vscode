namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public enum SpecPropertyContext
{
    /// <summary>
    /// Context is not specified.
    /// </summary>
    Unspecified,

    /// <summary>
    /// A normal property.
    /// </summary>
    Property,

    /// <summary>
    /// A property within an associated localization file. This does not include UI localization files like those found in the Unturned/Localization folder.
    /// </summary>
    Localization,

    /// <summary>
    /// Context is a cross-reference but the category is not specified.
    /// </summary>
    CrossReferenceUnspecified,

    /// <summary>
    /// A cross-referenced normal property.
    /// </summary>
    CrossReferenceProperty,

    /// <summary>
    /// A cross-referenced property within an associated localization file.
    /// </summary>
    CrossReferenceLocalization,

    /// <summary>
    /// An object within the asset's Unity bundle.
    /// </summary>
    BundleAsset
}