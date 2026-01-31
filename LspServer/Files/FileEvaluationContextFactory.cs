using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Diagnostics.CodeAnalysis;
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

    public bool TryCreate(Position position, DocumentUri uri, [UnscopedRef] out FileEvaluationContext fileCtx, out SpecPropertyTypeParseContext ctx, out ISourceNode? node)
    {
        return TryCreate(position, uri, null, out fileCtx, out ctx, out node);
    }
    
    public bool TryCreate(Position position, DocumentUri uri, IDiagnosticSink? diagnosticSink, [UnscopedRef] out FileEvaluationContext fileCtx, out SpecPropertyTypeParseContext ctx, out ISourceNode? node)
    {
        node = null;
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            Unsafe.SkipInit(out fileCtx);
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _specDatabase);

        node = sourceFile.GetNodeFromPosition(position.ToFilePosition());
        GetRelationalNodes(node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? parentNode);

        if (parentNode == null)
        {
            fileCtx = new FileEvaluationContext(
                null!,
                null!,
                sourceFile,
                _workspaceEnvironment,
                _installationEnvironment,
                _specDatabase,
                PropertyResolutionContext.Modern);
            ctx = new SpecPropertyTypeParseContext(
                in fileCtx,
                PropertyBreadcrumbs.Root,
                null)
            {
                Node = valueNode,
                Parent = null,
                BaseKey = parentNode?.Key
            };
            return false;
        }

        PropertyBreadcrumbs breadcrumbs;
        if (valueNode?.Parent is IListSourceNode)
        {
            for (; valueNode.Parent is IAnyValueSourceNode v; valueNode = v) ;
            breadcrumbs = PropertyBreadcrumbs.FromNode(valueNode.Parent);
        }
        else
            breadcrumbs = PropertyBreadcrumbs.FromNode(valueNode?.Parent ?? parentNode);
        DatProperty? property = _propertyVirtualizer.GetProperty(
            parentNode,
            in fileType,
            in breadcrumbs,
            out PropertyResolutionContext context
        );

        fileCtx = new FileEvaluationContext(
            property!,
            fileType.Information,
            sourceFile,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            context
        );

        ctx = SpecPropertyTypeParseContext.FromFileEvaluationContext(in fileCtx, breadcrumbs, property, parentNode, valueNode, diagnosticSink);
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
        GetRelationalNodes(node, out IAnyValueSourceNode? value, out IPropertySourceNode? propertyNode);

        if (propertyNode == null)
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

        PropertyBreadcrumbs breadcrumbs = PropertyBreadcrumbs.FromNode(value?.Parent ?? propertyNode);
        DatProperty? property = _propertyVirtualizer.GetProperty(
            propertyNode,
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

    private static void GetRelationalNodes(ISourceNode? node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? propertyNode)
    {
        switch (node)
        {
            case IPropertySourceNode kvp:
                propertyNode = kvp;
                valueNode = kvp.Value;
                break;

            case null:
                propertyNode = null;
                valueNode = null;
                break;

            default:
                ISourceNode parent = node.Parent;
                for (; !ReferenceEquals(parent, parent.File) && parent is not IPropertySourceNode; parent = parent.Parent) ;
                propertyNode = parent as IPropertySourceNode;
                valueNode = node as IAnyValueSourceNode;
                break;
        }
    }
}
