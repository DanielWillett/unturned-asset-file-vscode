using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Numerics;
using System.Reflection;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Max([Range(0, InactiveTypeLen - 1)] int xTypeNum, [Range(0, InactiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(MaxExecuteVisitor<,>).MakeGenericType(InactiveTypes[xTypeNum], InactiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Max([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(MaxExecuteVisitor<,>).MakeGenericType(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    [Test]
    public static void CheckValues_Max([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum)
    {
        MethodInfo m = typeof(MathMatrixTests).GetMethod(
            nameof(CheckValues_Max_Intl),
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
        )!.MakeGenericMethod(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);

        try
        {
            m.Invoke(null, Array.Empty<object>());
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static void CheckValues_Max_Intl<TInX, TInY>()
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
    {
        List<Exception> excs = new List<Exception>();
        foreach (TInX x in GetTestValues<TInX>.Values)
        {
            foreach (TInY y in GetTestValues<TInY>.Values)
            {
                try
                {
                    BaseExecuteVisitor testInstance = new MaxExecuteVisitor<TInX, TInY>();

                    Assert.That(testInstance.Execute(x, y), Is.True);
                    Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
                    BigInteger expected = BigInteger.Max(ToBigInt(x), ToBigInt(y));
                    Assert.That(ToBigInt(testInstance.Value), Is.EqualTo(expected), $"max(({typeof(TInX).Name}){x}, ({typeof(TInY).Name}){y})");
                }
                catch (Exception ex)
                {
                    excs.Add(ex);
                }
            }
        }

        switch (excs.Count)
        {
            case 0:
                break;

            case 1:
                throw excs[0];

            default:
                throw new AggregateException(excs);
        }
    }

    private class MaxExecuteVisitor<TInX, TInY> : BaseExecuteVisitor
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
    {
        public override bool Execute()
        {
            MaxExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Max(default(TInX)!, default(TInY)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            MaxExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Max<TInX, TInY, MaxExecuteVisitor<TInX, TInY>, decimal>(
                (TInX)values[0], (TInY)values[1], ref visitor
            );
        }
    }
}