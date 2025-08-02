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

        AssetFileTree tree = file.File;

        AssetFileType type = AssetFileType.FromFile(tree, _specDictionary);

        List<DatDiagnosticMessage> datDiags = new List<DatDiagnosticMessage>();

        foreach (AssetFileNode node in tree)
        {
            if (node is not AssetFileKeyValuePairNode kvp)
                continue;

            SpecProperty? property = _specDictionary.FindPropertyInfo(kvp.Key.Value, type);
            if (property == null)
                continue;

            FileEvaluationContext ctx = new FileEvaluationContext(
                property,
                property.Owner,
                tree,
                _workspace,
                _installationEnvironment,
                _specDictionary,
                file
            );

            ISpecPropertyType? propType = property.Type.GetType(in ctx);
            if (propType == null)
                continue;

            SpecPropertyTypeParseContext parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(ctx, property, kvp, kvp.Value, datDiags);

            propType.TryParseValue(in parse, out _);
        }

        List<Diagnostic> diagnostics = new List<Diagnostic>(datDiags.Count);
        foreach (DatDiagnosticMessage msg in datDiags)
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
}
