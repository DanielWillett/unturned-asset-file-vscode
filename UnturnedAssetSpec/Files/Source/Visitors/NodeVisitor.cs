using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// A base class for node visitors.
/// </summary>
public abstract class NodeVisitor
{
    /// <summary>
    /// Whether or not to skip metadata nodes (comments, whitespace).
    /// </summary>
    protected virtual bool IgnoreMetadata => false;

    /// <summary>
    /// The current node's comment information.
    /// </summary>
    public ICommentSourceNode? Comment { get; protected set; }

    /// <summary>
    /// A cancellation token cancelling this visitor when triggered.
    /// </summary>
    public CancellationToken Token { get; set; }

    /// <summary>
    /// Invoked for all nodes that aren't specifically overridden.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    protected virtual void AcceptNode(ISourceNode node)
    {

    }

    /// <summary>
    /// Invoked for all comment-only nodes.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptNode"/> by default.</remarks>
    protected virtual void AcceptCommentOnly(ICommentSourceNode node)
    {
        AcceptNode(node);
    }

    /// <summary>
    /// Invoked for all string value, dictionary, or list nodes that aren't specifically overridden.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptNode"/> by default.</remarks>
    protected virtual void AcceptAnyValue(IAnyValueSourceNode node)
    {
        AcceptNode(node);
    }

    /// <summary>
    /// Invoked for all white-space nodes.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptNode"/> by default.</remarks>
    protected virtual void AcceptWhiteSpace(IWhiteSpaceSourceNode node)
    {
        AcceptNode(node);
    }

    /// <summary>
    /// Invoked for all property nodes.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptNode"/> by default.</remarks>
    protected virtual void AcceptProperty(IPropertySourceNode node)
    {
        AcceptNode(node);
    }

    /// <summary>
    /// Invoked for all dictionary nodes.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptAnyValue"/> by default.</remarks>
    protected virtual void AcceptDictionary(IDictionarySourceNode node)
    {
        AcceptAnyValue(node);
    }

    /// <summary>
    /// Invoked for all list nodes.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptAnyValue"/> by default.</remarks>
    protected virtual void AcceptList(IListSourceNode node)
    {
        AcceptAnyValue(node);
    }

    /// <summary>
    /// Invoked for all string value nodes.
    /// <para><see cref="Comment"/> will also be non-null if the visited node is a comment (except for comment-only nodes).</para>
    /// </summary>
    /// <remarks>Invokes <see cref="AcceptAnyValue"/> by default.</remarks>
    protected virtual void AcceptValue(IValueSourceNode node)
    {
        AcceptAnyValue(node);
    }
}

/// <summary>
/// Accepts only the visited node.
/// </summary>
public abstract class IdentityNodeVisitor : NodeVisitor, ISourceNodeVisitor
{
    void ISourceNodeCommentOnlyVisitor.AcceptCommentOnly(ICommentSourceNode node)
    {
        Token.ThrowIfCancellationRequested();
        if (IgnoreMetadata)
            return;

        Comment = null;
        AcceptCommentOnly(node);
    }

    void ISourceNodeWhiteSpaceVisitor.AcceptWhiteSpace(IWhiteSpaceSourceNode node)
    {
        Token.ThrowIfCancellationRequested();
        if (IgnoreMetadata)
            return;

        Comment = node as ICommentSourceNode;
        AcceptWhiteSpace(node);
    }

    void ISourceNodeDictionaryVisitor.AcceptDictionary(IDictionarySourceNode node)
    {
        Token.ThrowIfCancellationRequested();
        Comment = node as ICommentSourceNode;
        AcceptDictionary(node);
    }

    void ISourceNodeListVisitor.AcceptList(IListSourceNode node)
    {
        Token.ThrowIfCancellationRequested();
        Comment = node as ICommentSourceNode;
        AcceptList(node);
    }

    void ISourceNodeValueVisitor.AcceptValue(IValueSourceNode node)
    {
        Token.ThrowIfCancellationRequested();
        Comment = node as ICommentSourceNode;
        AcceptValue(node);
    }

    void ISourceNodePropertyVisitor.AcceptProperty(IPropertySourceNode node)
    {
        Token.ThrowIfCancellationRequested();
        Comment = node as ICommentSourceNode;
        AcceptProperty(node);
    }
}