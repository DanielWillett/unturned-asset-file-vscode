namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Concatenate : ExpressionFunction
{
    public static readonly Concatenate Instance = new Concatenate();
    static Concatenate() { }

    public override string FunctionName => ExpressionFunctions.Concatenate;
    public override int ArgumentCountMask => (1 << 0) | (1 << 1) | (1 << 2);
    public override bool ReduceToKnownTypes => false;

    public override bool Evaluate<TIn, TOut, TVisitor>(TIn v1, ref TVisitor visitor)
    {
        visitor.Accept(ToString(v1));
        return true;
    }

    public override bool Evaluate<TIn1, TIn2, TOut, TVisitor>(TIn1 v1, TIn2 v2, ref TVisitor visitor)
    {
        visitor.Accept(ToString(v1) + ToString(v2));
        return true;
    }

    public override bool Evaluate<TIn1, TIn2, TIn3, TOut, TVisitor>(TIn1 v1, TIn2 v2, TIn3 v3, ref TVisitor visitor)
    {
        visitor.Accept(ToString(v1) + ToString(v2) + ToString(v3));
        return true;
    }
}