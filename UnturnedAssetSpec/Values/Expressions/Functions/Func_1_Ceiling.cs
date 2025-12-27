using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Ceiling : ExpressionFunction
{
    public static readonly Ceiling Instance = new Ceiling();
    static Ceiling() { }

    public override string FunctionName => ExpressionFunctions.Ceiling;
    public override int ArgumentCountMask => 1 << 0;

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.Ceiling<TIn, TVisitor, TOut>(v, ref visitor);
    }
}