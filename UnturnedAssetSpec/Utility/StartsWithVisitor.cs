using System;
using System.Runtime.CompilerServices;
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
using System.Runtime.InteropServices;
#endif

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

internal struct StartsWithVisitor<TSet> : IGenericVisitor, IEquatableArrayVisitor
    where TSet : IEquatable<TSet>
{
    public TSet? Set;
    public bool IsCaseInsensitive;
    public bool Success;
    public bool Result;

    /// <summary>
    /// Checks whether a value is at the beginning of a value. If one of these values are strings, a string starts-with operation will be performed instead.
    /// </summary>
    /// <typeparam name="TSubset">The type of value that will be checked to see if it's at the beginning of <paramref name="set"/>.</typeparam>
    /// <param name="set">The value to check whether or not <paramref name="value"/> is at the beginning of.</param>
    /// <param name="value">The value to look for in <paramref name="set"/>.</param>
    /// <param name="doesContain">Whether or not <paramref name="value"/> is at the beginning of <paramref name="set"/>.</param>
    /// <returns>Whether or not the check was successful.</returns>
    public static bool TryGetStartsWith<TSubset>(TSet? set, TSubset? value, out bool doesContain)
        where TSubset : IEquatable<TSubset>
    {
        return TryGetStartsWith(set, value, false, out doesContain);
    }

    /// <summary>
    /// Checks whether a value is at the beginning of a value. If one of these values are strings, a string starts-with operation will be performed instead.
    /// </summary>
    /// <typeparam name="TSubset">The type of value that will be checked to see if it's at the beginning of <paramref name="set"/>.</typeparam>
    /// <param name="set">The value to check whether or not <paramref name="value"/> is at the beginning of.</param>
    /// <param name="value">The value to look for in <paramref name="set"/>.</param>
    /// <param name="isCaseInsensitive">Whether or not the check should ignore character casing.</param>
    /// <param name="doesContain">Whether or not <paramref name="value"/> is at the beginning of <paramref name="set"/>.</param>
    /// <returns>Whether or not the check was successful.</returns>
    public static bool TryGetStartsWith<TSubset>(TSet? set, TSubset? value, bool isCaseInsensitive, out bool doesContain)
        where TSubset : IEquatable<TSubset>
    {
        StartsWithVisitor<TSet> visitor = default;
        visitor.IsCaseInsensitive = isCaseInsensitive;
        visitor.Set = set;

        visitor.Accept(value);

        doesContain = visitor.Result;
        return visitor.Success;
    }

    public void Accept<TSubset>(TSubset? subset) where TSubset : IEquatable<TSubset>
    {
        // array starts with array (calls Accept in this type)
        if (EquatableArrayHelper<TSubset>.IsEquatableArray
            && subset is IEquatableArray<TSubset> subsetStrongTyped)
        {
            subsetStrongTyped.Visit(ref this);
            return;
        }

        // value in array (calls accept in SupersetIsEquatableArrayVisitor)
        if (EquatableArrayHelper<TSet>.IsEquatableArray
            && Set is IEquatableArray<TSet> supersetStrongTyped)
        {
#if NET9_0_OR_GREATER
            scoped SupersetIsEquatableArrayVisitor<TSubset> visitor;
            visitor.Subset = ref subset;
#else
            SupersetIsEquatableArrayVisitor<TSubset> visitor;
            visitor.Subset = subset;
#endif
            visitor.IsCaseInsensitive = IsCaseInsensitive;
            visitor.StartsWith = false;
            supersetStrongTyped.Visit(ref visitor);
            Result = visitor.StartsWith;
            Success = true;
            return;
        }

        // string starts with value
        if (typeof(TSet) == typeof(string))
        {
            string? superstring = Unsafe.As<TSet?, string?>(ref Set);
            if (typeof(TSubset) == typeof(char))
            {
                if (string.IsNullOrEmpty(superstring))
                {
                    Result = false;
                    Success = true;
                    return;
                }

                // string starts with character
                char substringChar = Unsafe.As<TSubset?, char>(ref subset);
                if (!IsCaseInsensitive)
                {
                    Result = superstring[0] == substringChar;
                }
                else
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                    ReadOnlySpan<char> character = MemoryMarshal.CreateReadOnlySpan(ref substringChar, 1);
#else
                    Span<char> character = stackalloc char[1];
                    character[0] = substringChar;
#endif

                    Result = superstring.AsSpan().StartsWith(character, StringComparison.OrdinalIgnoreCase);
                }

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

                StartsWithString(superstring, substring);
            }
            return;
        }

        // value starts with string
        if (typeof(TSubset) == typeof(string))
        {
            string? substring = Unsafe.As<TSubset?, string?>(ref subset);
            ConvertVisitor<string>.TryConvert(subset, out string? superstring);
            StartsWithString(superstring, substring);
            return;
        }

        // value starts with value (equality)
        EqualityVisitor<TSet> equal = default;
        equal.CaseInsensitive = IsCaseInsensitive;
        equal.Value = Set;
        equal.IsNull = Set == null;

        equal.Accept(Set);

        Result = equal.IsEqual;
        Success = equal.Success;
    }

    private void StartsWithString(string? s1, string? s2)
    {
        if (string.IsNullOrEmpty(s2))
        {
            Result = true;
        }
        else if (string.IsNullOrEmpty(s1))
        {
            Result = false;
        }
        else
        {
            Result = s1.StartsWith(s2, IsCaseInsensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        Success = true;
    }

    /// <inheritdoc />
    public void Accept<T>(EquatableArray<T> subset) where T : IEquatable<T>
    {
        // array starts with array
        if (EquatableArrayHelper<TSet>.IsEquatableArray
            && Set is IEquatableArray<TSet> equatableArray)
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
            v.Accept(Set);

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
        Contains = SetOperationsHelper.StartsWith(superset, Subset, IsCaseInsensitive);
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
    public bool StartsWith;
    public bool IsCaseInsensitive;

    public void Accept<TElement>(EquatableArray<TElement> superset) where TElement : IEquatable<TElement>
    {
        StartsWith = SetOperationsHelper.StartsWith(superset, Subset, IsCaseInsensitive);
    }
}