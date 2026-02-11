using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_String
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("abc")]
    public async Task ParseStrings(string text)
    {
        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(text, text, StringType.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("")]
    public async Task ParseInvalidStrings(string text)
    {
        ParserTest<string> test = ParserTest<string>.CreateFromSinglePropertyExpectFailure(text, StringType.Instance);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = text.Length == 0 ? propertyNode.Range : propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT2004
        });
    }

    [Test]
    public async Task CheckMinLength()
    {
        const int minLength = 4;
        const string value = "abc";

        StringType type = new StringType(minCount: minLength);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(value, value, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT1024
        }, messageValidation: msg =>
        {
            Assert.That(msg, Does.Contain("at least"));
        });
    }

    [Test]
    public async Task CheckMaxLength()
    {
        const int maxLength = 2;
        const string value = "abc";

        StringType type = new StringType(maxCount: maxLength);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(value, value, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT1024
        }, messageValidation: msg =>
        {
            Assert.That(msg, Does.Contain("longer than"));
        });
    }

    [Test]
    [TestCase("Hello <color=#ffffff>World!</color>")]
    [TestCase("Hello <#fff>World!")]
    [TestCase("Hello <b>World!</b>")]
    [TestCase("Hello <u>World!</u>")]
    [TestCase("Hello <i>World!")]
    [TestCase("<color=#ffffff>Text</color>")]
    [TestCase("<color=#ffffff>Text")]
    [TestCase("<#ffffff>Text")]
    public async Task CheckUnexpectedRichText(string value)
    {
        StringType type = new StringType(allowRichText: false);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(value, value, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT1006
        });
    }

    [Test]
    [TestCase("Hello<br/>World", 5, 5)]
    [TestCase("Hello<br />World", 5, 6)]
    [TestCase("Hello<br>World", 5, 4)]
    [TestCase("Hello< br >World", 5, 6)]
    [TestCase("Hello< br>World", 5, 5)]
    [TestCase("<br/>", 0, 5)]
    [TestCase("</br>", 0, 5)]
    [TestCase("<br>", 0, 4)]
    public async Task CheckUnexpectedNewLine(string value, int offset, int len)
    {
        StringType type = new StringType(allowLineBreakTag: false);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(value, value, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        FileRange range = propertyNode.GetValueRange();
        range.Start.Character += offset;
        range.End.Character = range.Start.Character + len - 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = range,
            Diagnostic = DatDiagnostics.UNT1021
        });
    }

    [Test]
    [TestCase(@"Hello\nWorld", 5, 2)]
    [TestCase(@"Hello\r\nWorld", 5, 4)]
    [TestCase(@"\n", 0, 2)]
    [TestCase(@"\r\n", 0, 4)]
    public async Task CheckReplaceNewLineWithRichTextTag(string value, int offset, int len)
    {
        StringType type = new StringType(allowLineBreakTag: true);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(
            value,
            value.Replace("\\n", "\n"),
            type
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        FileRange range = propertyNode.GetValueRange();
        range.Start.Character += offset;
        range.End.Character = range.Start.Character + len - 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = range,
            Diagnostic = DatDiagnostics.UNT106
        });
    }

    [Test]
    [TestCase("Hello<br/>World", 5, 5)]
    [TestCase("Hello<br />World", 5, 6)]
    [TestCase("Hello< br >World", 5, 6)]
    [TestCase("Hello< br>World", 5, 5)]
    [TestCase("<br/>", 0, 5)]
    [TestCase("</br>", 0, 5)]
    public async Task CheckReplaceInvalidLineBreakWithCorrectTag(string value, int offset, int len)
    {
        StringType type = new StringType(allowLineBreakTag: true);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(value, value, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        FileRange range = propertyNode.GetValueRange();
        range.Start.Character += offset;
        range.End.Character = range.Start.Character + len - 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = range,
            Diagnostic = DatDiagnostics.UNT1022
        });
    }

    [Test]
    public async Task CheckParsesValidLineBreak()
    {
        const string value = "Test<br>value";
        StringType type = new StringType(allowLineBreakTag: true);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(value, value, type);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase(0u, "Value: {0}")]
    [TestCase(1u, "Value: {0} {1}")]
    [TestCase(1u, "Value: {1} {0}")]
    [TestCase(1u, "Value: {2} {0}")]
    [TestCase(2u, "Value: {0} {1} {2}")]
    [TestCase(2u, "Value: {0} {2} {1}")]
    [TestCase(2u, "Value: {2} {0}{1}")]
    [TestCase(2u, "Value: {11} {0}{1}")]
    [TestCase(2u, "Value: {-1} {0}{1}")]
    [TestCase(0u, "{0}")]
    public async Task CheckOutOfRangeFormattingArgs(uint args, string message)
    {
        StringType type = new StringType(maxFormatArguments: args);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(message, message, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT2012
        });
    }

    [Test]
    [TestCase(1u, "Value", -1)]
    [TestCase(2u, "Test Value {1}", 1)]
    [TestCase(5u, "Test {0} Value", 0)]
    public async Task CheckUnusedFormattingArgs(uint args, string message, int used)
    {
        StringType type = new StringType(maxFormatArguments: args);

        ParserTest<string> test = ParserTest<string>.CreateFromSingleProperty(message, message, type);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics((int)args - (used >= 0 ? 1 : 0));
        for (int i = 0; i < args; ++i)
        {
            if (i == used)
                continue;

            test.AssertHasDiagnostic(new DatDiagnosticMessage
            {
                Range = propertyNode.GetValueRange(),
                Diagnostic = DatDiagnostics.UNT102,
                Message = string.Format(DiagnosticResources.UNT102, i)
            }, matchMessage: true);
        }
    }
}