using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace UnturnedAssetSpecTests.Nodes;

internal class OrderedNodeVisitorTests
{
    [Test]
    public void CorrectOrder()
    {

    }
}

public enum OrderedNodeVisitorToken
{
    Whitespace,
    Comment,
    Property,
    Value,
    List,
    Dictionary,
    EndProperty,
    EndList,
    EndDictionary
}

public class TestOrderedNodeVisitor : OrderedNodeVisitor
{
    public Queue<OrderedNodeVisitorToken> Tokens = new Queue<OrderedNodeVisitorToken>();

    protected override void AcceptDictionary(IDictionarySourceNode node)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.Dictionary);
    }
    protected override void AcceptList(IListSourceNode node)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.List);
    }
    protected override void AcceptProperty(IPropertySourceNode node)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.Property);
    }
    protected override void AcceptValue(IValueSourceNode node)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.Value);
    }
    protected override void AcceptCommentOnly(ICommentSourceNode node)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.Comment);
    }
    protected override void AcceptWhiteSpace(IWhiteSpaceSourceNode node)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.Whitespace);
    }
    protected override void AcceptEndProperty(IPropertySourceNode property)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.EndProperty);
    }
    protected override void AcceptEndList(IListSourceNode list)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.EndList);
    }
    protected override void AcceptEndDictionary(IDictionarySourceNode dictionary)
    {
        Tokens.Enqueue(OrderedNodeVisitorToken.EndDictionary);
    }
}