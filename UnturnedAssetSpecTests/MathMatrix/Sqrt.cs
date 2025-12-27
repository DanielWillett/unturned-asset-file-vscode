using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Sqrt([Range(0, InactiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(SqrtExecuteVisitor<>).MakeGenericType(InactiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Sqrt([Range(0, ActiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(SqrtExecuteVisitor<>).MakeGenericType(ActiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    [Test]
    [TestCase((byte)0, 0)]
    [TestCase((sbyte)0, 0)]
    [TestCase((ushort)0, 0)]
    [TestCase((short)0, 0)]
    [TestCase((uint)0, 0)]
    [TestCase(0, 0)]
    [TestCase("0", 0)]
    [TestCase((ulong)0, 0)]
    [TestCase((long)0, 0)]
    [TestCase((float)0, 0)]
    [TestCase((double)0, 0)]
    [TestCase((sbyte)-1, double.NaN)]
    [TestCase((short)-1, double.NaN)]
    [TestCase(-1, double.NaN)]
    [TestCase("-1", double.NaN)]
    [TestCase((long)-1, double.NaN)]
    [TestCase(-1f, float.NaN)]
    [TestCase(-1d, double.NaN)]
    [TestCase(float.PositiveInfinity, float.PositiveInfinity)]
    [TestCase(double.PositiveInfinity, double.PositiveInfinity)]
    [TestCase(float.NegativeInfinity, float.NaN)]
    [TestCase(double.NegativeInfinity, double.NaN)]
    [TestCase((byte)16, 4)]
    [TestCase((sbyte)16, 4)]
    [TestCase((ushort)16, 4)]
    [TestCase((short)16, 4)]
    [TestCase((uint)16, 4)]
    [TestCase(16, 4)]
    [TestCase("16", 4)]
    [TestCase((ulong)16, 4)]
    [TestCase((long)16, 4)]
    [TestCase((float)16, 4)]
    [TestCase((double)16, 4)]
    public static void CheckValues_Sqrt(object arg, object result)
    {
        Type testType = typeof(SqrtExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult).Within(0.0001d));
    }

    [Test]
    public static void CheckValues_Sqrt_Decimal([Range(0, 2)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            1 => decimal.MinusOne,
            _ => 16
        };
        Type testType = typeof(SqrtExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(argType switch
        {
            0 => 0d,
            1 => double.NaN,
            _ => 4d
        }).Within(0.0001d));
    }

    private class SqrtExecuteVisitor<TIn> : BaseExecuteVisitor
        where TIn : IEquatable<TIn>
    {
        public override bool Execute()
        {
            SqrtExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Sqrt(default(TIn)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            SqrtExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Sqrt((TIn)values[0], ref visitor);
        }
    }
}