using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Guid
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("1471a10b365b406ca22bd4973ee2bc0d")]
    [TestCase("d901a99c-3220-4d85-84c6-c0086b76890d")]
    [TestCase("{567a4a56-64a8-4bfc-9466-a0f538990760}")]
    [TestCase("(14da93a8-8096-49b3-92cb-ed058c0fe0c8)")]
    [TestCase("{0x827189b7,0x2923,0x458d,{0xb1,0xe3,0x97,0x44,0xd7,0x0a,0x82,0xa4}}")]
    public async Task ParseGuids(string text)
    {
        Guid value = Guid.Parse(text);
        ParserTest<Guid> test = ParserTest<Guid>.CreateFromSingleProperty(text, value, GuidType.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    [TestCase("65536")]
    [TestCase("-1")]
    [TestCase("1471a10b365b406ca22bd4973ee2bc0df")]
    [TestCase("1471a10b365b406ca22bd4973ee2bc0_")]
    [TestCase("1471a10b365b406ca22bd4973ee2bc0")]
    public async Task ParseInvalidGuids(string text)
    {
        ParserTest<Guid> test = ParserTest<Guid>.CreateFromSinglePropertyExpectFailure(text, GuidType.Instance);

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