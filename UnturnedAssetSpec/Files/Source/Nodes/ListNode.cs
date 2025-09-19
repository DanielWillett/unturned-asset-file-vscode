﻿using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Immutable;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[DebuggerDisplay("{ToString(),nq}")]
internal class ListNode : AnySourceNode, IListSourceNode
{
    public override SourceNodeType Type => SourceNodeType.List;

    public ValueTypeDataRefType ValueType => ValueTypeDataRefType.List;

    public int Count { get; set; }

    public ImmutableArray<ISourceNode> Children => Values.UnsafeFreeze();

    private ISourceNode[] Values { get; set; }

    public static ListNode Create(int count, ISourceNode[] values, OneOrMore<Comment> comments, in AnySourceNodeProperties properties)
    {
        return comments.Length switch
        {
            0 => new ListNode(count, values, in properties),
            1 => new SingleCommentedListNode(count, values, comments.Value, in properties),
            _ => new MultipleCommentedListNode(count, values, comments, in properties),
        };
    }

    private protected ListNode(int count, ISourceNode[] values, in AnySourceNodeProperties properties) : base(in properties)
    {
        Count = count;
        Values = values;
        SetParentInfoOfChildren(values);
    }

    internal override void SetParentInfo(ISourceFile file, ISourceNode parent)
    {
        base.SetParentInfo(file, parent);
        SetParentInfoOfChildren(Values);
    }

    protected static bool EqualsHelper(ListNode n1, ListNode n2)
    {
        return n1.Count == n2.Count && ArraysEqual(n1.Values, n2.Values);
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && EqualsHelper(this, (ListNode)other);
    }

    public override string ToString()
    {
        return $"[ n = {Count} ]";
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.AcceptList(this);
    }
}

internal sealed class SingleCommentedListNode : ListNode, ICommentSourceNode
{
    public Comment Comment { get; set; }

    public override SourceNodeType Type => SourceNodeType.ListWithComment;

    public SingleCommentedListNode(int count, ISourceNode[] values, Comment comment, in AnySourceNodeProperties properties)
        : base(count, values, in properties)
    {
        Comment = comment;
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comment.Equals(((SingleCommentedListNode)other).Comment);
    }

    OneOrMore<Comment> ICommentSourceNode.Comments => new OneOrMore<Comment>(Comment);
}

internal sealed class MultipleCommentedListNode : ListNode, ICommentSourceNode
{
    public OneOrMore<Comment> Comments { get; set; }

    public override SourceNodeType Type => SourceNodeType.ListWithComment;

    public MultipleCommentedListNode(int count, ISourceNode[] values, OneOrMore<Comment> comments, in AnySourceNodeProperties properties)
        : base(count, values, in properties)
    {
        Comments = comments;
    }
    
    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comments.Equals(((MultipleCommentedListNode)other).Comments);
    }
}