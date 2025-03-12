using LspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LspServer.Handlers;

internal class DocumentSymbolHandler : IDocumentSymbolHandler
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly AssetSpecDictionary _specDictionary;
    private readonly ILogger<DocumentSymbolHandler> _logger;

    /// <inheritdoc />
    DocumentSymbolRegistrationOptions IRegistration<DocumentSymbolRegistrationOptions, DocumentSymbolCapability>.GetRegistrationOptions(
        DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentSymbolRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
    }

    public DocumentSymbolHandler(OpenedFileTracker fileTracker, AssetSpecDictionary specDictionary, ILogger<DocumentSymbolHandler> logger)
    {
        _fileTracker = fileTracker;
        _specDictionary = specDictionary;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Document symbol received.");

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return new SymbolInformationOrDocumentSymbolContainer();
        }

        AssetFileTree tree = file.Tree;

        string? type = tree.GetType(out bool onlyClrType);

        AssetSpec? spec = (type == null ? null
                              : await _specDictionary.GetAssetSpecAsync(type, onlyClrType, cancellationToken).ConfigureAwait(false)) ??
                                await _specDictionary.GetAssetSpecAsync("SDG.Unturned.Asset, Assembly-CSharp", true, cancellationToken).ConfigureAwait(false);


        List<SymbolInformationOrDocumentSymbol> symbols = new List<SymbolInformationOrDocumentSymbol>();

        foreach (AssetFileNode node in tree)
        {
            if (node is AssetFileKeyNode keyNode)
            {
                AssetSpecProperty? property = spec?.FindProperty(keyNode.Value);
                if (property != null)
                {
                    Range range = new Range(node.Range.Start.Line - 1, node.Range.Start.Character - 1, node.Range.End.Line - 1, node.Range.End.Character - 1);
                    symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                    {
                        Range = range,
                        Kind = SymbolKind.Property,
                        Deprecated = false,
                        SelectionRange = range,
                        Detail = property.Type,
                        Name = property.Key
                    }));
                }
            }
            else if (node is AssetFileStringValueNode strValue)
            {
                string? propertyName = (strValue.Parent as AssetFileKeyValuePairNode)?.Key?.Value;
                AssetSpecProperty? property = propertyName != null ? spec?.FindProperty(propertyName) : null;
                Range range = new Range(node.Range.Start.Line - 1, node.Range.Start.Character - 1, node.Range.End.Line - 1, node.Range.End.Character - 1);
                symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Range = range,
                    Kind = property?.GetSymbolKind() ?? SymbolKind.String,
                    Deprecated = false,
                    SelectionRange = range,
                    Name = strValue.Value
                }));
            }
        }

        return symbols;
    }
}
