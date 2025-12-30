using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Subtract : ExpressionFunction
{
    public static readonly Subtract Instance = new Subtract();
    static Subtract() { }

    public override string FunctionName => ExpressionFunctions.Subtract;
    public override int ArgumentCountMask => 1 << 1;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument is 0 or 1 ? NumericAnyType.Instance : null;
    }

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        return MathMatrix.Subtract<TIn1, TIn2, TVisitor, TOut>(v1, v2, ref visitor);
    }
}