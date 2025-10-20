using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// A node in a dat file.
/// </summary>
public interface ISourceNode : IEquatable<ISourceNode>
{
    /// <summary>
    /// The file this node is contained in, which also serves as the root node.
    /// </summary>
    ISourceFile File { get; }

    /// <summary>
    /// The parent of this node. Will be equal to itself for the root node of a document.
    /// </summary>
    ISourceNode Parent { get; }

    /// <summary>
    /// The range within the file that this node occupies.
    /// </summary>
    FileRange Range { get; }

    /// <summary>
    /// The depth from the root node.
    /// </summary>
    /// <remarks>
    /// The root node has a depth of 0.
    /// A property and it's value in the root node both have a depth of 1.
    /// A value in a list in the root node or a property and it's value in a dictionary in the root node has a depth of 2.
    /// </remarks>
    int Depth { get; }

    /// <summary>
    /// The type of <see cref="SourceNodeType"/> this node represents.
    /// </summary>
    SourceNodeType Type { get; }

    /// <summary>
    /// The index of this node in it's direct parent (list or dictionary), not including whitespace or comments.
    /// </summary>
    /// <remarks>If this node is not in a list or dictionary, it's index will return -1.</remarks>
    int Index { get; }

    /// <summary>
    /// The index of this node in it's direct parent (list or dictionary), including whitespace or comments.
    /// </summary>
    /// <remarks>If this node is not in a list or dictionary, it's index will return -1.</remarks>
    int ChildIndex { get; }

    /// <summary>
    /// Visit just this node.
    /// </summary>
    void Visit<TVisitor>(ref TVisitor visitor)
        where TVisitor : ISourceNodeVisitor;

    /// <summary>
    /// The index of the fist character in the original file.
    /// </summary>
    int FirstCharacterIndex { get; }

    /// <summary>
    /// The index of the last character in the original file.
    /// </summary>
    int LastCharacterIndex { get; }
}

/// <summary>
/// A node in a dat file that indicates one or more blank lines.
/// </summary>
public interface IWhiteSpaceSourceNode : ISourceNode
{
    /// <summary>
    /// Number of lines of whitespace to take up. Shouldn't ever be zero or lower.
    /// </summary>
    int Lines { get; }
}

/// <summary>
/// A node in a dat file that indicates a comment line.
/// </summary>
/// <remarks>Just because a node implements <see cref="ICommentSourceNode"/>, doesn't mean it couldn't be some other node as well, such as:
/// <code>Key "Value" // comment</code>
/// </remarks>
public interface ICommentSourceNode : ISourceNode
{
    /// <summary>
    /// At least one comment present in this node.
    /// </summary>
    /// <remarks>Should never be empty/null.</remarks>
    OneOrMore<Comment> Comments { get; }
}

/// <summary>
/// Any node (lists, dictionaries, and basic values) that can be contained in a property or list.
/// </summary>
public interface IAnyValueSourceNode : ISourceNode
{
    /// <summary>
    /// The type of value this value contains.
    /// </summary>
    ValueTypeDataRefType ValueType { get; }
}

/// <summary>
/// A node with a key and optional value.
/// </summary>
public interface IPropertySourceNode : ISourceNode
{
    /// <summary>
    /// The key of the property.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// If the key is wrapped in quotes.
    /// </summary>
    bool KeyIsQuoted { get; }

    /// <summary>
    /// The value of this property.
    /// </summary>
    IAnyValueSourceNode? Value { get; }

    /// <summary>
    /// If this property has a value (otherwise it's a flag property).
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// The type of value stored in <see cref="Value"/>.
    /// </summary>
    /// <remarks>If this node is a flag, this property will be equal to <see cref="ValueTypeDataRefType.Value"/>.</remarks>
    ValueTypeDataRefType ValueKind { get; }

    /// <summary>
    /// Gets the value as a string without necessarily instantiating the value node.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called on a dictionary or list node.</exception>
    /// <returns>Null if there is no value, otherwise the value without quotes.</returns>
    string? GetValueString(out bool isQuoted);

    /// <summary>
    /// Gets the range of the value without necessarily instantiating the value node.
    /// </summary>
    /// <returns>Zero if there is no value, otherwise the range of the value.</returns>
    FileRange GetValueRange();
}

/// <summary>
/// A node that represents a basic value.
/// </summary>
/// <remarks>Comments will only belong to this node when contained in a list.</remarks>
public interface IValueSourceNode : IAnyValueSourceNode
{
    /// <summary>
    /// If the value is wrapped in quotes.
    /// </summary>
    bool IsQuoted { get; }

    /// <summary>
    /// The unescaped value of this node, not including quotes or comments.
    /// </summary>
    string Value { get; }
}

