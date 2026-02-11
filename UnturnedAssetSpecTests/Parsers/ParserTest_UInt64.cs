using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_UInt64
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("0", 0ul)]
    [TestCase("18446744073709551615", 18446744073709551615ul)]
    [TestCase("1000000000", 1000000000ul)]
    public async Task ParseIntegers(string text, ulong number)
    {
        ParserTest<ulong> test = ParserTest<ulong>.CreateFromSingleProperty(text, number, UInt64Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-1")]
    [TestCase("18446744073709551616")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<ulong> test = ParserTest<ulong>.CreateFromSinglePropertyExpectFailure(text, UInt64Type.Instance);

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