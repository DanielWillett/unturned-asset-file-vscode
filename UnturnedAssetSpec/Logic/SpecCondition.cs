using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Logic;

[JsonConverter(typeof(SpecConditionConverter))]
public struct SpecCondition
{
    public PropertyReference Variable { get; set; }
}

public enum ConditionOperation
{
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    Equal,
    NotEqual,
    Containing,
    StartingWith,
    EndingWith,
    Matching,
    ContainingCaseInsensitive,
    EqualCaseInsensitive,
    StartingWithCaseInsensitive,
    EndingWithCaseInsensitive,
    AssignableTo,
    AssignableFrom,
    Included,
    ReferenceIsOfType
}