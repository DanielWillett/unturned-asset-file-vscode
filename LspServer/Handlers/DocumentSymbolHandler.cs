using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class DocumentSymbolHandler : IDocumentSymbolHandler
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly AssetSpecDatabase _specDictionary;
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

    public DocumentSymbolHandler(OpenedFileTracker fileTracker, AssetSpecDatabase specDictionary, ILogger<DocumentSymbolHandler> logger)
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

        AssetFileType type = AssetFileType.FromFile(tree, _specDictionary);

        List<SymbolInformationOrDocumentSymbol> symbols = new List<SymbolInformationOrDocumentSymbol>();

        foreach (AssetFileNode node in tree)
        {
            if (node is AssetFileKeyNode keyNode)
            {
                SpecProperty? property = _specDictionary.FindPropertyInfo(keyNode.Value, type);
                if (property == null)
                    continue;

                Range range = node.Range.ToRange();
                symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Range = range,
                    Kind = SymbolKind.Property,
                    Deprecated = false,
                    SelectionRange = range,
                    Detail = property.Type.DisplayName,
                    Name = property.Key
                }));
            }
            else if (node is AssetFileStringValueNode strValue)
            {
                string? propertyName = (strValue.Parent as AssetFileKeyValuePairNode)?.Key?.Value;
                SpecProperty? property = propertyName == null ? null : _specDictionary.FindPropertyInfo(propertyName, type);
                Range range = node.Range.ToRange();
                symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
                {
                    Range = range,
                    Kind = property?.Type?.GetSymbolKind() ?? SymbolKind.String,
                    Deprecated = false,
                    SelectionRange = range,
                    Name = strValue.Value
                }));
            }
        }

        return symbols;
    }
}
