using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Files;

internal class FileEvaluationContextFactory
{
    private readonly OpenedFileTracker _fileTracker;
    private readonly IParsingServices _services;

    public FileEvaluationContextFactory(
        OpenedFileTracker fileTracker,
        IParsingServices services)
    {
        _fileTracker = fileTracker;
        _services = services;
    }

    public bool TryCreate(Position position, DocumentUri uri, [UnscopedRef] out FileEvaluationContext ctx, out ISourceNode? node)
    {
        return TryCreate(position, uri, null, out ctx, out node);
    }
    
    public bool TryCreate(Position position, DocumentUri uri, IDiagnosticSink? diagnosticSink, [UnscopedRef] out FileEvaluationContext ctx, out ISourceNode? node)
    {
        node = null;
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        ctx = new FileEvaluationContext(
            _services,
            sourceFile
        );
        return true;
        // todo: remove this
#if false

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _specDatabase);

        node = sourceFile.GetNodeFromPosition(position.ToFilePosition());
        GetRelationalNodes(node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? parentNode);

        if (parentNode == null)
        {
            ctx = new FileEvaluationContext(
                _services,
                sourceFile);
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

        ctx = new FileEvaluationContext(
            property!,
            fileType.Information,
            sourceFile,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            context
        );

        ctx =
 SpecPropertyTypeParseContext.FromFileEvaluationContext(in ctx, breadcrumbs, property, parentNode, valueNode, diagnosticSink);
        return property != null;
#endif
    }

    public bool TryCreate(Position position, DocumentUri uri, out FileEvaluationContext ctx)
    {
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        ctx = new FileEvaluationContext(
            _services,
            sourceFile);
        return true;
        // todo remove this
#if false
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
#endif
    }

    //private static void GetRelationalNodes(ISourceNode? node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? propertyNode)
    //{
    //    switch (node)
    //    {
    //        case IPropertySourceNode kvp:
    //            propertyNode = kvp;
    //            valueNode = kvp.Value;
    //            break;
    //
    //        case null:
    //            propertyNode = null;
    //            valueNode = null;
    //            break;
    //
    //        default:
    //            ISourceNode parent = node.Parent;
    //            for (; !ReferenceEquals(parent, parent.File) && parent is not IPropertySourceNode; parent = parent.Parent) ;
    //            propertyNode = parent as IPropertySourceNode;
    //            valueNode = node as IAnyValueSourceNode;
    //            break;
    //    }
    //}
}
