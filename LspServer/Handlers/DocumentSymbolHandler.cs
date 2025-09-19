using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
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
    private readonly IAssetSpecDatabase _specDictionary;
    private readonly ILogger<DocumentSymbolHandler> _logger;
    private readonly IWorkspaceEnvironment _workspace;
    private readonly InstallationEnvironment _installationEnvironment;

    /// <inheritdoc />
    DocumentSymbolRegistrationOptions IRegistration<DocumentSymbolRegistrationOptions, DocumentSymbolCapability>.GetRegistrationOptions(
        DocumentSymbolCapability capability, ClientCapabilities clientCapabilities)
    {
        return new DocumentSymbolRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector
        };
    }

    public DocumentSymbolHandler(
        OpenedFileTracker fileTracker,
        IAssetSpecDatabase specDictionary,
        ILogger<DocumentSymbolHandler> logger,
        IWorkspaceEnvironment workspace,
        InstallationEnvironment installationEnvironment)
    {
        _fileTracker = fileTracker;
        _specDictionary = specDictionary;
        _logger = logger;
        _workspace = workspace;
        _installationEnvironment = installationEnvironment;
    }

    /// <inheritdoc />
    public async Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request, CancellationToken cancellationToken)
    {
        // todo:
        await Task.CompletedTask;

        _logger.LogInformation("Document symbol received.");

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return new SymbolInformationOrDocumentSymbolContainer();
        }

        ISourceFile sourceFile = file.SourceFile;

        AssetFileType type = AssetFileType.FromFile(sourceFile, _specDictionary);

        DocumentSymbolInformationVisitor visitor = new DocumentSymbolInformationVisitor(_specDictionary, _workspace, _installationEnvironment, type);

        sourceFile.Visit(ref visitor);

        return visitor.Symbols;
    }

    public class DocumentSymbolInformationVisitor(
        IAssetSpecDatabase database,
        IWorkspaceEnvironment workspaceEnvironment,
        InstallationEnvironment installEnvironment,
        AssetFileType type
    ) : OrderedNodeVisitor
    {
        public readonly List<SymbolInformationOrDocumentSymbol> Symbols = new List<SymbolInformationOrDocumentSymbol>(256);

        protected override bool IgnoreMetadata => true;

        /// <inheritdoc />
        protected override void AcceptProperty(IPropertySourceNode node)
        {
            SpecProperty? property = database.FindPropertyInfo(node.Key, type);
            if (property == null)
                return;

            FileEvaluationContext ctx = new FileEvaluationContext(
                property,
                property.Owner,
                node.File,
                workspaceEnvironment,
                installEnvironment,
                database,
                node.File.WorkspaceFile
            );

            ISpecPropertyType? propType = property.Type.GetType(in ctx);
            if (propType == null)
                return;

            Range range = node.Range.ToRange();
            Symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
            {
                Range = range,
                Kind = SymbolKind.Property,
                Deprecated = false,
                SelectionRange = range,
                Detail = propType.DisplayName,
                Name = property.Key
            }));
        }

        protected override void AcceptValue(IValueSourceNode node)
        {
            string? propertyName = (node.Parent as IPropertySourceNode)?.Key;
            SpecProperty? property = propertyName == null ? null : database.FindPropertyInfo(propertyName, type);
            Range range = node.Range.ToRange();

            ISpecPropertyType? propType = null;
            if (property != null)
            {
                FileEvaluationContext ctx = new FileEvaluationContext(
                    property,
                    property.Owner,
                    node.File,
                    workspaceEnvironment,
                    installEnvironment,
                    database,
                    node.File.WorkspaceFile
                );

                propType = property.Type.GetType(in ctx);
            }

            Symbols.Add(new SymbolInformationOrDocumentSymbol(new DocumentSymbol
            {
                Range = range,
                Kind = propType?.GetSymbolKind() ?? SymbolKind.String,
                Deprecated = false,
                SelectionRange = range,
                Name = node.Value
            }));
        }
    }
}