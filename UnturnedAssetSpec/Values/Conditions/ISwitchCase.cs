using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A weakly-typed case within a switch value or another case.
/// </summary>
public interface ISwitchCase : IValue
{
    /// <summary>
    /// The value this case will evaluate to. This may be a <see cref="SwitchValue"/> or another value.
    /// </summary>
    IValue Value { get; }

    /// <summary>
    /// Checks whether or not this case's value can be used.
    /// </summary>
    /// <param name="doesPassConditions">Whether or not this case's value can be used.</param>
    /// <returns>Whether or not the result of the conditions was able to be determined successfully.</returns>
    bool TryCheckConditionsConcrete(out bool doesPassConditions);

    /// <summary>
    /// Checks whether or not this case's value can be used with context.
    /// </summary>
    /// <param name="ctx">Evaluation context.</param>
    /// <param name="doesPassConditions">Whether or not this case's value can be used.</param>
    /// <returns>Whether or not the result of the conditions was able to be determined successfully.</returns>
    bool TryCheckConditions(in FileEvaluationContext ctx, out bool doesPassConditions);
}

/// <summary>
/// A strongly-typed case within a switch value or another case.
/// </summary>
public interface ISwitchCase<TResult> : ISwitchCase, IValue<TResult>
    where TResult : IEquatable<TResult>
{
    /// <summary>
    /// The value this case will evaluate to. This may be a <see cref="SwitchValue{TResult}"/> or another value.
    /// </summary>
    new IValue<TResult> Value { get; }
}

internal static class SwitchCase
{
    /// <summary>
    /// Read a typed switch case.
    /// </summary>
    /// <param name="switchType">The type of value to read.</param>
    /// <param name="root">The JSON object of the case.</param>
    /// <returns>The case, if it was successfully parsed.</returns>
    public static ISwitchCase<TResult>? TryReadSwitchCase<TResult>(IType<TResult> switchType, IAssetSpecDatabase database, DatProperty owner, in JsonElement root)
        where TResult : IEquatable<TResult>
    {
        IValue<TResult>? value;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return switchType.TryReadFromJson(in root, database, owner, out value)
                ? new DefaultSwitchCase<TResult>(value)
                : null;
        }

        if (root.TryGetProperty("Cases"u8, out JsonElement element))
        {
            if (!SwitchValue.TryRead(in element, switchType, database, owner, out SwitchValue<TResult>? switchValue))
                return null;

            value = switchValue;
        }
        else if (root.TryGetProperty("Value"u8, out element))
        {
            value = Value.TryReadValueFromJson(in element, ValueReadOptions.Default, switchType, database, owner);
            if (value == null)
                return null;
        }
        else
        {
            return null;
        }

        if (root.TryGetProperty("When"u8, out element))
        {
            ConditionToConditionalSwitchCaseVisitor<TResult> visitor;
            visitor.Case = null;
            visitor.Value = value;
            if (!Conditions.TryReadConditionFromJson(in element, database, owner, ref visitor) || visitor.Case == null)
                return null;

            return visitor.Case;
        }

        if (root.TryGetProperty("Case"u8, out element))
        {
            if (!Conditions.TryReadConditionFromJson(in element, database, owner, out IValue<bool>? condition))
                return null;

            return new ComplexConditionalSwitchCase<TResult>(ImmutableArray.Create(condition), JointConditionOperation.Or, value);
        }

        bool isAnd = root.TryGetProperty("And"u8, out element);
        if (isAnd || root.TryGetProperty("Or"u8, out element))
        {
            int cases = element.GetArrayLength();
            if (cases <= 0)
                return null;

            ImmutableArray<IValue<bool>>.Builder bldr = ImmutableArray.CreateBuilder<IValue<bool>>(cases);
            for (int i = 0; i < cases; ++i)
            {
                JsonElement item = element[i];
                if (!Conditions.TryReadConditionFromJson(in item, database, owner, out IValue<bool>? condition))
                    return null;

                bldr.Add(condition);
            }

            return new ComplexConditionalSwitchCase<TResult>(
                bldr.MoveToImmutable(),
                isAnd ? JointConditionOperation.And : JointConditionOperation.Or,
                value
            );
        }

