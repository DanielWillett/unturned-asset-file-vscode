using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

// ReSharper disable InconsistentNaming

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Exclusion condition that states that the given property must not exist if this property exists.
/// </summary>
public interface IExclusionCondition : IInclusionExclusionCondition
{
    /// <summary>
    /// The condition that needs to be met for this condition to be checked.
    /// </summary>
    IValue<bool>? FilterCondition { get; }

    /// <summary>
    /// Attempts to evaluate the condition, if it exists.
    /// </summary>
    /// <param name="result">The result of the condition evaluation.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <returns>Whether or not the condition could be evaluated. If there is not a condition a result of <see langword="true"/> will be returned.</returns>
    bool TryEvaluateCondition(out bool result, in FileEvaluationContext ctx);
}

/// <summary>
/// Exclusion condition that states that the given property must not exist if this property exists with a given value.
/// </summary>
public interface IExclusionCondition<TValue> : IExclusionCondition, IInclusionExclusionCondition<TValue>
    where TValue : IEquatable<TValue>;

/// <summary>
/// Exclusion condition that states that the given property must not exist if this property exists.
/// </summary>
/// <remarks>Create using the <see cref="Create(in PropertyReference)"/> method and it's overloads.</remarks>
public class ExclusionCondition : IExclusionCondition
{
    protected PropertyReference _PropertyReference;

    /// <inheritdoc />
    public ref readonly PropertyReference PropertyReference => ref _PropertyReference;

    /// <inheritdoc />
    public virtual bool IsAnyValue => true;

    /// <inheritdoc />
    public IValue<bool>? FilterCondition { get; }

    protected ExclusionCondition(in PropertyReference pRef, IValue<bool>? filterCondition)
    {
        _PropertyReference = pRef;
        FilterCondition = filterCondition;
    }

    /// <summary>
    /// Creates an exclusion condition that always excludes a property.
    /// </summary>
    /// <param name="propertyReference">A reference to the property being excluded.</param>
    public static IExclusionCondition Create(in PropertyReference propertyReference, IValue<bool>? filterCondition = null)
    {
        return new ExclusionCondition(in propertyReference, filterCondition);
    }

    /// <summary>
    /// Creates an exclusion condition that requires a value for the property to be excluded.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="propertyReference">A reference to the property being excluded.</param>
    /// <param name="value">The value the property must equate to to be excluded.</param>
    public static IExclusionCondition<TValue> Create<TValue>(
        in PropertyReference propertyReference,
        Optional<TValue> value,
        IValue<bool>? filterCondition = null
    ) where TValue : IEquatable<TValue>
    {
        return value.HasValue
            ? new ExclusionCondition<TValue>(in propertyReference, value.Value, filterCondition)
            : new ExclusionCondition<TValue>(in propertyReference, filterCondition);
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
    public virtual bool TryEvaluateCondition(out bool result, in FileEvaluationContext ctx)
    {
        if (FilterCondition == null)
        {
            result = true;
            return true;
        }

        if (!FilterCondition.TryEvaluateValue(out Optional<bool> value, in ctx))
        {
            result = false;
            return false;
        }

        result = value.Value;
        return value.HasValue;
    }

    /// <summary>
    /// Attempts to read an exclusion condition from a JSON string or object.
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
        [NotNullWhen(true)] out IExclusionCondition? condition
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
        else if (json.TryGetProperty("Condition"u8, out JsonElement conditionElement))
        {
            if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, database, owner, out IValue<bool>? filterCondition, ref c))
            {
                return false;
            }

            condition = Create(in propertyReference, filterCondition);
        }
        else
        {
            condition = Create(in propertyReference);
        }

        return true;
    }

    private struct ValueVisitor : Value.IReadValueVisitor
    {
        public IExclusionCondition? Condition;
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

            if (Json.TryGetProperty("Condition"u8, out JsonElement conditionElement))
            {
                DataRefs.NilDataRefContext c;
                if (!Conditions.TryReadComplexOrBasicConditionFromJson(in conditionElement, Database, Owner, out IValue<bool>? filterCondition, ref c))
                {
                    return;
                }

                Condition = Create(in PropertyReference, filterCondition);
            }
            else
            {
                Condition = Create(in PropertyReference, concrete);
            }
        }
    }
}

/// <summary>
/// Exclusion condition that states that the given property must not exist if this property exists with a given value.
/// </summary>
public class ExclusionCondition<TValue> : ExclusionCondition, IExclusionCondition<TValue>
    where TValue : IEquatable<TValue>
{
    private readonly TValue? _value;
    private readonly bool _valueIsNull;

    public override bool IsAnyValue => false;

    internal ExclusionCondition(in PropertyReference pRef, TValue value, IValue<bool>? filterCondition)
        : base(in pRef, filterCondition)
    {
        _value = value;
    }

    internal ExclusionCondition(in PropertyReference pRef, IValue<bool>? filterCondition)
        : base(in pRef, filterCondition)
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