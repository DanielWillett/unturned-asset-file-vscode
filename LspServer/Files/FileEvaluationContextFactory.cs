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
    private readonly InstallationEnvironment _installationEnvironment;

    public FileEvaluationContextFactory(
        OpenedFileTracker fileTracker,
        IAssetSpecDatabase specDatabase,
        IWorkspaceEnvironment workspaceEnvironment,
        InstallationEnvironment installationEnvironment)
    {
        _fileTracker = fileTracker;
        _specDatabase = specDatabase;
        _workspaceEnvironment = workspaceEnvironment;
        _installationEnvironment = installationEnvironment;
    }

    public bool TryCreate(Position position, DocumentUri uri, out SpecPropertyTypeParseContext ctx)
    {
        return TryCreate(position, uri, null, out ctx);
    }
    
    public bool TryCreate(Position position, DocumentUri uri, ICollection<DatDiagnosticMessage>? diagnosticSink, out SpecPropertyTypeParseContext ctx)
    {
        if (!_fileTracker.Files.TryGetValue(uri, out OpenedFile? file))
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        ISourceFile sourceFile = file.SourceFile;

        AssetFileType fileType = AssetFileType.FromFile(sourceFile, _specDatabase);

        ISourceNode? node = sourceFile.GetNodeFromPosition(position.ToFilePosition());
        GetRelationalNodes(node, out IAnyValueSourceNode? valueNode, out IPropertySourceNode? parentNode);

        if (valueNode == null || parentNode == null)
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        SpecProperty? property = _specDatabase.FindPropertyInfo(
            parentNode.Key,
            fileType,
            sourceFile is ILocalizationSourceFile
                ? SpecPropertyContext.Localization
                : SpecPropertyContext.Property
        );

        FileEvaluationContext evalCtx = new FileEvaluationContext(
            property!,
            fileType.Information,
            sourceFile,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            file
        );

        ctx = SpecPropertyTypeParseContext.FromFileEvaluationContext(evalCtx, property, parentNode, valueNode, diagnosticSink);
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
            Unsafe.SkipInit(out ctx);
            return false;
        }

        SpecProperty? property = _specDatabase.FindPropertyInfo(parentNode.Key, fileType, SpecPropertyContext.Property);

        ctx = new FileEvaluationContext(
            property!,
            fileType.Information,
            sourceFile,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            file
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
