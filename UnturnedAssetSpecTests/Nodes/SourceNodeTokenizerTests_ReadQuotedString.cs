using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace UnturnedAssetSpecTests.Nodes;

public partial class SourceNodeTokenizerTests
{
    [Test]
    public void ReadBasicQuotedString()
    {
        const string test = "\"Text\"";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);

        Assert.That(str, Is.EqualTo("Text"));
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 6)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));
        
        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadEmptyQuotedString()
    {
        const string test = "\"\"";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 2)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.Empty);

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadUnterminatedQuotedString()
    {
        const string test = "\"String";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 7)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1002, new FileRange(1, 1, 1, 7));

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadUnterminatedQuotedStringAtEndLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"\"String{endl}AnotherKey Value", diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 7)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("\"String"));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1002, new FileRange(1, 1, 1, 7));

        Assert.That(tok.IsAtEnd, Is.False);
    }

    [Test]
    public void ReadEmptyUnterminatedQuotedString()
    {
        const string test = "\"";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 1)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.Empty);

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1002, new FileRange(1, 1, 1, 1));

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    [TestCase(@"""String\t""", "String\t")]
    [TestCase(@"""\t""", "\t")]
    [TestCase(@"""\tString""", "\tString")]
    [TestCase(@"""\tString\t""", "\tString\t")]
    [TestCase(@"""\t\t""", "\t\t")]
    [TestCase(@"""\t\n""", "\t\n")]
    [TestCase(@"""\""""", "\"")]
    [TestCase(@"""\""\\\""""", "\"\\\"")]
    public void ReadEscapeSequenceString(string toParse, string expected)
    {
        using SourceNodeTokenizer tok = new SourceNodeTokenizer(toParse, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, toParse.Length)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(toParse));

        Assert.That(str, Is.EqualTo(expected));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadWithExtraData()
    {
        using SourceNodeTokenizer tok = new SourceNodeTokenizer("\"String\" // Comment", diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 8)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("\"String\""));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd, Is.False);
    }

    [Test]
    public void ReadInvalidEscapeSequenceString()
    {
        const string test = "\"Str\\ing\"";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 9)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("Str\\ing"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 5, 1, 6));

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadWithCommaWithNewLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        string test = $"\"String\",{endl}OtherProperty Value";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 8)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("\"String\""));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT105, new FileRange(1, 9, 1, 9));

        Assert.That(tok.IsAtEnd, Is.False);
        Assert.That(tok.Character, Is.EqualTo('O'));
    }

    [Test]
    public void ReadWithComma()
    {
        const string test = "\"String\",";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 8)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("\"String\""));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT105, new FileRange(1, 9, 1, 9));

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadTrailingEscapeString()
    {
        const string test = "\"String\\";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 8)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("String\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 8, 1, 8));

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadTrailingEscapeWithOtherEscapeString()
    {
        const string test = "\"St\\tring\\";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 10)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("St\tring\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 10, 1, 10));

        Assert.That(tok.IsAtEnd);
    }

    [Test]
    public void ReadTrailingEscapeStringAtEndLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";
        
        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"\"String\\{endl}AnotherKey Value", diagnosticSink: DiagnosticSink);

        string str = tok.ReadQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 8)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("\"String\\"));

        Assert.That(str, Is.EqualTo("String\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 8, 1, 8));

        Assert.That(tok.IsAtEnd, Is.False);
    }

}