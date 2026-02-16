using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current value in a dictionary's key.
/// </summary>
public sealed class KeyDataRef<TKey> : RootDataRef<KeyDataRef<TKey>>, IValue<TKey>
    where TKey : IEquatable<TKey>
{
    public string ActualValue { get; }
    public IType<TKey> Type { get; }

    public KeyDataRef(string actualValue, IType<TKey> type)
    {
        ActualValue = actualValue;
        Type = type;
    }

    /// <inheritdoc />
    public override string PropertyName => "Key";

    protected override bool IsPropertyNameKeyword => true;

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        return false;
    }

    /// <inheritdoc />
    protected override bool Equals(KeyDataRef<TKey> other)
    {
        return string.Equals(ActualValue, other.ActualValue, StringComparison.OrdinalIgnoreCase)
            && (other.Type?.Equals(Type) ?? Type == null);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1666914080, StringComparer.OrdinalIgnoreCase.GetHashCode(ActualValue), Type);
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TKey> value)
    {
        value = Optional<TKey>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TKey> value, in FileEvaluationContext ctx)
    {
        value = Optional<TKey>.Null;
        return false;
    }
}