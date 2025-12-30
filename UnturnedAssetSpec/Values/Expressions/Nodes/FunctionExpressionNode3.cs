using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal class FunctionExpressionNode3 : IFunctionExpressionNode
{
    public IExpressionFunction Function { get; }

    public IExpressionNode Argument1 { get; }
    public IExpressionNode Argument2 { get; }
    public IExpressionNode Argument3 { get; }

    public int Count => 3;
    public IExpressionNode this[int index] => index switch
    {
        0 => Argument1,
        1 => Argument2,
        2 => Argument3,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public FunctionExpressionNode3(IExpressionFunction function, IExpressionNode arg1, IExpressionNode arg2, IExpressionNode arg3)
    {
        Function = function;
        Argument1 = arg1;
        Argument2 = arg2;
        Argument3 = arg3;
    }

    public bool Equals(IExpressionNode? other)
    {
        if ((object?)other == this)
            return true;
        if (other == null)
            return false;
        return other is IFunctionExpressionNode { Count: 3 } funcNode
               && Function == funcNode.Function
               && Argument1.Equals(funcNode[0])
               && Argument2.Equals(funcNode[1])
               && Argument3.Equals(funcNode[2]);
    }
}