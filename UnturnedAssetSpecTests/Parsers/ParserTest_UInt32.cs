using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_UInt32
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("0", 0u)]
    [TestCase("4294967295", 4294967295u)]
    [TestCase("100000", 100000u)]
    public async Task ParseIntegers(string text, uint number)
    {
        ParserTest<uint> test = ParserTest<uint>.CreateFromSingleProperty(text, number, UInt32Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-1")]
    [TestCase("4294967296")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<uint> test = ParserTest<uint>.CreateFromSinglePropertyExpectFailure(text, UInt32Type.Instance);

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