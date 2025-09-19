using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Accepts every node in a tree in order, not including the visited node.
/// </summary>
public abstract class OrderedNodeVisitor : NodeVisitor, ISourceNodeVisitor
{
    private bool _tokenCanCancel;

    protected virtual void AcceptEndList(IListSourceNode list) { }
    protected virtual void AcceptEndDictionary(IDictionarySourceNode dictionary) { }
    protected virtual void AcceptEndProperty(IPropertySourceNode property) { }

    void ISourceNodeVisitor.AcceptCommentOnly(ICommentSourceNode node)
    {
        TryDispose();
    }

    void ISourceNodeVisitor.AcceptWhiteSpace(IWhiteSpaceSourceNode node)
    {
        TryDispose();
    }

    void ISourceNodeVisitor.AcceptDictionary(IDictionarySourceNode node)
    {
        lock (node.File.TreeSync)
        {
            StepThroughContainerIntl(node);
        }
    }

    void ISourceNodeVisitor.AcceptList(IListSourceNode node)
    {
        lock (node.File.TreeSync)
        {
            StepThroughContainerIntl(node);
        }
    }

    void ISourceNodeVisitor.AcceptValue(IValueSourceNode node)
    {
        TryDispose();
    }

    void ISourceNodeVisitor.AcceptProperty(IPropertySourceNode node)
    {
        lock (node.File.TreeSync)
        {
            switch (node.Value)
            {
                case IAnyChildrenSourceNode listOrDict:
                    StepThroughContainerIntl(listOrDict);
                    break;

                case IValueSourceNode value:
                    try
                    {
                        Token.ThrowIfCancellationRequested();
                        Comment = value as ICommentSourceNode;
                        AcceptValue(value);
                    }
                    finally
                    {
                        TryDispose();
                    }
                    break;
            }
        }
    }

    private void StepThroughContainerIntl(IAnyChildrenSourceNode listOrDict)
    {
        try
        {
            _tokenCanCancel = Token.CanBeCanceled;
            if (_tokenCanCancel)
                Token.ThrowIfCancellationRequested();

            ImmutableArray<ISourceNode> children = listOrDict.Children;
            if (children.IsDefaultOrEmpty)
                return;

            for (ISourceNode? node = children[0]; node != null ; node = GetNextNode(node, listOrDict))
            {
                if (_tokenCanCancel)
                    Token.ThrowIfCancellationRequested();

                SourceNodeType type = node.Type;
                Comment = node as ICommentSourceNode;
                switch (type)
                {
                    case SourceNodeType.Whitespace:
                        if (!IgnoreMetadata)
                            AcceptWhiteSpace((IWhiteSpaceSourceNode)node);
                        break;

                    case SourceNodeType.Comment:
                        if (!IgnoreMetadata)
                        {
                            Comment = null;
                            AcceptCommentOnly((ICommentSourceNode)node);
                        }
                        break;

                    case SourceNodeType.Property:
                    case SourceNodeType.PropertyWithComment:
                        AcceptProperty((IPropertySourceNode)node);
                        break;

                    case SourceNodeType.Value:
                    case SourceNodeType.ValueWithComment:
                        AcceptValue((IValueSourceNode)node);
                        break;

                    case SourceNodeType.List:
                    case SourceNodeType.ListWithComment:
                        AcceptList((IListSourceNode)node);
                        break;

                    case SourceNodeType.Dictionary:
                    case SourceNodeType.DictionaryWithComment:
                        AcceptDictionary((IDictionarySourceNode)node);
                        break;
                }
            }
        }
        finally
        {
            TryDispose();
        }
    }

    private struct StepThroughOperation
    {
        public ISourceNode Node;
        public bool StepUp;
        public ISourceNode? NextNode;
    }

