using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal class FunctionExpressionNode1 : IFunctionExpressionNode
{
    public IExpressionFunction Function { get; }

    public IExpressionNode Argument { get; }

    public int Count => 1;
    public IExpressionNode this[int index] => index switch
    {
        0 => Argument,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public FunctionExpressionNode1(IExpressionFunction function, IExpressionNode arg)
    {
        Function = function;
        Argument = arg;
    }

    public bool Equals(IExpressionNode? other)
    {
        if ((object?)other == this)
            return true;
        if (other == null)
            return false;
        return other is IFunctionExpressionNode { Count: 1 } funcNode
               && Function == funcNode.Function
               && Argument.Equals(funcNode[0]);
    }
}