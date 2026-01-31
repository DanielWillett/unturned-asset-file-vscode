using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Provides context about the current object.
/// </summary>
public interface INestedObjectContext
{
    /// <summary>
    /// The object this object is defined in, if any.
    /// </summary>
    INestedObjectContext? Parent { get; }

    /// <summary>
    /// Whether or not the object is defined as a legacy object.
    /// </summary>
    PropertyResolutionContext Context { get; }

    /// <summary>
    /// If this object is defined in a property, this is the base key of all properties relative to it's parent dictionary.
    /// <code>
    /// For legacy objects: 'Condition_0'_Whatever ...
    /// For modern objects: 'Conditions' [ ...
    /// For objects in lists: <see langword="null"/>.
    /// </code>
    /// </summary>
    string? BaseKey { get; }
}