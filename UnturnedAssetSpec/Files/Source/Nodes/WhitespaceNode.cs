using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[DebuggerDisplay("{ToString(),nq}")]
internal class WhitespaceNode : AnySourceNode, IWhiteSpaceSourceNode
{
    public int Lines { get; }

    public override SourceNodeType Type => SourceNodeType.Whitespace;
    
    public static WhitespaceNode Create(int lines, in AnySourceNodeProperties properties)
    {
        if (lines <= 0)
            throw new ArgumentOutOfRangeException(nameof(lines));

        return new WhitespaceNode(lines, in properties);
    }

    private WhitespaceNode(int lines, in AnySourceNodeProperties properties) : base(in properties)
    {
        Lines = lines;
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Lines == ((WhitespaceNode)other).Lines;
    }

    public override string ToString()
    {
        return Lines == 1 ? "-whitespace-" : $"-whitespace x{Lines}-";
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.AcceptWhiteSpace(this);
    }
}