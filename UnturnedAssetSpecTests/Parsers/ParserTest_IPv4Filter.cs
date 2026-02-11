using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_IPv4Filter
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("123.122.121.120/30:55433-55456")]
    [TestCase("123.122.121.120/30:55433")]
    [TestCase("123.122.121.120:55433-55456")]
    [TestCase("123.122.121.120:55433")]
    [TestCase("123.122.121.120/30")]
    [TestCase("123.122.121.120")]
    [TestCase("1.2.3.4/6:8-9")]
    [TestCase("1.2.3.4/6:8")]
    [TestCase("1.2.3.4:8-9")]
    [TestCase("1.2.3.4:8")]
    [TestCase("1.2.3.4/6")]
    [TestCase("1.2.3.4")]
    public async Task ParseIPv4Filters(string text)
    {
        IPv4Filter value = IPv4Filter.Parse(text);
        ParserTest<IPv4Filter> test = ParserTest<IPv4Filter>.CreateFromSingleProperty(text, value, IPv4FilterType.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("123.122.121.120.180/30:55433-55456")]
    [TestCase("256.122.121.120/30:55433-55456")]
    [TestCase("123.122.121.120/30:")]
    [TestCase("123.122.121.120/")]
    [TestCase("123.122.121.")]
    [TestCase("123.122.121/30:55433-55456")]
    [TestCase("123.122/30:55433-55456")]
    [TestCase("123/30:55433-55456")]
    [TestCase("123.../30:55433-55456")]
    [TestCase("123../30:55433-55456")]
    [TestCase("123./30:55433-55456")]
    [TestCase("...123/30:55433-55456")]
    [TestCase("..123./30:55433-55456")]
    [TestCase(".123../30:55433-55456")]
    [TestCase("..123/30:55433-55456")]
    [TestCase(".123./30:55433-55456")]
    [TestCase(".123/30:55433-55456")]
    [TestCase("123.122.121.120/:55433-55456")]
    [TestCase("123.122.121.120:55433-55456/30")]
    [TestCase("123.122.121.120/30-55456")]
    [TestCase("123.122.121.120/30:-55456")]
    [TestCase("123.122.121.120/30:55433-")]
    [TestCase("123.122.121.120:-55433")]
    [TestCase("123.122.121.120:55433-")]
    [TestCase("123.122.121.120:55433/30-55456")]
    [TestCase("55433-55456:123.122.121.120/30")]
    [TestCase("55433-55456/30:123.122.121.120")]
    [TestCase("1.2.3.4.5/6:8-9")]
    [TestCase("256.2.3.4/6:8-9")]
    [TestCase("1.2.3.4/6:")]
    [TestCase("1.2.3.4/")]
    [TestCase("1.2.3.")]
    [TestCase("1.2.3/6:8-9")]
    [TestCase("1.2/6:8-9")]
    [TestCase("1/6:8-9")]
    [TestCase("1.../6:8-9")]
    [TestCase("1../6:8-9")]
    [TestCase("1./6:8-9")]
    [TestCase("...1/6:8-9")]
    [TestCase("..1./6:8-9")]
    [TestCase(".1../6:8-9")]
    [TestCase("..1/6:8-9")]
    [TestCase(".1./6:8-9")]
    [TestCase(".1/6:8-9")]
    [TestCase("1.2.3.4/:8-9")]
    [TestCase("1.2.3.4:8-9/6")]
    [TestCase("1.2.3.4/6-9")]
    [TestCase("1.2.3.4/6:-9")]
    [TestCase("1.2.3.4/6:8-")]
    [TestCase("1.2.3.4:-8")]
    [TestCase("1.2.3.4:8-")]
    [TestCase("1.2.3.4:1/30-9")]
    [TestCase("1-2:1.2.3.4/3")]
    [TestCase("1-2/3:1.2.3.4")]
    [TestCase("")]
    [TestCase(".../:-")]
    [TestCase(".../:")]
    [TestCase("...:-")]
    [TestCase("...:")]
    [TestCase("...")]
    [TestCase("../:-")]
    [TestCase("../:")]
    [TestCase("..:-")]
    [TestCase("..:")]
    [TestCase("..")]
    [TestCase("./:-")]
    [TestCase("./:")]
    [TestCase(".:-")]
    [TestCase(".:")]
    [TestCase(".")]
    [TestCase("/:-")]
    [TestCase("/:")]
    [TestCase(":-")]
    [TestCase(":")]
    public async Task ParseInvalidIPv4Filters(string text)
    {
        ParserTest<IPv4Filter> test = ParserTest<IPv4Filter>.CreateFromSinglePropertyExpectFailure(text, IPv4FilterType.Instance);

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