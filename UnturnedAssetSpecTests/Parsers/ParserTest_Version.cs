using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Version
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("0.0.0.0")]
    [TestCase("128.129.128.5")]
    [TestCase("1.2.3.4")]
    [TestCase("1.2.3")]
    [TestCase("1.2")]
    [TestCase("9999.9999.9999.9999")]
    public async Task ParseVersions(string text)
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSingleProperty(text, Version.Parse(text), VersionType.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("0")]
    [TestCase("128")]
    [TestCase("1_2.3.4")]
    [TestCase("1.2.3.4.5")]
    [TestCase("2147483648.2147483648.2147483648.2147483648")]
    public async Task ParseInvalidVersions(string text)
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(text, VersionType.Instance);

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

    [Test]
    public async Task ParseLargeFromVersion()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.11.999",
            new VersionType(0, maxVersion: new Version(4, 10, 9, 100), strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(4);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.End.Character = expectedRange.Start.Character;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("6").And.Contain("4").And.Contain("exceed")));

        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 2;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("25").And.Contain("10").And.Contain("exceed")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 5;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("11").And.Contain("9").And.Contain("exceed")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 8;
        expectedRange.End.Character = expectedRange.Start.Character + 2;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("999").And.Contain("100").And.Contain("exceed")));
    }

    [Test]
    public async Task ParseLargeFromGlobal()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.11.999",
            new VersionType(0, maxComponent: 4, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(4);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.End.Character = expectedRange.Start.Character;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("6").And.Contain("4").And.Contain("exceed")));

        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 2;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("25").And.Contain("4").And.Contain("exceed")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 5;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("11").And.Contain("4").And.Contain("exceed")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 8;
        expectedRange.End.Character = expectedRange.Start.Character + 2;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("999").And.Contain("4").And.Contain("exceed")));
    }

    [Test]
    public async Task ParseSmallFromVersion()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.11.999",
            new VersionType(0, minVersion: new Version(8, 35, 91, 1000), strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(4);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.End.Character = expectedRange.Start.Character;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("6").And.Contain("8").And.Contain("less than")));

        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 2;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("25").And.Contain("35").And.Contain("less than")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 5;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("11").And.Contain("91").And.Contain("less than")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 8;
        expectedRange.End.Character = expectedRange.Start.Character + 2;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("999").And.Contain("1000").And.Contain("less than")));
    }

    [Test]
    public async Task ParseSmallFromGlobal()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.11.999",
            new VersionType(0, minComponent: 1000, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(4);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.End.Character = expectedRange.Start.Character;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("6").And.Contain("1000").And.Contain("less than")));

        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 2;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("25").And.Contain("1000").And.Contain("less than")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 5;
        expectedRange.End.Character = expectedRange.Start.Character + 1;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("11").And.Contain("1000").And.Contain("less than")));
        
        expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 8;
        expectedRange.End.Character = expectedRange.Start.Character + 2;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("999").And.Contain("1000").And.Contain("less than")));
    }

    [Test]
    public async Task ParseMatchesAllLimitsGlobal([Range(2, 4)] int digitCt)
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSingleProperty(
            digitCt switch
            {
                2 => "1.2",
                3 => "1.2.3",
                _ => "1.2.3.4"
            },
            digitCt switch
            {
                2 => new Version(1, 2),
                3 => new Version(1, 2, 3),
                _ => new Version(1, 2, 3, 4)
            },
            new VersionType(digitCt, maxComponent: 4, minComponent: 1, strictFormatting: true)
        );

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    public async Task ParseMatchesAllLimitsComponentWise([Range(2, 4)] int digitCt)
    {
        Version version = digitCt switch
        {
            2 => new Version(1, 2),
            3 => new Version(1, 2, 3),
            _ => new Version(1, 2, 3, 4)
        };
        ParserTest<Version> test = ParserTest<Version>.CreateFromSingleProperty(
            digitCt switch
            {
                2 => "1.2",
                3 => "1.2.3",
                _ => "1.2.3.4"
            },
            version,
            new VersionType(digitCt, maxVersion: version, minVersion: version, strictFormatting: true)
        );

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    public async Task ParseTooFewDigits_2_3()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25",
            new VersionType(3, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("2 digits,").And.Contain("exactly 3 digits")));
    }

    [Test]
    public async Task ParseTooFewDigits_2_4()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25",
            new VersionType(4, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("2 digits,").And.Contain("exactly 4 digits")));
    }

    [Test]
    public async Task ParseTooFewDigits_3_4()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.3",
            new VersionType(4, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = propertyNode.GetValueRange(),
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("3 digits,").And.Contain("exactly 4 digits")));
    }

    [Test]
    public async Task ParseTooManyDigits_3_2()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.3",
            new VersionType(2, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 4;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("3 digits,").And.Contain("exactly 2 digits")));
    }

    [Test]
    public async Task ParseTooManyDigits_4_2()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.3.4",
            new VersionType(2, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 4;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("4 digits,").And.Contain("exactly 2 digits")));
    }

    [Test]
    public async Task ParseTooManyDigits_4_3()
    {
        ParserTest<Version> test = ParserTest<Version>.CreateFromSinglePropertyExpectFailure(
            "6.25.3.4",
            new VersionType(3, strictFormatting: true)
        );

        _disposable = test;

        IPropertySourceNode propertyNode = await test.Execute();
        test.AssertNoReferencedProperties();
        test.AssertNoDereferencedProperties();

        test.AssertDiagnostics(1);
        FileRange expectedRange = propertyNode.GetValueRange();
        expectedRange.Start.Character += 6;
        test.AssertHasDiagnostic(new DatDiagnosticMessage
        {
            Range = expectedRange,
            Diagnostic = DatDiagnostics.UNT2031
        }, messageValidation: message => Assert.That(message, Does.Contain("4 digits,").And.Contain("exactly 3 digits")));
    }
}