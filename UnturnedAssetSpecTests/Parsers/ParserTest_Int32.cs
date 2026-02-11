using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Int32
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("-2147483648", -2147483648)]
    [TestCase("0", 0)]
    [TestCase("2147483647", 2147483647)]
    [TestCase("100000", 100000)]
    [TestCase("-100000", -100000)]
    public async Task ParseIntegers(string text, int number)
    {
        ParserTest<int> test = ParserTest<int>.CreateFromSingleProperty(text, number, Int32Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-2147483649")]
    [TestCase("2147483648")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<int> test = ParserTest<int>.CreateFromSinglePropertyExpectFailure(text, Int32Type.Instance);

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