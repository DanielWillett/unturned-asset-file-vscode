using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace UnturnedAssetSpecTests;

[TestFixture]
partial class MathMatrixTests
{
    [Test]
    public static void CheckInactiveTypesRemoved_ArcTangentRad2([Range(0, InactiveTypeLen - 1)] int xTypeNum, [Range(0, InactiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(ArcTangentRad2ExecuteVisitor<,>).MakeGenericType(InactiveTypes[xTypeNum], InactiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.False);
        Assert.That(testInstance.ExecutedCt, Is.Zero);
    }

    [Test]
    public static void CheckActiveTypesDontRecurse_ArcTangentRad2([Range(0, ActiveTypeLen - 1)] int xTypeNum, [Range(0, ActiveTypeLen - 1)] int yTypeNum)
    {
        Type testType = typeof(ArcTangentRad2ExecuteVisitor<,>).MakeGenericType(ActiveTypes[xTypeNum], ActiveTypes[yTypeNum]);
        BaseExecuteVisitor testInstance = (BaseExecuteVisitor)Activator.CreateInstance(testType)!;

        Assert.That(testInstance.Execute(), Is.True);
        Assert.That(testInstance.ExecutedCt, Is.EqualTo(1));
    }

    private class ArcTangentRad2ExecuteVisitor<TInX, TInY> : BaseExecuteVisitor
        where TInX : IEquatable<TInX>
        where TInY : IEquatable<TInY>
    {
        public override bool Execute()
        {
            ArcTangentRad2ExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Atan2Rad(default(TInX)!, default(TInY)!, ref visitor);
        }

        public override bool Execute(params object[] values)
        {
            ArcTangentRad2ExecuteVisitor<TInX, TInY> visitor = this;
            return MathMatrix.Atan2Rad((TInX)values[0], (TInY)values[1], ref visitor);
        }
    }
}