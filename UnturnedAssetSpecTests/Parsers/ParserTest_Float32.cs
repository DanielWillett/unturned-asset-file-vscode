using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Float32
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("1.0", 1.0f)]
    [TestCase("3", 3f)]
    [TestCase("49.5", 49.5f)]
    [TestCase("-15.04", -15.04f)]
    [TestCase("0", 0f)]
    [TestCase("NaN", float.NaN)]
    [TestCase("Infinity", float.PositiveInfinity)]
    [TestCase("-Infinity", float.NegativeInfinity)]
    [TestCase("1E+1", 10f)]
    public async Task ParseFloats(string text, float number)
    {
        ParserTest<float> test = ParserTest<float>.CreateFromSingleProperty(text, number, Float32Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    public async Task ParseInvalidFloats(string text)
    {
        ParserTest<float> test = ParserTest<float>.CreateFromSinglePropertyExpectFailure(text, Float32Type.Instance);

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