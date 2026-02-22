using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable InconsistentNaming

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Inclusion condition that states that the given property should exist if this property exists.
/// </summary>
public interface IInclusionCondition : IInclusionExclusionCondition
{
    /// <summary>
    /// The condition that needs to be met for this condition to be checked.
    /// </summary>
    IValue<bool>? FilterCondition { get; }

    /// <summary>
    /// The condition that needs to be met in addition to the property being included.
    /// </summary>
    IValue<bool>? RequirementCondition { get; }

    /// <summary>
    /// Attempts to evaluate the requested condition, if it exists.
    /// </summary>
    /// <param name="result">The result of the condition evaluation.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <exception cref="InvalidEnumArgumentException">Invalid value for <paramref name="condition"/>.</exception>
    /// <returns>Whether or not the condition could be evaluated. If there is not a condition a result of <see langword="true"/> will be returned.</returns>
    bool TryEvaluateCondition(InclusionCondition.ConditionBehavior condition, out bool result, in FileEvaluationContext ctx);
}

/// <summary>
/// Inclusion condition that states that the given property should exist with a given value if this property exists
/// </summary>
public interface IInclusionCondition<TValue> : IInclusionCondition, IInclusionExclusionCondition<TValue>
    where TValue : IEquatable<TValue>;

/// <inheritdoc cref="IInclusionCondition"/>
/// <remarks>Create using the <see cref="Create(in PropertyReference)"/> method and it's overloads.</remarks>
public class InclusionCondition : IInclusionCondition
{
    /// <summary>
    /// Determines how a condition behaves when evaluating an <see cref="InclusionCondition"/>.
    /// </summary>
    public enum ConditionBehavior
    {
        /// <summary>
        /// Condition which decides whether or not this rule is applied.
        /// </summary>
        Filter,

        /// <summary>
        /// Condition that must be met for it to be considered 'included'.
        /// </summary>
        Requirement
    }

    protected PropertyReference _PropertyReference;

    /// <inheritdoc />
    public ref readonly PropertyReference PropertyReference => ref _PropertyReference;

    /// <inheritdoc />
    public virtual bool IsAnyValue => true;

    /// <inheritdoc />
    public IValue<bool>? FilterCondition { get; }

    /// <inheritdoc />
    public IValue<bool>? RequirementCondition { get; }

    protected InclusionCondition(in PropertyReference pRef, IValue<bool>? filterCondition, IValue<bool>? requirementCondition)
    {
        _PropertyReference = pRef;
        FilterCondition = filterCondition;
        RequirementCondition = requirementCondition;
    }

    /// <summary>
    /// Creates an inclusion condition that requires that a property exists.
    /// </summary>
    /// <param name="propertyReference">A reference to the property being included.</param>
    public static IInclusionCondition Create(in PropertyReference propertyReference, IValue<bool>? filterCondition = null, IValue<bool>? requirementCondition = null)
    {
        return new InclusionCondition(in propertyReference, filterCondition, requirementCondition);
    }

    /// <summary>
    /// Creates an inclusion condition that requires that a property exists and has a given <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="propertyReference">A reference to the property being included.</param>
    /// <param name="value">The value the property must equate to.</param>
    public static IInclusionCondition<TValue> Create<TValue>(
        in PropertyReference propertyReference,
        Optional<TValue> value,
        IValue<bool>? filterCondition = null,
        IValue<bool>? requirementCondition = null
    ) where TValue : IEquatable<TValue>
    {
        return value.HasValue
            ? new InclusionCondition<TValue>(in propertyReference, value.Value, filterCondition, requirementCondition)
            : new InclusionCondition<TValue>(in propertyReference, filterCondition, requirementCondition);
    }

    /// <summary>
    /// Creates an inclusion condition that requires that a property exists, with 1 condition.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="propertyReference">A reference to the property being included.</param>
    /// <param name="condition">The condition <paramref name="conditionBehavior"/> applies to.</param>
    /// <param name="conditionBehavior">The purpose of <paramref name="condition"/>.</param>
    /// <exception cref="InvalidEnumArgumentException">Invalid value for <paramref name="conditionBehavior"/>.</exception>
    public static IInclusionCondition Create(
        in PropertyReference propertyReference,
        IValue<bool> condition,
        ConditionBehavior conditionBehavior
    )
    {
        return conditionBehavior switch
        {
            ConditionBehavior.Filter => new InclusionCondition(in propertyReference, condition, null),
            ConditionBehavior.Requirement => new InclusionCondition(in propertyReference, null, condition),
            _ => throw new InvalidEnumArgumentException(nameof(conditionBehavior), (int)conditionBehavior, typeof(ConditionBehavior))
        };
    }

