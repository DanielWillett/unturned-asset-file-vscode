using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface IFileRelationalModelProvider
{
    /// <summary>
    /// Get or create a relational model for a file.
    /// </summary>
    IFileRelationalModel GetProvider(ISourceFile file, SpecPropertyContext context = SpecPropertyContext.Property);
}
