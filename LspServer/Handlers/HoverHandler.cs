using LspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LspServer.Handlers;

internal class HoverHandler : IHoverHandler
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly AssetSpecDictionary _specDictionary;

    /// <inheritdoc />
    HoverRegistrationOptions IRegistration<HoverRegistrationOptions, HoverCapability>.GetRegistrationOptions(
        HoverCapability capability, ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
    }

    public HoverHandler(OpenedFileTracker fileTracker, AssetSpecDictionary specDictionary)
    {
        _fileTracker = fileTracker;
        _specDictionary = specDictionary;
    }

    /// <inheritdoc />
    public async Task<Hover?> Handle(HoverParams request, CancellationToken cancellationToken)
    {
        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return null;
        }

        ++request.Position.Line;
        ++request.Position.Character;

        AssetFileTree tree = file.Tree;

        string? type = tree.GetType(out bool onlyClrType);

        AssetSpec? spec = (type == null ? null
                              : await _specDictionary.GetAssetSpecAsync(type, onlyClrType, cancellationToken).ConfigureAwait(false)) ??
                                await _specDictionary.GetAssetSpecAsync("SDG.Unturned.Asset, Assembly-CSharp", true, cancellationToken).ConfigureAwait(false);

        AssetFileNode? node = tree.GetNode(request.Position);

        if (node is not AssetFileKeyNode keyNode)
            return null;

        Range range = new Range(node.Range.Start.Line - 1, node.Range.Start.Character - 1, node.Range.End.Line - 1, node.Range.End.Character - 1);
        AssetSpecProperty? property = spec?.FindProperty(keyNode.Value);
        
        return new Hover
        {
            Range = range,
            Contents = new MarkedStringsOrMarkupContent(new MarkupContent
            {
                Kind = MarkupKind.PlainText,
                Value = property != null ? property.Description ?? property.Key : "Unknown property"
            })
        };
    }
}
