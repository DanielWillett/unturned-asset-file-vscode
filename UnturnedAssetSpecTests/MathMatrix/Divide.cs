using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Reflection;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Divide([Range(0, InactiveTypeLen - 1)] int xTypeNum, [Range(0, InactiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(DivideExecuteVisitor<,>).MakeGenericType(InactiveTypes[xTypeNum], InactiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Divide([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(DivideExecuteVisitor<,>).MakeGenericType(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    [Test]
    public static void CheckValues_Divide([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum)
    {
        MethodInfo m = typeof(MathMatrixTests).GetMethod(
            nameof(CheckValues_Divide_Intl),
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

    private static void CheckValues_Divide_Intl<TInX, TInY>()
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
                    BaseExecuteVisitor testInstance = new DivideExecuteVisitor<TInX, TInY>();

                    Assert.That(testInstance.Execute(x, y), Is.True);
                    Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
                    decimal xVal = ToDecimal(x), yVal = ToDecimal(y);
                    if (yVal != decimal.Zero)
                    {
                        decimal expected = xVal / yVal;
                        decimal actual = ToDecimal(testInstance.Value);
                        Assert.That(actual, Is.EqualTo(expected).Within(testInstance.Value is float ? 0.01m : 0.000001m), $"({typeof(TInX).Name}){x} / ({typeof(TInY).Name}){y}");
                    }
                    else
                    {
                        double expected = xVal == 0 ? double.NaN : xVal >= 0 ? double.PositiveInfinity : double.NegativeInfinity;
                        if (testInstance.Value is float f)
                        {
                            Assert.That(f, Is.EqualTo((float)expected), $"({typeof(TInX).Name}){x} / ({typeof(TInY).Name}){y}");
                        }
                        else
                        {
                            Assert.That(testInstance.Value, Is.EqualTo(expected), $"({typeof(TInX).Name}){x} / ({typeof(TInY).Name}){y}");
                        }
                    }
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

    private class DivideExecuteVisitor<TInX, TInY> : BaseExecuteVisitor
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
    {
        public override bool Execute()
        {
            DivideExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Divide(default(TInX)!, default(TInY)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            DivideExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Divide<TInX, TInY, DivideExecuteVisitor<TInX, TInY>, decimal>(
                (TInX)values[0], (TInY)values[1], ref visitor
            );
        }
    }
}