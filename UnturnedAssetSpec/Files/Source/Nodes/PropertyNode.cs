using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

[DebuggerDisplay("{ToString(),nq}")]
internal class PropertyNode : AnySourceNode, IPropertySourceNode
{
    private IAnyValueSourceNode? _value;

    internal LazySource ValueSource { get; set; }
    private bool _hasValue;

    public string Key { get; set; }
    public bool KeyIsQuoted { get; set; }

    public bool HasValue => _hasValue ? _value != null : !ValueSource.Segment.IsEmpty;

    public ValueTypeDataRefType ValueKind { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public IAnyValueSourceNode? Value
    {
        get
        {
            if (_hasValue)
            {
                IAnyValueSourceNode? v = _value;
                if (_hasValue && ReferenceEquals(v, _value))
                    return v;
            }

            lock (File.TreeSync)
            {
                _value = ValueSource.GetNode(null) as IAnyValueSourceNode;
                (_value as AnySourceNode)?.SetParentInfo(File, this);
                _hasValue = true;
                return _value;
            }
        }
    }

    public override SourceNodeType Type => SourceNodeType.Property;

    public static PropertyNode Create(string key, bool keyIsQuoted, LazySource valueSource, OneOrMore<Comment> comments, in AnySourceNodeProperties properties)
    {
        return comments.Length switch
        {
            0 => new PropertyNode(key, keyIsQuoted, valueSource, in properties),
            1 => new SingleCommentedPropertyNode(key, keyIsQuoted, valueSource, comments.Value, in properties),
            _ => new MultipleCommentedPropertyNode(key, keyIsQuoted, valueSource, comments, in properties),
        };
    }

    private protected PropertyNode(string key, bool keyIsQuoted, LazySource valueSource, in AnySourceNodeProperties properties) : base(in properties)
    {
        Key = key;
        KeyIsQuoted = keyIsQuoted;
        ValueSource = valueSource;
        if (valueSource.CachedNode != null)
        {
            _value = (IAnyValueSourceNode)valueSource.CachedNode;
            _hasValue = true;
            ValueKind = _value.ValueType;
        }
        else
        {
            ValueKind = valueSource.Segment.IsEmpty ? ValueTypeDataRefType.Value : valueSource.ExpectedType;
        }
    }

    internal override void SetParentInfo(ISourceFile file, ISourceNode parent)
    {
        base.SetParentInfo(file, parent);
        if (_value != null)
        {
            lock (File.TreeSync)
            {
                if (_value is AnySourceNode n)
                {
                    n.SetParentInfo(file, this);
                }
            }
        }
    }

    protected static bool EqualsHelper(PropertyNode n1, PropertyNode n2)
    {
        return string.Equals(n1.Key, n2.Key, StringComparison.Ordinal) && NodesEqual(n1.Value, n2.Value);
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && EqualsHelper(this, (PropertyNode)other);
    }

    public override string ToString()
    {
        IAnyValueSourceNode? v;
        string key;
        lock (File.TreeSync)
        {
            v = Value;
            key = Key;
        }

        if (v == null)
            return key;

        return $"{key} {v}";
    }

    public override void Visit<TVisitor>(ref TVisitor visitor)
    {
        visitor.AcceptProperty(this);
    }
}

internal sealed class SingleCommentedPropertyNode : PropertyNode, ICommentSourceNode
{
    public Comment Comment { get; set; }

    public override SourceNodeType Type => SourceNodeType.PropertyWithComment;

    public SingleCommentedPropertyNode(string key, bool keyIsQuoted, LazySource valueSource, Comment comment, in AnySourceNodeProperties properties)
        : base(key, keyIsQuoted, valueSource, in properties)
    {
        Comment = comment;
    }

    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comment.Equals(((SingleCommentedPropertyNode)other).Comment);
    }

    OneOrMore<Comment> ICommentSourceNode.Comments => new OneOrMore<Comment>(Comment);
}

internal sealed class MultipleCommentedPropertyNode : PropertyNode, ICommentSourceNode
{
    public OneOrMore<Comment> Comments { get; set; }

    public override SourceNodeType Type => SourceNodeType.PropertyWithComment;

    public MultipleCommentedPropertyNode(string key, bool keyIsQuoted, LazySource valueSource, OneOrMore<Comment> comments, in AnySourceNodeProperties properties)
        : base(key, keyIsQuoted, valueSource, in properties)
    {
        Comments = comments;
    }
    
    public override bool Equals(ISourceNode other)
    {
        return base.Equals(other) && Comments.Equals(((MultipleCommentedPropertyNode)other).Comments);
    }
}