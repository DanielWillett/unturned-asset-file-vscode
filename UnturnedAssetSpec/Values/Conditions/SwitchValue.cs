using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed switch value that can be used alongside type switches.
/// </summary>
/// <remarks>When possible use <see cref="SwitchValue{TResult}"/> instead.</remarks>
public class SwitchValue : IValue, IEquatable<SwitchValue?>
{
    private int? _hashCode;

    /// <summary>
    /// All cases in this switch value.
    /// </summary>
    public ImmutableArray<ISwitchCase> Cases { get; }

    public SwitchValue(ImmutableArray<ISwitchCase> cases)
    {
        Cases = cases.IsDefault ? ImmutableArray<ISwitchCase>.Empty : cases;
    }

    /// <summary>
    /// Attempts to read a switch value from a JSON array.
    /// </summary>
    public static bool TryRead<TResult>(in JsonElement element, IType<TResult> type, IAssetSpecDatabase database, IDatSpecificationObject owner, [NotNullWhen(true)] out SwitchValue<TResult>? value)
        where TResult : IEquatable<TResult>
    {
        value = null;
        if (element.ValueKind != JsonValueKind.Array)
            return false;

        int cases = element.GetArrayLength();
        if (cases <= 0)
            return false;

        ImmutableArray<ISwitchCase<TResult>>.Builder bldr = ImmutableArray.CreateBuilder<ISwitchCase<TResult>>(cases);
        for (int i = 0; i < cases; ++i)
        {
            JsonElement obj = element[i];
            if (SwitchCase.TryReadSwitchCase(type, database, owner, in obj) is not { } sc)
            {
                for (int j = 0; j < i; ++j)
                {
                    if (bldr[j] is IDisposable d)
                        d.Dispose();
                }

                return false;
            }

            bldr.Add(sc);
        }

        value = new SwitchValue<TResult>(type, bldr.MoveToImmutable());
        return true;
    }

    /// <summary>
    /// Attempts to read an untyped switch value from a JSON array.
    /// </summary>
    public static unsafe bool TryRead(in JsonElement element, IPropertyType type, IAssetSpecDatabase database, IDatSpecificationObject owner, [NotNullWhen(true)] out SwitchValue? value)
    {
        if (type.TryGetConcreteType(out IType? actualType))
        {
            CreateTypedSwitchValueVisitor v;
            v.Visited = false;
            v.Created = null;
            v.Database = database;
            v.Owner = owner;
            fixed (JsonElement* elementPtr = &element)
            {
                v.Element = elementPtr;
                actualType.Visit(ref v);
            }

            if (v.Visited)
            {
                value = v.Created;
                return v.Created != null;
            }
        }

        value = null;
        if (element.ValueKind != JsonValueKind.Array)
            return false;

        int cases = element.GetArrayLength();
        if (cases <= 0)
            return false;

        SwitchValue? typeSwitch = type as SwitchValue;
        if (typeSwitch == null && actualType == null)
            return false;

        ImmutableArray<ISwitchCase>.Builder bldr = ImmutableArray.CreateBuilder<ISwitchCase>(cases);
        for (int i = 0; i < cases; ++i)
        {
            JsonElement item = element[i];

            ISwitchCase? sc = SwitchCase.TryReadSwitchCase(actualType, database, owner, in item);
            if (sc == null)
                return false;

            if (typeSwitch != null)
            {
                if (!typeSwitch.TryEvaluateMatchingSwitchCase(bldr, sc, out ISwitchCase? matchingCase)
                    || matchingCase.Value is not IValue<IType> typeOfValue)
                {
                    return false;
                }

                sc = SwitchCase.TryReadSwitchCase(typeOfValue.Type, database, owner, in item);
                if (sc == null)
                    return false;
            }

            bldr.Add(sc);
        }

        value = new SwitchValue(bldr.MoveToImmutable());
        return true;
    }

