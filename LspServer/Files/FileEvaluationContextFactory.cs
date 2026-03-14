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
    private readonly IParsingServices _services;

    public FileEvaluationContextFactory(
        OpenedFileTracker fileTracker,
        IParsingServices services)
    {
        _fileTracker = fileTracker;
        _services = services;
    }

    public bool TryCreate(
        Position position,
        DocumentUri uri,
        [UnscopedRef] out FileEvaluationContext ctx,
        [NotNullWhen(true)] out DatProperty? property,
        [NotNullWhen(true)] out IPropertySourceNode? propertyNode,
        [NotNullWhen(true)] out ISourceNode? node
    )
    {
        node = null;
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            property = null;
            propertyNode = null;
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        node = sourceFile.GetNodeFromPosition(position.ToFilePosition());
        GetRelationalNodes(node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? parentNode);

        propertyNode = parentNode;
        if (parentNode == null)
        {
            ctx = new FileEvaluationContext(
                _services,
                sourceFile
            );
            property = null;
            return false;
        }

        ctx = new FileEvaluationContext(
            _services,
            sourceFile,
            parentNode.GetRootPosition()
        );

        PropertyBreadcrumbs breadcrumbs;
        if (valueNode?.Parent is IListSourceNode)
        {
            for (; valueNode.Parent is IAnyValueSourceNode v; valueNode = v) ;
            breadcrumbs = PropertyBreadcrumbs.FromNode(valueNode.Parent);
        }
        else
            breadcrumbs = PropertyBreadcrumbs.FromNode(valueNode?.Parent ?? parentNode);

        ctx.RootBreadcrumbs = breadcrumbs;

        IFileRelationalModel model = ctx.GetRelationalModel();
        return model.TryGetPropertyFromNode(parentNode, out property);
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
