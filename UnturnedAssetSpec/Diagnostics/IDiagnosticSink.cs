namespace DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;

public interface IDiagnosticSink
{
    void AcceptDiagnostic(DatDiagnosticMessage diagnostic);
}
