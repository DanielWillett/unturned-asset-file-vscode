using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Maximum : ExpressionFunction
{
    public static readonly Maximum Instance = new Maximum();
    static Maximum() { }

    public override string FunctionName => ExpressionFunctions.Maximum;
    public override int ArgumentCountMask => 1 << 1;

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        return MathMatrix.Max<TIn1, TIn2, TVisitor, TOut>(v1, v2, ref visitor);
    }
}