using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_Character
{
    private IDisposable? _disposable;

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("a", 'a')]
    [TestCase("A", 'A')]
    [TestCase("0", '0')]
    [TestCase("-", '-')]
    [TestCase("\" \"", ' ')]
    [TestCase(@"\n", '\n')]
    [TestCase(@"\t", '\t')]
    [TestCase(@"\\", '\\')]
    [TestCase(@"""\""""", '"')]
    public async Task ParseCharacters(string text, char character)
    {
        ParserTest<char> test = ParserTest<char>.CreateFromSingleProperty(text, character, CharacterType.Instance);

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("str")]
    [TestCase("")]
    public async Task ParseInvalidCharacters(string text)
    {
        ParserTest<char> test = ParserTest<char>.CreateFromSinglePropertyExpectFailure(text, CharacterType.Instance);

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