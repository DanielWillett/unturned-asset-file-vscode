using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Int8
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("-128", (sbyte)-128)]
    [TestCase("0", (sbyte)0)]
    [TestCase("127", (sbyte)127)]
    [TestCase("72", (sbyte)72)]
    [TestCase("-72", (sbyte)-72)]
    public async Task ParseIntegers(string text, sbyte number)
    {
        ParserTest<sbyte> test = ParserTest<sbyte>.CreateFromSingleProperty(text, number, Int8Type.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("-129")]
    [TestCase("128")]
    public async Task ParseInvalidIntegers(string text)
    {
        ParserTest<sbyte> test = ParserTest<sbyte>.CreateFromSinglePropertyExpectFailure(text, Int8Type.Instance);

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