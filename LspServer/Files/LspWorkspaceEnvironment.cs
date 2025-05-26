using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class LspWorkspaceEnvironment : IWorkspaceEnvironment
{
    private readonly OpenedFileTracker _tracker;
    private readonly ILogger<LspWorkspaceEnvironment> _logger;

    public LspWorkspaceEnvironment(OpenedFileTracker tracker, ILogger<LspWorkspaceEnvironment> logger)
    {
        _tracker = tracker;
        _logger = logger;
    }

    public IWorkspaceFile? TemporarilyGetOrLoadFile(DiscoveredDatFile datFile)
    {
        if (!File.Exists(datFile.FilePath))
            return null;

        DocumentUri uri = DocumentUri.File(datFile.FilePath);
        if (_tracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            return file;
        }

        try
        {
            return new OpenedFile(uri, File.ReadAllText(datFile.FilePath, Encoding.UTF8), _logger);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read file {0}.", datFile.FilePath);
            return null;
        }
    }
}