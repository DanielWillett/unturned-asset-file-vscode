using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace UnturnedAssetSpecTests.Nodes;

public partial class SourceNodeTokenizerTests
{
    protected SourceNodeTokenizerTestDiagnosticSink DiagnosticSink { get; private set; }

    [SetUp]
    public void Setup()
    {
        DiagnosticSink = new SourceNodeTokenizerTestDiagnosticSink();
    }

}

public class SourceNodeTokenizerTestDiagnosticSink : IDiagnosticSink
{
    public IList<DatDiagnosticMessage> Diagnostics { get; } = new List<DatDiagnosticMessage>();

    /// <inheritdoc />
    public void AcceptDiagnostic(DatDiagnosticMessage diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }

    public void AssertSingleDiagnostic(DatDiagnostic diag, FileRange range)
    {
        DatDiagnosticMessage msg = Diagnostics.Single(x => x.Diagnostic.ErrorId == diag.ErrorId);
        Assert.That(msg.Range, Is.EqualTo(range));

        Assert.That(Diagnostics, Has.Count.EqualTo(1));
    }

    public void AssertNoDiagnostics()
    {
        Assert.That(Diagnostics, Is.Empty);
    }
}