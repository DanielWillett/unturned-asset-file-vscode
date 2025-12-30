using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal sealed class Tau : ExpressionFunction, IFunctionExpressionNode
{
    public static readonly Tau Instance = new Tau();
    static Tau() { }

    public override string FunctionName => ExpressionFunctions.Tau;
    public override int ArgumentCountMask => 0;

    public override bool Evaluate<TOut, TVisitor>(ref TVisitor visitor)
    {
        if (typeof(TOut) == typeof(float))
        {
            visitor.Accept(MathF.PI * 2f);
        }
        else
        {
            visitor.Accept(Math.PI * 2d);
        }
        return true;
    }

    int IFunctionExpressionNode.Count => 0;
    IExpressionFunction IFunctionExpressionNode.Function => this;
    IExpressionNode IFunctionExpressionNode.this[int index] => throw new ArgumentOutOfRangeException(nameof(index));
    bool IEquatable<IExpressionNode>.Equals(IExpressionNode? other) => other is Tau;
}