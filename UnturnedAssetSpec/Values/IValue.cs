using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A dynamic value that can be resolved at runtime.
/// </summary>
public interface IValue
{
    /// <summary>
    /// Whether or not this value represents a <see langword="null"/> value.
    /// </summary>
    /// <remarks>It's still possible for a value to evaluate to <see langword="null"/> even if this is <see langword="false"/>.</remarks>
    bool IsNull { get; }

    /// <summary>
    /// The type stored in this value.
    /// </summary>
    IType Type { get; }

    /// <summary>
    /// Writes this value to a <see cref="Utf8JsonWriter"/> in a way that it can be recreated later.
    /// </summary>
    void WriteToJson(Utf8JsonWriter writer);
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

public static class ValueExtensions
{
    extension<T>(IValue<T> value) where T : IEquatable<T>
    {
        

    }
}