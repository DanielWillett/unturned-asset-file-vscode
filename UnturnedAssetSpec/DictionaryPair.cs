using System;
using System.Diagnostics;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// A case-insensitive key-value-pair used by <see cref="DictionarySpecPropertyType{TElementType}"/>.
/// </summary>
/// <typeparam name="TElementType">The value type.</typeparam>
[DebuggerDisplay("{ToString(),nq}")]
public readonly struct DictionaryPair<TElementType> : IDictionaryPair<DictionaryPair<TElementType>>
    where TElementType : IEquatable<TElementType>?
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

    /// <inheritdoc />
    public void Visit<TVisitor>(ref TVisitor visitor)
        where TVisitor : IDictionaryPairVisitor
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    {
        visitor.Accept(in this);
    }

    object? IDictionaryPair<DictionaryPair<TElementType>>.Value => Value;
}

/// <summary>
/// Interface implemented by <see cref="DictionaryPair{TElementType}"/>.
/// </summary>
/// <typeparam name="TSelf">The equatable array type. Should be <see cref="DictionaryPair{TElementType}"/>.</typeparam>
/// <remarks>Should not be implemented.</remarks>
public interface IDictionaryPair<TSelf> : IEquatable<TSelf>
    where TSelf : IEquatable<TSelf>
{
    /// <summary>
    /// Key of this pair.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Boxed value of this pair.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TVisitor"></typeparam>
    /// <param name="visitor"></param>
    void Visit<TVisitor>(ref TVisitor visitor)
        where TVisitor : IDictionaryPairVisitor
#if NET9_0_OR_GREATER
            , allows ref struct
#endif
    ;
}

/// <summary>
/// A visitor invoked from <see cref="DictionaryPair{TElementType}.Visit"/>. Used to transform generic method parameters.
/// </summary>
public interface IDictionaryPairVisitor
{
    /// <summary>
    /// Invoked from <see cref="DictionaryPair{TElementType}.Visit"/>.
    /// </summary>
    /// <typeparam name="TElementType">Element type.</typeparam>
    /// <param name="pair">The dictionary pair being visited.</param>
    void Accept<TElementType>(in DictionaryPair<TElementType> pair)
        where TElementType : IEquatable<TElementType>?;
}