    /// <summary>
    /// Creates an inclusion condition that requires that a property exists and has a given <paramref name="value"/>, with 1 condition.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="propertyReference">A reference to the property being included.</param>
    /// <param name="value">The value the property must equate to.</param>
    /// <param name="condition">The condition <paramref name="conditionBehavior"/> applies to.</param>
    /// <param name="conditionBehavior">The purpose of <paramref name="condition"/>.</param>
    /// <exception cref="InvalidEnumArgumentException">Invalid value for <paramref name="conditionBehavior"/>.</exception>
    public static IInclusionCondition<TValue> Create<TValue>(
        in PropertyReference propertyReference,
        Optional<TValue> value,
        IValue<bool> condition,
        ConditionBehavior conditionBehavior
    ) where TValue : IEquatable<TValue>
    {
        return conditionBehavior switch
        {
            ConditionBehavior.Filter => value.HasValue
                ? new InclusionCondition<TValue>(in propertyReference, value.Value, condition, null)
                : new InclusionCondition<TValue>(in propertyReference, condition, null),
            ConditionBehavior.Requirement => value.HasValue
                ? new InclusionCondition<TValue>(in propertyReference, value.Value, null, condition)
                : new InclusionCondition<TValue>(in propertyReference, null, condition),
            _ => throw new InvalidEnumArgumentException(nameof(conditionBehavior), (int)conditionBehavior, typeof(ConditionBehavior))
        };
    }

