using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal sealed class FileRelationalModelProvider : IFileRelationalModelProvider
{
    private readonly IParsingServices _parsingServices;

    public FileRelationalModelProvider(IParsingServices parsingServices)
    {
        _parsingServices = parsingServices;
    }

    public IFileRelationalModel GetProvider(ISourceFile file)
    {
        // TODO: optimize
        FileRelationalCache cache = new FileRelationalCache(file, false, _parsingServices);
        return cache;
    }
}
