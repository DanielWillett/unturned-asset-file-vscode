using System.Collections.Generic;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Immutable;
using System.IO;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class RootDictionaryNode : DictionaryNode, ISourceFile
{
    internal IAssetSpecDatabase? Database { get; }
    public OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; }

    public IWorkspaceFile WorkspaceFile { get; internal set; }
    public object TreeSync { get; }
    public int FileVersion { get; private set; }
    public ImmutableArray<IPropertySourceNode> Properties { get; internal set; }
    public QualifiedType ActualType { get; protected set; }

    public static RootDictionaryNode Create(
        IWorkspaceFile file,
        IAssetSpecDatabase? database,
        int count,
        ISourceNode[] nodes,
        in AnySourceNodeProperties properties,
        OneOrMore<KeyValuePair<string, object?>> additionalProperties)
    {
        return new RootDictionaryNode(file, database, count, nodes, in properties, additionalProperties);
    }

    private protected RootDictionaryNode(
        IWorkspaceFile file,
        IAssetSpecDatabase? database,
        int count,
        ISourceNode[] nodes,
        in AnySourceNodeProperties properties,
        OneOrMore<KeyValuePair<string, object?>> additionalProperties
    )
       : base(count, nodes, in properties)
    {
        FileVersion = 1;
        WorkspaceFile = file;
        Database = database;
        AdditionalProperties = additionalProperties;
        TreeSync = new object();
        
        ImmutableArray<IPropertySourceNode>.Builder builder = ImmutableArray.CreateBuilder<IPropertySourceNode>(count);
        for (int i = 0; i < nodes.Length; ++i)
        {
            if (nodes[i] is not IPropertySourceNode pn)
                continue;

            builder.Add(pn);
        }

        if (database is { IsInitialized: true } && this is not RootAssetNodeSkippedLocalization and not RootLocalizationNode)
        {
            if (this.TryGetAdditionalProperty(Comment.TypeAdditionalProperty, out string? str) && str != null)
            {
                ActualType = new QualifiedType(str, true);
            }
            else if (database.Information.KnownFileNames.TryGetValue(Path.GetFileName(file.File), out QualifiedType t) && !t.IsNull)
            {
                ActualType = t.CaseInsensitive;
            }
        }

        Properties = builder.MoveToImmutableOrCopy();
        SetParentInfo(this, this);
    }

    internal sealed override void SetParentInfo(ISourceFile? file, IParentSourceNode parent)
    {
        base.SetParentInfo(file, parent);
    }

    int ISourceFile.FileVersion { get => FileVersion; set => FileVersion = value; }
}