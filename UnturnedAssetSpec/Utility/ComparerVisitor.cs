using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Compares one value with another of a different type and determines if one is greater than/less than the other.
/// </summary>
internal struct ComparerVisitor<TValue> : IValueVisitor, IGenericVisitor
    where TValue : IEquatable<TValue>
{
    public int Comparison;
    public bool IsNull;
    public bool Success;
    public bool CaseInsensitive;
    
    /// <summary>
    /// The 'left' value. The visited value will be the 'right' value.
    /// (ex. 4 compare-to 2 = '1', 2 compare-to 4 = '-1')
    /// </summary>
    public TValue? Value;

    /// <summary>
    /// Try comparing two values using the <see cref="ComparerVisitor{TValue}"/>.
    /// </summary>
    public static bool TryCompare<TIn2>(TValue in1, TIn2 in2, bool caseInsensitive, out int cmp)
        where TIn2 : IEquatable<TIn2>
    {
        ComparerVisitor<TValue> v = default;
        v.Value = in1;
        v.CaseInsensitive = caseInsensitive;

        v.Accept(new Optional<TIn2>(in2));

        cmp = v.Comparison;
        return v.Success;
    }

    public void Accept<TOtherValue>(Optional<TOtherValue> optVal)
        where TOtherValue : IEquatable<TOtherValue>
    {
        Success = true;
        if (!optVal.HasValue)
        {
            Comparison = IsNull ? 0 : 1;
            return;
        }

        if (IsNull)
        {
            Comparison = -1;
            return;
        }

        // already same type
        if (typeof(TValue) == typeof(TOtherValue))
        {
            if (CaseInsensitive && typeof(TValue) == typeof(string))
            {
                Comparison = string.Compare(
                    MathMatrix.As<TValue?, string?>(Value),
                    MathMatrix.As<TOtherValue?, string?>(optVal.Value),
                    StringComparison.OrdinalIgnoreCase
                );
            }
            else if (CaseInsensitive && typeof(TValue) == typeof(char))
            {
                Comparison = char.ToLowerInvariant(
                    MathMatrix.As<TValue?, char>(Value)
                ).CompareTo(
                    char.ToLowerInvariant(
                        MathMatrix.As<TOtherValue?, char>(optVal.Value)
                    )
                );
            }
            else
            {
                Comparison = Comparer<TValue>.Default.Compare(Value!, MathMatrix.As<TOtherValue, TValue>(optVal.Value));
            }
            return;
        }

        // compare char with string (need this to take priority over MathMatrix)
        if (typeof(TValue) == typeof(char))
        {
            if (typeof(TOtherValue) == typeof(string))
            {
                string s = MathMatrix.As<TOtherValue, string>(optVal.Value);
                Comparison = string.Compare(new string(MathMatrix.As<TValue?, char>(Value), 1), s, CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                return;
            }
        }
        else if (typeof(TOtherValue) == typeof(char))
        {
            if (typeof(TValue) == typeof(string))
            {
                string? s = MathMatrix.As<TValue?, string?>(Value);
                Comparison = string.Compare(s, new string(MathMatrix.As<TValue?, char>(Value), 1), CaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
                return;
            }
        }

        if (MathMatrix.IsValidMathExpressionInputType<TValue>()
            || MathMatrix.IsValidMathExpressionInputType<TOtherValue>())
        {
            ReduceLeft<TOtherValue> v;
            v.Success = false;
            v.RightValue = optVal.Value!;
            v.Comparison = 0;
            v.CaseInsensitive = CaseInsensitive;
            if (MathMatrix.TryReduce(Value!, ref v) && v.Success)
            {
                Comparison = v.Comparison;
                return;
            }
        }

        // compare Guid with GuidOrId (MathMatrix will take care of IDs)
        if (typeof(TOtherValue) == typeof(Guid))
        {
            if (typeof(TValue) == typeof(GuidOrId))
            {
                ref GuidOrId guidOrId = ref Unsafe.As<TValue?, GuidOrId>(ref Value);
                if (guidOrId.IsId)
                {
                    Success = false;
                    return;
                }

                Comparison = guidOrId.Guid.CompareTo(MathMatrix.As<TOtherValue, Guid>(optVal.Value));
                return;
            }
        }
        else if (typeof(TOtherValue) == typeof(GuidOrId))
        {
            if (typeof(TValue) == typeof(Guid))
            {
                GuidOrId guidOrId = MathMatrix.As<TOtherValue, GuidOrId>(optVal.Value);
                if (guidOrId.IsId)
                {
                    Success = false;
                    return;
                }

                Comparison = Unsafe.As<TValue?, Guid>(ref Value).CompareTo(guidOrId.Guid);
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
                    Comparison = Comparer<TOtherValue>.Default.Compare(parsedValue, optVal.Value);
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
                    Comparison = Comparer<TValue>.Default.Compare(Value!, parsedValue);
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
    public int Comparison;
    public bool Success;
    public bool CaseInsensitive;
    public TRight RightValue;
    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        ReduceRight<T> reduceRight;
        reduceRight.Comparison = 0;
        reduceRight.Value = value;
        reduceRight.Success = false;
        reduceRight.CaseInsensitive = CaseInsensitive;

        Success = MathMatrix.TryReduce(RightValue, ref reduceRight) & reduceRight.Success;
        Comparison = reduceRight.Comparison;
    }
}

file struct ReduceRight<TLeft> : IGenericVisitor
    where TLeft : IEquatable<TLeft>
{
    public int Comparison;
    public bool Success;
    public bool CaseInsensitive;
    public TLeft? Value;
    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        Int32Visitor v;
        v.Value = 0;
        v.Success = false;
        Success = MathMatrix.Compare(Value, value, CaseInsensitive, ref v) & v.Success;
        Comparison = v.Value;
    }
}

file struct Int32Visitor : IGenericVisitor
{
    public int Value;
    public bool Success;

    /// <inheritdoc />
    public void Accept<T>(T? value) where T : IEquatable<T>
    {
        if (typeof(T) != typeof(int))
            return;

        Success = true;
        Value = MathMatrix.As<T, int>(value!);
    }
}