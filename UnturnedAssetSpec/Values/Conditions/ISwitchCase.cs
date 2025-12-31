using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.Text.Json;

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
    public static ISwitchCase<TResult>? TryReadSwitchCase<TResult>(IType<TResult> switchType, in JsonElement root)
        where TResult : IEquatable<TResult>
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            if (switchType.Parser.TryReadValueFromJson(in root, out Optional<TResult> implicitValue, switchType))
            {
                return new DefaultSwitchCase<TResult>(
                    implicitValue.HasValue
                        ? Values.Create(implicitValue.Value, switchType)
                        : Values.Null(switchType)
                );
            }

            return null;
        }

        IValue<TResult> value;
        if (root.TryGetProperty("Cases"u8, out JsonElement element))
        {
            if (!SwitchValue.TryRead(in element, switchType, out SwitchValue<TResult>? switchValue))
                return null;

            value = switchValue;
        }
        else if (root.TryGetProperty("Value"u8, out element))
        {
            if (TypeConverters.TryGet<TResult>() is { } typeConverter)
            {
                TypeConverterParseArgs<TResult> args = default;
                args.Type = switchType;

                if (!typeConverter.TryReadJson(in element, out Optional<TResult> optionalValue, ref args))
                    return null;

                value = args.Type.CreateValue(optionalValue);
            }
            else
            {
                return null;
            }
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
            if (!Conditions.TryReadConditionFromJson(in element, ref visitor) || visitor.Case == null)
                return null;

            return visitor.Case;
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
                if (Conditions.TryReadConditionFromJson(in element) is not { } condition)
                    return null;

                bldr.Add(condition);
            }

            return new ComplexConditionalSwitchCase<TResult>(
                bldr.MoveToImmutable(),
                isAnd ? SpecDynamicSwitchCaseOperation.And : SpecDynamicSwitchCaseOperation.Or,
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

        public void Accept<TComparand>(Condition<TComparand> condition) where TComparand : IEquatable<TComparand>
        {
            Case = new ConditionalSwitchCase<TResult, TComparand>(condition, Value);
        }
    }
}