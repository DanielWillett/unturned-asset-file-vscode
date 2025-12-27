using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal interface IExpressionNode
{
   
}

internal interface IFunctionExpressionNode : IExpressionNode
{
    IExpressionFunction Function { get; }

    /// <summary>
    /// Gets the node at a given argument index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    IExpressionNode this[int index] { get; }
}