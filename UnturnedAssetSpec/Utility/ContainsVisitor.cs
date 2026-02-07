using System;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal struct ContainedVisitor<TSuperset> : IGenericVisitor, IEquatableArrayVisitor
    where TSuperset : IEquatable<TSuperset>
{
    public TSuperset? Superset;
    public bool IsCaseInsensitive;
    public bool Success;
    public bool Result;

    /// <summary>
    /// Checks whether a value is a subset of a value. If one of these values are strings, a string contains operation will be performed instead.
    /// </summary>
    /// <typeparam name="TSubset"></typeparam>
    /// <param name="superset"></param>
    /// <param name="value"></param>
    /// <param name="doesContain"></param>
    /// <returns></returns>
    public static bool TryGetContains<TSubset>(TSuperset? superset, TSubset? value, out bool doesContain)
        where TSubset : IEquatable<TSubset>
    {
        return TryGetContains(superset, value, false, out doesContain);
    }

    public static bool TryGetContains<TSubset>(TSuperset? superset, TSubset? value, bool isCaseInsensitive, out bool doesContain)
        where TSubset : IEquatable<TSubset>
    {
        ContainedVisitor<TSuperset> visitor = default;
        visitor.IsCaseInsensitive = isCaseInsensitive;
        visitor.Superset = superset;

        visitor.Accept(value);

        doesContain = visitor.Result;
        return visitor.Success;
    }

    public void Accept<TSubset>(TSubset? subset) where TSubset : IEquatable<TSubset>
    {
        // array in array (calls Accept in this type)
        if (EquatableArrayHelper<TSubset>.IsEquatableArray
            && subset is IEquatableArray<TSuperset> subsetStrongTyped)
        {
            subsetStrongTyped.Visit(ref this);
            return;
        }

        // value in array (calls accept in SupersetIsEquatableArrayVisitor)
        if (EquatableArrayHelper<TSuperset>.IsEquatableArray
            && Superset is IEquatableArray<TSuperset> supersetStrongTyped)
        {
#if NET9_0_OR_GREATER
            scoped SupersetIsEquatableArrayVisitor<TSubset> visitor;
            visitor.Subset = ref subset;
#else
            SupersetIsEquatableArrayVisitor<TSubset> visitor;
            visitor.Subset = subset;
#endif
            visitor.IsCaseInsensitive = IsCaseInsensitive;
            visitor.Contains = false;
            supersetStrongTyped.Visit(ref visitor);
            Result = visitor.Contains;
            Success = true;
            return;
        }

        // value in string
        if (typeof(TSuperset) == typeof(string))
        {
            string? superstring = Unsafe.As<TSuperset?, string?>(ref Superset);
            if (typeof(TSubset) == typeof(char))
            {
                if (string.IsNullOrEmpty(superstring))
                {
                    Result = false;
                    Success = true;
                    return;
                }

                // character in string
                char substringChar = Unsafe.As<TSubset?, char>(ref subset);
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                ReadOnlySpan<char> character = MemoryMarshal.CreateReadOnlySpan(ref substringChar, 1);
#else
                Span<char> character = stackalloc char[1];
                character[0] = substringChar;
#endif

                Result = superstring.AsSpan().IndexOf(character, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
                Success = true;
            }
            else
            {
                string? substring;
                if (typeof(TSubset) == typeof(string))
                {
                    substring = Unsafe.As<TSubset?, string?>(ref subset);
                }
                else
                {
                    ConvertVisitor<string>.TryConvert(subset, out substring);
                }

                ContainsString(superstring, substring);
            }
            return;
        }

        // string in value
        if (typeof(TSubset) == typeof(string))
        {
            string? substring = Unsafe.As<TSubset?, string?>(ref subset);
            ConvertVisitor<string>.TryConvert(subset, out string? superstring);
            ContainsString(superstring, substring);
            return;
        }

        // value in value (equality)
        EqualityVisitor<TSuperset> equal = default;
        equal.CaseInsensitive = IsCaseInsensitive;
        equal.Value = Superset;
        equal.IsNull = Superset == null;

        equal.Accept(Superset);

        Result = equal.IsEqual;
        Success = equal.Success;
    }

    private void ContainsString(string? s1, string? s2)
    {
        if (string.IsNullOrEmpty(s2))
        {
            Result = true;
            Success = true;
            return;
        }

        if (string.IsNullOrEmpty(s1))
        {
            Result = false;
            Success = true;
            return;
        }


        Result = s1.IndexOf(s2, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) >= 0;
        Success = true;
    }

    /// <inheritdoc />
    public void Accept<T>(EquatableArray<T> subset) where T : IEquatable<T>
    {
        // array in array
        if (EquatableArrayHelper<TSuperset>.IsEquatableArray
            && Superset is IEquatableArray<TSuperset> equatableArray)
        {
            ContainedBothEquatableArrayVisitor<T> visitor;
            visitor.Subset = subset;
            visitor.IsCaseInsensitive = IsCaseInsensitive;
            visitor.Contains = false;
            equatableArray.Visit(ref visitor);
            Success = true;
            Result = visitor.Contains;
            return;
        }

        if (subset.Array == null || subset.Array.Length == 0)
        {
            Result = true;
            Success = true;
        }
        else if (subset.Array.Length == 1)
        {
            EqualityVisitor<T> v = default;
            v.Value = subset.Array[0];
            v.CaseInsensitive = IsCaseInsensitive;
            v.IsNull = v.Value == null;
            v.Accept(Superset);

            Result = v.IsEqual;
            Success = v.Success;
        }
        else
        {
            Success = false;
            Result = false;
        }
    }
}

file struct ContainedBothEquatableArrayVisitor<TSubsetElement> : IEquatableArrayVisitor
    where TSubsetElement : IEquatable<TSubsetElement>
{
    public EquatableArray<TSubsetElement> Subset;
    public bool Contains;
    public bool IsCaseInsensitive;
    public void Accept<TElement>(EquatableArray<TElement> superset) where TElement : IEquatable<TElement>
    {
        Contains = SetOperationsHelper.ContainsAll(superset, Subset, IsCaseInsensitive);
    }
}

file
#if NET7_0_OR_GREATER
    ref
#endif
    struct SupersetIsEquatableArrayVisitor<TSubset> : IEquatableArrayVisitor
    where TSubset : IEquatable<TSubset>
{
#if NET7_0_OR_GREATER
    public ref TSubset? Subset;
#else
    public TSubset? Subset;
#endif
    public bool Contains;
    public bool IsCaseInsensitive;

    public void Accept<TElement>(EquatableArray<TElement> superset) where TElement : IEquatable<TElement>
    {
        Contains = SetOperationsHelper.Contains(superset, Subset, IsCaseInsensitive);
    }
}