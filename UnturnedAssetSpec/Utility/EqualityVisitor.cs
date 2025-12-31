using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Compares one value with another of a different type.
/// </summary>
internal struct EqualityVisitor<TValue> : IValueVisitor, IGenericVisitor
    where TValue : IEquatable<TValue>
{
    public bool IsEqual;
    public bool IsNull;
    public bool Success;
    public TValue? Value;
    public bool CaseInsensitive;

    public void Accept<TOtherValue>(Optional<TOtherValue> optVal)
        where TOtherValue : IEquatable<TOtherValue>
    {
        Success = true;
        if (!optVal.HasValue)
        {
            IsEqual = IsNull;
            return;
        }

        if (IsNull)
        {
            IsEqual = false;
            return;
        }

        // already same type
        if (typeof(TValue) == typeof(TOtherValue))
        {
            if (CaseInsensitive && typeof(TValue) == typeof(string))
            {
                IsEqual = string.Equals(
                    MathMatrix.As<TValue?, string?>(Value),
                    MathMatrix.As<TOtherValue?, string?>(optVal.Value),
                    StringComparison.OrdinalIgnoreCase
                );
            }
            else
            {
                IsEqual = EqualityComparer<TValue>.Default.Equals(Value!, MathMatrix.As<TOtherValue, TValue>(optVal.Value));
            }
            return;
        }

        // compare char with string (need this to take priority over MathMatrix)
        if (typeof(TValue) == typeof(char))
        {
            if (typeof(TOtherValue) == typeof(string))
            {
                string s = MathMatrix.As<TOtherValue, string>(optVal.Value);
                if (s is { Length: 1 })
                {
                    char c = MathMatrix.As<TValue?, char>(Value);
                    IsEqual = CaseInsensitive ? char.ToLowerInvariant(c) == char.ToLowerInvariant(s[0]) : c == s[0];
                }
                else
                {
                    IsEqual = false;
                }
                return;
            }
        }
        else if (typeof(TOtherValue) == typeof(char))
        {
            if (typeof(TValue) == typeof(string))
            {
                string? s = MathMatrix.As<TValue?, string?>(Value);
                if (s is { Length: 1 })
                {
                    char c = MathMatrix.As<TOtherValue?, char>(optVal.Value);
                    IsEqual = CaseInsensitive ? char.ToLowerInvariant(s[0]) == char.ToLowerInvariant(c) : s[0] == c;
                }
                else
                {
                    IsEqual = false;
                }
                return;
            }
        }

        if (MathMatrix.IsValidMathExpressionInputType<TValue>()
            || MathMatrix.IsValidMathExpressionInputType<TOtherValue>())
        {
            ReduceLeft<TOtherValue> v;
            v.Success = false;
            v.RightValue = optVal.Value!;
            v.IsEqual = false;
            if (MathMatrix.TryReduce(Value!, ref v) && v.Success)
            {
                IsEqual = v.IsEqual;
                return;
            }
        }

        // compare Guid with GuidOrId (MathMatrix will take care of IDs)
        if (typeof(TOtherValue) == typeof(Guid))
        {
            if (typeof(TValue) == typeof(GuidOrId))
            {
                IsEqual = Unsafe.As<TValue, GuidOrId>(ref Value!).Equals(MathMatrix.As<TOtherValue, Guid>(optVal.Value));
                return;
            }
        }
        else if (typeof(TOtherValue) == typeof(GuidOrId))
        {
            if (typeof(TValue) == typeof(Guid))
            {
                IsEqual = MathMatrix.As<TOtherValue, GuidOrId>(optVal.Value).Equals(Unsafe.As<TValue, Guid>(ref Value!));
                return;
            }
        }

        // compare value with string
        if (typeof(TValue) == typeof(string))
        {
            if (TypeConverters.TryGet<TOtherValue>() is { } rightTypeConverter)
            {
                TypeConverterParseArgs<TOtherValue> ov = default;
                ov.Type = rightTypeConverter.DefaultType;
                ov.TextAsString = MathMatrix.As<TValue?, string?>(Value);
                if (rightTypeConverter.TryParse(ov.TextAsString, ref ov, out TOtherValue? parsedValue))
                {
                    IsEqual = EqualityComparer<TOtherValue>.Default.Equals(parsedValue, optVal.Value);
                    return;
                }
            }
        }
        else if (typeof(TOtherValue) == typeof(string))
        {
            if (TypeConverters.TryGet<TValue>() is { } rightTypeConverter)
            {
                TypeConverterParseArgs<TValue> ov = default;
                ov.Type = rightTypeConverter.DefaultType;
                ov.TextAsString = MathMatrix.As<TOtherValue?, string?>(optVal.Value);
                if (rightTypeConverter.TryParse(ov.TextAsString, ref ov, out TValue? parsedValue))
                {
                    IsEqual = EqualityComparer<TValue>.Default.Equals(Value!, parsedValue);
                    return;
                }
            }
        }

        Success = false;
    }

    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        Accept(value == null ? Optional<T>.Null : new Optional<T>(value));
    }
}

file struct ReduceLeft<TRight> : IGenericVisitor
    where TRight : IEquatable<TRight>
{
    public bool IsEqual;
    public bool Success;
    public TRight RightValue;
    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        ReduceRight<T> reduceRight;
        reduceRight.IsEqual = false;
        reduceRight.Value = value;
        reduceRight.Success = false;

        Success = MathMatrix.TryReduce(RightValue, ref reduceRight) & reduceRight.Success;
        IsEqual = reduceRight.IsEqual;
    }
}

file struct ReduceRight<TLeft> : IGenericVisitor
    where TLeft : IEquatable<TLeft>
{
    public bool IsEqual;
    public bool Success;
    public TLeft? Value;
    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        BooleanVisitor v;
        v.Value = false;
        v.Success = false;
        Success = MathMatrix.Equals(Value, value, ref v) & v.Success;
        IsEqual = v.Value;
    }
}

file struct BooleanVisitor : IGenericVisitor
{
    public bool Value;
    public bool Success;

    /// <inheritdoc />
    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        if (typeof(T) != typeof(bool))
            return;

        Success = true;
        Value = MathMatrix.As<T, bool>(value!);
    }
}