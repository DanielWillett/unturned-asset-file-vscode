using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Logic;

[JsonConverter(typeof(SpecConditionConverter))]
public readonly struct SpecCondition : IEquatable<SpecCondition>
{
    public ISpecDynamicValue Variable { get; }
    public ConditionOperation Operation { get; }
    public object? Comparand { get; }
    public bool IsInverted { get; }

    [Pure]
    internal bool Invert(bool value)
    {
        return IsInverted ? !value : value;
    }

    public SpecCondition(ISpecDynamicValue variable, ConditionOperation operation, object? comparand, bool isInverted)
    {
        Variable = variable;
        Operation = operation;
        Comparand = comparand;
        IsInverted = isInverted;
    }

    public bool TryGetOpposite(out SpecCondition condition)
    {
        if (IsInverted)
        {
            condition = new SpecCondition(Variable, Operation, Comparand, false);
            return true;
        }

        switch (Operation)
        {
            case ConditionOperation.Included:
                condition = new SpecCondition(Variable, ConditionOperation.Excluded, Comparand, false);
                return true;
            case ConditionOperation.Excluded:
                condition = new SpecCondition(Variable, ConditionOperation.Included, Comparand, false);
                return true;
            case ConditionOperation.Equal:
                condition = new SpecCondition(Variable, ConditionOperation.NotEqual, Comparand, false);
                return true;
            case ConditionOperation.NotEqual:
                condition = new SpecCondition(Variable, ConditionOperation.Equal, Comparand, false);
                return true;
            case ConditionOperation.EqualCaseInsensitive:
                condition = new SpecCondition(Variable, ConditionOperation.NotEqualCaseInsensitive, Comparand, false);
                return true;
            case ConditionOperation.NotEqualCaseInsensitive:
                condition = new SpecCondition(Variable, ConditionOperation.EqualCaseInsensitive, Comparand, false);
                return true;
            case ConditionOperation.GreaterThan:
                condition = new SpecCondition(Variable, ConditionOperation.LessThanOrEqual, Comparand, false);
                return true;
            case ConditionOperation.LessThan:
                condition = new SpecCondition(Variable, ConditionOperation.GreaterThanOrEqual, Comparand, false);
                return true;
            case ConditionOperation.GreaterThanOrEqual:
                condition = new SpecCondition(Variable, ConditionOperation.LessThan, Comparand, false);
                return true;
            case ConditionOperation.LessThanOrEqual:
                condition = new SpecCondition(Variable, ConditionOperation.GreaterThan, Comparand, false);
                return true;
            default:
                condition = new SpecCondition(Variable, Operation, Comparand, true);
                return true;
        }
    }

    public bool Equals(SpecCondition other)
    {
        return (Variable?.Equals(other.Variable) ?? other.Variable == null) && Operation == other.Operation && Equals(Comparand, other.Comparand);
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

    public override string ToString()
    {
        if (Operation == ConditionOperation.Included)
            return $"{Variable} {Operation}";

        return Comparand == null ? $"{Variable} {Operation} null" : $"{Variable} {Operation} {Comparand}";
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
        false,         // ReferenceIsOfType
        false,         // ValueIncluded
        false          // Excluded
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
        false,         // ReferenceIsOfType
        false,         // ValueIncluded
        false          // Excluded
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
        true,          // ReferenceIsOfType
        false,         // ValueIncluded
        false          // Excluded
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

    public static bool EvaluateNulls(this SpecCondition condition, bool valueIsNull, bool comparandIsNull)
    {
        bool eval = EvaluateNulls(condition.Operation, valueIsNull, comparandIsNull);
        if (condition.IsInverted)
            eval = !eval;

        return eval;
    }

    private static bool EvaluateNulls(ConditionOperation op, bool valueIsNull, bool comparandIsNull)
    {
        if (!valueIsNull && !comparandIsNull)
            throw new ArgumentException("Expected at least one null value.");

        if (op == ConditionOperation.Included)
            return true;

        if (op == ConditionOperation.Excluded)
            return false;

        if (op == ConditionOperation.ValueIncluded)
            return !valueIsNull;

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

    private static readonly IComparer<Vector2> Vector2Comparer = Comparer<Vector2>.Create((a, b) =>
    {
        const float tolerance = 0.0001f;

        float sub = a.X - b.X;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = a.Y - b.Y;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }

        return 0;
    });

    private static readonly IComparer<Vector3> Vector3Comparer = Comparer<Vector3>.Create((a, b) =>
    {
        const float tolerance = 0.0001f;

        float sub = a.X - b.X;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = a.Y - b.Y;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = a.Z - b.Z;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }

        return 0;
    });

    private static readonly IComparer<Vector4> Vector4Comparer = Comparer<Vector4>.Create((a, b) =>
    {
        const float tolerance = 0.0001f;

        float sub = a.X - b.X;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = a.Y - b.Y;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = a.Z - b.Z;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }
        sub = a.W - b.W;
        if (Math.Abs(sub) >= tolerance)
        {
            return sub > 0 ? 1 : -1;
        }

        return 0;
    });

    private static IComparer<T> GetComparer<T>()
    {
        if (typeof(T) == typeof(Vector2))
            return (IComparer<T>)Vector2Comparer;
        if (typeof(T) == typeof(Vector3))
            return (IComparer<T>)Vector3Comparer;
        if (typeof(T) == typeof(Vector4))
            return (IComparer<T>)Vector4Comparer;

        return Comparer<T>.Default;
    }

    private static bool Evaluate<T>(ConditionOperation op, T value, T comparand, AssetInformation? information)
    {
        if (value == null)
        {
            return EvaluateNulls(op, true, comparand == null);
        }
        if (comparand == null)
        {
            return EvaluateNulls(op, false, true);
        }

        switch (op)
        {
            case ConditionOperation.LessThan:
                return GetComparer<T>().Compare(value, comparand) < 0;

            case ConditionOperation.GreaterThan:
                return GetComparer<T>().Compare(value, comparand) > 0;

            case ConditionOperation.LessThanOrEqual:
                return GetComparer<T>().Compare(value, comparand) <= 0;

            case ConditionOperation.GreaterThanOrEqual:
                return GetComparer<T>().Compare(value, comparand) >= 0;

            case ConditionOperation.Equal:
                {
                    return value is string str ? string.Equals(str, (string)(object)comparand, StringComparison.Ordinal) : GetComparer<T>().Compare(value, comparand) == 0;
                }

            case ConditionOperation.NotEqual:
                {
                    return value is string str ? !string.Equals(str, (string)(object)comparand, StringComparison.Ordinal) : GetComparer<T>().Compare(value, comparand) != 0;
                }

            case ConditionOperation.NotEqualCaseInsensitive:
                {
                    return value is string str ? !string.Equals(str, (string)(object)comparand, StringComparison.OrdinalIgnoreCase) : (GetComparer<T>().Compare(value, comparand) != 0);
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
            case ConditionOperation.ValueIncluded:
                return true;

            case ConditionOperation.Excluded:
                return false;

            default:
                throw new ArgumentOutOfRangeException(nameof(op), op, null);
        }
    }

    public static bool Evaluate<T>(this SpecCondition c, T value, T comparand, AssetInformation? information)
    {
        ConditionOperation op = c.Operation;
        bool result = Evaluate(op, value, comparand, information);
        if (c.IsInverted)
            result = !result;

        return result;
    }
}

public enum ConditionOperation
{
    // when adding:
    // * update SpecConditionConverter.Operations
    // * udpate 3 arrays in ConditionOperationExtensions
    // * update conditions.md in docs
    // * update JSON spec
    // * add needed checks and switch cases
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
    ReferenceIsOfType,
    ValueIncluded,
    Excluded
}