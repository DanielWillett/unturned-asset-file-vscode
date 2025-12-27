using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class E : ExpressionFunction
{
    public static readonly E Instance = new E();
    static E() { }

    public override string FunctionName => ExpressionFunctions.E;
    public override int ArgumentCountMask => 0;

    public override bool Evaluate<TOut, TVisitor>(ref TVisitor visitor)
    {
        if (typeof(TOut) == typeof(float))
        {
            visitor.Accept(MathF.E);
        }
        else
        {
            visitor.Accept(Math.E);
        }
        return true;
    }
}