using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal class RootDictionaryNode : DictionaryNode, ISourceFile
{
    private protected IAssetSpecDatabase Database { get; }

    public IWorkspaceFile WorkspaceFile { get; internal set; }
    public object TreeSync { get; }
    public ImmutableArray<IPropertySourceNode> Properties { get; internal set; }
    public virtual QualifiedType ActualType { get; protected set; }

    public static RootDictionaryNode Create(
        IWorkspaceFile file,
        IAssetSpecDatabase database,
        int count,
        ISourceNode[] nodes,
        in AnySourceNodeProperties properties)
    {
        return new RootDictionaryNode(file, database, count, nodes, in properties);
    }

    private protected RootDictionaryNode(
        IWorkspaceFile file,
        IAssetSpecDatabase database,
        int count,
        ISourceNode[] nodes,
        in AnySourceNodeProperties properties
    )
       : base(count, nodes, in properties)
    {
        WorkspaceFile = file;
        Database = database;
        TreeSync = new object();
        
        ImmutableArray<IPropertySourceNode>.Builder builder = ImmutableArray.CreateBuilder<IPropertySourceNode>(count);
        for (int i = 0; i < nodes.Length; ++i)
        {
            if (nodes[i] is not IPropertySourceNode pn)
                continue;

            builder.Add(pn);
        }

        Properties = builder.MoveToImmutable();
        SetParentInfo(this, this);
    }

    internal sealed override void SetParentInfo(ISourceFile file, ISourceNode parent)
    {
        base.SetParentInfo(file, parent);
    }
}