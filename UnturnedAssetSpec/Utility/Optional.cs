using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Allows differentiating between a null value and a default value generically.
/// </summary>
/// <typeparam name="T">The type being contained.</typeparam>
public readonly struct Optional<T>(T? value) : IEquatable<T>, IEquatable<Optional<T>> where T : IEquatable<T>?
{
    /// <summary>
    /// A null value.
    /// </summary>
    // ReSharper disable once UnassignedReadonlyField
    public static readonly Optional<T> Null;

    /// <summary>
    /// The underlying value stored in this object.
    /// </summary>
    public readonly T? Value = value;

    /// <summary>
    /// Whether or not the value is logically <see langword="null"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue { get; } = value != null;

    /// <summary>
    /// Gets the value as a boxed object.
    /// </summary>
    public object? Boxed => HasValue ? Value : null;

    /// <summary>
    /// Gets <see cref="Value"/>, returning <paramref name="defaultValue"/> if <see cref="HasValue"/> is <see langword="false"/>.
    /// </summary>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public T? GetValueOrDefault(T? defaultValue)
    {
        return HasValue ? Value : defaultValue;
    }

    /// <summary>
    /// Attempts to get the value, or <see langword="null"/> if <typeparamref name="T"/> is a reference type.
    /// </summary>
    public bool TryGetValueOrNull(out T? value)
    {
        if (HasValue)
        {
            value = Value;
            return true;
        }

        value = default;
        return value == null;
    }

    /// <inheritdoc />
    public bool Equals(T? other)
    {
        return other == null ? !HasValue : HasValue && Value.Equals(other);
    }

    /// <inheritdoc />
    public bool Equals(Optional<T> other)
    {
        if (other.HasValue)
        {
            return HasValue && Value.Equals(other.Value);
        }

        return !HasValue;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj switch
        {
            T o => Equals(o),
            Optional<T> opt => Equals(opt),
            null => !HasValue,
            _ => false
        };
    }

    /// <inheritdoc />
    public override int GetHashCode() => HasValue ? Value.GetHashCode() : 0;

    /// <summary>
    /// Converts a <typeparamref name="T"/> value to an <see cref="Optional{T}"/> value with the same value.
    /// </summary>
    public static implicit operator Optional<T>(T obj) => new Optional<T>(obj);

    /// <inheritdoc />
    public override string ToString() => HasValue ? (Value.ToString() ?? string.Empty) : string.Empty;
}

/// <summary>
/// Extensions for the <see cref="Optional{T}"/> data structure.
/// </summary>
public static class OptionalExtensions
{
    extension<T>(Optional<T> optional) where T : struct, IEquatable<T>
    {
        /// <summary>
        /// Gets the value of this <see cref="Optional{T}"/> object as a nullable value type.
        /// </summary>
        public T? AsNullable() => optional.HasValue ? optional.Value : null;
    }
}