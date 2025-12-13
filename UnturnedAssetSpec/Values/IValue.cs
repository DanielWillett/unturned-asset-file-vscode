using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue
{
    /// <summary>
    /// Whether or not this value represents a <see langword="null"/> value.
    /// </summary>
    bool IsNull { get; }

    /// <summary>
    /// The type stored in this value.
    /// </summary>
    IType Type { get; }
}

/// <summary>
/// A strongly-typed dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue<TValue> : IValue where TValue : IEquatable<TValue>
{
    /// <summary>
    /// The type stored in this value.
    /// </summary>
    new IType<TValue> Type { get; }

    /// <summary>
    /// Attempts to evaluate the value without any workspace context.
    /// </summary>
    bool TryGetConcreteValue(out Optional<TValue> value);

    /// <summary>
    /// Attempts to evaluate the current value of this <see cref="IValue{TValue}"/>.
    /// </summary>
    bool TryEvaluateValue(out Optional<TValue> value, in FileEvaluationContext ctx);
}