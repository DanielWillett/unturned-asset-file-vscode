using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal class FunctionExpressionNode2 : IFunctionExpressionNode
{
    public IExpressionFunction Function { get; }

    public IExpressionNode Argument1 { get; }
    public IExpressionNode Argument2 { get; }

    public IExpressionNode this[int index] => index switch
    {
        0 => Argument1,
        1 => Argument2,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public FunctionExpressionNode2(IExpressionFunction function, IExpressionNode arg1, IExpressionNode arg2)
    {
        Function = function;
        Argument1 = arg1;
        Argument2 = arg2;
    }
}