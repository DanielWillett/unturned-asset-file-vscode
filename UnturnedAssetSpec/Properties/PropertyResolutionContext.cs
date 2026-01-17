namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// Differentiates between a legacy and modern context for filtered keys and aliases.
/// </summary>
public enum PropertyResolutionContext
{
    /// <summary>
    /// Could go either way.
    /// </summary>
    Unknown,

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

/// <summary>
/// Extension methods for the <see cref="PropertyResolutionContext"/> enum.
/// </summary>
public static class PropertyResolutionContextExtensions
{
    /// <summary>
    /// Converts a <see cref="PropertyResolutionContext"/> value to the corresponding <see cref="LegacyExpansionFilter"/> value.
    /// </summary>
    public static LegacyExpansionFilter ToKeyFilter(this PropertyResolutionContext value)
    {
        return value switch
        {
            PropertyResolutionContext.Modern => LegacyExpansionFilter.Modern,
            PropertyResolutionContext.Legacy => LegacyExpansionFilter.Legacy,
            _ => LegacyExpansionFilter.Either
        };
    }
}