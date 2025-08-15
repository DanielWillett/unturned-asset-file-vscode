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

        AssetFileTree tree = file.File;

        AssetFileType fileType = AssetFileType.FromFile(file.File, _specDatabase);

        AssetFileNode? node = tree.GetNode(position.ToFilePosition());
        GetRelationalNodes(node, out AssetFileValueNode? valueNode, out AssetFileKeyValuePairNode? parentNode, out AssetFileKeyNode? keyNode);

        if (valueNode == null || keyNode == null)
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        SpecProperty? property = _specDatabase.FindPropertyInfo(
            keyNode.Value,
            fileType,
            file.IsLocalization
                ? SpecPropertyContext.Localization
                : SpecPropertyContext.Property
        );

        FileEvaluationContext evalCtx = new FileEvaluationContext(
            property!,
            fileType.Information,
            tree,
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

        AssetFileTree tree = file.File;

        AssetFileType fileType = AssetFileType.FromFile(file.File, _specDatabase);

        AssetFileNode? node = tree.GetNode(position.ToFilePosition());
        GetRelationalNodes(node, out _, out _, out AssetFileKeyNode? keyNode);

        if (keyNode == null)
        {
            Unsafe.SkipInit(out ctx);
            return false;
        }

        SpecProperty? property = _specDatabase.FindPropertyInfo(keyNode.Value, fileType, SpecPropertyContext.Property);

        ctx = new FileEvaluationContext(
            property!,
            fileType.Information,
            tree,
            _workspaceEnvironment,
            _installationEnvironment,
            _specDatabase,
            file
        );
        return property != null;
    }

    private static void GetRelationalNodes(AssetFileNode? node, out AssetFileValueNode? valueNode, out AssetFileKeyValuePairNode? pairNode, out AssetFileKeyNode? keyNode)
    {
        switch (node)
        {
            case AssetFileKeyNode k:
                pairNode = k.Parent as AssetFileKeyValuePairNode;
                keyNode = k;
                valueNode = pairNode?.Value;
                break;

            case AssetFileKeyValuePairNode kvp:
                pairNode = kvp;
                keyNode = kvp.Key;
                valueNode = kvp.Value;
                break;

            case null:
                pairNode = null;
                keyNode = null;
                valueNode = null;
                break;

            default:
                pairNode = node.Parent as AssetFileKeyValuePairNode;
                keyNode = pairNode?.Key;
                valueNode = node as AssetFileValueNode;
                break;
        }
    }
}
