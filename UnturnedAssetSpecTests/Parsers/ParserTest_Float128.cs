using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Globalization;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Float128
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("1.0")]
    [TestCase("3")]
    [TestCase("49.5")]
    [TestCase("-15.04")]
    [TestCase("0")]
    public async Task ParseDecimals(string text)
    {
        decimal number = decimal.Parse(text, CultureInfo.InvariantCulture);
        ParserTest<decimal> test = ParserTest<decimal>.CreateFromSingleProperty(text, number, Float128Type.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    public async Task ParseInvalidDecimals(string text)
    {
        ParserTest<decimal> test = ParserTest<decimal>.CreateFromSinglePropertyExpectFailure(text, Float128Type.Instance);

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
}