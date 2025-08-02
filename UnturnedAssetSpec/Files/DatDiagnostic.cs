namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public record struct DatDiagnostic
{
    public string ErrorId;
    public DatDiagnosticSeverity Severity;

    public DatDiagnostic(string errorId, DatDiagnosticSeverity severity)
    {
        ErrorId = errorId;
        Severity = severity;
    }
}

public enum DatDiagnosticSeverity
{
    Error = 1,
    Warning = 2,
    Information = 3,
    Hint = 4
}

public record struct DatDiagnosticMessage
{
    public DatDiagnostic Diagnostic;
    public FileRange Range;
    public string Message;
}