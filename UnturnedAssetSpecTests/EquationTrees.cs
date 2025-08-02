using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace UnturnedAssetSpecTests;

public class EquationTrees
{
    [Test]
    public void UnaryEquationTrees()
    {
        FileEvaluationContext ctx = default;

        Assert.That(SpecDynamicValue.TryParse("=ABS(-1)", SpecDynamicValueContext.Optional, KnownTypes.Int32, out ISpecDynamicValue equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out int intValue, out _));
        Assert.That(intValue, Is.EqualTo(1));

        Assert.That(SpecDynamicValue.TryParse("=ABS(=ROUND(-3.1))", SpecDynamicValueContext.Optional, KnownTypes.Float64, out equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out double doubleValue, out _));
        Assert.That(doubleValue, Is.EqualTo(3));

        Assert.That(SpecDynamicValue.TryParse("=ABS(=MAX(-8.2 -5.7))", SpecDynamicValueContext.Optional, KnownTypes.Float64, out equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out doubleValue, out _));
        Assert.That(doubleValue, Is.EqualTo(5.7).Within(0.0001));

        Assert.That(SpecDynamicValue.TryParse("=ABS(=MAX((-8.2) (-5.7)))", SpecDynamicValueContext.Optional, KnownTypes.Float64, out equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out doubleValue, out _));
        Assert.That(doubleValue, Is.EqualTo(5.7).Within(0.0001));

        Assert.That(SpecDynamicValue.TryParse("=ABS((=MAX((-8.2) (-5.7))))", SpecDynamicValueContext.Optional, KnownTypes.Float64, out equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out doubleValue, out _));
        Assert.That(doubleValue, Is.EqualTo(5.7).Within(0.0001));
    }

    [Test]
    public void BinaryEquationTrees()
    {
        FileEvaluationContext ctx = default;

        Assert.That(SpecDynamicValue.TryParse("=MAX(-1 1)", SpecDynamicValueContext.Optional, KnownTypes.Int32, out ISpecDynamicValue equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out int intValue, out _));
        Assert.That(intValue, Is.EqualTo(1));

        Assert.That(SpecDynamicValue.TryParse("=MAX(=ABS(-2) (1))", SpecDynamicValueContext.Optional, KnownTypes.Float64, out equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out double doubleValue, out _));
        Assert.That(doubleValue, Is.EqualTo(2));

        Assert.That(SpecDynamicValue.TryParse("=MAX(=MAX(-8.2 -5.7) =MAX(2 4))", SpecDynamicValueContext.Optional, KnownTypes.Float64, out equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out doubleValue, out _));
        Assert.That(doubleValue, Is.EqualTo(4).Within(0.0001));
    }

    [Test]
    public void TertiaryEquationTrees()
    {
        FileEvaluationContext ctx = default;

        Assert.That(SpecDynamicValue.TryParse("=REP(( / ) / _)", SpecDynamicValueContext.Optional, KnownTypes.String, out ISpecDynamicValue equation));

        Assert.That(equation.TryEvaluateValue(in ctx, out string? stringValue, out _));
        Assert.That(stringValue, Is.Not.Null);
        Assert.That(stringValue, Is.EqualTo(" _ "));
    }
}
