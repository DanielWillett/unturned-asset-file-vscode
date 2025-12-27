using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Floor : ExpressionFunction
{
    public static readonly Floor Instance = new Floor();
    static Floor() { }

    public override string FunctionName => ExpressionFunctions.Floor;
    public override int ArgumentCountMask => 1 << 0;

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v, ref TVisitor visitor)
    {
        return MathMatrix.Floor<TIn, TVisitor, TOut>(v, ref visitor);
    }
}