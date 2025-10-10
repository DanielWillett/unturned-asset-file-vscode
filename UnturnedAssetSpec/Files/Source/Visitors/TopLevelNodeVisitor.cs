using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Accepts every node that is a direct child of the visited node.
/// </summary>
/// <remarks>Ex: All properties in a dictionary or all values in a list.</remarks>
public abstract class TopLevelNodeVisitor : NodeVisitor, ISourceNodeVisitor
{
    void ISourceNodeCommentOnlyVisitor.AcceptCommentOnly(ICommentSourceNode node) { }

    void ISourceNodeWhiteSpaceVisitor.AcceptWhiteSpace(IWhiteSpaceSourceNode node) { }

    void ISourceNodeDictionaryVisitor.AcceptDictionary(IDictionarySourceNode node)
    {
        lock (node.File.TreeSync)
        {
            ImmutableArray<ISourceNode> nodes = node.Children;
            for (int i = 0; i < nodes.Length; ++i)
            {
                AcceptAnyIntl(nodes[i]);
            }
        }
    }

    void ISourceNodeListVisitor.AcceptList(IListSourceNode node)
    {
        lock (node.File.TreeSync)
        {
            ImmutableArray<ISourceNode> nodes = node.Children;
            for (int i = 0; i < nodes.Length; ++i)
            {
                AcceptAnyIntl(nodes[i]);
            }
        }
    }

    void ISourceNodeValueVisitor.AcceptValue(IValueSourceNode node) { }

    void ISourceNodePropertyVisitor.AcceptProperty(IPropertySourceNode node)
    {
        lock (node.File.TreeSync)
        {
            IAnyValueSourceNode? value = node.Value;
            AcceptAnyValueIntl(value);
        }
    }

    private void AcceptAnyIntl(ISourceNode? node)
    {
        switch (node)
        {
            case IAnyValueSourceNode anyValue:
                AcceptAnyValueIntl(anyValue);
                break;

            case IPropertySourceNode property:
                AcceptProperty(property);
                break;

            case IWhiteSpaceSourceNode whiteSpace:
                if (!IgnoreMetadata)
                    AcceptWhiteSpace(whiteSpace);
                break;

            case ICommentSourceNode comment:
                if (!IgnoreMetadata)
                    AcceptCommentOnly(comment);
                break;
        }
    }

    private void AcceptAnyValueIntl(IAnyValueSourceNode? anyValue)
    {
        switch (anyValue)
        {
            case IValueSourceNode val:
                AcceptValue(val);
                break;

            case IListSourceNode list:
                AcceptList(list);
                break;

            case IDictionarySourceNode dict:
                AcceptDictionary(dict);
                break;
        }
    }
}