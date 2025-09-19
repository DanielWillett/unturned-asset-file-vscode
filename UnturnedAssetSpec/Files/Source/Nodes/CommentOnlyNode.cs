using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[DebuggerDisplay("{ToString(),nq}")]
internal sealed class CommentOnlyNode : AnySourceNode, ICommentSourceNode
{
    public Comment Comment { get; set; }

    public override SourceNodeType Type => SourceNodeType.Comment;

    public static CommentOnlyNode Create(Comment comment, in AnySourceNodeProperties properties)
    {
        if (comment.Content == null)
            throw new ArgumentNullException(nameof(comment));

        return new CommentOnlyNode(comment, in properties);
    }

    private CommentOnlyNode(Comment comment, in AnySourceNodeProperties properties) : base(in properties)
    {
        Comment = comment;
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comment.Equals(((CommentOnlyNode)other).Comment);
    }

    public override string ToString()
    {
        return Comment.ToString();
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.AcceptCommentOnly(this);
    }

    OneOrMore<Comment> ICommentSourceNode.Comments => new OneOrMore<Comment>(Comment);
}