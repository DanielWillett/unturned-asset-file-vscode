namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Kind of backwards-compatible asset reference.
/// </summary>
public enum BackwardsCompatibleAssetReferenceKind
{
    /// <summary>
    /// A GUID or ID string that references an asset.
    /// <code>
    /// Asset 37f46b2d573d47579df637a39d70c365
    ///
    /// // or
    ///
    /// Asset 12345
    /// </code>
    /// </summary>
    /// <remarks>Corresponds to the <c>UnturnedDatEx.ParseGuidOrLegacyId</c> extension method.</remarks>
    GuidOrLegacyId,

    /// <summary>
    /// A backwards-compatible asset reference object or string.
    /// <code>
    /// Asset 37f46b2d573d47579df637a39d70c365
    ///
    /// // or
    /// 
    /// Asset 12345
    ///
    /// // or
    /// 
    /// Asset ITEM:12345
    ///
    /// // or
    ///
    /// Asset
    /// {
    ///     GUID 37f46b2d573d47579df637a39d70c365
    /// }
    ///
    /// // or
    /// 
    /// Asset
    /// {
    ///     Type ITEM
    ///     ID 12345
    /// }
    /// </code>
    /// </summary>
    /// <remarks>Corresponds to the <c>CachingBcAssetRef</c> type.</remarks>
    BcAssetReference,

    /// <summary>
    /// A backwards-compatible asset reference string.
    /// <code>
    /// Asset 37f46b2d573d47579df637a39d70c365
    ///
    /// // or
    /// 
    /// Asset 12345
    ///
    /// // or
    /// 
    /// Asset ITEM:12345
    /// </code>
    /// </summary>
    /// <remarks>Corresponds to the <c>CachingBcAssetRef</c> type.</remarks>
    BcAssetReferenceString
}