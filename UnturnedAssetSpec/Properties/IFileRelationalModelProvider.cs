using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public interface IFileRelationalModelProvider
{
    /// <summary>
    /// Gets or creates an updated relational model for a file.
    /// </summary>
    IFileRelationalModel GetProvider(ISourceFile file, SpecPropertyContext context = SpecPropertyContext.Property);

    /// <summary>
    /// Gets an updated relational model for a file if it's already been created elsewhere, otherwise returns <see langword="false"/>.
    /// </summary>
    bool TryGetProvider(
        ISourceFile file,
        [NotNullWhen(true)] out IFileRelationalModel? model,
        SpecPropertyContext context = SpecPropertyContext.Property);
}