/// <summary>
/// A node that can have multiple children, like a list or dictionary.
/// </summary>
public interface IAnyChildrenSourceNode : IAnyValueSourceNode
{
    /// <summary>
    /// The number of properties or values present in the list. This does not include whitespace or comments.
    /// </summary>
    /// <remarks>This is not the same as the length of <see cref="Children"/>.</remarks>
    int Count { get; }

    /// <summary>
    /// Set of all nodes contained in this list in their original order, including whitespace and comments.
    /// </summary>
    ImmutableArray<ISourceNode> Children { get; }
}

internal sealed class SourceNodeIndexComparer : IComparer<ISourceNode>
{
    public static readonly SourceNodeIndexComparer Instance = new SourceNodeIndexComparer();

    static SourceNodeIndexComparer() { }
    private SourceNodeIndexComparer() { }

    public int Compare(ISourceNode x, ISourceNode y)
    {
        if (x == null)
            return y == null ? 0 : -1;

        return y == null ? 1 : x.Index.CompareTo(y.Index);
    }
}

/// <summary>
/// A node that represents a list of values.
/// </summary>
public interface IListSourceNode : IAnyChildrenSourceNode
{
    /// <summary>
    /// Try to get an element by index.
    /// </summary>
    bool TryGetElement(int index, [MaybeNullWhen(false)] out IAnyValueSourceNode node);
}

/// <summary>
/// A node that represents a list of properties.
/// </summary>
public interface IDictionarySourceNode : IAnyChildrenSourceNode
{
    /// <summary>
    /// Try to get a property by name.
    /// </summary>
    bool TryGetProperty(string propertyName, [MaybeNullWhen(false)] out IPropertySourceNode node);
}

/// <summary>
/// The type of node represented by a <see cref="ISourceNode"/>.
/// </summary>
public enum SourceNodeType
{
    /// <summary>
    /// One or more blank lines.
    /// </summary>
    Whitespace,

    /// <summary>
    /// A comment taking up it's own line.
    /// <code>
    /// // Comment
    /// </code>
    /// </summary>
    Comment,

    /// <summary>
    /// A Key-Value pair, containing either no value, a basic value, a list, or a dictionary.
    /// <code>
    /// Key
    /// 
    /// Key Value
    /// 
    /// Key "Value"
    /// 
    /// Key
    /// {
    /// }
    /// 
    /// Key
    /// [
    /// ]
    /// </code>
    /// </summary>
    Property,

    /// <summary>
    /// A basic string value in either a property or a list.
    /// <code>
    /// Value
    /// 
    /// "Value"
    /// </code>
    /// </summary>
    Value,

    /// <summary>
    /// A list of values in either a property or a list.
    /// <code>
    /// [
    ///     Value1
    ///     
    ///     "Value2" // Comment
    ///     
    ///     {
    ///         // Dictionary
    ///     }
    ///     
    ///     // Comment
    ///     
    ///     [
    ///         // List
    ///     ]
    /// ]
    /// </code>
    /// </summary>
    List,

    /// <summary>
    /// A dictionary of properties in either a property or a list. This can also be the root node.
    /// <code>
    /// {
    ///     Key Value
    ///     
    ///     Key "Value" // Comment
    ///     
    ///     Key
    ///     {
    ///         // Dictionary
    ///     }
    ///     
    ///     // Comment
    ///     
    ///     Key
    ///     [
    ///         // List
    ///     ]
    /// }
    /// </code>
    /// </summary>
    Dictionary,

    /// <summary>
    /// A Key-Value pair, containing either no value, a basic value, a list, or a dictionary, also containing a trailing comment.
    /// <code>
    /// Key "Value" // Comment
    /// 
    /// Key
    /// { // Comment
    /// }
    /// 
    /// Key
    /// [
    /// ] // Comment
    /// </code>
    /// </summary>
    PropertyWithComment,

    /// <summary>
    /// A basic string value in either a property or a list, also containing a trailing comment.
    /// <code>
    /// "Value" // Comment
    /// </code>
    /// </summary>
    ValueWithComment,

    /// <summary>
    /// A list of values in either a property or a list, also containing a trailing comment.
    /// <code>
    /// [ // Comment
    ///     Value1
    ///     
    ///     "Value2" // Not a Comment
    ///     
    ///     {
    ///         // Dictionary
    ///     }
    ///     
    ///     // Comment
    ///     
    ///     [
    ///         // List
    ///     ]
    /// ] // Comment
    /// </code>
    /// </summary>
    ListWithComment,

    /// <summary>
    /// A dictionary of properties in either a property or a list, also containing a trailing comment.
    /// <code>
    /// { // Comment
    ///     Key Value
    ///     
    ///     Key "Value" // Not a Comment
    ///     
    ///     Key
    ///     {
    ///         // Dictionary
    ///     }
    ///     
    ///     // Comment
    ///     
    ///     Key
    ///     [
    ///         // List
    ///     ]
    /// } // Comment
    /// </code>
    /// </summary>
    DictionaryWithComment
}