using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System.Collections.Immutable;

namespace UnturnedAssetSpecTests.Nodes;

public partial class SourceNodeTokenizerTests
{
    [Test]
    public void ReadPropertyStartingWithSpecialCharacter([Values("{", "[")] string c, [Values(SourceNodeTokenizerOptions.Lazy, SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.Lazy | SourceNodeTokenizerOptions.Metadata, SourceNodeTokenizerOptions.None)] SourceNodeTokenizerOptions options)
    {
        string test = $"Property {c}_value";

        using StaticSourceFile file = StaticSourceFile.FromOtherFile(string.Empty, test, null, options);
        ImmutableArray<IPropertySourceNode> properties = file.SourceFile.Properties;

        Assert.That(properties, Has.Length.EqualTo(1));

        IPropertySourceNode prop = properties[0];
        Assert.That(prop.Key, Is.EqualTo("Property"));
        Assert.That(prop.ValueKind, Is.EqualTo(SourceValueType.Value));
        Assert.That(((IValueSourceNode?)prop.Value)?.Value, Is.EqualTo($"{c}_value"));
        Assert.That(prop.KeyIsQuoted, Is.False);
    }
    
    [Test]
    public void ReadBasicNonQuotedString()
    {
        const string test = "Text";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);

        Assert.That(str, Is.EqualTo("Text"));
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 4)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));
        
        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadBasicOneLetterNonQuotedString()
    {
        const string test = "A";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);

        Assert.That(str, Is.EqualTo("A"));
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 1)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));
        
        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadEmptyNonQuotedString()
    {
        const string test = "";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 1)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.Empty);

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadNonQuotedStringAtEndLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"String{endl}AnotherKey Value", diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 6)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("String"));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd, Is.False);
        Assert.That(tok.Character, Is.EqualTo('A'));
    }

    [Test]
    public void ReadNonQuotedStringAtEndLineWithNoValueNext([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"String{endl}", diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 6)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("String"));

        Assert.That(str, Is.EqualTo("String"));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    [TestCase(@"String\t", "String\t")]
    [TestCase(@"\t", "\t")]
    [TestCase(@"\tString", "\tString")]
    [TestCase(@"\tString\t", "\tString\t")]
    [TestCase(@"\t\t", "\t\t")]
    [TestCase(@"\t\n", "\t\n")]
    [TestCase(@"\\n", "\\n")]
    [TestCase(@"\\\n", "\\\n")]
    [TestCase(@"\\\\", "\\\\")]
    public void ReadEscapeSequenceNonQuotedString(string toParse, string expected)
    {
        using SourceNodeTokenizer tok = new SourceNodeTokenizer(toParse, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, toParse.Length)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(toParse));

        Assert.That(str, Is.EqualTo(expected));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadNonQuotedStringIncludesComment()
    {
        using SourceNodeTokenizer tok = new SourceNodeTokenizer("String // Comment", diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 17)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("String // Comment"));

        Assert.That(str, Is.EqualTo("String // Comment"));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadInvalidEscapeSequenceNonQuotedString()
    {
        const string test = "Str\\ing";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 7)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("Str\\ing"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 4, 1, 5));

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadInvalidEscapeSequenceStartingQuote()
    {
        const string test = @"\""String";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 8)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo(@"\""String"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 1, 1, 2));
        Assert.That(DiagnosticSink.Diagnostics[0].Message, Is.EqualTo(DiagnosticResources.UNT1004_LeadingEscapedSlash));

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadTrailingEscapeNonQuotedString()
    {
        const string test = "String\\";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 7)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("String\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 7, 1, 7));

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadTrailingEscapeWithOtherEscapeNonQuotedString()
    {
        const string test = "St\\tring\\";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(test, diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 9)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo(test));

        Assert.That(str, Is.EqualTo("St\tring\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 9, 1, 9));

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }

    [Test]
    public void ReadTrailingEscapeNonQuotedStringAtEndLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";
        
        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"String\\{endl}AnotherKey Value", diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 7)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("String\\"));

        Assert.That(str, Is.EqualTo("String\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 7, 1, 7));

        Assert.That(tok.IsAtEnd, Is.False);
        Assert.That(tok.Character, Is.EqualTo('A'));
    }

    [Test]
    public void ReadTrailingEscapeNonQuotedStringAtEndLineWithNoValue([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";
        
        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"String\\{endl}", diagnosticSink: DiagnosticSink);

        string str = tok.ReadNonQuotedString(out FileRange range, out ReadOnlySpan<char> rangeSpan);
        Assert.That(range, Is.EqualTo(new FileRange(1, 1, 1, 7)));
        Assert.That(rangeSpan.ToString(), Is.EqualTo("String\\"));

        Assert.That(str, Is.EqualTo("String\\"));

        DiagnosticSink.AssertSingleDiagnostic(DatDiagnostics.UNT1004, new FileRange(1, 7, 1, 7));

        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Character, Is.EqualTo('\0'));
    }
}