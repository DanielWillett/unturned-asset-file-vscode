using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Pi : ExpressionFunction, IFunctionExpressionNode
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

    int IFunctionExpressionNode.Count => 0;
    IExpressionFunction IFunctionExpressionNode.Function => this;
    IExpressionNode IFunctionExpressionNode.this[int index] => throw new ArgumentOutOfRangeException(nameof(index));
    bool IEquatable<IExpressionNode>.Equals(IExpressionNode? other) => other is Pi;
}