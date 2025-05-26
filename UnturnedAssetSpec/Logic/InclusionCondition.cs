using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
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
    }

    public InclusionCondition(OneOrMore<InclusionConditionProperty> properties)
    {
        Properties = properties;
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
    public PropertyRef PropertyName { get; }
    public object? Value { get; }
    public SpecDynamicSwitchCaseOrCondition Condition { get; }

    public InclusionConditionProperty(PropertyRef propertyName, object? value, SpecDynamicSwitchCaseOrCondition condition = default)
    {
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