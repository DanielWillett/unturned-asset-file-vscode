namespace LspServer.Files;

public struct DatError
{
    public string ErrorId;

    public string Context;
    public int LineNumber;
    public int ColumnNumber;
    public string Message;

    public bool IsWarning;
}