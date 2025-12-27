using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Absolute : ExpressionFunction
{
    public static readonly Absolute Instance = new Absolute();
    static Absolute() { }

    public override string FunctionName => ExpressionFunctions.Absolute;
    public override int ArgumentCountMask => 1 << 0;

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.Abs<TIn, TVisitor, TOut>(v, ref visitor);
    }
}