    /// <inheritdoc />
    public virtual bool VisitValue<TVisitor>(ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateCondition(ConditionBehavior condition, out bool result, in FileEvaluationContext ctx)
    {
        bool success;
        Optional<bool> value;
        switch (condition)
        {
            case ConditionBehavior.Filter:
                if (FilterCondition == null)
                {
                    result = true;
                    return true;
                }
                success = FilterCondition.TryEvaluateValue(out value, in ctx);
                break;

            case ConditionBehavior.Requirement:
                if (RequirementCondition == null)
                {
                    result = true;
                    return true;
                }
                success = RequirementCondition.TryEvaluateValue(out value, in ctx);
                break;

            default:
                throw new InvalidEnumArgumentException(nameof(condition), (int)condition, typeof(ConditionBehavior));
        }

        result = value.Value;
        return success;
    }

    /// <summary>
    /// Attempts to read an inclusion condition from a JSON string or object.
    /// </summary>
    /// <param name="json">A JSON token which should be a string or object.</param>
    /// <param name="owner">The property defining this condition.</param>
    /// <param name="database">Asset information database.</param>
    /// <param name="condition">The parsed condition.</param>
    /// <returns>Whether or not a condition could be parsed.</returns>
    public static bool TryReadFromJson(
        in JsonElement json,
        DatProperty owner,
        IAssetSpecDatabase database,
        [NotNullWhen(true)] out IInclusionCondition? condition
    )
    {
        condition = null;
        if (json.ValueKind == JsonValueKind.String)
        {
            PropertyReference pref = PropertyReference.Parse(json.GetString()!);
            condition = Create(in pref);
            return true;
        }

        if (json.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (!json.TryGetProperty("Key"u8, out JsonElement element)
            || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        string key = element.GetString()!;

        PropertyReference propertyReference = PropertyReference.Parse(key);

        DataRefs.NilDataRefContext c;

        if (json.TryGetProperty("Value"u8, out element))
        {
            ValueVisitor v;
            v.Condition = null;
            v.Json = json;
            v.PropertyReference = propertyReference;
            v.Owner = owner;
            v.Database = database;
            IValue? value = Value.TryReadValueFromJson(element, ValueReadOptions.AssumeValue, ref v, null, database, owner, ref c);
            if (v.Condition == null)
            {
                if (value is { IsNull: true })
                {
                    v.Accept(Value.Null(StringType.Instance));
                    if (v.Condition == null)
                    {
                        return false;
                    }
                }
                else return false;
            }

            condition = v.Condition;
        }
        else if (json.TryGetProperty("RequirementCondition"u8, out JsonElement conditionElement))
        {
            if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, database, owner, out IValue<bool>? requirementCondition, ref c))
            {
                return false;
            }

            if (json.TryGetProperty("Condition"u8, out conditionElement))
            {
                if (json.TryGetProperty("ConditionIsRequirement"u8, out JsonElement condIsReqElement) && condIsReqElement.ValueKind == JsonValueKind.True)
                {
                    // conflicts with RequirementCondition
                    return false;
                }

                if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, database, owner, out IValue<bool>? filterCondition, ref c))
                {
                    return false;
                }

                condition = Create(in propertyReference, filterCondition, requirementCondition);
            }
            else
            {
                condition = Create(in propertyReference, requirementCondition, ConditionBehavior.Requirement);
            }
        }
        else if (json.TryGetProperty("Condition"u8, out conditionElement))
        {
            ConditionBehavior behavior;
            if (json.TryGetProperty("ConditionIsRequirement"u8, out JsonElement condIsReqElement) && condIsReqElement.ValueKind == JsonValueKind.True)
            {
                behavior = ConditionBehavior.Requirement;
            }
            else
            {
                behavior = propertyReference.IsReferenceTo(owner) ? ConditionBehavior.Requirement : ConditionBehavior.Filter;
            }

            if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, database, owner, out IValue<bool>? cond, ref c))
            {
                return false;
            }

            condition = Create(in propertyReference, cond, behavior);
        }
        else
        {
            condition = Create(in propertyReference);
        }

        return true;
    }

    private struct ValueVisitor : Value.IReadValueVisitor
    {
        public IInclusionCondition? Condition;
        public IAssetSpecDatabase Database;
        public DatProperty Owner;
        public JsonElement Json;
        public PropertyReference PropertyReference;

        public void Accept<TValue>(IValue<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!value.TryGetConcreteValue(out Optional<TValue> concrete))
            {
                return;
            }

            DataRefs.NilDataRefContext c;
            if (Json.TryGetProperty("RequirementCondition"u8, out JsonElement conditionElement))
            {
                if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, Database, Owner, out IValue<bool>? requirementCondition, ref c))
                {
                    return;
                }

                if (Json.TryGetProperty("Condition"u8, out conditionElement))
                {
                    if (Json.TryGetProperty("ConditionIsRequirement"u8, out JsonElement condIsReqElement) && condIsReqElement.ValueKind == JsonValueKind.True)
                    {
                        // conflicts with RequirementCondition
                        return;
                    }

                    if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, Database, Owner, out IValue<bool>? filterCondition, ref c))
                    {
                        return;
                    }

                    Condition = Create(in PropertyReference, concrete, filterCondition, requirementCondition);
                }
                else
                {
                    Condition = Create(in PropertyReference, concrete, requirementCondition, ConditionBehavior.Requirement);
                }
            }
            else if (Json.TryGetProperty("Condition"u8, out conditionElement))
            {
                ConditionBehavior behavior;
                if (Json.TryGetProperty("ConditionIsRequirement"u8, out JsonElement condIsReqElement) && condIsReqElement.ValueKind == JsonValueKind.True)
                {
                    behavior = ConditionBehavior.Requirement;
                }
                else
                {
                    behavior = PropertyReference.IsReferenceTo(Owner) ? ConditionBehavior.Requirement : ConditionBehavior.Filter;
                }

                if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, Database, Owner, out IValue<bool>? cond, ref c))
                {
                    return;
                }

                Condition = Create(in PropertyReference, concrete, cond, behavior);
            }
            else
            {
                Condition = Create(in PropertyReference, concrete);
            }
        }
    }
}

/// <inheritdoc cref="IInclusionCondition{TValue}"/>
public class InclusionCondition<TValue> : InclusionCondition, IInclusionCondition<TValue>
    where TValue : IEquatable<TValue>
{
    private readonly TValue? _value;
    private readonly bool _valueIsNull;

    public override bool IsAnyValue => false;

    internal InclusionCondition(in PropertyReference pRef, TValue value, IValue<bool>? filterCondition, IValue<bool>? requirementCondition)
        : base(in pRef, filterCondition, requirementCondition)
    {
        _value = value;
    }

    internal InclusionCondition(in PropertyReference pRef, IValue<bool>? filterCondition, IValue<bool>? requirementCondition)
        : base(in pRef, filterCondition, requirementCondition)
    {
        _valueIsNull = true;
    }

    public override bool VisitValue<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(_valueIsNull ? Optional<TValue>.Null : new Optional<TValue>(_value));
        return true;
    }

    Optional<TValue> IInclusionExclusionCondition<TValue>.Value => _valueIsNull ? Optional<TValue>.Null : new Optional<TValue>(_value);
}