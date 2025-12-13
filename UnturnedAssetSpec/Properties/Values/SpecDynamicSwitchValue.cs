using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Logic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A switch statement which maps ordered conditions to different values.
/// </summary>
public class SpecDynamicSwitchValue :
    ISpecDynamicValue,
    IEquatable<ISpecDynamicValue>,
    IEquatable<SpecDynamicSwitchValue>
{
    public OneOrMore<SpecDynamicSwitchCaseValue> Cases { get; private set; }

    public ISpecPropertyType? ValueType { get; }

    public bool HasCases => !Cases.IsNull;

    public SpecDynamicSwitchValue(ISpecPropertyType? valueType, OneOrMore<SpecDynamicSwitchCaseValue> valueCase)
    {
        ValueType = valueType;
        Cases = valueCase;
    }

    /// <returns><see langword="true"/> if any values were updated.</returns>
    internal bool UpdateValues(Func<ISpecDynamicValue, ISpecDynamicValue> valueTransformation)
    {
        Queue<SpecDynamicSwitchCaseValue> queue = new Queue<SpecDynamicSwitchCaseValue>(4);
        bool anyUpdates = false;
        foreach (SpecDynamicSwitchCaseValue c in Cases)
        {
            queue.Enqueue(c);
        }

        while (queue.Count > 0)
        {
            SpecDynamicSwitchCaseValue c = queue.Dequeue();
            anyUpdates |= Visit(c, valueTransformation);
            foreach (SpecDynamicSwitchCaseOrCondition c2 in c.Conditions)
            {
                if (c2.Case == null)
                    continue;

                queue.Enqueue(c2.Case);
            }
        }

        return anyUpdates;

        static bool Visit(SpecDynamicSwitchCaseValue c, Func<ISpecDynamicValue, ISpecDynamicValue> valueTransformation)
        {
            ISpecDynamicValue newValue = valueTransformation(c.Value);
            if (ReferenceEquals(newValue, c.Value))
            {
                return false;
            }

            c.Value = newValue;
            return true;
        }
    }

    internal bool TryZipExact(SpecDynamicSwitchValue other, Func<ISpecDynamicValue, ISpecDynamicValue, ISpecDynamicValue> combinator)
    {
        Queue<(SpecDynamicSwitchCaseValue c1, SpecDynamicSwitchCaseValue c2)> queue = new Queue<(SpecDynamicSwitchCaseValue, SpecDynamicSwitchCaseValue)>(4);
        bool anyUpdates = false;
        
        if (other.Cases.Length != Cases.Length)
            return false;

        for (int i = 0; i < Cases.Length; i++)
        {
            SpecDynamicSwitchCaseValue c1 = Cases[i];
            SpecDynamicSwitchCaseValue c2 = other.Cases[i];
            if (!c1.ConditionsEqual(c2))
                return false;

            queue.Enqueue((c1, c2));
        }

        while (queue.Count > 0)
        {
            (SpecDynamicSwitchCaseValue c1, SpecDynamicSwitchCaseValue c2) = queue.Dequeue();
            anyUpdates |= Visit(c1, c2, combinator);
            for (int i = 0; i < c1.Conditions.Length; i++)
            {
                SpecDynamicSwitchCaseOrCondition cond = c1.Conditions[i];
                if (cond.Case == null)
                    continue;
                
                // already checked they're equal with recursive ConditionsEqual
                queue.Enqueue((cond.Case, c2.Conditions[i].Case!));
            }
        }

        return anyUpdates;

        static bool Visit(SpecDynamicSwitchCaseValue c1, SpecDynamicSwitchCaseValue c2, Func<ISpecDynamicValue, ISpecDynamicValue, ISpecDynamicValue> combinator)
        {
            ISpecDynamicValue newValue = combinator(c1.Value, c2.Value);
            c1.Value = newValue;
            return true;
        }
    }

    public bool TryEvaluateMatchingSwitchCase(
        IReadOnlyList<SpecDynamicSwitchCaseValue>? previousCases,
        SpecDynamicSwitchCaseValue @case,
        [MaybeNullWhen(false)]
        out SpecDynamicSwitchCaseValue matchingCase)
    {
        // all in previousCases can be assumed false
        // all in case can be assumed true

        // with that in mind, find a case from this switch that is 100% valid
        // returns false if inconclusive

        // mainly for type switching:

        /*
         * Type (this):
         * switch ("Uniform_Scale".Included)
         * {
         *     case true:
         *          Type = Float32;
         *     case false:
         *          Type = Vector3;
         *     default:
         *          Type = Vector4;
         * }
         *
         * DefaultValue:
         * switch ("Uniform_Scale".Included)
         * {
         *     case true or 1 or 2: // can match
         *          DefaultValue = 0;
         *     case false and 1: // can not match
         *          DefaultValue = new Vector3(0, 0, 0);
         *     default:
         *          DefaultValue = new Vector4(0, 0, 0);
         * }
         */

        List<SpecCondition> falseConditions = new List<SpecCondition>();
        List<SpecCondition> trueConditions = new List<SpecCondition>();
        List<SpecCondition[]> trueConditionGroups = new List<SpecCondition[]>();
        List<SpecCondition[]> falseConditionGroups = new List<SpecCondition[]>();

        FigureOutConditions(previousCases, @case, falseConditions, trueConditions, trueConditionGroups, falseConditionGroups);

        // if no conditions have been inconclusive
        return EvaluateConditionIntl(
            out matchingCase,
            falseConditions, trueConditions, falseConditionGroups, trueConditionGroups
        );
    }

    private bool EvaluateConditionIntl(
        [MaybeNullWhen(false)]
        out SpecDynamicSwitchCaseValue matchingCase,
        List<SpecCondition> falseConditions,
        List<SpecCondition> trueConditions,
        List<SpecCondition[]> falseConditionGroups,
        List<SpecCondition[]> trueConditionGroups
        )
    {
        bool isSure = true;

        for (int i = 0; i < Cases.Length; ++i)
        {
            SpecDynamicSwitchCaseValue c = Cases[i];
            if (!c.HasConditions)
            {
                if (isSure)
                {
                    matchingCase = c;
                    return true;
                }

                break;
            }

            switch (c.Operation)
            {
                case SpecDynamicSwitchCaseOperation.When:
                    // condition must be true
                    bool? whenCheck = CheckCondition(new SpecDynamicSwitchCaseOrCondition(c.WhenCondition), trueConditions, falseConditions);
                    if (!whenCheck.HasValue)
                    {
                        isSure = false;
                    }
                    else if (isSure && whenCheck.Value && c.Value is SpecDynamicSwitchValue sw)
                    {
                        sw.EvaluateConditionIntl(out matchingCase, falseConditions,
                            trueConditions, falseConditionGroups, trueConditionGroups);
                    }

                    break;

                case SpecDynamicSwitchCaseOperation.And:
                    // must contain all cases from parent
                    if (c.Conditions.Length > 1)
                    {
                        if (ContainsGroup(falseConditionGroups, c.Conditions, true))
                            continue;
                    }

                    bool anyFalse = false;
                    for (int j = 0; j < c.Conditions.Length; ++j)
                    {
                        SpecDynamicSwitchCaseOrCondition cond1 = c.Conditions[j];
                        bool? check = CheckCondition(cond1, trueConditions, falseConditions);
                        if (!check.HasValue)
                        {
                            isSure = false;
                        }
                        else if (!check.Value)
                        {
                            anyFalse = true;
                            break;
                        }
                    }

                    if (!anyFalse && isSure)
                    {
                        matchingCase = c;
                        return true;
                    }

                    break;

                case SpecDynamicSwitchCaseOperation.Or:
                    // must contain at least one case from parent

                    // check group first
                    if (c.Conditions.Length > 1)
                    {
                        if (isSure && ContainsGroup(trueConditionGroups, c.Conditions, false))
                        {
                            matchingCase = c;
                            return true;
                        }
                    }

                    bool wasSure = isSure;
                    for (int j = 0; j < c.Conditions.Length; ++j)
                    {
                        SpecDynamicSwitchCaseOrCondition cond1 = c.Conditions[j];
                        bool? check = CheckCondition(cond1, trueConditions, falseConditions);
                        if (!check.HasValue)
                        {
                            isSure = false;
                        }
                        else if (check.Value)
                        {
                            if (wasSure)
                            {
                                matchingCase = c;
                                return true;
                            }
                        }
                    }

                    break;
            }
        }

        matchingCase = null;
        return false;

        static bool ContainsGroup(List<SpecCondition[]> groups, OneOrMore<SpecDynamicSwitchCaseOrCondition> conditions, bool falseGroup)
        {
            bool exists = true;
            if (falseGroup)
            {
                // all in conditions are in group
                foreach (SpecCondition[] group in groups)
                {
                    if (!conditions.All(x => x.Case == null && Array.IndexOf(group, x.Condition) >= 0))
                    {
                        exists = false;
                        break;
                    }
                }
            }
            else
            {
                // all in group are in conditions
                foreach (SpecCondition[] group in groups)
                {
                    if (!Array.TrueForAll(group, x => conditions.Contains(new SpecDynamicSwitchCaseOrCondition(x))))
                    {
                        exists = false;
                        break;
                    }
                }
            }

            return exists;
        }

        static bool? CheckCondition(SpecDynamicSwitchCaseOrCondition cond, List<SpecCondition> trueConditions, List<SpecCondition> falseConditions)
        {
            if (cond.IsNull || cond.Case != null)
                return null;

            if (trueConditions.Contains(cond.Condition))
                return true;

            return falseConditions.Contains(cond.Condition) ? false : null;
        }
    }

    private static void FigureOutConditions(
        IReadOnlyList<SpecDynamicSwitchCaseValue>? previousCases,
        SpecDynamicSwitchCaseValue @case,
        List<SpecCondition> falseConditions,
        List<SpecCondition> trueConditions,
        List<SpecCondition[]> trueConditionGroups,
        List<SpecCondition[]> falseConditionGroups)
    {
        GatherTrueConditions(@case, trueConditions, trueConditionGroups);

        foreach (SpecCondition tp in trueConditions)
        {
            if (!tp.TryGetOpposite(out SpecCondition fp))
                continue;

            if (!falseConditions.Contains(fp))
                falseConditions.Add(fp);
        }

        if (previousCases != null)
        {
            GatherFalseConditions(previousCases, falseConditions, falseConditionGroups);
            foreach (SpecCondition fp in falseConditions)
            {
                if (!fp.TryGetOpposite(out SpecCondition tp))
                    continue;

                if (!trueConditions.Contains(tp))
                    trueConditions.Add(tp);
            }
        }

        for (int grpIndex = trueConditionGroups.Count - 1; grpIndex >= 0; grpIndex--)
        {
            SpecCondition[] grp = trueConditionGroups[grpIndex];
            int trueConds = 0, falseConds = 0;
            int lastInconclusiveIndex = -1;
            for (int i = 0; i < grp.Length; ++i)
            {
                SpecCondition c = grp[i];
                if (trueConditions.Contains(c))
                    ++trueConds;
                else if (falseConditions.Contains(c))
                    ++falseConds;
                else if (c.TryGetOpposite(out SpecCondition opp))
                {
                    if (trueConditions.Contains(opp))
                        ++falseConds;
                    else if (falseConditions.Contains(opp))
                        ++trueConds;
                    else
                        lastInconclusiveIndex = i;
                }
                else
                    lastInconclusiveIndex = i;
            }

            if (trueConds == grp.Length || falseConds == grp.Length)
            {
                trueConditionGroups.RemoveAt(grpIndex);
            }
            else if (lastInconclusiveIndex != -1 && trueConds == 0 && falseConds == grp.Length - 1)
            {
                trueConditionGroups.RemoveAt(grpIndex);
                // all others are false, last one must be true
                SpecCondition trueCondition = grp[lastInconclusiveIndex];
                trueConditions.Add(trueCondition);
                if (trueCondition.TryGetOpposite(out SpecCondition falseCondition) && !falseConditions.Contains(falseCondition))
                    falseConditions.Add(falseCondition);
            }
        }
        for (int grpIndex = falseConditionGroups.Count - 1; grpIndex >= 0; grpIndex--)
        {
            SpecCondition[] grp = falseConditionGroups[grpIndex];
            int trueConds = 0, falseConds = 0;
            int lastInconclusiveIndex = -1;
            for (int i = 0; i < grp.Length; ++i)
            {
                SpecCondition c = grp[i];
                if (trueConditions.Contains(c))
                    ++trueConds;
                else if (falseConditions.Contains(c))
                    ++falseConds;
                else if (c.TryGetOpposite(out SpecCondition opp))
                {
                    if (trueConditions.Contains(opp))
                        ++falseConds;
                    else if (falseConditions.Contains(opp))
                        ++trueConds;
                    else
                        lastInconclusiveIndex = i;
                }
                else
                    lastInconclusiveIndex = i;
            }

            if (trueConds == grp.Length || falseConds == grp.Length)
            {
                falseConditionGroups.RemoveAt(grpIndex);
            }
            else if (lastInconclusiveIndex != -1 && falseConds == 0 && trueConds == grp.Length - 1)
            {
                falseConditionGroups.RemoveAt(grpIndex);
                // all others are true, last one must be false
                SpecCondition falseCondition = grp[lastInconclusiveIndex];
                falseConditions.Add(falseCondition);
                if (falseCondition.TryGetOpposite(out SpecCondition trueCondition) && !trueConditions.Contains(trueCondition))
                    trueConditions.Add(trueCondition);
            }
        }
    }

    private static void GatherFalseConditions(
        IReadOnlyList<SpecDynamicSwitchCaseValue> previousCases,
        List<SpecCondition> falseConditions,
        List<SpecCondition[]> falseConditionGroups)
    {
        foreach (SpecDynamicSwitchCaseValue falseCase in previousCases)
        {
            if (falseCase.Operation == SpecDynamicSwitchCaseOperation.When)
            {
                falseConditions.Add(falseCase.WhenCondition);
                continue;
            }

            // shouldn't ever really get to this
            if (!falseCase.HasConditions)
                continue;

            if (falseCase.Operation == SpecDynamicSwitchCaseOperation.Or || falseCase.Conditions.Length == 1)
            {
                foreach (SpecDynamicSwitchCaseOrCondition condition in falseCase.Conditions)
                {
                    if (condition.Case == null)
                    {
                        if (!falseConditions.Contains(condition.Condition))
                            falseConditions.Add(condition.Condition);
                    }
                }
            }
            else if (falseCase.Conditions.All(x => x.Case == null))
            {
                SpecCondition[] falseConditionGroup = new SpecCondition[falseCase.Conditions.Length];
                for (int i = 0; i < falseCase.Conditions.Length; ++i)
                    falseConditionGroup[i] = falseCase.Conditions[i].Condition;

                bool exists = false;
                foreach (SpecCondition[] arr in falseConditionGroups)
                {
                    if (!arr.SequenceEqual(falseConditionGroup))
                        continue;

                    exists = true;
                    break;
                }

                if (!exists)
                    falseConditionGroups.Add(falseConditionGroup);
            }
        }
    }

    private static void GatherTrueConditions(SpecDynamicSwitchCaseValue trueCase, List<SpecCondition> trueConditions, List<SpecCondition[]> trueConditionGroups)
    {
        if (trueCase.Operation == SpecDynamicSwitchCaseOperation.When)
        {
            trueConditions.Add(trueCase.WhenCondition);
            return;
        }

        if (!trueCase.HasConditions)
            return;

        if (trueCase.Operation == SpecDynamicSwitchCaseOperation.And || trueCase.Conditions.Length == 1)
        {
            foreach (SpecDynamicSwitchCaseOrCondition condition in trueCase.Conditions)
            {
                if (condition.Case == null)
                {
                    if (!trueConditions.Contains(condition.Condition))
                        trueConditions.Add(condition.Condition);
                }
            }
        }
        else if (trueCase.Conditions.All(x => x.Case == null))
        {
            SpecCondition[] trueConditionGroup = new SpecCondition[trueCase.Conditions.Length];
            for (int i = 0; i < trueCase.Conditions.Length; ++i)
                trueConditionGroup[i] = trueCase.Conditions[i].Condition;

            bool exists = false;
            foreach (SpecCondition[] arr in trueConditionGroups)
            {
                if (!arr.SequenceEqual(trueConditionGroup))
                    continue;

                exists = true;
                break;
            }

            if (!exists)
                trueConditionGroups.Add(trueConditionGroup);
        }
    }

    public bool EvaluateCondition(in FileEvaluationContext ctx, in SpecCondition condition)
    {
        SpecDynamicSwitchCaseValue? match = TryMatchCase(in ctx);
        if (match == null)
        {
            return condition.EvaluateNulls(true, condition.Comparand == null);
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

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        SpecDynamicSwitchCaseValue? match = TryMatchCase(in ctx);
        if (match != null)
            return match.Value.TryEvaluateValue(in ctx, out value);

        value = null;
        return false;
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

/// <summary>
/// A case of a <see cref="SpecDynamicSwitchValue"/>. Can also be used as a standalone value.
/// </summary>
public sealed class SpecDynamicSwitchCaseValue : ISpecDynamicValue, IEquatable<ISpecDynamicValue>, IEquatable<SpecDynamicSwitchCaseValue>
{
    public ISpecPropertyType? ValueType { get; }

    public OneOrMore<SpecDynamicSwitchCaseOrCondition> Conditions { get; internal set; }
    public SpecCondition WhenCondition { get; internal set; }
    public SpecDynamicSwitchCaseOperation Operation { get; }
    public ISpecDynamicValue Value { get; internal set; }

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

    public bool TryEvaluateValue(in FileEvaluationContext ctx, out object? value)
    {
        return Value.TryEvaluateValue(in ctx, out value);
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

    internal bool ConditionsEqual(SpecDynamicSwitchCaseValue other)
    {
        if (!HasConditions)
            return WhenCondition.Equals(other.WhenCondition);

        if (!other.HasConditions)
            return false;

        OneOrMore<SpecDynamicSwitchCaseOrCondition> c1 = Conditions, c2 = other.Conditions;
        if (c1.Length != c2.Length)
            return false;

        for (int i = 0; i < c1.Length; i++)
        {
            SpecDynamicSwitchCaseOrCondition cc1 = c1[i];
            SpecDynamicSwitchCaseOrCondition cc2 = c2[i];
            if (cc1.Case == null)
            {
                if (cc2.Case != null || !cc1.Condition.Equals(cc2.Condition))
                    return false;
            }
            else
            {
                if (cc2.Case == null || !cc1.Case.ConditionsEqual(cc2.Case))
                    return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is SpecDynamicSwitchCaseValue v && Equals(v);
    public override string ToString() => Conditions.IsSingle ? $"{Operation} Case [1 case]" : $"{Operation} Case [{Conditions.Length} cases]";
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

/// <summary>
/// An operation of a <see cref="SpecDynamicSwitchCaseValue"/> condition list.
/// </summary>
public enum SpecDynamicSwitchCaseOperation
{
    /// <summary>
    /// All conditions or cases must be <see langword="true"/>.
    /// </summary>
    And,

    /// <summary>
    /// At least one condition or case must be <see langword="true"/>.
    /// </summary>
    Or,

    /// <summary>
    /// Defines one condition which, when <see langword="true"/>, will be resolved to a nested switch statement.
    /// </summary>
    When
}

/// <summary>
/// A nested case or <see cref="SpecCondition"/> as a condition of a <see cref="SpecDynamicSwitchCaseValue"/>.
/// </summary>
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