using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed switch-case that defines a 'When' property that has to be met before checking cases.
/// </summary>
/// <remarks>Implemented by <see cref="ConditionalSwitchCase{TComparand}"/>.</remarks>
public interface IConditionalSwitchCase : ISwitchCase
{
    /// <summary>
    /// The condition that must be met for this case to apply.
    /// </summary>
    IValue<bool> Condition { get; }

    /// <summary>
    /// Invokes a visitor that allows the caller to get an unboxed version of the condition.
    /// </summary>
    void VisitCondition<TVisitor>(ref TVisitor visitor)
        where TVisitor : Conditions.IConditionVisitor;
}

/// <summary>
/// A weakly-typed switch-case that defines a 'When' property that has to be met before checking cases.
/// </summary>
public class ConditionalSwitchCase<TComparand> : IConditionalSwitchCase
    where TComparand : IEquatable<TComparand>
{
    private readonly Condition<TComparand> _condition;

    /// <summary>
    /// The condition that must be met for this case to apply.
    /// </summary>
    public Condition<TComparand> Condition => _condition;

    /// <summary>
    /// The value this case will evaluate to. This may be a <see cref="SwitchValue"/> or another value.
    /// </summary>
    public IValue Value { get; }

    public ConditionalSwitchCase(Condition<TComparand> condition, IValue value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
        _condition = condition;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WritePropertyName("When"u8);
        _condition.WriteToJson(writer, options);

        if (Value is SwitchValue sv)
        {
            writer.WritePropertyName("Cases"u8);
            sv.WriteToJson(writer, options);
        }
        else
        {
            writer.WritePropertyName("Value"u8);
            Value.WriteToJson(writer, options);
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public bool TryCheckConditionsConcrete(out bool doesPassConditions)
    {
        if (!Condition.TryGetConcreteValue(out Optional<bool> v))
        {
            doesPassConditions = false;
            return false;
        }

        doesPassConditions = v.GetValueOrDefault(false);
        return true;
    }

    /// <inheritdoc />
    public bool TryCheckConditions(in FileEvaluationContext ctx, out bool doesPassConditions)
    {
        if (!Condition.TryEvaluateValue(out Optional<bool> v, in ctx))
        {
            doesPassConditions = false;
            return false;
        }

        doesPassConditions = v.GetValueOrDefault(false);
        return true;
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        if (!Condition.TryGetConcreteValue(out Optional<bool> v) || !v.GetValueOrDefault(false))
            return false;

        return Value.VisitConcreteValue(ref visitor);
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
    {
        if (!Condition.TryEvaluateValue(out Optional<bool> v, in ctx) || !v.GetValueOrDefault(false))
            return false;

        return Value.VisitValue(ref visitor, in ctx);
    }

    /// <inheritdoc />
    public virtual bool Equals(IValue? other)
    {
        return other is ConditionalSwitchCase<TComparand> sw && _condition.Equals(sw._condition) && Value.Equals(sw.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as ConditionalSwitchCase<TComparand>);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1761846473, _condition, Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{_condition.ToString()} ? {Value}";
    }

    [field: MaybeNull]
    IValue<bool> IConditionalSwitchCase.Condition => field ??= _condition;
    bool IValue.IsNull => Value.IsNull;
    void IConditionalSwitchCase.VisitCondition<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(in _condition);
    }
}

/// <summary>
/// A strongly-typed switch-case that defines a 'When' property that has to be met before checking cases.
/// </summary>
public class ConditionalSwitchCase<TResult, TComparand> : ConditionalSwitchCase<TComparand>, ISwitchCase<TResult>
    where TResult : IEquatable<TResult>
    where TComparand : IEquatable<TComparand>
{

    /// <inheritdoc />
    public IType<TResult> Type { get; }

    /// <inheritdoc />
    public new IValue<TResult> Value => (IValue<TResult>)base.Value;

    public ConditionalSwitchCase(Condition<TComparand> condition, IValue<TResult> value) : base(condition, value)
    {
        Type = value.Type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        if (!Condition.TryGetConcreteValue(out Optional<bool> v) || !v.GetValueOrDefault(false))
        {
            value = Optional<TResult>.Null;
            return false;
        }

        return Value.TryGetConcreteValue(out value);
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        if (!Condition.TryEvaluateValue(out Optional<bool> v, in ctx) || !v.GetValueOrDefault(false))
        {
            value = Optional<TResult>.Null;
            return false;
        }

        return Value.TryEvaluateValue(out value, in ctx);
    }

    /// <inheritdoc />
    public override bool Equals(IValue? other)
    {
        return other is ConditionalSwitchCase<TResult, TComparand> sw && Type.Equals(sw.Type) && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, base.GetHashCode());
    }
}