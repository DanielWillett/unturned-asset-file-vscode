namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// Specifies a filter on which aliases can be used with each legacy expansion type.
/// </summary>
public enum LegacyExpansionFilter
{
    /// <summary>
    /// Can be used with either format.
    /// </summary>
    Either = 3,

    /// <summary>
    /// Can only be used with the legacy format.
    /// </summary>
    Legacy = 1,

    /// <summary>
    /// Can only be used with the modern format.
    /// </summary>
    Modern = 2
}