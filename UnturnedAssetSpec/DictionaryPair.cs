using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// A case-insensitive key-value-pair used by <see cref="DictionarySpecPropertyType{TElementType}"/>.
/// </summary>
/// <typeparam name="TElementType">The value type.</typeparam>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct DictionaryPair<TElementType> : IEquatable<DictionaryPair<TElementType>> where TElementType : IEquatable<TElementType>?
{
    public string Key { get; }
    public TElementType? Value { get; }

    public DictionaryPair(string key, TElementType? value)
    {
        Key = key;
        Value = value;
    }

    /// <inheritdoc />
    public bool Equals(DictionaryPair<TElementType> other)
    {
        return string.Equals(other.Key, Key, StringComparison.OrdinalIgnoreCase) && (Value == null ? other.Value == null : other.Value != null && Value.Equals(other.Value));
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DictionaryPair<TElementType> pair && Equals(pair);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (Key == null)
        {
            return Value == null ? 0 : Value.GetHashCode();
        }

        int hc = StringComparer.OrdinalIgnoreCase.GetHashCode(Key);
        return Value == null ? hc : (hc ^ Value.GetHashCode());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"({Key}, {Value})";
    }
}