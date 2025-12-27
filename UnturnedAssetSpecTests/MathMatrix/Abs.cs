using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
[Parallelizable]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Abs([Range(0, InactiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(AbsExecuteVisitor<>).MakeGenericType(InactiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Abs([Range(0, ActiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(AbsExecuteVisitor<>).MakeGenericType(ActiveTypes[typeNum]);
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
    [TestCase(byte.MaxValue, byte.MaxValue)]
    [TestCase(sbyte.MaxValue, sbyte.MaxValue)]
    [TestCase(ushort.MaxValue, ushort.MaxValue)]
    [TestCase(short.MaxValue, short.MaxValue)]
    [TestCase(uint.MaxValue, uint.MaxValue)]
    [TestCase(int.MaxValue, int.MaxValue)]
    [TestCase(ulong.MaxValue, ulong.MaxValue)]
    [TestCase(long.MaxValue, long.MaxValue)]
    [TestCase(float.MaxValue, float.MaxValue)]
    [TestCase(double.MaxValue, double.MaxValue)]
    [TestCase(sbyte.MinValue, sbyte.MaxValue + 1)]
    [TestCase(short.MinValue, short.MaxValue + 1)]
    [TestCase(int.MinValue, int.MaxValue + 1u)]
    [TestCase(long.MinValue, long.MaxValue + 1ul)]
    [TestCase(float.MinValue, -float.MinValue)]
    [TestCase(double.MinValue, -double.MinValue)]
    public static void CheckValues_Abs(object arg, object result)
    {
        Type testType = typeof(AbsExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult));
    }

    [Test]
    public static void CheckValues_Abs_Decimal([Range(0, 2)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            1 => decimal.MinValue,
            _ => decimal.MaxValue
        };
        Type testType = typeof(AbsExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(arg >= 0 ? arg : -arg));
    }

    private class AbsExecuteVisitor<TIn> : BaseExecuteVisitor
        where TIn : IEquatable<TIn>
    {
        public override bool Execute()
        {
            AbsExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Abs(default(TIn)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            AbsExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Abs((TIn)values[0], ref visitor);
        }
    }
}