namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public interface IDiagnosticSink
{
    void AcceptDiagnostic(DatDiagnosticMessage diagnostic);
}
