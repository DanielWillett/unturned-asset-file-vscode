using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_Power([Range(0, InactiveTypeLen - 1)] int xTypeNum, [Range(0, InactiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(PowerExecuteVisitor<,>).MakeGenericType(InactiveTypes[xTypeNum], InactiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_Power([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(PowerExecuteVisitor<,>).MakeGenericType(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    private class PowerExecuteVisitor<TInX, TInY> : BaseExecuteVisitor
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
    {
        public override bool Execute()
        {
            PowerExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Pow(default(TInX)!, default(TInY)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            PowerExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Pow((TInX)values[0], (TInY)values[1], ref visitor);
        }
    }
}