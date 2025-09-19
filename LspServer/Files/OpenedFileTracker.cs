using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Collections.Concurrent;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class OpenedFileTracker : IDisposable
{
    private readonly ILogger<OpenedFileTracker> _logger;
    private readonly IAssetSpecDatabase _database;
    public ConcurrentDictionary<DocumentUri, OpenedFile> Files { get; } = new ConcurrentDictionary<DocumentUri, OpenedFile>();

    public OpenedFileTracker(ILogger<OpenedFileTracker> logger, IAssetSpecDatabase database)
    {
        _logger = logger;
        _database = database;
    }

    public OpenedFile CreateFile(DocumentUri uri, ReadOnlySpan<char> text)
    {
        return new OpenedFile(uri, text, _logger, _database, useVirtualFiles: true);
    }

    public void Dispose()
    {
        foreach (OpenedFile file in Files.Values)
        {
            file.Dispose();
        }
    }
}
