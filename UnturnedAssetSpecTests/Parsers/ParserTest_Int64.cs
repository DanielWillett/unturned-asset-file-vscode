using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Int64
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("-9223372036854775808", -9223372036854775808)]
    [TestCase("0", 0L)]
    [TestCase("9223372036854775807", 9223372036854775807L)]
    [TestCase("100000000", 100000000)]
    [TestCase("-100000000", -100000000)]
    public async Task ParseIntegers(string text, long number)
    {
        ParserTest<long> test = ParserTest<long>.CreateFromSingleProperty(text, number, Int64Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-9223372036854775809")]
    [TestCase("9223372036854775808")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<long> test = ParserTest<long>.CreateFromSinglePropertyExpectFailure(text, Int64Type.Instance);

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