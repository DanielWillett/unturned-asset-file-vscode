using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed value created from a <see cref="PropertyReference"/>.
/// </summary>
public interface IPropertyReferenceValue : IValue
{
    /// <summary>
    /// The property being referenced.
    /// </summary>
    /// <remarks>Shouldn't usually be accessed on cross-reference properties.</remarks>
    DatProperty Property { get; }

    /// <summary>
    /// The owner of the property reference (the property being referenced from).
    /// </summary>
    DatProperty Owner { get; }
}

/// <summary>
/// An implementation of <see cref="IPropertyReferenceValue"/> that accesses a cross-referenced file.
/// </summary>
public interface ICrossedPropertyReference : IPropertyReferenceValue
{
    /// <summary>
    /// Attempts to create a new <see cref="FileEvaluationContext"/> for the cross-referenced file.
    /// Callers of this method must always call <see cref="DisposeContext"/> after they're done using it.
    /// </summary>
    bool TryResolveReference(in FileEvaluationContext oldContext, [UnscopedRef] out FileEvaluationContext newContext, [NotNullWhen(true)] out DatProperty? property);

    /// <summary>
    /// Disposes a context created from <see cref="TryResolveReference"/> after the caller is done with it.
    /// </summary>
    void DisposeContext(in FileEvaluationContext newContext);
}

/// <summary>
/// A strongly-typed value created from a <see cref="PropertyReference"/>.
/// </summary>
public interface IPropertyReferenceValue<TValue> : IPropertyReferenceValue, IValue<TValue>
    where TValue : IEquatable<TValue>;