using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using System;

#pragma warning disable IDE0130

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning restore IDE0130

/// <summary>
/// An operation that can be performed by a condition.
/// </summary>
public interface IConditionOperation : IEquatable<IConditionOperation?>
{
    /// <summary>
    /// Unique name of this operation as defined in JSON.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Operator symbol used to represent conditions in text.
    /// </summary>
    string Symbol { get; }

    /// <summary>
    /// If a condition doesn't depend on the case of the value.
    /// </summary>
    bool IsCaseInsensitive { get; }

    /// <summary>
    /// If a condition is always <see langword="true"/> when two values are the same (after considering <see cref="IsCaseInsensitive"/>).
    /// </summary>
    bool IsEquality { get; }

    /// <summary>
    /// If a condition is always <see langword="true"/> when two values are different (after considering <see cref="IsCaseInsensitive"/>).
    /// </summary>
    bool IsInequality { get; }

    /// <summary>
    /// If a condition is always <see langword="false"/> when two values are the same (after considering <see cref="IsCaseInsensitive"/>).
    /// </summary>
    bool IsNonEquality { get; }

    /// <summary>
    /// If a condition is always <see langword="false"/> when two values are different (after considering <see cref="IsCaseInsensitive"/>).
    /// </summary>
    bool IsNonInequality { get; }

    /// <summary>
    /// Whether or not this operation evaluates to <see langword="true"/> when at least one of the value or comparand is <see langword="null"/>.
    /// </summary>
    bool EvaluateNullValues(bool valueIsNull, bool comparandIsNull);

    /// <summary>
    /// Evaluates this operation on two values.
    /// </summary>
    /// <typeparam name="TValue">The type of value being compared against.</typeparam>
    /// <returns>Whether or not this operation evaluates to <see langword="true"/>.</returns>
    bool TryEvaluate<TValue, TComparand>(TValue value, TComparand comparand, bool concreteOnly, in FileEvaluationContext ctx, out bool result)
        where TValue : IEquatable<TValue>
        where TComparand : IEquatable<TComparand>;
}

internal abstract class ConditionOperation<TSelf> : IConditionOperation
    where TSelf : ConditionOperation<TSelf>, new()
{
    public static readonly TSelf Instance = new TSelf();
    static ConditionOperation() { }

    public bool Equals(IConditionOperation? other) => other is TSelf;
    public override bool Equals(object? obj) => obj is TSelf;
    public abstract override int GetHashCode();
    public abstract string Name { get; }
    public abstract string Symbol { get; }
    public virtual bool IsCaseInsensitive => false;
    public virtual bool IsEquality => false;
    public virtual bool IsInequality => false;
    public virtual bool IsNonEquality => false;
    public virtual bool IsNonInequality => false;

    public bool EvaluateNullValues(bool valueIsNull, bool comparandIsNull)
    {
        if (!valueIsNull && !comparandIsNull)
            throw new ArgumentException("Expected at least one null value.");

        return EvaluateNullValuesImpl(valueIsNull, comparandIsNull);
    }

    protected virtual bool EvaluateNullValuesImpl(bool valueIsNull, bool comparandIsNull)
    {
        if (valueIsNull == comparandIsNull)
        {
            return !IsNonEquality && IsEquality;
        }

        return !IsNonInequality && IsInequality;
    }

    protected abstract bool TryEvaluateConcrete<TValue, TComparand>(TValue value, TComparand comparand, out bool result)
        where TValue : IEquatable<TValue>
        where TComparand : IEquatable<TComparand>;
    
    protected virtual bool TryEvaluate<TValue, TComparand>(TValue value, TComparand comparand, in FileEvaluationContext ctx, out bool result)
        where TValue : IEquatable<TValue>
        where TComparand : IEquatable<TComparand>
    {
        return TryEvaluateConcrete(value, comparand, out result);
    }

    public bool TryEvaluate<TValue, TComparand>(
        TValue value,
        TComparand comparand,
        bool concreteOnly,
        in FileEvaluationContext ctx,
        out bool result)
        where TValue : IEquatable<TValue>
        where TComparand : IEquatable<TComparand>
    {
        return concreteOnly
            ? TryEvaluateConcrete(value, comparand, out result)
            : TryEvaluate(value, comparand, in ctx, out result);
    }
}