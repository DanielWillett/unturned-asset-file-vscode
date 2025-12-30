namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Kind of asset reference.
/// </summary>
public enum AssetReferenceKind
{
    Unspecified,

    /// <summary>
    /// A GUID string that references an asset.
    /// <code>
    /// Asset 37f46b2d573d47579df637a39d70c365
    /// </code>
    /// </summary>
    String,

    /// <summary>
    /// A GUID string or object that references an asset.
    /// <code>
    /// Asset 37f46b2d573d47579df637a39d70c365
    ///
    /// // or
    ///
    /// Asset
    /// {
    ///     GUID 37f46b2d573d47579df637a39d70c365
    /// }
    /// </code>
    /// </summary>
    Object
}
