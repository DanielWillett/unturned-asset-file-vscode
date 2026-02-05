using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Files;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class DocumentDiagnosticHandler : DocumentDiagnosticHandlerBase
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly IParsingServices _parsingServices;
    private readonly ILogger<DocumentDiagnosticHandler> _logger;
    private readonly IFileRelationalModelProvider _modelProvider;
    private readonly DiagnosticsManager _diagnosticsManager;

    public DocumentDiagnosticHandler(
        OpenedFileTracker fileTracker,
        IParsingServices parsingServices,
        ILogger<DocumentDiagnosticHandler> logger,
        DiagnosticsManager diagnosticsManager,
        IFileRelationalModelProvider modelProvider)
    {
        _fileTracker = fileTracker;
        _parsingServices = parsingServices;
        _logger = logger;
        _diagnosticsManager = diagnosticsManager;
        _modelProvider = modelProvider;
    }

    /// <inheritdoc />
    protected override DiagnosticsRegistrationOptions CreateRegistrationOptions(
        DiagnosticClientCapabilities capability, ClientCapabilities clientCapabilities)
    {
        return new DiagnosticsRegistrationOptions
        {
            DocumentSelector = UnturnedAssetFileLspServer.AssetFileSelector,
            InterFileDependencies = true,
            WorkspaceDiagnostics = false
        };
    }

    /// <inheritdoc />
    public override async Task<RelatedDocumentDiagnosticReport> Handle(DocumentDiagnosticParams request, CancellationToken cancellationToken)
    {
        string filePath = Path.GetFullPath(request.TextDocument.Uri.GetFileSystemPath());
        FileDiagnostics diag = _diagnosticsManager.GetOrAddFile(filePath, request.TextDocument.Uri);

        //Container<Diagnostic> d = diag.Recalculate();

        return new RelatedFullDocumentDiagnosticReport
        {
            Items = new Container<Diagnostic>()
        };

        //Debugger.Launch();
        //_logger.LogInformation("Document diagnostic pull request received.");
        //
        //if (!_fileTracker.Files.TryGetValue(request.TextDocument.Uri, out OpenedFile? file))
        //{
        //    return new RelatedFullDocumentDiagnosticReport { Items = new Container<Diagnostic>() };
        //}
        //
        //ISourceFile tree = file.SourceFile;
        //
        //DiagnosticsNodeVisitor visitor = new DiagnosticsNodeVisitor(_specDatabase, _virtualizer, _workspace, _installationEnvironment);
        //
        //if (tree is IAssetSourceFile asset)
        //{
        //    asset.GetMetadataDictionary()?.Visit(ref visitor);
        //    asset.AssetData.Visit(ref visitor);
        //}
        //else
        //{
        //    tree.Visit(ref visitor);
        //}
        //
        //List<Diagnostic> diagnostics = new List<Diagnostic>(visitor.Diagnostics.Count);
        //foreach (DatDiagnosticMessage msg in visitor.Diagnostics)
        //{
        //    diagnostics.Add(new Diagnostic
        //    {
        //        Code = new DiagnosticCode(msg.Diagnostic.ErrorId),
        //        Source = UnturnedAssetFileLspServer.DiagnosticSource,
        //        Message = msg.Message,
        //        Range = msg.Range.ToRange(),
        //        Tags = msg.Diagnostic == DatDiagnostics.UNT1018 ? new Container<DiagnosticTag>(DiagnosticTag.Deprecated) : null
        //    });
        //}
        //
        //return new RelatedFullDocumentDiagnosticReport
        //{
        //    Items = diagnostics
        //};
    }

    private class DiagnosticsNodeVisitor : ResolvedPropertyNodeVisitor, IDiagnosticSink
    {
        /// <inheritdoc />
        protected override bool IgnoreMetadata => true;

        public readonly List<DatDiagnosticMessage> Diagnostics = new List<DatDiagnosticMessage>();

        public DiagnosticsNodeVisitor(
            IFileRelationalModelProvider modelProvider,
            IParsingServices parsingServices)
            : base(modelProvider, parsingServices)
        {
        }

        /// <inheritdoc />
        protected override void AcceptResolvedProperty(
            DatProperty property,
            IType propertyType,
            in FileEvaluationContext ctx,
            IPropertySourceNode node,
            in PropertyBreadcrumbs breadcrumbs)
        {
            // todo SpecPropertyTypeParseContext ctx = parseCtx.WithDiagnostics(Diagnostics);
            // todo propertyType.TryParseValue(in ctx, out _);
        }

        /// <inheritdoc />
        public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
        {
            Diagnostics.Add(diagnostic);
        }
    }
}
