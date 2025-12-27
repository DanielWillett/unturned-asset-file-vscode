namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Null : ExpressionFunction
{
    public static readonly Null Instance = new Null();
    static Null() { }

    public override string FunctionName => ExpressionFunctions.Null;
    public override int ArgumentCountMask => 0;

    public override bool Evaluate<TOut, TVisitor>(ref TVisitor visitor)
    {
        if (default(TOut) == null)
        {
            visitor.Accept<TOut>(default);
        }
        else
        {
            visitor.Accept<string>(null);
        }
        return true;
    }
}