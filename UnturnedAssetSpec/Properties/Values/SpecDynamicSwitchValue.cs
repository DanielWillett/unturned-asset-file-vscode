using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

public class SpecDynamicSwitchValue :
    ISpecDynamicValue,
    IEquatable<ISpecDynamicValue>,
    IEquatable<SpecDynamicSwitchValue>
{
    public OneOrMore<SpecDynamicSwitchCaseValue> Cases { get; }

    public ISpecPropertyType? ValueType { get; }

    public bool HasCases => !Cases.IsNull;

    public SpecDynamicSwitchValue(ISpecPropertyType? valueType, OneOrMore<SpecDynamicSwitchCaseValue> valueCase)
    {
        ValueType = valueType;
        Cases = valueCase;
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        SpecDynamicSwitchCaseValue? match = TryMatchCase(in ctx);
        if (match == null)
        {
            return condition.Operation.EvaluateNulls(true, condition.Comparand == null);
        }

        return match.Value.EvaluateCondition(in ctx, in condition);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        SpecDynamicSwitchCaseValue? match = TryMatchCase(in ctx);
        if (match == null)
        {
            value = default;
            isNull = false;
            return false;
        }

        return match.Value.TryEvaluateValue(in ctx, out value, out isNull);
    }

    private SpecDynamicSwitchCaseValue? TryMatchCase(in FileEvaluationContext ctx)
    {
        foreach (SpecDynamicSwitchCaseValue @case in Cases)
        {
            if (IsCaseMet(@case, in ctx))
                return @case;
        }

        return null;
    }

    private static bool IsCaseMet(SpecDynamicSwitchCaseValue? c, in FileEvaluationContext ctx)
    {
        return c != null && c.IsCaseMet(in ctx);
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options)
    {
        SpecDynamicSwitchValueConverter.WriteSwitch(writer, this, options);
    }

    public bool Equals(SpecDynamicSwitchValue? other)
    {
        if (other == null)
            return false;

        return EqualityComparer<ISpecPropertyType?>.Default.Equals(ValueType, other.ValueType)
               && Cases.Equals(other.Cases);
    }

    public bool Equals(ISpecDynamicValue? other) => other is SpecDynamicSwitchValue v && Equals(v);
    public override bool Equals(object? obj) => obj is SpecDynamicSwitchValue v && Equals(v);
    public override string ToString() => $"Switch [{Cases.Length} cases]";
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = ValueType == null ? 0 : ValueType.GetHashCode();
            hashCode = (hashCode * 397) ^ Cases.GetHashCode();
            return hashCode;
        }
    }
}

public sealed class SpecDynamicSwitchCaseValue : ISpecDynamicValue, IEquatable<ISpecDynamicValue>, IEquatable<SpecDynamicSwitchCaseValue>
{
    public ISpecPropertyType? ValueType { get; }

    public OneOrMore<SpecDynamicSwitchCaseOrCondition> Conditions { get; }
    public SpecCondition WhenCondition { get; }
    public SpecDynamicSwitchCaseOperation Operation { get; }
    public ISpecDynamicValue Value { get; }

    public bool HasConditions => !Conditions.IsNull;

    public SpecDynamicSwitchCaseValue(SpecCondition when, ISpecPropertyType? valueType, OneOrMore<SpecDynamicSwitchCaseValue> cases)
    {
        WhenCondition = when;
        Operation = SpecDynamicSwitchCaseOperation.When;
        Value = new SpecDynamicSwitchValue(valueType, cases);
    }

    public SpecDynamicSwitchCaseValue(SpecDynamicSwitchCaseOperation operation, ISpecDynamicValue value, OneOrMore<SpecDynamicSwitchCaseOrCondition> cases)
    {
        Operation = cases.IsSingle ? SpecDynamicSwitchCaseOperation.And : operation;
        Conditions = cases;
        Value = value;
        ValueType = value.ValueType;
    }
    
    public bool EvaluateWhenCondition(in FileEvaluationContext ctx)
    {
        if (Operation != SpecDynamicSwitchCaseOperation.When)
            throw new InvalidOperationException("No when value.");

        SpecCondition whenCondition = WhenCondition;
        if (whenCondition.Variable == null)
        {
            // evaluates to true if excluded default
            return true;
        }

        return whenCondition.Variable.EvaluateCondition(in ctx, in whenCondition);
    }

