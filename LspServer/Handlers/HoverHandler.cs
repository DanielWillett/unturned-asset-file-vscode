using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class HoverHandler : IHoverHandler
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly IAssetSpecDatabase _specDictionary;

    /// <inheritdoc />
    HoverRegistrationOptions IRegistration<HoverRegistrationOptions, HoverCapability>.GetRegistrationOptions(
        HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
    }

    public HoverHandler(OpenedFileTracker fileTracker, IAssetSpecDatabase specDictionary)
    {
        _fileTracker = fileTracker;
        _specDictionary = specDictionary;
    }

    /// <inheritdoc />
    public Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return Task.FromResult<Hover?>(null);
        }

        AssetFileTree tree = file.File;

        AssetFileType fileType = AssetFileType.FromFile(file.File, _specDictionary);

        AssetFileNode? node = tree.GetNode(request.Position.ToFilePosition());

        if (node is not AssetFileKeyNode keyNode)
            return Task.FromResult<Hover?>(null);

        Range range = node.Range.ToRange();

        SpecProperty? property = _specDictionary.FindPropertyInfo(keyNode.Value, fileType, SpecPropertyContext.Property);

        return Task.FromResult<Hover?>(new Hover
        {
            Range = range,
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.PlainText,
                Value = property != null ? property.Description ?? property.Key : "Unknown property"
            })
        });
    }
}
