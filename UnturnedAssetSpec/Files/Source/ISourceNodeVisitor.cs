namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public interface ISourceNodeVisitor
{
    void AcceptWhiteSpace(IWhiteSpaceSourceNode node);

    void AcceptCommentOnly(ICommentSourceNode node);

    void AcceptProperty(IPropertySourceNode node);
    
    void AcceptValue(IValueSourceNode node);

    void AcceptDictionary(IDictionarySourceNode node);

    void AcceptList(IListSourceNode node);
}