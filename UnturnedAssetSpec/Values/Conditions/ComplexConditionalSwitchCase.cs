using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed switch-case that defines either and 'And' or 'Or' property that contains multiple cases that must be met for this case to apply.
/// </summary>
public class ComplexConditionalSwitchCase : ISwitchCase
{
    private int? _hashCode;

    /// <summary>
    /// The value this case will evaluate to. This may be a <see cref="SwitchValue"/> or another value.
    /// </summary>
    public ImmutableArray<IValue<bool>> Conditions { get; }

    /// <summary>
    /// The operation used to combine <see cref="Conditions"/>.
    /// </summary>
    public JointConditionOperation Operation { get; }

    /// <inheritdoc />
    public IValue Value { get; }

    protected virtual bool WriteValueToJson => true;

    public ComplexConditionalSwitchCase(ImmutableArray<IValue<bool>> conditions, JointConditionOperation operation, IValue value)
    {
        if (operation is not JointConditionOperation.And and not JointConditionOperation.Or)
            throw new InvalidEnumArgumentException(nameof(operation), (int)operation, typeof(JointConditionOperation));

        Value = value ?? throw new ArgumentNullException(nameof(value));
        Conditions = conditions;
        Operation = conditions.Length switch
        {
            0 => JointConditionOperation.And,
            1 => JointConditionOperation.Or,
            _ => operation
        };
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        ImmutableArray<IValue<bool>> conditions = Conditions;
        if (!conditions.IsDefaultOrEmpty)
        {
            if (conditions.Length == 1)
            {
                writer.WritePropertyName("Case"u8);
                conditions[0].WriteToJson(writer, options);
            }
            else
            {
                writer.WritePropertyName(Operation == JointConditionOperation.And ? "And"u8 : "Or"u8);
                writer.WriteStartArray();
                foreach (IValue<bool> condition in conditions)
                {
                    condition.WriteToJson(writer, options);
                }
                writer.WriteEndArray();
            }
        }

        if (WriteValueToJson)
        {
            writer.WritePropertyName("Value"u8);
            Value.WriteToJson(writer, options);
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public bool TryCheckConditionsConcrete(out bool doesPassConditions)
    {
        foreach (IValue<bool> value in Conditions)
        {
            if (!value.TryGetConcreteValue(out Optional<bool> v))
            {
                doesPassConditions = false;
                return false;
            }
            
            bool passesCondition = v.GetValueOrDefault(false);

            switch (Operation)
            {
                case JointConditionOperation.Or when passesCondition:
                    doesPassConditions = true;
                    return true;

                case JointConditionOperation.And when !passesCondition:
                    doesPassConditions = false;
                    return true;
            }
        }

        doesPassConditions = Operation == JointConditionOperation.And;
        return true;
    }

    /// <inheritdoc />
    public bool TryCheckConditions(in FileEvaluationContext ctx, out bool doesPassConditions)
    {
        foreach (IValue<bool> value in Conditions)
        {
            if (!value.TryEvaluateValue(out Optional<bool> v, in ctx))
            {
                doesPassConditions = false;
                return false;
            }

            bool passesCondition = v.GetValueOrDefault(false);

            switch (Operation)
            {
                case JointConditionOperation.Or when passesCondition:
                    doesPassConditions = true;
                    return true;

                case JointConditionOperation.And when !passesCondition:
                    doesPassConditions = false;
                    return true;
            }
        }

        doesPassConditions = Operation == JointConditionOperation.And;
        return true;
    }

    /// <inheritdoc />
    public virtual bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!TryCheckConditionsConcrete(out bool v) || !v)
            return false;

        return Value.VisitConcreteValue(ref visitor);
    }

    /// <inheritdoc />
    public virtual bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (!TryCheckConditions(in ctx, out bool v) || !v)
            return false;

