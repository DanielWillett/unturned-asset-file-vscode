using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal class FunctionExpressionNode0 : IFunctionExpressionNode
{
    public IExpressionFunction Function { get; }


    public FunctionExpressionNode0(IExpressionFunction function)
    {
        Function = function;
    }

    int IFunctionExpressionNode.Count => 0;
    IExpressionNode IFunctionExpressionNode.this[int index] => throw new ArgumentOutOfRangeException(nameof(index));
    public bool Equals(IExpressionNode? other) => other is IFunctionExpressionNode { Count: 0 };
}