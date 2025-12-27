using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Round([Range(0, InactiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(RoundExecuteVisitor<>).MakeGenericType(InactiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Round([Range(0, ActiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(RoundExecuteVisitor<>).MakeGenericType(ActiveTypes[typeNum]);
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
    [TestCase(sbyte.MinValue, sbyte.MinValue)]
    [TestCase(short.MinValue, short.MinValue)]
    [TestCase(int.MinValue, int.MinValue)]
    [TestCase(long.MinValue, long.MinValue)]
    [TestCase(95.4f, 95f)]
    [TestCase(95.5f, 96f)]
    [TestCase(95.4d, 95d)]
    [TestCase(95.5d, 96d)]
    [TestCase(-95.4f, -95f)]
    [TestCase(-95.5f, -96f)]
    [TestCase(-95.4d, -95d)]
    [TestCase(-95.5d, -96d)]
    public static void CheckValues_Round(object arg, object result)
    {
        Type testType = typeof(RoundExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult));
    }

    [Test]
    public static void CheckValues_Round_Decimal([Range(0, 4)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            1 => 95.4m,
            2 => 95.5m,
            3 => -95.4m,
            _ => -95.5m
        };
        Type testType = typeof(RoundExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(Math.Round(arg)));
    }

    private class RoundExecuteVisitor<TIn> : BaseExecuteVisitor
        where TIn : IEquatable<TIn>
    {
        public override bool Execute()
        {
            RoundExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Round(default(TIn)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            RoundExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Round((TIn)values[0], ref visitor);
        }
    }
}