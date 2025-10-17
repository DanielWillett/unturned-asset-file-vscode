using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class DocumentDiagnosticHandler : DocumentDiagnosticHandlerBase
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly IAssetSpecDatabase _specDictionary;
    private readonly ILogger<DocumentSymbolHandler> _logger;
    private readonly IWorkspaceEnvironment _workspace;
    private readonly InstallationEnvironment _installationEnvironment;

    public DocumentDiagnosticHandler(
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
    protected override DiagnosticsRegistrationOptions CreateRegistrationOptions(
        DiagnosticClientCapabilities capability, ClientCapabilities clientCapabilities)
    {
        return new DiagnosticsRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector,
            InterFileDependencies = true,
            WorkspaceDiagnostics = true
        };
    }

    /// <inheritdoc />
    public override async Task<RelatedDocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;

        _logger.LogInformation("Document diagnostic pull request received.");

        if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        {
            return new RelatedFullDocumentDiagnosticReport { Items = new Container<Diagnostic>() };
        }

        ISourceFile tree = file.SourceFile;

        AssetFileType type = AssetFileType.FromFile(tree, _specDictionary);

        DiagnosticsNodeVisitor visitor = new DiagnosticsNodeVisitor(_specDictionary, _workspace, _installationEnvironment, type);

        if (tree is IAssetSourceFile asset)
        {
            asset.GetMetadataDictionary()?.Visit(ref visitor);
            asset.AssetData.Visit(ref visitor);
        }
        else
        {
            tree.Visit(ref visitor);
        }

        List<Diagnostic> diagnostics = new List<Diagnostic>(visitor.Diagnostics.Count);
        foreach (DatDiagnosticMessage msg in visitor.Diagnostics)
        {
            diagnostics.Add(new Diagnostic
            {
                Code = new DiagnosticCode(msg.Diagnostic.ErrorId),
                Source = "unturned-dat",
                Message = msg.Message,
                Range = msg.Range.ToRange(),
                Tags = msg.Diagnostic == DatDiagnostics.UNT1018 ? new Container<DiagnosticTag>(DiagnosticTag.Deprecated) : null
            });
        }

        return new RelatedFullDocumentDiagnosticReport
        {
            Items = diagnostics
        };
    }

    private class DiagnosticsNodeVisitor(
        IAssetSpecDatabase database,
        IWorkspaceEnvironment workspace,
        InstallationEnvironment installEnvironment,
        AssetFileType type
    ) : TopLevelNodeVisitor, IDiagnosticSink
    {
        /// <inheritdoc />
        protected override bool IgnoreMetadata => true;

        public readonly List<DatDiagnosticMessage> Diagnostics = new List<DatDiagnosticMessage>();

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
                workspace,
                installEnvironment,
                database,
                // todo: should support legacy and nested types
                PropertyResolutionContext.Modern
            );

            ISpecPropertyType? propType = property.Type.GetType(in ctx);
            if (propType == null)
                return;

            SpecPropertyTypeParseContext parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(ctx, property, node, node.Value, Diagnostics);

            propType.TryParseValue(in parse, out _);
        }

        /// <inheritdoc />
        public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
        {
            Diagnostics.Add(diagnostic);
        }
    }
}
