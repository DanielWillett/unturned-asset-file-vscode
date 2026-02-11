using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_UInt16
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("0", (ushort)0)]
    [TestCase("65535", (ushort)65535)]
    [TestCase("1000", (ushort)1000)]
    public async Task ParseIntegers(string text, ushort number)
    {
        ParserTest<ushort> test = ParserTest<ushort>.CreateFromSingleProperty(text, number, UInt16Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-1")]
    [TestCase("65536")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<ushort> test = ParserTest<ushort>.CreateFromSinglePropertyExpectFailure(text, UInt16Type.Instance);

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