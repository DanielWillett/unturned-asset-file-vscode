using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal class FunctionExpressionNode1 : IFunctionExpressionNode
{
    public IExpressionFunction Function { get; }

    public IExpressionNode Argument { get; }

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
}