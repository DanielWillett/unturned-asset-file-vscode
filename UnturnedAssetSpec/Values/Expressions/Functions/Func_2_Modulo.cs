using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Modulo : ExpressionFunction
{
    public static readonly Modulo Instance = new Modulo();
    static Modulo() { }

    public override string FunctionName => ExpressionFunctions.Modulo;
    public override int ArgumentCountMask => 1 << 1;

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        return MathMatrix.Modulo<TIn1, TIn2, TVisitor, TOut>(v1, v2, ref visitor);
    }
}