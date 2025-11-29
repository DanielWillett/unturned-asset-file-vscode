using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal abstract class AnySourceNode : ISourceNode
{
    public abstract SourceNodeType Type { get; }

    // note: these are part of the hash code
    //       so changing them will mess up property
    //       dictionaries containing this object

    public int Index { get; set; }
    public int ChildIndex { get; set; }

    public FileRange Range { get; set; }

#nullable disable

    public ISourceFile File { get; private set; }

    public IParentSourceNode Parent { get; private set; }

#nullable restore

    public int Depth { get; }

    public int FirstCharacterIndex { get; private set; }
    public int LastCharacterIndex { get; private set; }

    internal virtual void SetParentInfo(ISourceFile? file, IParentSourceNode parent)
    {
        Parent = parent;
        File = file!;
    }

    internal AnySourceNodeProperties GetAnyNodeProperties()
    {
        return new AnySourceNodeProperties
        {
            Index = Index,
            Range = Range,
            Depth = Depth,
            ChildIndex = ChildIndex,
            FirstCharacterIndex = FirstCharacterIndex,
            LastCharacterIndex = LastCharacterIndex
        };
    }

    protected void SetParentInfoOfChildren(ISourceNode[] values)
    {
        IParentSourceNode thisParent = (IParentSourceNode)this;
        foreach (ISourceNode node in values)
        {
            if (node is AnySourceNode n)
                n.SetParentInfo(File, thisParent);
        }
    }

    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
    protected AnySourceNode(in AnySourceNodeProperties properties)
    {
        Range = properties.Range;
        Depth = properties.Depth;
        Index = properties.Index;
        ChildIndex = properties.ChildIndex;
        FirstCharacterIndex = properties.FirstCharacterIndex;
        LastCharacterIndex = properties.LastCharacterIndex;
    }

    public virtual bool Equals(ISourceNode other)
    {
        if (ReferenceEquals(this, other))
            return true;

        if (other == null || other.GetType() != GetType())
            return false;

        return Index == other.Index && Range == other.Range && File.Equals(other.File) && Parent.Equals(other.Parent) && Depth == other.Depth;
    }

    public override bool Equals(object? obj)
    {
        return obj is ISourceNode n && Equals(n);
    }

    protected static bool ArraysEqual<T>(T[] array1, T[] array2) where T : IEquatable<T>
    {
        if (array1.Length != array2.Length)
            return false;

        for (int i = 0; i < array1.Length; i++)
        {
            T t1 = array1[i];
            T t2 = array2[i];
            if (ReferenceEquals(t1, t2))
                continue;
            if (t1 == null || t2 == null || !t1.Equals(t2))
                return false;
        }

        return true;
    }

    protected static bool NodesEqual(ISourceNode? n1, ISourceNode? n2)
    {
        if (ReferenceEquals(n1, n2)) return true;
        if (n1 == null || n2 == null) return false;
        return n1.Equals(n2);
    }

    public override int GetHashCode()
    {
        int hashCode = -268197062;
        hashCode = hashCode * -1521134295 + (int)Type;
        hashCode = hashCode * -1521134295 + Index;
        hashCode = hashCode * -1521134295 + Range.GetHashCode();
        hashCode = hashCode * -1521134295 + Depth.GetHashCode();
        return hashCode;
    }

    public abstract void Visit<TVisitor>(ref TVisitor visitor)
        where TVisitor : ISourceNodeVisitor;
}
internal struct AnySourceNodeProperties
{
    public int Depth;
    public FileRange Range;
    public int Index;
    public int FirstCharacterIndex;
    public int LastCharacterIndex;
    public int ChildIndex;
}