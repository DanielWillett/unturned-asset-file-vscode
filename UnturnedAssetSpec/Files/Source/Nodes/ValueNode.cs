using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[DebuggerDisplay("{ToString(),nq}")]
internal class ValueNode : AnySourceNode, IValueSourceNode
{
    public override SourceNodeType Type => SourceNodeType.Value;

    public ValueTypeDataRefType ValueType => ValueTypeDataRefType.Value;

    public string Value { get; set; }

    public bool IsQuoted { get; set; }

    public static ValueNode Create(string value, bool isQuoted, Comment comment, in AnySourceNodeProperties properties)
    {
        return comment.Content == null
            ? new ValueNode(value, isQuoted, in properties)
            : new CommentedValueNode(value, isQuoted, comment, in properties);
    }

    private protected ValueNode(string value, bool isQuoted, in AnySourceNodeProperties properties) : base(in properties)
    {
        Value = value;
        IsQuoted = isQuoted;
    }

    protected static bool EqualsHelper(ValueNode n1, ValueNode n2)
    {
        return n1.IsQuoted == n2.IsQuoted && string.Equals(n1.Value, n2.Value, StringComparison.Ordinal);
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && EqualsHelper(this, (ValueNode)other);
    }

    public override string ToString()
    {
        if (IsQuoted)
            return $"\"{Value}\"";

        return Value;
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.AcceptValue(this);
    }
}

internal sealed class CommentedValueNode : ValueNode, ICommentSourceNode
{
    public Comment Comment { get; set; }

    public override SourceNodeType Type => SourceNodeType.ValueWithComment;

    public CommentedValueNode(string value, bool isQuoted, Comment comment, in AnySourceNodeProperties properties)
        : base(value, isQuoted, in properties)
    {
        Comment = comment;
    }
    
    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comment.Equals(((CommentedValueNode)other).Comment);
    }

    OneOrMore<Comment> ICommentSourceNode.Comments => new OneOrMore<Comment>(Comment);
}