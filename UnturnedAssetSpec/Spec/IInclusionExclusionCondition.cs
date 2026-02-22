using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Base interface for <see cref="IInclusionCondition"/> and <see cref="IExclusionCondition"/>.
/// </summary>
public interface IInclusionExclusionCondition
{
    /// <summary>
    /// Reference to the property being included.
    /// </summary>
    ref readonly PropertyReference PropertyReference { get; }

    /// <summary>
    /// Whether or not this condition accepts any value.
    /// </summary>
    bool IsAnyValue { get; }

    /// <summary>
    /// Invokes a visitor with the checked value, if it exists.
    /// </summary>
    /// <typeparam name="TVisitor">The visitor type (<see cref="IValueVisitor"/>).</typeparam>
    /// <param name="visitor">The visitor to invoke <see cref="IValueVisitor.Accept{TValue}"/> on.</param>
    /// <returns>Whether or not the visitor was invoked. If there is not a value for this condition it will not be invoked and <see langword="false"/> will be returned.</returns>
    bool VisitValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    ;
}

/// <summary>
/// Base interface for <see cref="IInclusionCondition{TValue}"/> and <see cref="IExclusionCondition{TValue}"/>.
/// </summary>
public interface IInclusionExclusionCondition<TValue>
    where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The value the property must or can't have.
    /// </summary>
    Optional<TValue> Value { get; }
}