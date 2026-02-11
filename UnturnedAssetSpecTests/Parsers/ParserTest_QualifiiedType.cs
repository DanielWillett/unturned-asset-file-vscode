using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_QualifiedType
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("SDG.Unturned.ItemBarricadeAsset, Assembly-CSharp")]
    [TestCase("SDG.Unturned.ItemBarricadeAsset, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null")]
    public async Task ParseQualifiedTypes(string text)
    {
        QualifiedType value = new QualifiedType(text, isCaseInsensitive: true);
        ParserTest<QualifiedType> test = ParserTest<QualifiedType>.CreateFromSingleProperty(text, value, QualifiedTypeType.Instance);
        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("")]
    [TestCase("Barricade")]
    [TestCase("System.Object")]
    [TestCase("SDG.Unturned.ItemBarricadeAsset")]
    public async Task ParseInvalidQualifiedTypes(string text)
    {
        ParserTest<QualifiedType> test = ParserTest<QualifiedType>.CreateFromSinglePropertyExpectFailure(text, QualifiedTypeType.Instance);

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