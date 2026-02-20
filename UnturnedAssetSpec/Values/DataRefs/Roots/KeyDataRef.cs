using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current element in a dictionary's key in <see cref="DictionaryTypeArgs{TValueType}.DefaultValue"/>.
/// </summary>
public sealed class KeyDataRef<TKey> : RootDataRef<KeyDataRef<TKey>>, IValue<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc />
    public IType<TKey> Type { get; }

    /// <inheritdoc />
    public override string PropertyName => "Key";

    protected override bool IsPropertyNameKeyword => true;

    public KeyDataRef(IType<TKey> type)
    {
        Type = type;
    }


    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        if (!TryEvaluateValue(out Optional<TKey> value, in ctx))
        {
            return false;
        }

        visitor.Accept(value);
        return true;
    }

    /// <inheritdoc />
    protected override bool Equals(KeyDataRef<TKey> other)
    {
        return Type.Equals(other.Type);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(127281853, Type);
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TKey> value)
    {
        // Allowing concrete parsing would give the impression that context doesn't matter.

        // It does in fact matter, it's just being accessed through a static ThreadLocal<string>
        // so it doesn't need a reference to the context.

        value = Optional<TKey>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TKey> value, in FileEvaluationContext ctx)
    {
        string? key = DictionaryType.Key.Value;
        if (key != null)
        {
            return TypeConverters.String.TryConvertTo(new Optional<string>(key), out value);
        }

        value = Optional<TKey>.Null;
        return false;
    }
}