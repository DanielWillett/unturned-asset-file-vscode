using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspWorkspaceEnvironment : IWorkspaceEnvironment
{
    private readonly OpenedFileTracker _tracker;
    private readonly ILogger<LspWorkspaceEnvironment> _logger;
    private readonly IAssetSpecDatabase _database;

    public LspWorkspaceEnvironment(OpenedFileTracker tracker, ILogger<LspWorkspaceEnvironment> logger, IAssetSpecDatabase database)
    {
        _tracker = tracker;
        _logger = logger;
        _database = database;
    }

    public IWorkspaceFile? TemporarilyGetOrLoadFile(DiscoveredDatFile datFile)
    {
        if (!File.Exists(datFile.FilePath))
            return null;

        DocumentUri uri = DocumentUri.File(datFile.FilePath);
        if (_tracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            return new DontDisposeWorkspaceFile(file);
        }

        try
        {
            return StaticSourceFile.FromAssetFile(datFile.FilePath, _database, SourceNodeTokenizerOptions.Lazy);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file {0}.", datFile.FilePath);
            return null;
        }
    }

    private class DontDisposeWorkspaceFile(IWorkspaceFile file) : IWorkspaceFile
    {
        /// <inheritdoc />
        public string File => file.File;

        /// <inheritdoc />
        public ISourceFile SourceFile => file.SourceFile;

        /// <inheritdoc />
        public string GetFullText() => file.GetFullText();

        /// <inheritdoc />
        public void Dispose() { }
    }
}