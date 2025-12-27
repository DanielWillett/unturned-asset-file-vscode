using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Defines what syntaxes of vector properties can be parsed.
/// </summary>
[Flags]
public enum VectorTypeOptions
{
    /// <summary>
    /// A string in the format: <c>[(]x,y,z,w[)]</c>.
    /// </summary>
    String = 1,

    /// <summary>
    /// Parses legacy properties in the format:
    /// <code>
    /// Prop_X 1
    /// Prop_Y 2
    /// Prop_Z 3
    /// Prop_W 4
    /// </code>
    /// </summary>
    Legacy = 2,

    /// <summary>
    /// Parses an object in the format:
    /// <code>
    /// Prop
    /// {
    ///     X 1
    ///     Y 2
    ///     Z 3
    ///     W 4
    /// }
    /// </code>
    /// </summary>
    Object = 4,

    /// <summary>
    /// The default options to parse, which is <c><see cref="Object"/> | <see cref="String"/></c>.
    /// </summary>
    Default = Object | String
}