using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class FileEvaluationContextFactory
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly IAssetSpecDatabase _specDatabase;
    private readonly IWorkspaceEnvironment _workspaceEnvironment;
    private readonly IFilePropertyVirtualizer _propertyVirtualizer;
    private readonly InstallationEnvironment _installationEnvironment;

    public FileEvaluationContextFactory(
        OpenedFileTracker fileTracker,
        IAssetSpecDatabase specDatabase,
        IWorkspaceEnvironment workspaceEnvironment,
        InstallationEnvironment installationEnvironment,
        IFilePropertyVirtualizer propertyVirtualizer)
    {
        _fileTracker = fileTracker;
        _specDatabase = specDatabase;
        _workspaceEnvironment = workspaceEnvironment;
        _installationEnvironment = installationEnvironment;
        _propertyVirtualizer = propertyVirtualizer;
    }

    public bool TryCreate(Position position, DocumentUri uri, out SpecPropertyTypeParseContext ctx, out ISourceNode? node)
    {
        return TryCreate(position, uri, null, out ctx, out node);
    }
    
    public bool TryCreate(Position position, DocumentUri uri, ICollection<DatDiagnosticMessage>? diagnosticSink, out SpecPropertyTypeParseContext ctx, out ISourceNode? node)
    {
        node = null;
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _specDatabase);

        node = sourceFile.GetNodeFromPosition(position.ToFilePosition());
        GetRelationalNodes(node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? parentNode);

        if (parentNode == null)
        {
            ctx = new SpecPropertyTypeParseContext(
                new FileEvaluationContext(
                    null!,
                    null!,
                    sourceFile,
                    _workspaceEnvironment,
                    _installationEnvironment,
                    _specDatabase,
                    PropertyResolutionContext.Modern),
                PropertyBreadcrumbs.Root,
                null)
            {
                Node = valueNode,
                Parent = null
            };
            return false;
        }

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(parentNode);
        SpecProperty? property = _propertyVirtualizer.GetProperty(
            parentNode,
            in fileType,
            in breadcrumbs,
            out PropertyResolutionContext context
        );

        FileEvaluationContext evalCtx = new FileEvaluationContext(
            property!,
            fileType.Information,
            sourceFile,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            context
        );

        ctx = SpecPropertyTypeParseContext.FromFileEvaluationContext(evalCtx, breadcrumbs, property, parentNode, valueNode, diagnosticSink);
        return property != null;
    }

    public bool TryCreate(Position position, DocumentUri uri, out FileEvaluationContext ctx)
    {
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _specDatabase);

        ISourceNode? node = sourceFile.GetNodeFromPosition(position.ToFilePosition());
        GetRelationalNodes(node, out _, out IPropertySourceNode? parentNode);

        if (parentNode == null)
        {
            ctx = new FileEvaluationContext(
                null!,
                null!,
                sourceFile,
                _workspaceEnvironment,
                _installationEnvironment,
                _specDatabase,
                PropertyResolutionContext.Modern
            );
            return false;
        }

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(parentNode);
        SpecProperty? property = _propertyVirtualizer.GetProperty(
            parentNode,
            in fileType,
            in breadcrumbs,
            out PropertyResolutionContext context
        );

        ctx = new FileEvaluationContext(
            property!,
            fileType.Information,
            sourceFile,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            context
        );
        return property != null;
    }

    private static void GetRelationalNodes(ISourceNode? node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? pairNode)
    {
        switch (node)
        {
            case IPropertySourceNode kvp:
                pairNode = kvp;
                valueNode = kvp.Value;
                break;

            case null:
                pairNode = null;
                valueNode = null;
                break;

            default:
                pairNode = node.Parent as IPropertySourceNode;
                valueNode = node as IAnyValueSourceNode;
                break;
        }
    }
}
