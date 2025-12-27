using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Round : ExpressionFunction
{
    public static readonly Round Instance = new Round();
    static Round() { }

    public override string FunctionName => ExpressionFunctions.Round;
    public override int ArgumentCountMask => 1 << 0;

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.Round<TIn, TVisitor, TOut>(v, ref visitor);
    }
}