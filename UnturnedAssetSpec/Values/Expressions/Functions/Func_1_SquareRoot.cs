using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class SquareRoot : ExpressionFunction
{
    public static readonly SquareRoot Instance = new SquareRoot();
    static SquareRoot() { }

    public override string FunctionName => ExpressionFunctions.SquareRoot;
    public override int ArgumentCountMask => 1 << 0;

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.Sqrt<TIn, TVisitor, TOut>(v, ref visitor);
    }
}