using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Int16
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("-32768", (short)-32768)]
    [TestCase("0", (short)0)]
    [TestCase("32767", (short)32767)]
    [TestCase("1000", (short)1000)]
    [TestCase("-1000", (short)-1000)]
    public async Task ParseIntegers(string text, short number)
    {
        ParserTest<short> test = ParserTest<short>.CreateFromSingleProperty(text, number, Int16Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-32769")]
    [TestCase("32768")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<short> test = ParserTest<short>.CreateFromSinglePropertyExpectFailure(text, Int16Type.Instance);

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