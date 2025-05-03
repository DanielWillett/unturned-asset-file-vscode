using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Collections.Concurrent;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class OpenedFileTracker
{
    public ConcurrentDictionary<DocumentUri, OpenedFile> Files { get; } = new ConcurrentDictionary<DocumentUri, OpenedFile>();
}
