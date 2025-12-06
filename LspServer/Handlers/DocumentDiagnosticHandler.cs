using System.Collections.Immutable;
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
using System.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Diagnostics;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers;

internal class DocumentDiagnosticHandler : DocumentDiagnosticHandlerBase
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly IAssetSpecDatabase _specDatabase;
    private readonly ILogger<DocumentDiagnosticHandler> _logger;
    private readonly IWorkspaceEnvironment _workspace;
    private readonly IFilePropertyVirtualizer _virtualizer;
    private readonly InstallationEnvironment _installationEnvironment;
    private readonly DiagnosticsManager _diagnosticsManager;

    public DocumentDiagnosticHandler(
        OpenedFileTracker fileTracker,
        IAssetSpecDatabase specDatabase,
        ILogger<DocumentDiagnosticHandler> logger,
        IWorkspaceEnvironment workspace,
        InstallationEnvironment installationEnvironment,
        DiagnosticsManager diagnosticsManager,
        IFilePropertyVirtualizer virtualizer)
    {
        _fileTracker = fileTracker;
        _specDatabase = specDatabase;
        _logger = logger;
        _workspace = workspace;
        _installationEnvironment = installationEnvironment;
        _diagnosticsManager = diagnosticsManager;
        _virtualizer = virtualizer;
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

        public DiagnosticsNodeVisitor(IAssetSpecDatabase database,
            IFilePropertyVirtualizer virtualizer,
            IWorkspaceEnvironment workspace,
            InstallationEnvironment installEnvironment)
            : base(virtualizer, database, installEnvironment, workspace)
        {
        }

        /// <inheritdoc />
        protected override void AcceptResolvedProperty(
            SpecProperty property,
            ISpecPropertyType propertyType,
            in SpecPropertyTypeParseContext parseCtx,
            IPropertySourceNode node,
            PropertyBreadcrumbs breadcrumbs)
        {
            SpecPropertyTypeParseContext ctx = parseCtx.WithDiagnostics(Diagnostics);
            propertyType.TryParseValue(in ctx, out _);
        }

        /// <inheritdoc />
        public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
        {
            Diagnostics.Add(diagnostic);
        }
    }
}
