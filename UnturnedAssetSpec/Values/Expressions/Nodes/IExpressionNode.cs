using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.ComponentModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

/// <summary>
/// A node/value in an expression.
/// </summary>
/// <remarks>This is an internal API, do not use.</remarks>
#if RELEASE
[EditorBrowsable(EditorBrowsableState.Never)]
#endif
public interface IExpressionNode : IEquatable<IExpressionNode?>;

/// <summary>
/// A function call in an expression.
/// </summary>
/// <remarks>This is an internal API, do not use.</remarks>
#if RELEASE
[EditorBrowsable(EditorBrowsableState.Never)]
#endif
public interface IFunctionExpressionNode : IExpressionNode
{
    /// <summary>
    /// The function being called.
    /// </summary>
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

/// <summary>
/// A literal value in an expression.
/// </summary>
/// <remarks>This is an internal API, do not use.</remarks>
#if RELEASE
[EditorBrowsable(EditorBrowsableState.Never)]
#endif
public interface IValueExpressionNode : IExpressionNode, IValue;

/// <summary>
/// A property-ref in an expression.
/// </summary>
/// <remarks>This is an internal API, do not use.</remarks>
#if RELEASE
[EditorBrowsable(EditorBrowsableState.Never)]
#endif
public interface IPropertyReferenceExpressionNode : IExpressionNode
{
    /// <summary>
    /// The property being referenced.
    /// </summary>
    PropertyReference Reference { get; }
}

/// <summary>
/// A data-ref in an expression.
/// </summary>
/// <remarks>This is an internal API, do not use.</remarks>
#if RELEASE
[EditorBrowsable(EditorBrowsableState.Never)]
#endif
public interface IDataRefExpressionNode : IExpressionNode
{
    /// <summary>
    /// The underlying data-ref value for this node. Usually this is just <see langword="this"/>.
    /// </summary>
    IDataRef DataRef { get; }
}