        return new DefaultSwitchCase<TResult>(value);
    }

    private struct ConditionToConditionalSwitchCaseVisitor<TResult> : Conditions.IConditionVisitor
        where TResult : IEquatable<TResult>
    {
        public ISwitchCase<TResult>? Case;
        public IValue<TResult> Value;

        public void Accept<TComparand>(in Condition<TComparand> condition) where TComparand : IEquatable<TComparand>
        {
            Case = new ConditionalSwitchCase<TResult, TComparand>(condition, Value);
        }
    }

    /// <summary>
    /// Read an un-typed switch case.
    /// </summary>
    /// <param name="switchType">If <see langword="null"/>, indicates that the value should be skipped, otherwise the type of value to read.</param>
    /// <param name="root">The JSON object of the case.</param>
    /// <returns>The case, if it was successfully parsed.</returns>
    internal static unsafe ISwitchCase? TryReadSwitchCase(IType? switchType, IAssetSpecDatabase database, DatProperty owner, in JsonElement root)
    {
        if (switchType != null)
        {
            CreateTypedSwitchCaseVisitor v;
            v.Visited = false;
            v.Created = null;
            v.Database = database;
            v.Owner = owner;
            fixed (JsonElement* elementPtr = &root)
            {
                v.Element = elementPtr;
                switchType.Visit(ref v);
            }

            if (v.Visited)
                return v.Created;
        }

        IValue value;
        JsonElement element;
        if (switchType != null)
        {
            if (root.TryGetProperty("Cases"u8, out element))
            {
                if (!SwitchValue.TryRead(in element, switchType, database, owner, out SwitchValue? switchValue))
                    return null;

                value = switchValue;
            }
            else
            {
                return null;
            }
        }
        else
        {
            value = NullValue.Instance;
        }

        if (root.TryGetProperty("When"u8, out element))
        {
            ConditionToConditionalSwitchCaseVisitor visitor;
            visitor.Case = null;
            visitor.Value = value;
            if (!Conditions.TryReadConditionFromJson(in element, database, owner, ref visitor) || visitor.Case == null)
                return null;

            return visitor.Case;
        }

        if (root.TryGetProperty("Case"u8, out element))
        {
            if (!Conditions.TryReadConditionFromJson(in element, database, owner, out IValue<bool>? condition))
                return null;

            return new ComplexConditionalSwitchCase(ImmutableArray.Create(condition), JointConditionOperation.Or, value);
        }

        bool isAnd = root.TryGetProperty("And"u8, out element);
        if (isAnd || root.TryGetProperty("Or"u8, out element))
        {
            int cases = element.GetArrayLength();
            if (cases <= 0)
                return null;

            ImmutableArray<IValue<bool>>.Builder bldr = ImmutableArray.CreateBuilder<IValue<bool>>(cases);
            for (int i = 0; i < cases; ++i)
            {
                JsonElement item = element[i];
                if (!Conditions.TryReadConditionFromJson(in item, database, owner, out IValue<bool>? condition))
                    return null;

                bldr.Add(condition);
            }

            return new ComplexConditionalSwitchCase(
                bldr.MoveToImmutable(),
                isAnd ? JointConditionOperation.And : JointConditionOperation.Or,
                value
            );
        }

        return new DefaultSwitchCase(value);
    }

    private unsafe struct CreateTypedSwitchCaseVisitor : ITypeVisitor
    {
        public ISwitchCase? Created;
        public bool Visited;
        public JsonElement* Element;
        public IAssetSpecDatabase Database;
        public DatProperty Owner;
        public void Accept<TValue>(IType<TValue> type) where TValue : IEquatable<TValue>
        {
            Visited = true;
            Created = TryReadSwitchCase(type, Database, Owner, in Unsafe.AsRef<JsonElement>(Element));
        }
    }

    private struct ConditionToConditionalSwitchCaseVisitor : Conditions.IConditionVisitor
    {
        public ISwitchCase? Case;
        public IValue Value;

        public void Accept<TComparand>(in Condition<TComparand> condition) where TComparand : IEquatable<TComparand>
        {
            Case = new ConditionalSwitchCase<TComparand>(condition, Value);
        }
    }
}