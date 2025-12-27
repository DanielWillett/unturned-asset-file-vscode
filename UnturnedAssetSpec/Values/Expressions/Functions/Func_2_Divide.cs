using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Divide : ExpressionFunction
{
    public static readonly Divide Instance = new Divide();
    static Divide() { }

    public override string FunctionName => ExpressionFunctions.Divide;
    public override int ArgumentCountMask => 1 << 1;

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        return MathMatrix.Divide<TIn1, TIn2, TVisitor, TOut>(v1, v2, ref visitor);
    }
}