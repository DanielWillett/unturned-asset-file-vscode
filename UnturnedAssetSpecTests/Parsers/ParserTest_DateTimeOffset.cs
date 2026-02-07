using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Globalization;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_DateTimeOffset
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("02/06/2026 03:15:08 +00:00")]
    [TestCase("02/05/2026 22:15:27 -05:00")]
    public async Task ParseDates(string text)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
        ParserTest<DateTimeOffset> test = ParserTest<DateTimeOffset>.CreateFromSingleProperty(text, dateTimeOffset, DateTimeOffsetType.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("lmao")]
    [TestCase("")]
    public async Task ParseInvalidDates(string text)
    {
        ParserTest<DateTimeOffset> test = ParserTest<DateTimeOffset>.CreateFromSinglePropertyExpectFailure(text, DateTimeOffsetType.Instance);

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