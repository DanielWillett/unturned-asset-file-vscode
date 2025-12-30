using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

namespace UnturnedAssetSpecTests.Expressions;

[TestFixture]
public class EvaluationTests
{
    [Test]
    [TestCase("=PI", Math.PI)]
    [TestCase("=E", Math.E)]
    [TestCase("=TAU", Math.PI * 2d)]
    public void TestValueConstants(string constant, double expectedValue)
    {
        IValue<double> value = Values.FromExpression(Float64Type.Instance, constant, simplifyConstantExpressions: false);

        Assert.That(value.TryGetConcreteValue(out Optional<double> result), Is.True);
        Assert.That(result.HasValue);
        Assert.That(result.Value, Is.EqualTo(expectedValue).Within(0.00000001d));
    }
    
    [Test]
    public void TestNullConstant()
    {
        IValue<double> value = Values.FromExpression(Float64Type.Instance, "=NULL", simplifyConstantExpressions: false);

        Assert.That(value.TryGetConcreteValue(out Optional<double> result), Is.True);
        Assert.That(result.HasValue, Is.False);
    }

    [Test]

    // 1 arg
    [TestCase("=ROUND(3.1)", 3d)]
    [TestCase("=ABS(-3.1)", 3.1d)]
    [TestCase("=ABS(3.1)", 3.1d)]
    [TestCase("=CEIL(3.1)", 4d)]
    [TestCase("=FLOOR(3.1)", 3d)]
    [TestCase("=SINR(0.7853981633974483)", 0.7071067811865476d)]
    [TestCase("=SIND(45)", 0.7071067811865476d)]
    [TestCase("=SINR(0)", 0)]
    [TestCase("=SIND(0)", 0)]
    [TestCase("=COSR(0.7853981633974483)", 0.7071067811865476d)]
    [TestCase("=COSD(45)", 0.7071067811865476d)]
    [TestCase("=COSR(0)", 1d)]
    [TestCase("=COSD(0)", 1d)]
    [TestCase("=TANR(0.7853981633974483)", 1d)]
    [TestCase("=TAND(45)", 1d)]
    [TestCase("=TANR(0)", 0d)]
    [TestCase("=TAND(0)", 0d)]
    [TestCase("=ASINR(0.7071067811865476)", 0.7853981633974483d)]
    [TestCase("=ASIND(0.7071067811865476)", 45d)]
    [TestCase("=ASINR(0)", 0)]
    [TestCase("=ASIND(0)", 0)]
    [TestCase("=ACOSR(0.7071067811865476)", 0.7853981633974483d)]
    [TestCase("=ACOSD(0.7071067811865476)", 45d)]
    [TestCase("=ACOSR(1)", 0d)]
    [TestCase("=ACOSD(1)", 0d)]
    [TestCase("=ATANR(1)", 0.7853981633974483d)]
    [TestCase("=ATAND(1)", 45d)]
    [TestCase("=ATANR(0)", 0d)]
    [TestCase("=ATAND(0)", 0d)]
    [TestCase("=SQRT(9)", 3d)]
    [TestCase("=SQRT(14)", 3.7416573867739413d)]
    [TestCase("=CAT(3.1)", "3.1")]

    // 2 args
    [TestCase("=CAT((a b) ( c))", "a b c")]
    [TestCase("=ATAND(1 1)", 45d)]
    [TestCase("=ATANR(1 1)", Math.PI / 4)]
    [TestCase("=ADD(1 2)", 3)]
    [TestCase("=SUB(1 2)", -1)]
    [TestCase("=SUB(5 1)", 4)]
    [TestCase("=MUL(20 5)", 100)]
    [TestCase("=DIV(20 5)", 4)]
    [TestCase("=MOD(20 3)", 2)]
    [TestCase("=MIN(3.1 4.2)", 3.1d)]
    [TestCase("=MAX(-3.1 3.5)", 3.5d)]
    [TestCase("=POW(3 3)", 27d)]

    // 3 args
    [TestCase("=CAT((a b) ( c) d)", "a b cd")]
    [TestCase("=REP((unfair chair) ai x)", "unfxr chxr")]
    [TestCase("=REP(12 1 ())", "2")]
    [TestCase("=CUSTOM_BALLISTIC_GRAV(120f 4f 0.4f)", 6934.8894924017459d)]

    // nested functions
    [TestCase("MAX(=MIN(3 2) 1)", 2)]
    [TestCase("MAX(=ADD(=MIN(4 3) 2) 1)", 5)]
    [TestCase("MIN(=ADD(=MIN(4 3) 2) =SUB(4 1))", 3)]
    [TestCase("MUL(=CAT(=ADD(4 3) =SUB(4 1)) -1)", -73)]
    public void TestValueArg<TResult>(string constant, TResult expectedValue)
        where TResult : IEquatable<TResult>
    {
        IValue<TResult> value = Values.FromExpression(TypeConverters.Get<TResult>().DefaultType, constant, simplifyConstantExpressions: false);

        Assert.That(value.TryGetConcreteValue(out Optional<TResult> result), Is.True);
        Assert.That(result.HasValue);
        if (typeof(TResult) == typeof(float) || typeof(TResult) == typeof(double))
        {
            Assert.That(result.Value, Is.EqualTo(expectedValue).Within(0.00000001d));
        }
        else
        {
            Assert.That(result.Value, Is.EqualTo(expectedValue));
        }
    }

    [Test]
    [TestCase("NULL", "")]
    [TestCase("PI", Math.PI)]
    [TestCase("TAU", Math.PI * 2)]
    [TestCase("E", Math.E)]
    [TestCase("ABS(1)", 1)]
    [TestCase("MAX(=MIN(3 2) 1)", 2)]
    [TestCase("MAX(=ADD(=MIN(4 3) 2) 1)", 5)]
    [TestCase("MIN(=ADD(=MIN(4 3) 2) =SUB(4 1))", 3)]
    [TestCase("MUL(=CAT(=ADD(4 3) =SUB(4 1)) -1)", -73)]
    [TestCase("=REP((unfair chair) ai x)", "unfxr chxr")]
    public void Simplification<TResult>(string constant, TResult? expectedValue)
        where TResult : IEquatable<TResult>
    {
        if (expectedValue is string { Length: 0 })
            expectedValue = default;

        IValue<TResult> value = Values.FromExpression(TypeConverters.Get<TResult>().DefaultType, constant, simplifyConstantExpressions: true);

        Assert.That(value.TryGetConcreteValue(out Optional<TResult> result), Is.True);
        Assert.That(result.HasValue, Is.EqualTo(expectedValue != null));
        if (typeof(TResult) == typeof(float) || typeof(TResult) == typeof(double))
        {
            Assert.That(result.Value, Is.EqualTo(expectedValue).Within(0.00000001d));
        }
        else
        {
            Assert.That(result.Value, Is.EqualTo(expectedValue));
        }

        Assert.That(value, Is.Not.AssignableFrom<IFunctionExpressionNode>());
        Assert.That(value, Is.AssignableFrom<ConcreteValue<TResult>>().Or.AssignableFrom<NullValue<TResult>>());
    }
}