    private unsafe struct CreateTypedSwitchValueVisitor : ITypeVisitor
    {
        public SwitchValue? Created;
        public bool Visited;
        public JsonElement* Element;
        public IAssetSpecDatabase Database;
        public IDatSpecificationObject Owner;
        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            Visited = true;
            if (TryRead(in Unsafe.AsRef<JsonElement>(Element), type, Database, Owner, out SwitchValue<TValue>? value))
            {
                Created = value;
            }
        }
    }

    /// <summary>
    /// Tries to identify which case from this <see cref="SwitchValue"/> would be matched given that <paramref name="case"/> was matched in another <see cref="SwitchValue"/> and <paramref name="previousCases"/> were not matched..
    /// </summary>
    /// <param name="previousCases">Previous cases that wouldve have already evaluated to <see langword="false"/> by this point.</param>
    /// <param name="case">The case being matched against.</param>
    /// <param name="matchingCase">The case from this <see cref="SwitchValue"/> that is always chosen when <paramref name="case"/> is chosen.</param>
    /// <returns>Whether or not a case could be reliably identified.</returns>
    public bool TryEvaluateMatchingSwitchCase(IReadOnlyList<ISwitchCase>? previousCases, ISwitchCase @case, [NotNullWhen(true)] out ISwitchCase? matchingCase)
    {
        // all in previousCases can be assumed false
        // all in case can be assumed true

        // with that in mind, find a case from this switch that is 100% valid
        // returns false if inconclusive

        // mainly for type switching:

        /*
         *
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
         *
         */

        HashSet<IValue<bool>> trueConditions = new HashSet<IValue<bool>>(EqualityComparer<IValue>.Default);
        HashSet<IValue<bool>> falseConditions = new HashSet<IValue<bool>>(EqualityComparer<IValue>.Default);
        List<IValue<bool>[]> trueConditionGroups = new List<IValue<bool>[]>();
        List<IValue<bool>[]> falseConditionGroups = new List<IValue<bool>[]>();

        FigureOutConditions(previousCases, @case, falseConditions, trueConditions, trueConditionGroups, falseConditionGroups);

        // if no conditions have been inconclusive
        return EvaluateConditionIntl(
            out matchingCase,
            falseConditions, trueConditions, falseConditionGroups, trueConditionGroups
        );
    }

    private bool EvaluateConditionIntl(
        [NotNullWhen(true)]
        out ISwitchCase? matchingCase,
        HashSet<IValue<bool>> falseConditions,
        HashSet<IValue<bool>> trueConditions,
        List<IValue<bool>[]> falseConditionGroups,
        List<IValue<bool>[]> trueConditionGroups)
    {
        bool isSure = true;

        for (int i = 0; i < Cases.Length; ++i)
        {
            ISwitchCase c = Cases[i];
            if (c.TryCheckConditionsConcrete(out bool doesPassConditions) && doesPassConditions)
            {
                if (isSure)
                {
                    matchingCase = c;
                    return true;
                }

                break;
            }

            switch (c)
            {
                // When conditional
                case IConditionalSwitchCase conditional:
                    // condition must be true
                    bool? whenCheck = CheckCondition(conditional.Condition, trueConditions, falseConditions);
                    if (!whenCheck.HasValue)
                    {
                        isSure = false;
                    }
                    else if (isSure && whenCheck.Value && c.Value is SwitchValue sw)
                    {
                        sw.EvaluateConditionIntl(
                            out matchingCase,
                            falseConditions,
                            trueConditions,
                            falseConditionGroups,
                            trueConditionGroups
                        );
                    }

                    break;

                case ComplexConditionalSwitchCase complexConditional:

                    ImmutableArray<IValue<bool>> conditions = complexConditional.Conditions;

                    switch (complexConditional.Operation)
                    {
                        case JointConditionOperation.And:
                            // must contain all cases from parent
                            if (conditions.Length > 1)
                            {
                                if (ContainsGroup(falseConditionGroups, conditions, true))
                                    continue;
                            }

                            bool anyFalse = false;
                            for (int j = 0; j < conditions.Length; ++j)
                            {
                                IValue<bool> cond1 = conditions[j];
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

                        case JointConditionOperation.Or:
                            // must contain at least one case from parent

                            // check group first
                            if (conditions.Length > 1)
                            {
                                if (isSure && ContainsGroup(trueConditionGroups, conditions, false))
                                {
                                    matchingCase = c;
                                    return true;
                                }
                            }

                            bool wasSure = isSure;
                            for (int j = 0; j < conditions.Length; ++j)
                            {
                                IValue<bool> cond1 = conditions[j];
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

                    break;
            }
        }

        matchingCase = null;
        return false;

        static bool ContainsGroup(List<IValue<bool>[]> groups, ImmutableArray<IValue<bool>> conditions, bool falseGroup)
        {
            bool exists = true;
            if (falseGroup)
            {
                // all in conditions are in group
                foreach (IValue<bool>[] group in groups)
                {
                    bool all = true;
                    foreach (IValue<bool> x in conditions)
                    {
                        if (Array.IndexOf(group, x) >= 0)
                            continue;

                        all = false;
                        break;
                    }

                    if (all)
                        continue;

                    exists = false;
                    break;
                }
            }
            else
            {
                // all in group are in conditions
                foreach (IValue<bool>[] group in groups)
                {
                    bool all = true;
                    foreach (IValue<bool> x in group)
                    {
                        bool any = false;
                        foreach (IValue<bool> v in conditions)
                        {
                            if (!v.Equals(x))
                                continue;

                            any = true;
                            break;
                        }

                        if (any)
                            continue;

                        all = false;
                        break;
                    }

                    if (all)
                        continue;

                    exists = false;
                    break;
                }
            }

            return exists;
        }

        static bool? CheckCondition(IValue<bool> cond, HashSet<IValue<bool>> trueConditions, HashSet<IValue<bool>> falseConditions)
        {
            if (trueConditions.Contains(cond))
                return true;

            if (falseConditions.Contains(cond))
                return false;

            return null;
        }
    }

    private static void FigureOutConditions(
        IReadOnlyList<ISwitchCase>? previousCases,
        ISwitchCase @case,
        HashSet<IValue<bool>> falseConditions,
        HashSet<IValue<bool>> trueConditions,
        List<IValue<bool>[]> trueConditionGroups,
        List<IValue<bool>[]> falseConditionGroups)
    {
        GatherTrueConditions(@case, trueConditions, trueConditionGroups);

        foreach (IValue<bool> tp in trueConditions)
        {
            if (tp is not ICondition c)
                continue;

            falseConditions.Add(c.GetOpposite());
        }

        if (previousCases != null)
        {
            GatherFalseConditions(previousCases, falseConditions, falseConditionGroups);
            foreach (IValue<bool> fp in falseConditions)
            {
                if (fp is not ICondition c)
                    continue;

                trueConditions.Add(c.GetOpposite());
            }
        }

        for (int grpIndex = trueConditionGroups.Count - 1; grpIndex >= 0; grpIndex--)
        {
            IValue<bool>[] grp = trueConditionGroups[grpIndex];
            int trueConds = 0, falseConds = 0;
            int lastInconclusiveIndex = -1;
            for (int i = 0; i < grp.Length; ++i)
            {
                IValue<bool> c = grp[i];
                if (trueConditions.Contains(c))
                    ++trueConds;
                else if (falseConditions.Contains(c))
                    ++falseConds;
                else if (c is ICondition cond)
                {
                    ICondition opp = cond.GetOpposite();
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
                IValue<bool> trueCondition = grp[lastInconclusiveIndex];
                trueConditions.Add(trueCondition);
                if (trueCondition is ICondition c)
                    falseConditions.Add(c.GetOpposite());
            }
        }
        for (int grpIndex = falseConditionGroups.Count - 1; grpIndex >= 0; grpIndex--)
        {
            IValue<bool>[] grp = falseConditionGroups[grpIndex];
            int trueConds = 0, falseConds = 0;
            int lastInconclusiveIndex = -1;
            for (int i = 0; i < grp.Length; ++i)
            {
                IValue<bool> c = grp[i];
                if (trueConditions.Contains(c))
                    ++trueConds;
                else if (falseConditions.Contains(c))
                    ++falseConds;
                else if (c is ICondition cond)
                {
                    ICondition opp = cond.GetOpposite();
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
                IValue<bool> falseCondition = grp[lastInconclusiveIndex];
                falseConditions.Add(falseCondition);
                if (falseCondition is ICondition c)
                    trueConditions.Add(c.GetOpposite());
            }
        }
    }

    private static void GatherFalseConditions(
        IReadOnlyList<ISwitchCase> previousCases,
        HashSet<IValue<bool>> falseConditions,
        List<IValue<bool>[]> falseConditionGroups)
    {
        foreach (ISwitchCase falseCase in previousCases)
        {
            if (falseCase is IConditionalSwitchCase c)
            {
                falseConditions.Add(c.Condition);
                return;
            }

            // shouldn't ever really get to this
            if (falseCase is not ComplexConditionalValue falseConditional)
                continue;

            ImmutableArray<IValue<bool>> conditions = falseConditional.Conditions;
            if (falseConditional.Operation == JointConditionOperation.Or || conditions.Length == 1)
            {
                foreach (IValue<bool> condition in conditions)
                {
                    falseConditions.Add(condition);
                }
            }
            else
            {
                IValue<bool>[] falseConditionGroup = conditions.UnsafeThaw();

                bool exists = false;
                foreach (IValue<bool>[] arr in falseConditionGroups)
                {

                    if (!CollectionHelper.ContainsSameElements(arr, falseConditionGroup))
                        continue;

                    exists = true;
                    break;
                }

                if (!exists)
                    falseConditionGroups.Add(falseConditionGroup);
            }
        }
    }

    private static void GatherTrueConditions(ISwitchCase trueCase, HashSet<IValue<bool>> trueConditions, List<IValue<bool>[]> trueConditionGroups)
    {
        if (trueCase is IConditionalSwitchCase c)
        {
            trueConditions.Add(c.Condition);
            return;
        }

        if (trueCase is not ComplexConditionalSwitchCase conditional)
            return;

        ImmutableArray<IValue<bool>> conditions = conditional.Conditions;
        if (conditional.Operation == JointConditionOperation.And || conditions.Length == 1)
        {
            foreach (IValue<bool> condition in conditions)
            {
                trueConditions.Add(condition);
            }
        }
        else
        {
            IValue<bool>[] trueConditionGroup = conditions.UnsafeThaw();

            bool exists = false;
            foreach (IValue<bool>[] arr in trueConditionGroups)
            {
                if (!CollectionHelper.ContainsSameElements(arr, trueConditionGroup))
                    continue;

                exists = true;
                break;
            }

            if (!exists)
                trueConditionGroups.Add(trueConditionGroup);
        }
    }


    /// <inheritdoc />
    public bool VisitConcreteValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        foreach (ISwitchCase c in Cases)
        {
            if (!c.TryCheckConditionsConcrete(out bool doesPassConditions))
                return false;

            if (!doesPassConditions)
                continue;

            c.Value.VisitConcreteValue(ref visitor);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        foreach (ISwitchCase c in Cases)
        {
            if (!c.TryCheckConditions(in ctx, out bool doesPassConditions))
                return false;

            if (!doesPassConditions)
                continue;

            c.Value.VisitValue(ref visitor, in ctx);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (ISwitchCase c in Cases)
        {
            c.WriteToJson(writer, options);
        }
        writer.WriteEndArray();
    }

    /// <inheritdoc />
    public bool Equals(IValue? other)
    {
        return Equals(other as SwitchValue);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as SwitchValue);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // ReSharper disable NonReadonlyMemberInGetHashCode
        if (_hashCode.HasValue)
            return _hashCode.Value;

        HashCode hc = new HashCode();
        hc.Add(388121854);
        foreach (ISwitchCase c in Cases)
        {
            hc.Add(c);
        }

        int code = hc.ToHashCode();
        _hashCode = code;
        return code;
        // ReSharper restore NonReadonlyMemberInGetHashCode
    }

    /// <inheritdoc />
    public virtual bool Equals(SwitchValue? other)
    {
        if (other == null)
            return false;
        if ((object)other == this)
            return true;

        ImmutableArray<ISwitchCase> otherCases = other.Cases;
        ImmutableArray<ISwitchCase> thisCases = Cases;
        if (otherCases.Length != thisCases.Length)
            return false;

        for (int i = 0; i < thisCases.Length; ++i)
        {
            if (!thisCases[i].Equals(otherCases[i]))
                return false;
        }

        return true;
    }

    bool IValue.IsNull => false;
}

/// <summary>
/// A strongly-typed switch value.
/// </summary>
/// <typeparam name="TResult">The type of value that all cases will evaluate to.</typeparam>
public class SwitchValue<TResult> : SwitchValue, IValue<TResult>, IEquatable<SwitchValue<TResult>?>
    where TResult : IEquatable<TResult>
{
    /// <summary>
    /// The type of value that all cases will evalate to.
    /// </summary>
    public IType<TResult> Type { get; }

    public SwitchValue(IType<TResult> type, ImmutableArray<ISwitchCase<TResult>> cases)
        : base(cases.UnsafeConvert<ISwitchCase<TResult>, ISwitchCase>())
    {
        Type = type;
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<TResult> value)
    {
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (ISwitchCase<TResult> c in Cases)
        {
            if (!c.TryCheckConditionsConcrete(out bool doesPassConditions))
            {
                value = Optional<TResult>.Null;
                return false;
            }

            if (!doesPassConditions)
                continue;

            return c.Value.TryGetConcreteValue(out value);
        }

        value = Optional<TResult>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<TResult> value, in FileEvaluationContext ctx)
    {
        // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
        foreach (ISwitchCase<TResult> c in Cases)
        {
            if (!c.TryCheckConditions(in ctx, out bool doesPassConditions))
            {
                value = Optional<TResult>.Null;
                return false;
            }

            if (!doesPassConditions)
                continue;

            return c.Value.TryEvaluateValue(out value, in ctx);
        }

        value = Optional<TResult>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(SwitchValue<TResult>? other)
    {
        return (object)this == other || (other != null && Type.Equals(other.Type) && base.Equals(other));
    }

    /// <inheritdoc />
    public override bool Equals(SwitchValue? other)
    {
        if ((object?)other == this)
            return true;

        if (other is not SwitchValue<TResult> r || !Type.Equals(r.Type))
            return false;

        return base.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, base.GetHashCode());
    }
}