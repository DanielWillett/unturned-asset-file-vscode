using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Collections.Concurrent;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class OpenedFileTracker : IDisposable
{
    private readonly ILogger<OpenedFileTracker> _logger;
    private readonly Lazy<IParsingServices> _parsingServices;
    public ConcurrentDictionary<DocumentUri, OpenedFile> Files { get; } = new ConcurrentDictionary<DocumentUri, OpenedFile>();

    public OpenedFileTracker(ILogger<OpenedFileTracker> logger, Lazy<IParsingServices> parsingServices)
    {
        _logger = logger;
        _parsingServices = parsingServices;
    }

    public OpenedFile CreateFile(DocumentUri uri, ReadOnlySpan<char> text)
    {
        return new OpenedFile(uri, text, _logger, _parsingServices.Value, useVirtualFiles: true);
    }

    public void Dispose()
    {
        foreach (OpenedFile file in Files.Values)
        {
            file.Dispose();
        }
    }
}
