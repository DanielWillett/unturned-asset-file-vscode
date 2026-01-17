using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System;
using System.Text;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Weakly-typed data-ref.
/// </summary>
public interface IDataRef : IValue, IDataRefExpressionNode
{
    /// <summary>
    /// The target of this property, unless it's a root property.
    /// </summary>
    IDataRefTarget? Target { get; }

    /// <summary>
    /// The name of the property being accessed.
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Appends this data-ref's string representation.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to write to.</param>
    /// <param name="hash">Whether or not to include the initial hashtag. Defaults to <see langword="true"/>.</param>
    /// <returns>The original value of <paramref name="sb"/>.</returns>
    StringBuilder AppendExpressionString(StringBuilder sb, bool hash = true);

    /// <summary>
    /// Gets this data-ref's string representation.
    /// </summary>
    /// <param name="hash">Whether or not to include the initial hashtag. Defaults to <see langword="true"/>.</param>
    string GetExpressionString(bool hash = true);
}

/// <summary>
/// Base interface for all data-refs.
/// </summary>
/// <typeparam name="TValue">The type of value returned by this property.</typeparam>
public interface IDataRef<TValue> : IDataRef, IValue<TValue>
    where TValue : IEquatable<TValue>
{

}