using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Globalization;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_TimeSpan
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("03:17:37")]
    [TestCase("-03:17:37")]
    [TestCase("0")]
    public async Task ParseTimeSpans(string text)
    {
        TimeSpan dateTimeOffset = TimeSpan.Parse(text, CultureInfo.InvariantCulture);
        ParserTest<TimeSpan> test = ParserTest<TimeSpan>.CreateFromSingleProperty(text, dateTimeOffset, TimeSpanType.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("lmao")]
    [TestCase("")]
    public async Task ParseInvalidTimeSpans(string text)
    {
        ParserTest<TimeSpan> test = ParserTest<TimeSpan>.CreateFromSinglePropertyExpectFailure(text, TimeSpanType.Instance);

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