using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A model of a file storing all properties and directly relating properties.
/// </summary>
public interface IFileRelationalModel
{
    /// <summary>
    /// The file being modeled.
    /// </summary>
    ISourceFile SourceFile { get; }
}