using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
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
    public SpecDynamicSwitchCaseOperation Operation { get; }

    /// <inheritdoc />
    public IValue Value { get; }

    public ComplexConditionalSwitchCase(ImmutableArray<IValue<bool>> conditions, SpecDynamicSwitchCaseOperation operation, IValue value)
    {
        if (operation is not SpecDynamicSwitchCaseOperation.And and not SpecDynamicSwitchCaseOperation.Or)
            throw new InvalidEnumArgumentException(nameof(operation), (int)operation, typeof(SpecDynamicSwitchCaseOperation));

        Value = value ?? throw new ArgumentNullException(nameof(value));
        Conditions = conditions;
        Operation = conditions.Length switch
        {
            0 => SpecDynamicSwitchCaseOperation.And,
            1 => SpecDynamicSwitchCaseOperation.Or,
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
                writer.WritePropertyName(Operation == SpecDynamicSwitchCaseOperation.And ? "And"u8 : "Or"u8);
                writer.WriteStartArray();
                foreach (IValue<bool> condition in conditions)
                {
                    condition.WriteToJson(writer, options);
                }
                writer.WriteEndArray();
            }
        }

        writer.WritePropertyName("Value"u8);
        Value.WriteToJson(writer, options);

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public bool TryCheckConditionsConcrete(out bool doesPassConditions)
    {
        foreach (IValue<bool> value in Conditions)
        {
            if (!value.TryGetConcreteValue(out Optional<bool> v))
                continue;
            
            bool passesCondition = v.GetValueOrDefault(false);

            switch (Operation)
            {
                case SpecDynamicSwitchCaseOperation.Or when passesCondition:
                    doesPassConditions = true;
                    return true;

                case SpecDynamicSwitchCaseOperation.And when !passesCondition:
                    doesPassConditions = false;
                    return true;
            }
        }

        doesPassConditions = Operation == SpecDynamicSwitchCaseOperation.And;
        return true;
    }

    /// <inheritdoc />
    public bool TryCheckConditions(in FileEvaluationContext ctx, out bool doesPassConditions)
    {
        foreach (IValue<bool> value in Conditions)
        {
            if (!value.TryEvaluateValue(out Optional<bool> v, in ctx))
                continue;
            
            bool passesCondition = v.GetValueOrDefault(false);

            switch (Operation)
            {
                case SpecDynamicSwitchCaseOperation.Or when passesCondition:
                    doesPassConditions = true;
                    return true;

                case SpecDynamicSwitchCaseOperation.And when !passesCondition:
                    doesPassConditions = false;
                    return true;
            }
        }

        doesPassConditions = Operation == SpecDynamicSwitchCaseOperation.And;
        return true;
    }

    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor) where TVisitor : IValueVisitor
    {
        if (!TryCheckConditionsConcrete(out bool v) || !v)
            return false;

        return Value.VisitConcreteValue(ref visitor);
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx) where TVisitor : IValueVisitor
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

    public ComplexConditionalSwitchCase(ImmutableArray<IValue<bool>> conditions, SpecDynamicSwitchCaseOperation operation, IValue<TResult> value)
        : base(conditions, operation, value)
    {
        Type = value.Type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        if (!TryCheckConditionsConcrete(out bool v) || !v)
        {
            value = Optional<TResult>.Null;
            return false;
        }

        return Value.TryGetConcreteValue(out value);
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
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