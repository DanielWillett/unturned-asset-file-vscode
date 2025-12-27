using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Ceiling([Range(0, InactiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(CeilingExecuteVisitor<>).MakeGenericType(InactiveTypes[typeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Ceiling([Range(0, ActiveTypeLen - 1)] int typeNum)
    {
        Type testType = typeof(CeilingExecuteVisitor<>).MakeGenericType(ActiveTypes[typeNum]);
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
    [TestCase(95f, 95f)]
    [TestCase(95.1f, 96f)]
    [TestCase(95.9f, 96f)]
    [TestCase(95d, 95d)]
    [TestCase(95.1d, 96d)]
    [TestCase(95.9d, 96d)]
    [TestCase(-95f, -95f)]
    [TestCase(-95.1f, -95f)]
    [TestCase(-95.9f, -95f)]
    [TestCase(-95d, -95d)]
    [TestCase(-95.1d, -95d)]
    [TestCase(-95.9d, -95d)]
    public static void CheckValues_Ceiling(object arg, object result)
    {
        Type testType = typeof(CeilingExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));

        Type resultType = testInstance.Value?.GetType() ?? typeof(object);
        object typedResult = Convert.ChangeType(result, resultType);
        Assert.That(testInstance.Value, Is.EqualTo(typedResult));
    }

    [Test]
    public static void CheckValues_Ceiling_Decimal([Range(0, 6)] int argType)
    {
        decimal arg = argType switch
        {
            0 => decimal.Zero,
            1 => 95m,
            2 => 95.1m,
            3 => 95.9m,
            4 => -95m,
            5 => -95.1m,
            _ => -95.9m
        };
        Type testType = typeof(CeilingExecuteVisitor<>).MakeGenericType(arg.GetType());
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(arg), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
        Assert.That(testInstance.Value, Is.EqualTo(Math.Ceiling(arg)));
    }

    private class CeilingExecuteVisitor<TIn> : BaseExecuteVisitor
        where TIn : IEquatable<TIn>
    {
        public override bool Execute()
        {
            CeilingExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Ceiling(default(TIn)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            CeilingExecuteVisitor<TIn> visitor = this;
            return MathMatrix.Ceiling((TIn)values[0], ref visitor);
        }
    }
}