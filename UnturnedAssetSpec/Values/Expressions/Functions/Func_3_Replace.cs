using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Replace : ExpressionFunction
{
    public static readonly Replace Instance = new Replace();
    static Replace() { }

    public override string FunctionName => ExpressionFunctions.Replace;
    public override int ArgumentCountMask => 1 << 2;
    public override bool ReduceToKnownTypes => false;
    public override IType? GetIdealArgumentType(int argument)
    {
        return argument is 0 or 1 or 3 ? StringType.Instance : null;
    }

    public override bool Evaluate<TIn1, TIn2, TIn3, TOut, TVisitor>(TIn1 v1, TIn2 v2, TIn3 v3, ref TVisitor visitor)
    {
        visitor.Accept(ToString(v1).Replace(ToString(v2), ToString(v3)));
        return true;
    }
}