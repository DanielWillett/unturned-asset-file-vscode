using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Logic;

[JsonConverter(typeof(InclusionConditionConverter))]
public class InclusionCondition
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
}

public class InclusionConditionProperty
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
}