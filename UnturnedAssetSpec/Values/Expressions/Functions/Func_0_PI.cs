using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Pi : ExpressionFunction
{
    public static readonly Pi Instance = new Pi();
    static Pi() { }

    public override string FunctionName => ExpressionFunctions.Pi;
    public override int ArgumentCountMask => 0;

    public override bool Evaluate<TOut, TVisitor>(ref TVisitor visitor)
    {
        if (typeof(TOut) == typeof(float))
        {
            visitor.Accept(MathF.PI);
        }
        else
        {
            visitor.Accept(Math.PI);
        }
        return true;
    }
}