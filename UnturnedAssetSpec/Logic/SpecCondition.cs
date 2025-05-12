using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Logic;

[JsonConverter(typeof(SpecConditionConverter))]
public readonly struct SpecCondition : IEquatable<SpecCondition>
{
    public ISpecDynamicValue Variable { get; }
    public ConditionOperation Operation { get; }
    public object? Comparand { get; }

    public SpecCondition(ISpecDynamicValue variable, ConditionOperation operation, object? comparand)
    {
        Variable = variable;
        Operation = operation;
        Comparand = comparand;
    }

    public bool Equals(SpecCondition other)
    {
        return Variable.Equals(other.Variable) && Operation == other.Operation && Equals(Comparand, other.Comparand);
    }

    public override bool Equals(object? obj) => obj is SpecCondition c && Equals(c);

    public static bool operator ==(SpecCondition left, SpecCondition right) => left.Equals(right);

    public static bool operator !=(SpecCondition left, SpecCondition right) => !left.Equals(right);

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = Variable == null ? 0 : Variable.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)Operation;
            hashCode = (hashCode * 397) ^ (Comparand != null ? Comparand.GetHashCode() : 0);
            return hashCode;
        }
    }
}

public static class ConditionOperationExtensions
{
    private static readonly bool[] CaseInsensitive =
    [
        false,         // LessThan,
        false,         // GreaterThan,
        false,         // LessThanOrEqual,
        false,         // GreaterThanOrEqual,
        false,         // Equal,
        false,         // NotEqual,
        true,          // NotEqualCaseInsensitive,
        false,         // Containing,
        false,         // StartingWith,
        false,         // EndingWith,
        false,         // Matching,
        true,          // ContainingCaseInsensitive,
        true,          // EqualCaseInsensitive,
        true,          // StartingWithCaseInsensitive,
        true,          // EndingWithCaseInsensitive,
        false,         // AssignableTo,
        false,         // AssignableFrom,
        false,         // Included,
        false          // ReferenceIsOfType
    ];

    /// <summary>
    /// If a condition is true when two values are the same.
    /// </summary>
    private static readonly bool[] Equality =
    [
        false,         // LessThan,
        false,         // GreaterThan,
        true,          // LessThanOrEqual,
        true,          // GreaterThanOrEqual,
        true,          // Equal,
        false,         // NotEqual,
        false,         // NotEqualCaseInsensitive,
        true,          // Containing,
        true,          // StartingWith,
        true,          // EndingWith,
        false,         // Matching,
        true,          // ContainingCaseInsensitive,
        true,          // EqualCaseInsensitive,
        true,          // StartingWithCaseInsensitive,
        true,          // EndingWithCaseInsensitive,
        true,          // AssignableTo,
        true,          // AssignableFrom,
        false,         // Included,
        false          // ReferenceIsOfType
    ];

    /// <summary>
    /// If a condition can be true when two values are different.
    /// </summary>
    private static readonly bool[] Inequality =
    [
        true,          // LessThan,
        true,          // GreaterThan,
        true,          // LessThanOrEqual,
        true,          // GreaterThanOrEqual,
        false,         // Equal,
        true,          // NotEqual,
        true,          // NotEqualCaseInsensitive,
        true,          // Containing,
        true,          // StartingWith,
        true,          // EndingWith,
        true,          // Matching,
        true,          // ContainingCaseInsensitive,
        false,         // EqualCaseInsensitive,
        true,          // StartingWithCaseInsensitive,
        true,          // EndingWithCaseInsensitive,
        true,          // AssignableTo,
        true,          // AssignableFrom,
        false,         // Included,
        true           // ReferenceIsOfType
    ];

    public static bool IsCaseInsensitive(this ConditionOperation op)
    {
        return CaseInsensitive[(int)op];
    }

    public static bool IsCaseSensitive(this ConditionOperation op)
    {
        return !CaseInsensitive[(int)op];
    }

    /// <summary>
    /// If a condition is true when two values are the same.
    /// </summary>
    public static bool IsEquality(this ConditionOperation op)
    {
        return Equality[(int)op];
    }

    /// <summary>
    /// If a condition can be true when two values are different.
    /// </summary>
    public static bool IsInequality(this ConditionOperation op)
    {
        return Inequality[(int)op];
    }

