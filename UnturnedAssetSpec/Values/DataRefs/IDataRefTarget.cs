using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A data-ref that can be extended by another data-ref that implements <see cref="IDataRefProperty"/>.
/// </summary>
public interface IDataRefTarget : IEquatable<IDataRefTarget?>, IDataRefExpressionNode
{
    /// <summary>
    /// Accepts a data-ref property and invokes the <paramref name="valueVisitor"/> with the resulting value, if supported.
    /// </summary>
    /// <typeparam name="TProperty">The data-ref property type.</typeparam>
    /// <typeparam name="TVisitor">The visitor type to invoke with the value.</typeparam>
    /// <param name="property">The data-ref property.</param>
    /// <param name="valueVisitor">The visitor to invoke with the value.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <returns>Whether or not the operation succeeded.</returns>
    bool AcceptProperty<TProperty, TVisitor>(in TProperty property, ref TVisitor valueVisitor, in FileEvaluationContext ctx)
        where TProperty : IDataRefProperty
        where TVisitor : IValueVisitor;
}