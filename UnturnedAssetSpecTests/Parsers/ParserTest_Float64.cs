using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Float64
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("1.0", 1.0d)]
    [TestCase("3", 3d)]
    [TestCase("49.5", 49.5d)]
    [TestCase("-15.04", -15.04d)]
    [TestCase("0", 0d)]
    [TestCase("NaN", double.NaN)]
    [TestCase("Infinity", double.PositiveInfinity)]
    [TestCase("-Infinity", double.NegativeInfinity)]
    [TestCase("1E+1", 10d)]
    public async Task ParseDoubles(string text, double number)
    {
        ParserTest<double> test = ParserTest<double>.CreateFromSingleProperty(text, number, Float64Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    public async Task ParseInvalidDoubles(string text)
    {
        ParserTest<double> test = ParserTest<double>.CreateFromSinglePropertyExpectFailure(text, Float64Type.Instance);

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