    public static bool EvaluateNulls(this ConditionOperation op, bool valueIsNull, bool comparandIsNull)
    {
        if (!valueIsNull && !valueIsNull)
            throw new ArgumentException("Expected at least one null value.");

        if (op == ConditionOperation.Included)
            return true;

        if (valueIsNull)
        {
            if (comparandIsNull)
            {
                return op.IsEquality() || op is ConditionOperation.Matching or ConditionOperation.ReferenceIsOfType;
            }

            return op.IsInequality() && op is
                not ConditionOperation.GreaterThan
                and not ConditionOperation.GreaterThanOrEqual
                and not ConditionOperation.StartingWith
                and not ConditionOperation.EndingWith
                and not ConditionOperation.Containing
                and not ConditionOperation.StartingWithCaseInsensitive
                and not ConditionOperation.EndingWithCaseInsensitive
                and not ConditionOperation.ContainingCaseInsensitive
                and not ConditionOperation.AssignableFrom
                and not ConditionOperation.ReferenceIsOfType;
        }

        return op.IsInequality() && op is
            not ConditionOperation.LessThan
            and not ConditionOperation.LessThanOrEqual
            and not ConditionOperation.AssignableTo;
    }

    public static bool Evaluate<T>(this ConditionOperation op, T value, T comparand, AssetInformation? information)
    {
        if (value == null)
        {
            return op.EvaluateNulls(true, comparand == null);
        }
        if (comparand == null)
        {
            return op.EvaluateNulls(false, true);
        }

        switch (op)
        {
            case ConditionOperation.LessThan:
                return Comparer<T>.Default.Compare(value, comparand) < 0;

            case ConditionOperation.GreaterThan:
                return Comparer<T>.Default.Compare(value, comparand) > 0;

            case ConditionOperation.LessThanOrEqual:
                return Comparer<T>.Default.Compare(value, comparand) <= 0;

            case ConditionOperation.GreaterThanOrEqual:
                return Comparer<T>.Default.Compare(value, comparand) >= 0;

            case ConditionOperation.Equal:
            {
                return value is string str ? string.Equals(str, (string)(object)comparand, StringComparison.Ordinal) : Comparer<T>.Default.Compare(value, comparand) == 0;
            }

            case ConditionOperation.NotEqual:
            {
                return value is string str ? !string.Equals(str, (string)(object)comparand, StringComparison.Ordinal) : Comparer<T>.Default.Compare(value, comparand) != 0;
            }

            case ConditionOperation.NotEqualCaseInsensitive:
            {
                return value is string str ? !string.Equals(str, (string)(object)comparand, StringComparison.OrdinalIgnoreCase) : (Comparer<T>.Default.Compare(value, comparand) != 0);
            }

            case ConditionOperation.Containing:
            {
                return value is string str && str.IndexOf((string)(object)comparand, StringComparison.Ordinal) >= 0;
            }

            case ConditionOperation.StartingWith:
            {
                return value is string str && str.StartsWith((string)(object)comparand, StringComparison.Ordinal);
            }

            case ConditionOperation.EndingWith:
            {
                return value is string str && str.EndsWith((string)(object)comparand, StringComparison.Ordinal);
            }

            case ConditionOperation.Matching:
            {
                return value is string str && Regex.IsMatch((string)(object)comparand, str, RegexOptions.Singleline | RegexOptions.CultureInvariant);
            }

            case ConditionOperation.ContainingCaseInsensitive:
            {
                return value is string str && str.IndexOf((string)(object)comparand, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            case ConditionOperation.EqualCaseInsensitive:
            {
                return value is string str ? string.Equals(str, (string)(object)comparand, StringComparison.OrdinalIgnoreCase) : Comparer<T>.Default.Compare(value, comparand) == 0;
            }

            case ConditionOperation.StartingWithCaseInsensitive:
            {
                return value is string str && str.StartsWith((string)(object)comparand, StringComparison.OrdinalIgnoreCase);
            }

            case ConditionOperation.EndingWithCaseInsensitive:
            {
                return value is string str && str.EndsWith((string)(object)comparand, StringComparison.OrdinalIgnoreCase);
            }

            case ConditionOperation.AssignableTo:
                return value switch
                {
                    Type type => ((Type)(object)comparand).IsAssignableFrom(type),
                    QualifiedType qType when information != null => information.IsAssignableFrom((QualifiedType)(object)comparand, qType),
                    _ => false
                };
            
            case ConditionOperation.AssignableFrom:
                return value switch
                {
                    Type type => type.IsAssignableFrom((Type)(object)comparand),
                    QualifiedType qType when information != null => information.IsAssignableFrom(qType, (QualifiedType)(object)comparand),
                    _ => false
                };
            
            case ConditionOperation.ReferenceIsOfType:
            {
                // todo
                return false;
            }

            case ConditionOperation.Included:
                return true;

            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }
}

public enum ConditionOperation
{
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    Equal,
    NotEqual,
    NotEqualCaseInsensitive,
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