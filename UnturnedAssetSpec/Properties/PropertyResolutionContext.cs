namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Differentiates between a legacy and modern context for filtered keys and aliases.
/// </summary>
public enum PropertyResolutionContext
{
    /// <summary>
    /// Property is being read from a modern dictionary node.
    /// <code>
    /// Blueprints
    /// [
    ///     {
    ///         Property "Value" // Modern
    ///     }
    /// ]
    /// </code>
    /// </summary>
    Modern,

    /// <summary>
    /// Property is being read from a legacy template node.
    /// <code>
    /// Blueprints 1
    /// Blueprint_0_Property "Value" // Legacy
    /// </code>
    /// </summary>
    Legacy
}