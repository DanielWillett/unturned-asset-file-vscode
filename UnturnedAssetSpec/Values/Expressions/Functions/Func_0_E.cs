using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class E : ExpressionFunction, IFunctionExpressionNode
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

    int IFunctionExpressionNode.Count => 0;
    IExpressionFunction IFunctionExpressionNode.Function => this;
    IExpressionNode IFunctionExpressionNode.this[int index] => throw new ArgumentOutOfRangeException(nameof(index));
    bool IEquatable<IExpressionNode>.Equals(IExpressionNode? other) => other is E;
}