    private ISourceNode? GetNextNode(ISourceNode node, ISourceNode parent)
    {
        if (ReferenceEquals(node.Parent, node))
            return null;

        StepThroughOperation op = default;
        op.Node = node;
        op.StepUp = false;
        op.NextNode = null;

        do
        {
            bool isRoot = ReferenceEquals(op.Node, parent);
            switch (op.Node)
            {
                case IPropertySourceNode prop:
                    if (prop.Value == null)
                    {
                        Comment = prop as ICommentSourceNode;
                        AcceptEndProperty(prop);
                        StepUpDictionaryValue(prop, ref op);
                    }
                    else if (op.StepUp)
                    {
                        StepUpDictionaryValue(prop, ref op);
                    }
                    else
                        op.NextNode = prop.Value;

                    break;

                case IValueSourceNode:
                    switch (op.Node.Parent)
                    {
                        case IListSourceNode list:
                            StepUpListValue(list, op.Node, ref op);
                            break;

                        case IPropertySourceNode property:
                            Comment = property as ICommentSourceNode;
                            AcceptEndProperty(property);
                            StepUpDictionaryValue(property, ref op);
                            break;
                    }

                    break;

                case IAnyChildrenSourceNode listOrDict:
                    if (op.StepUp)
                    {
                        if (!ReferenceEquals(parent, listOrDict))
                            StepUpChildContainer(listOrDict, ref op);
                        break;
                    }

                    ImmutableArray<ISourceNode> children = listOrDict.Children;
                    if (children.IsDefaultOrEmpty)
                    {
                        if (!ReferenceEquals(parent, listOrDict))
                            StepUpChildContainer(listOrDict, ref op);
                        break;
                    }

                    op.NextNode = children[0];
                    break;

                case IWhiteSpaceSourceNode:
                case ICommentSourceNode:
                    if (op.Node.Parent is IListSourceNode l)
                        StepUpListValue(l, op.Node, ref op);
                    else
                        StepUpDictionaryValue(op.Node, ref op);
                    break;

                default:
                    op.NextNode = null;
                    break;
            }

            if (isRoot)
                return null;

        } while (op.StepUp);

        return ReferenceEquals(op.NextNode, parent) ? null : op.NextNode;

        // if the previous 
        void StepUpChildContainer(IAnyChildrenSourceNode childContainer, ref StepThroughOperation op)
        {
            op.StepUp = false;
            op.NextNode = null;
            switch (childContainer)
            {
                case IDictionarySourceNode dict:
                    Comment = dict as ICommentSourceNode;
                    AcceptEndDictionary(dict);
                    break;

                case IListSourceNode list:
                    Comment = list as ICommentSourceNode;
                    AcceptEndList(list);
                    break;
            }

            switch (childContainer.Parent)
            {
                case IPropertySourceNode property:
                    Comment = property as ICommentSourceNode;
                    AcceptEndProperty(property);
                    StepUpDictionaryValue(property, ref op);
                    break;

                case IListSourceNode list:
                    Comment = list as ICommentSourceNode;
                    StepUpListValue(list, childContainer, ref op);
                    break;
            }
        }

        static void StepUpListValue(IListSourceNode list, ISourceNode value, ref StepThroughOperation op)
        {
            op.StepUp = false;
            op.NextNode = null;
            int index = value.ChildIndex;
            if (index >= list.Children.Length - 1)
            {
                op.Node = list;
                op.StepUp = true;
                return;
            }

            op.NextNode = list.Children[index + 1];
        }

        static void StepUpDictionaryValue(ISourceNode dictEntry, ref StepThroughOperation op)
        {
            op.StepUp = false;
            op.NextNode = null;
            if (dictEntry.Parent is not IDictionarySourceNode dict)
            {
                op.NextNode = null;
                return;
            }

            int index = dictEntry.ChildIndex;
            if (index >= dict.Children.Length - 1)
            {
                op.Node = dict;
                op.StepUp = true;
                return;
            }

            op.NextNode = dict.Children[index + 1];
        }
    }
}