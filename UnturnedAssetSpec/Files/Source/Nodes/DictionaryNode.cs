using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[DebuggerDisplay("{ToString(),nq}")]
internal class DictionaryNode : AnySourceNode, IDictionarySourceNode
{
    private StringDictionary<IPropertySourceNode>? _properties;

    public override SourceNodeType Type => SourceNodeType.Dictionary;

    public ValueTypeDataRefType ValueType => ValueTypeDataRefType.Dictionary;

    public int Count { get; set; }

    public ImmutableArray<ISourceNode> Children => Values.UnsafeFreeze();

    private ISourceNode[] Values { get; set; }

    internal void RebuildProperties()
    {
        if (_properties == null)
            _properties = new StringDictionary<IPropertySourceNode>(Count);
        else
            _properties.Clear();

        foreach (ISourceNode node in Values)
        {
            if (node is not IPropertySourceNode property)
                continue;

            _properties[property.Key] = property;
        }
    }

    public bool TryGetProperty(string propertyName, [NotNullWhen(true)] out IPropertySourceNode? node)
    {
        StringDictionary<IPropertySourceNode>? p = _properties;
        if (p == null)
        {
            RebuildProperties();
            p = _properties;
            if (p == null)
            {
                node = null;
                return false;
            }
        }

        return p.TryGetValue(propertyName, out node);
    }

    public static DictionaryNode Create(int count, ISourceNode[] values, OneOrMore<Comment> comments, in AnySourceNodeProperties properties)
    {
        return comments.Length switch
        {
            0 => new DictionaryNode(count, values, in properties),
            1 => new SingleCommentedDictionaryNode(count, values, comments.Value, in properties),
            _ => new MultipleCommentedDictionaryNode(count, values, comments, in properties),
        };
    }

    private protected DictionaryNode(int count, ISourceNode[] values, in AnySourceNodeProperties properties) : base(in properties)
    {
        Count = count;
        Values = values;
        SetParentInfoOfChildren(values);
    }

    internal override void SetParentInfo(ISourceFile? file, IParentSourceNode parent)
    {
        base.SetParentInfo(file, parent);
        SetParentInfoOfChildren(Values);
    }

    protected static bool EqualsHelper(DictionaryNode n1, DictionaryNode n2)
    {
        return n1.Count == n2.Count && ArraysEqual(n1.Values, n2.Values);
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && EqualsHelper(this, (DictionaryNode)other);
    }

    public override string ToString()
    {
        return $$"""{ n = {{Count}} }""";
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.AcceptDictionary(this);
    }
}

internal sealed class SingleCommentedDictionaryNode : DictionaryNode, ICommentSourceNode
{
    public Comment Comment { get; set; }

    public override SourceNodeType Type => SourceNodeType.DictionaryWithComment;

    public SingleCommentedDictionaryNode(int count, ISourceNode[] values, Comment comment, in AnySourceNodeProperties properties)
        : base(count, values, in properties)
    {
        Comment = comment;
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comment.Equals(((SingleCommentedDictionaryNode)other).Comment);
    }

    OneOrMore<Comment> ICommentSourceNode.Comments => new OneOrMore<Comment>(Comment);
}

internal sealed class MultipleCommentedDictionaryNode : DictionaryNode, ICommentSourceNode
{
    public OneOrMore<Comment> Comments { get; set; }

    public override SourceNodeType Type => SourceNodeType.DictionaryWithComment;

    public MultipleCommentedDictionaryNode(int count, ISourceNode[] values, OneOrMore<Comment> comments, in AnySourceNodeProperties properties)
        : base(count, values, in properties)
    {
        Comments = comments;
    }
    
    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comments.Equals(((MultipleCommentedDictionaryNode)other).Comments);
    }
}