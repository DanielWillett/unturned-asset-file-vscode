using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Logic;

[JsonConverter(typeof(InclusionConditionConverter))]
public sealed class InclusionCondition : IEquatable<InclusionCondition?>
{
    public OneOrMore<PropertyRef> PropertyNames { get; }

    public OneOrMore<InclusionConditionProperty> Properties { get; }

    public InclusionCondition(OneOrMore<PropertyRef> propertyNames)
    {
        PropertyNames = propertyNames;
        Properties = OneOrMore<InclusionConditionProperty>.Null;
    }

    public InclusionCondition(OneOrMore<InclusionConditionProperty> properties)
    {
        Properties = properties;
        PropertyNames = OneOrMore<PropertyRef>.Null;
    }

    public bool Equals(InclusionCondition? other) => other != null && PropertyNames.Equals(other.PropertyNames) && Properties.Equals(other.Properties);
    public override bool Equals(object? obj) => obj is InclusionCondition c && Equals(c);
    public override int GetHashCode()
    {
        return PropertyNames.IsNull ? Properties.GetHashCode() : PropertyNames.GetHashCode();
    }
}

public sealed class InclusionConditionProperty : IEquatable<InclusionConditionProperty?>
{
    // ReSharper disable once ReplaceWithFieldKeyword
    private readonly bool _isInclusive;
    private Func<SpecDynamicSwitchCaseOrCondition, bool>? _isConditionRequirementSelector;

    public PropertyRef PropertyName { get; }
    public object? Value { get; }
    public SpecDynamicSwitchCaseOrCondition Condition { get; internal set; }

    /// <summary>
    /// If this is inclusive and condition refers to the same variable as <see cref="PropertyName"/>, this condition is a requirement of <see cref="PropertyName"/> instead of a condition of inclusion.
    /// </summary>
    public bool IsConditionRequirement
    {
        get
        {
            if (!_isInclusive)
                return false;

            if (Condition.Case == null)
            {
                return PropertyName.Equals(Condition.Condition.Variable);
            }

            _isConditionRequirementSelector ??= x => PropertyName.Equals(x.Condition.Variable);
            return Condition.Case.HasConditions && Condition.Case.Conditions.Any(_isConditionRequirementSelector);
        }
    }

    public InclusionConditionProperty(bool isInclusive, PropertyRef propertyName, object? value, SpecDynamicSwitchCaseOrCondition condition = default)
    {
        _isInclusive = isInclusive;
        PropertyName = propertyName;
        Value = value;
        Condition = condition;
    }

    public bool Equals(InclusionConditionProperty? other)
    {
        return other != null
               && EqualityComparer<PropertyRef>.Default.Equals(other.PropertyName, PropertyName)
               && Equals(Value, other.Value)
               && Condition.Equals(other.Condition);
    }

    public override bool Equals(object? obj) => obj is InclusionConditionProperty p && Equals(p);

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = PropertyName.GetHashCode();
            hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ Condition.GetHashCode();
            return hashCode;
        }
    }
}