    public bool IsCaseMet(in FileEvaluationContext ctx)
    {
        if (Operation == SpecDynamicSwitchCaseOperation.When)
        {
            return EvaluateWhenCondition(in ctx);
        }

        if (!HasConditions)
            return true;

        if (Conditions.IsSingle)
        {
            SpecDynamicSwitchCaseOrCondition c = Conditions.First();
            return IsConditionMet(in c, in ctx);
        }

        SpecDynamicSwitchCaseOperation op = Operation;
        for (int i = 0; i < Conditions.Length; ++i)
        {
            if (IsConditionMet(in Conditions.Values[i], in ctx))
            {
                if (op == SpecDynamicSwitchCaseOperation.Or)
                    return true;
            }
            else if (op == SpecDynamicSwitchCaseOperation.And)
            {
                return false;
            }
        }

        return false;
    }

    private static bool IsConditionMet(in SpecDynamicSwitchCaseOrCondition c, in FileEvaluationContext ctx)
    {
        return c.Case?.IsCaseMet(in ctx) ?? c.Condition.Variable.EvaluateCondition(in ctx, in c.Condition);
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        return Value.EvaluateCondition(in ctx, in condition);
    }

    public bool TryEvaluateValue<TValue>(in FileEvaluationContext ctx, out TValue? value, out bool isNull)
    {
        return Value.TryEvaluateValue(in ctx, out value, out isNull);
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options = null)
    {
        SpecDynamicSwitchCaseValueConverter.WriteCase(writer, this, options);
    }

    public bool Equals(SpecDynamicSwitchCaseValue? other)
    {
        if (other == null || Operation != other.Operation)
        {
            return false;
        }

        if (!HasConditions)
        {
            if (other.HasConditions)
                return false;
        }
        else if (!other.HasConditions)
        {
            return false;
        }

        if (Operation == SpecDynamicSwitchCaseOperation.When)
        {
            if (!WhenCondition.Equals(other.WhenCondition))
                return false;
        }

        if (EqualityComparer<ISpecDynamicValue>.Default.Equals(Value, other.Value))
        {
            return false;
        }

        return Conditions.Equals(other.Conditions);
    }

    public bool Equals(ISpecDynamicValue? other) => other is SpecDynamicSwitchCaseValue v && Equals(v);
    public override bool Equals(object? obj) => obj is SpecDynamicSwitchCaseValue v && Equals(v);
    public override string ToString() => Conditions == null ? $"{Operation} Case [1 case]" : $"{Operation} Case [{Conditions.Length} cases]";
    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = WhenCondition.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)Operation;
            hashCode = (hashCode * 397) ^ Value.GetHashCode();
            hashCode = (hashCode * 397) ^ Conditions.GetHashCode();
            return hashCode;
        }
    }
}

public enum SpecDynamicSwitchCaseOperation
{
    And,
    Or,
    When
}

public readonly struct SpecDynamicSwitchCaseOrCondition : IEquatable<SpecDynamicSwitchCaseOrCondition>
{
    public readonly SpecCondition Condition;
    public readonly SpecDynamicSwitchCaseValue? Case;

    public bool IsNull => Case == null && Condition.Variable == null;

    public SpecDynamicSwitchCaseOrCondition(SpecCondition condition)
    {
        Condition = condition;
    }

    public SpecDynamicSwitchCaseOrCondition(SpecDynamicSwitchCaseValue? @case)
    {
        Case = @case;
    }

    public void WriteToJsonWriter(Utf8JsonWriter writer, JsonSerializerOptions? options = null)
    {
        if (Case != null)
        {
            Case.WriteToJsonWriter(writer, options);
            return;
        }

        SpecConditionConverter.Write(writer, Condition);
    }

    public bool Equals(SpecDynamicSwitchCaseOrCondition other)
    {
        return Case?.Equals(other.Case) ?? Condition.Equals(other.Condition);
    }

    public override bool Equals(object? obj) => obj is SpecDynamicSwitchCaseOrCondition c && Equals(c);

    public override int GetHashCode() => Case != null ? Case.GetHashCode() : Condition.GetHashCode();

    public override string ToString() => Case?.ToString() ?? Condition.ToString();
}