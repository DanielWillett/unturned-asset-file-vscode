using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Globalization;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_DateTime
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("02/06/2026 03:17:37")]
    [TestCase("2026-02-05T22:17:37.0-05:00")]
    [TestCase("2026-02-05T22:17:37.0Z")]
    public async Task ParseDates(string text)
    {
        DateTime dateTimeOffset = DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
        ParserTest<DateTime> test = ParserTest<DateTime>.CreateFromSingleProperty(text, dateTimeOffset.ToUniversalTime(), DateTimeType.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("lmao")]
    [TestCase("")]
    public async Task ParseInvalidDates(string text)
    {
        ParserTest<DateTime> test = ParserTest<DateTime>.CreateFromSinglePropertyExpectFailure(text, DateTimeType.Instance);

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