        return Value.VisitValue(ref visitor, in ctx);
    }

    /// <inheritdoc />
    public virtual bool Equals(IValue? other)
    {
        if (other is not ComplexConditionalSwitchCase sw || Operation != sw.Operation || !Value.Equals(sw.Value))
            return false;

        ImmutableArray<IValue<bool>> thisConditions = Conditions;
        ImmutableArray<IValue<bool>> otherConditions = sw.Conditions;
        if (thisConditions.Length != otherConditions.Length)
            return false;

        for (int i = 0; i < thisConditions.Length; ++i)
        {
            if (!thisConditions[i].Equals(otherConditions[i]))
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as ComplexConditionalSwitchCase);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (_hashCode.HasValue)
            return _hashCode.Value;

        HashCode hc = new HashCode();
        hc.Add(991754369);
        hc.Add(Operation);
        hc.Add(Value);
        foreach (IValue<bool> c in Conditions)
        {
            hc.Add(c);
        }

        int code = hc.ToHashCode();
        _hashCode = code;
        return code;
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    bool IValue.IsNull => Value.IsNull;
}

/// <summary>
/// A strongly-typed switch-case that defines either and 'And' or 'Or' property that contains multiple cases that must be met for this case to apply.
/// </summary>
public class ComplexConditionalSwitchCase<TResult> : ComplexConditionalSwitchCase, ISwitchCase<TResult>
    where TResult : IEquatable<TResult>
{
    /// <inheritdoc />
    public IType<TResult> Type { get; }

    /// <inheritdoc />
    public new IValue<TResult> Value => (IValue<TResult>)base.Value;

    public ComplexConditionalSwitchCase(ImmutableArray<IValue<bool>> conditions, JointConditionOperation operation, IValue<TResult> value)
        : base(conditions, operation, value)
    {
        Type = value.Type;
    }

    /// <inheritdoc />
    public virtual bool TryGetConcreteValue(out Optional<TResult> value)
    {
        if (!TryCheckConditionsConcrete(out bool v) || !v)
        {
            value = Optional<TResult>.Null;
            return false;
        }

        return Value.TryGetConcreteValue(out value);
    }

    /// <inheritdoc />
    public virtual bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        if (!TryCheckConditions(in ctx, out bool v) || !v)
        {
            value = Optional<TResult>.Null;
            return false;
        }

        return Value.TryEvaluateValue(out value, in ctx);
    }

    /// <inheritdoc />
    public override bool Equals(IValue? other)
    {
        return other is ComplexConditionalSwitchCase<TResult> sw && Type.Equals(sw.Type) && base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, base.GetHashCode());
    }
}

/// <summary>
/// A version of <see cref="ComplexConditionalSwitchCase{TResult}"/> that evaluates to <see langword="true"/> if the conditions match, otherwise <see langword="false"/>.
/// </summary>
public class ComplexConditionalValue : ComplexConditionalSwitchCase<bool>
{
    protected override bool WriteValueToJson => false;

    public ComplexConditionalValue(ImmutableArray<IValue<bool>> conditions, JointConditionOperation operation)
        : base(conditions, operation, Values.Value.True) { }

    /// <inheritdoc />
    public override bool TryEvaluateValue(out Optional<bool> value, in FileEvaluationContext ctx)
    {
        if (!TryCheckConditions(in ctx, out bool doesPassConditions))
        {
            value = Optional<bool>.Null;
            return false;
        }

        value = doesPassConditions;
        return true;
    }

    /// <inheritdoc />
    public override bool TryGetConcreteValue(out Optional<bool> value)
    {
        if (!TryCheckConditionsConcrete(out bool doesPassConditions))
        {
            value = Optional<bool>.Null;
            return false;
        }

        value = doesPassConditions;
        return true;
    }

    /// <inheritdoc />
    public override bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
    {
        if (!TryCheckConditionsConcrete(out bool doesPassConditions))
            return false;

        visitor.Accept(new Optional<bool>(doesPassConditions));
        return true;
    }

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        if (!TryCheckConditions(in ctx, out bool doesPassConditions))
            return false;

        visitor.Accept(new Optional<bool>(doesPassConditions));
        return true;
    }
}
