using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

internal interface IExpressionNode : IEquatable<IExpressionNode>;

internal interface IFunctionExpressionNode : IExpressionNode
{
    IExpressionFunction Function { get; }

    /// <summary>
    /// Gets the number of arguments supplied.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Gets the node at a given argument index.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    IExpressionNode this[int index] { get; }
}

internal interface IValueExpressionNode : IExpressionNode, IValue;
internal interface IPropertyReferenceExpressionNode : IExpressionNode
{
    PropertyReference Reference { get; }
}

internal interface IDataRefExpressionNode : IExpressionNode
{
    DataRef DataRef { get; }
}