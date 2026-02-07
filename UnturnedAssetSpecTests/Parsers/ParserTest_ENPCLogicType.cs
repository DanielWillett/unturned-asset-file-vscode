using DanielWillett.UnturnedDataFileLspServer.Data;
using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable RawStringCanBeSimplified

namespace UnturnedAssetSpecTests.Parsers;

[TestFixture]
public class ParserTest_ENPCLogicType
{
    private IDisposable? _disposable;
    private static readonly QualifiedType EnumType = new QualifiedType("SDG.Unturned.ENPCLogicType, Assembly-CSharp", isCaseInsensitive: true);

    [TearDown]
    public void Teardown()
    {
        Interlocked.Exchange(ref _disposable, null)?.Dispose();
    }

    [Test]
    [TestCase("<", "LESS_THAN")]
    [TestCase("<=", "LESS_THAN_OR_EQUAL_TO")]
    [TestCase("≤", "LESS_THAN_OR_EQUAL_TO")]
    [TestCase("=", "EQUAL")]
    [TestCase("==", "EQUAL")]
    [TestCase("!=", "NOT_EQUAL")]
    [TestCase("≠", "NOT_EQUAL")]
    [TestCase(">=", "GREATER_THAN_OR_EQUAL_TO")]
    [TestCase("≥", "GREATER_THAN_OR_EQUAL_TO")]
    [TestCase(">", "GREATER_THAN")]
    [TestCase("NONE", "NONE")]
    [TestCase("LESS_THAN", "LESS_THAN")]
    [TestCase("LESS_THAN_OR_EQUAL_TO", "LESS_THAN_OR_EQUAL_TO")]
    [TestCase("EQUAL", "EQUAL")]
    [TestCase("NOT_EQUAL", "NOT_EQUAL")]
    [TestCase("GREATER_THAN_OR_EQUAL_TO", "GREATER_THAN_OR_EQUAL_TO")]
    [TestCase("GREATER_THAN", "GREATER_THAN")]
    public async Task ParseLogicType(string text, string enumValue)
    {
        ParserTest<DatEnumValue> test = ParserTest<DatEnumValue>.CreateFromSingleProperty(
            text,
            sp => ((DatEnumType)sp.Database.AllTypes[EnumType]).Values.First(x => x.Value == enumValue),
            sp => (DatEnumType)sp.Database.AllTypes[EnumType]
        );

        _disposable = test;

        await test.Execute();
        test.AssertNoEmissions();
    }

    [Test]
    [TestCase("lmao")]
    [TestCase("")]
    [TestCase("*")]
    [TestCase("**")]
    public async Task ParseInvalidLogicType(string text)
    {
        ParserTest<DatEnumValue> test = ParserTest<DatEnumValue>.CreateFromSinglePropertyExpectFailure(
            text,
            sp => (DatEnumType)sp.Database.AllTypes[EnumType]
        );

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