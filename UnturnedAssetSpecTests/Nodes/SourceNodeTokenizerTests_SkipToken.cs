using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace UnturnedAssetSpecTests.Nodes;

public partial class SourceNodeTokenizerTests
{
    [Test]
    public void SkipQuotedValue([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"\"Key\" \"Value\"{endl}OtherKey", diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('"'));
        Assert.That(tok.Index, Is.EqualTo(6));

        tok.SkipToken(out FileRange range);
        
        Assert.That(range, Is.EqualTo(new FileRange(1, 7, 1, 13)));
        Assert.That(tok.Character, Is.EqualTo('O'));
        Assert.That(tok.Index, Is.EqualTo(13 + endl.Length));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.ReadNonQuotedString(out _, out _), Is.EqualTo("OtherKey"));
    }

    [Test]
    public void SkipNonQuotedValue([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"\"Key\" Value{endl}OtherKey", diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('V'));
        Assert.That(tok.Index, Is.EqualTo(6));

        tok.SkipToken(out FileRange range);
        
        Assert.That(range, Is.EqualTo(new FileRange(1, 7, 1, 11)));
        Assert.That(tok.Character, Is.EqualTo('O'));
        Assert.That(tok.Index, Is.EqualTo(11 + endl.Length));

        DiagnosticSink.AssertNoDiagnostics();

        Assert.That(tok.ReadNonQuotedString(out _, out _), Is.EqualTo("OtherKey"));
    }

    [Test]
    public void SkipQuotedValueToEnd()
    {
        using SourceNodeTokenizer tok = new SourceNodeTokenizer("\"Key\" \"Value\"", diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('"'));
        Assert.That(tok.Index, Is.EqualTo(6));

        tok.SkipToken(out FileRange range);
        
        Assert.That(range, Is.EqualTo(new FileRange(1, 7, 1, 13)));
        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Index, Is.EqualTo(13));

        DiagnosticSink.AssertNoDiagnostics();
    }

    [Test]
    public void SkipNonQuotedValueToEnd()
    {
        using SourceNodeTokenizer tok = new SourceNodeTokenizer("\"Key\" Value", diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('V'));
        Assert.That(tok.Index, Is.EqualTo(6));

        tok.SkipToken(out FileRange range);
        
        Assert.That(range, Is.EqualTo(new FileRange(1, 7, 1, 11)));
        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Index, Is.EqualTo(11));

        DiagnosticSink.AssertNoDiagnostics();
    }

    [Test]
    public void SkipQuotedValueToEndWithNewLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"\"Key\" \"Value\"{endl}", diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('"'));
        Assert.That(tok.Index, Is.EqualTo(6));

        tok.SkipToken(out FileRange range);
        
        Assert.That(range, Is.EqualTo(new FileRange(1, 7, 1, 13)));
        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Index, Is.EqualTo(13 + endl.Length));

        DiagnosticSink.AssertNoDiagnostics();
    }

    [Test]
    public void SkipNonQuotedValueToEndWithNewLine([Values(true, false)] bool unix)
    {
        string endl = unix ? "\n" : "\r\n";

        using SourceNodeTokenizer tok = new SourceNodeTokenizer($"\"Key\" Value{endl}", diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('V'));
        Assert.That(tok.Index, Is.EqualTo(6));

        tok.SkipToken(out FileRange range);
        
        Assert.That(range, Is.EqualTo(new FileRange(1, 7, 1, 11)));
        Assert.That(tok.IsAtEnd);
        Assert.That(tok.Index, Is.EqualTo(11 + endl.Length));

        DiagnosticSink.AssertNoDiagnostics();
    }

    private static void FixLineEnds(bool unix, ref string text, out int endlLen)
    {
        endlLen = 1 + (!unix ? 1 : 0);
        bool textIsUnix = !text.Contains("\r\n");
        if (unix)
        {
            if (!textIsUnix)
                text = text.Replace("\r\n", "\n");
        }
        else if (textIsUnix)
        {
            text = text.Replace("\n", "\r\n");
        }
    }

    [Test]
    public void SkipDictionary([Values(true, false)] bool unix, [Range(0, 2)] int hasEndNewLineAndProperty)
    {
        string text = """
                      "Key"
                      {
                          Key1 Value1
                          Key2 Value2
                          Key3
                          [
                             {
                                  Key12
                                  Key34 Value
                                  D2
                                  {
                                      L2
                                      [
                                      
                                      ]
                                  }
                                  L2
                                  [
                                      V1
                                  ]
                              }
                              {

                              }
                              {
                              }
                              [
                                  V
                              ]
                              [
                              ]
                              [
                                  
                              ]
                          ]
                          "Key4" Value
                      }
                      """;

        switch (hasEndNewLineAndProperty)
        {
            case 1:
                text += """

                        OtherKey
                        """;
                break;

            case 2:
                text += """

                        
                        """;
                break;
        }

        FixLineEnds(unix, ref text, out int endlLen);

        using SourceNodeTokenizer tok = new SourceNodeTokenizer(text, diagnosticSink: DiagnosticSink);

        Assert.That(tok.ReadQuotedString(out _, out ReadOnlySpan<char> keyRange), Is.EqualTo("Key"));
        Assert.That(keyRange.ToString(), Is.EqualTo(@"""Key"""));

        Assert.That(tok.Character, Is.EqualTo('{'));
        Assert.That(tok.Index, Is.EqualTo(5 + endlLen));

        tok.SkipToken(out FileRange range);
        Assert.That(range, Is.EqualTo(new FileRange(2, 1, 37, 1)));

        DiagnosticSink.AssertNoDiagnostics();

        switch (hasEndNewLineAndProperty)
        {
            case 1:
                Assert.That(tok.ReadNonQuotedString(out _, out _), Is.EqualTo("OtherKey"));
                break;

            default:
                Assert.That(tok.IsAtEnd);
                break;
        }
    }
}