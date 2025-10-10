namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public interface ISourceNodeVisitor
    : ISourceNodePropertyVisitor,
      ISourceNodeWhiteSpaceVisitor,
      ISourceNodeCommentOnlyVisitor,
      ISourceNodeValueVisitor,
      ISourceNodeDictionaryVisitor,
      ISourceNodeListVisitor;

public interface ISourceNodePropertyVisitor
{
    void AcceptProperty(IPropertySourceNode node);
}
public interface ISourceNodeWhiteSpaceVisitor
{
    void AcceptWhiteSpace(IWhiteSpaceSourceNode node);
}
public interface ISourceNodeCommentOnlyVisitor
{
    void AcceptCommentOnly(ICommentSourceNode node);
}
public interface ISourceNodeValueVisitor
{
    void AcceptValue(IValueSourceNode node);
}
public interface ISourceNodeDictionaryVisitor
{
    void AcceptDictionary(IDictionarySourceNode node);
}
public interface ISourceNodeListVisitor
{
    void AcceptList(IListSourceNode node);
}