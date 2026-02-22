using System.Globalization;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Reflection;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Compare([Range(0, InactiveTypeLen - 1)] int xTypeNum, [Range(0, InactiveTypeLen - 1)] int yTypeNum, [Values(true, false)] bool caseInsensitive)
    {
        Type testType = typeof(CompareExecuteVisitor<,>).MakeGenericType(InactiveTypes[xTypeNum], InactiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, [ caseInsensitive ])!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Compare([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum, [Values(true, false)] bool caseInsensitive)
    {
        Type testType = typeof(CompareExecuteVisitor<,>).MakeGenericType(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType, [ caseInsensitive ])!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    [Test]
    public static void CheckValues_Compare([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum, [Values(true, false)] bool caseInsensitive)
    {
        MethodInfo m = typeof(MathMatrixTests).GetMethod(
            nameof(CheckValues_Compare_Intl),
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
        )!.MakeGenericMethod(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);

        try
        {
            m.Invoke(null, [ caseInsensitive ]);
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            throw ex.InnerException;
        }
    }

    private static void CheckValues_Compare_Intl<TInX, TInY>(bool caseInsensitive)
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
                    BaseExecuteVisitor testInstance = new CompareExecuteVisitor<TInX, TInY>(caseInsensitive);

                    Assert.That(testInstance.Execute(x, y), Is.True);
                    Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
                    int expected;
                    if (typeof(TInX) == typeof(string) && typeof(TInY) == typeof(string))
                    {
                        string xStr = x is IFormattable f1 ? f1.ToString(null, CultureInfo.InvariantCulture) : x.ToString();
                        string yStr = y is IFormattable f2 ? f2.ToString(null, CultureInfo.InvariantCulture) : y.ToString();
                        expected = Math.Sign(string.Compare(xStr, yStr, caseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal));
                    }
                    else
                    {
                        expected = Math.Sign(ToBigInt(x).CompareTo(ToBigInt(y)));
                    }
                    Assert.That((int)testInstance.Value!, Is.EqualTo(expected), $"(({typeof(TInX).Name}){x}).CompareTo(({typeof(TInY).Name}){y})");
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

    private class CompareExecuteVisitor<TInX, TInY>(bool caseInsensitive) : BaseExecuteVisitor
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
    {
        public override bool Execute()
        {
            CompareExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Compare(default(TInX)!, default(TInY)!, caseInsensitive, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            CompareExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Compare((TInX)values[0], (TInY)values[1], caseInsensitive, ref visitor);
        }
    }
}