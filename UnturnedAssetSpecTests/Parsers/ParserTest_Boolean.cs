using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Boolean
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("true", true)]
    [TestCase("True", true)]
    [TestCase("TRUE", true)]
    [TestCase("false", false)]
    [TestCase("False", false)]
    [TestCase("FALSE", false)]
    public async Task ParseBooleanKeywords(string keyword, bool expectedValue)
    {
        ParserTest<bool> test = ParserTest<bool>.CreateFromSingleProperty(keyword, expectedValue, BooleanType.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("1", true)]
    [TestCase("t", true)]
    [TestCase("y", true)]
    [TestCase("0", false)]
    [TestCase("f", false)]
    [TestCase("n", false)]
    public async Task ParseBooleanCharacters(string character, bool expectedValue)
    {
        ParserTest<bool> test = ParserTest<bool>.CreateFromSingleProperty(character, expectedValue, BooleanType.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("T")]
    [TestCase("Y")]
    [TestCase("F")]
    [TestCase("N")]
    public async Task ParseInvalidUppercaseBooleanCharacters(string character)
    {
        ParserTest<bool> test = ParserTest<bool>.CreateFromSinglePropertyExpectFailure(character, BooleanType.Instance);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(
            new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Range = propertyNode.GetValueRange()
            },
            messageValidation: msg =>
            {
                Assert.That(msg, Does.Contain("lowercase"));
            }
        );
    }

    [Test]
    [TestCase("2")]
    [TestCase("-1")]
    [TestCase("no")]
    [TestCase("yes")]
    [TestCase("someotherword")]
    [TestCase("12345")]
    [TestCase("z")]
    public async Task ParseInvalidBooleanValues(string text)
    {
        ParserTest<bool> test = ParserTest<bool>.CreateFromSinglePropertyExpectFailure(text, BooleanType.Instance);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(
            new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Range = propertyNode.GetValueRange()
            },
            messageValidation: msg =>
            {
                Assert.That(msg, Does.Not.Contain("lowercase"));
            }
        );
    }

    [Test]
    public async Task ParseMissingValue()
    {
        ParserTest<bool> test = ParserTest<bool>.CreateFromSinglePropertyExpectFailure(string.Empty, BooleanType.Instance);

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(
            new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT2004,
                Range = propertyNode.Range
            },
            messageValidation: msg =>
            {
                Assert.That(msg, Does.Contain("expects a value"));
            }
        );
    }
}
