using System;
using System.Collections.Generic;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Stores one or more generic values efficiently.
/// </summary>
public readonly struct OneOrMore<T> : IEquatable<OneOrMore<T>>, IEquatable<T>
{
    public static readonly OneOrMore<T> Null = new OneOrMore<T>(Array.Empty<T>());

    public delegate T ShiftElement<in TState>(T input, TState state);

#nullable disable
    public readonly T Value;
    public readonly T[] Values;
#nullable restore
    public bool IsNull => Values != null && Values.Length == 0;
    public bool IsSingle => Values == null;

    public int Length => Values?.Length ?? 1;

    public OneOrMore(T value)
    {
        Value = value;
    }

    public OneOrMore(T[] values)
    {
        if (values == null || values.Length == 0)
            Values = Array.Empty<T>();
        else if (values.Length == 1)
            Value = values[0];
        else
            Values = values;
    }

    public bool Contains(T value)
    {
        EqualityComparer<T> comparer = EqualityComparer<T>.Default;
        if (Values == null)
            return comparer.Equals(Value, value);

        for (int i = 0; i < Values.Length; ++i)
        {
            if (comparer.Equals(Values[i], value))
                return true;
        }

        return false;
    }

    public OneOrMore<T> Shift<TState>(ShiftElement<TState> shift, TState state, bool canShiftInPlace)
    {
        if (Values == null)
        {
            return new OneOrMore<T>(shift(Value, state));
        }

        if (Values.Length == 0)
            return Null;

        if (Values.Length == 1)
        {
            return new OneOrMore<T>(shift(Values[0], state));
        }

        if (canShiftInPlace)
        {
            for (int i = 0; i < Values.Length; ++i)
            {
                Values[i] = shift(Values[i], state);
            }

            return this;
        }

        T[] newArray = new T[Values.Length];
        for (int i = 0; i < Values.Length; ++i)
        {
            newArray[i] = shift(Values[i], state);
        }

        return new OneOrMore<T>(newArray);
    }

    public OneOrMore<T> Add(T value)
    {
        if (Values == null)
        {
            return EqualityComparer<T>.Default.Equals(Value, value)
                ? this
                : new OneOrMore<T>([ Value, value ]);
        }
        if (Values.Length == 0)
        {
            return new OneOrMore<T>(value);
        }

        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        if (Values.Length == 1)
        {
            return comparer.Equals(Values[0], value)
                ? new OneOrMore<T>(value)
                : new OneOrMore<T>([ Values[0], value ]);
        }

        for (int i = 0; i < Values.Length; ++i)
        {
            if (comparer.Equals(Values[i], value))
                return this;
        }

        T[] arr = new T[Values.Length + 1];
        for (int i = 0; i < Values.Length; ++i)
            arr[i] = Values[i];
        arr[Values.Length] = value;
        return new OneOrMore<T>(arr);
    }

    public OneOrMore<T> Remove(T value)
    {
        if (Values == null)
        {
            return EqualityComparer<T>.Default.Equals(Value, value) ? Null : this;
        }

        if (Values.Length == 0)
            return this;

        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        if (Values.Length == 1)
            return comparer.Equals(Values[0], value) ? Null : new OneOrMore<T>(Values[0]);

        for (int i = 0; i < Values.Length; ++i)
        {
            if (!comparer.Equals(Values[i], value))
                continue;

            if (Values.Length == 2)
                return new OneOrMore<T>(Values[1 - i]);

            T[] arr = new T[Values.Length - 1];
            for (int k = 0; k < i; ++k)
                arr[k] = Values[k];
            for (int k = i + 1; k < Values.Length; ++k)
                arr[k - 1] = Values[k];
            return new OneOrMore<T>(arr);
        }

        return this;
    }

    /// <inheritdoc />
    public bool Equals(OneOrMore<T> other)
    {
        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;

        if (Values == null)
        {
            if (other.Values == null)
                return comparer.Equals(Value, other.Value);
            if (other.Values.Length == 1)
                return comparer.Equals(Value, other.Values[0]);
            return false;
        }

        if (other.Values == null)
        {
            return Values.Length == 1 && comparer.Equals(Value, Values[0]);
        }

        if (Values.Length == 0)
            return other.Values.Length == 0;
        if (other.Values.Length == 0)
            return false;

        if (Values.Length == 1)
            return other.Values.Length == 1 && comparer.Equals(other.Values[0], Values[0]);

        if (other.Values.Length == 1)
            return Values.Length == 1 && comparer.Equals(Values[0], other.Values[0]);

        if (other.Values.Length != Values.Length)
            return false;

        int n = Values.Length;
        for (int i = 0; i < n; ++i)
        {
            T ind = Values[i];
            bool foundOne = false;
            for (int j = 0; j < n; ++j)
            {
                if (!comparer.Equals(other.Values[(j + i) % n], ind))
                    continue;

                foundOne = true;
                break;
            }

            if (!foundOne)
                return false;
        }

        return true;
    }

    /// <inheritdoc />
    public bool Equals(T other)
    {
        if (Values == null)
            return EqualityComparer<T>.Default.Equals(Value, other);

        return Values.Length == 1 && EqualityComparer<T>.Default.Equals(Values[0], other);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is OneOrMore<T> i && Equals(i);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        if (Values == null || Values.Length == 0)
            return comparer.GetHashCode(Value);

        int num = comparer.GetHashCode(Values[0]);
        for (int i = 1; i < num; ++i)
        {
            int ind = comparer.GetHashCode(Values[i]);
            int shift = i * 4;
            num ^= (ind << shift) | (ind >> 32 - shift - 1);
        }

        return num;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Values == null)
            return Value.ToString();

        if (Values.Length == 0)
            return "-";

        if (Values.Length == 1)
            return Values[0].ToString();

        return "[ " + string.Join(", ", Values) + " ]";
    }

    public static bool operator ==(OneOrMore<T> left, OneOrMore<T> right) => left.Equals(right);
    public static bool operator !=(OneOrMore<T> left, OneOrMore<T> right) => !left.Equals(right);
    public static OneOrMore<T> operator +(OneOrMore<T> left, T index)
    {
        return left.Add(index);
    }

    public static OneOrMore<T> operator -(OneOrMore<T> left, T index)
    {
        return left.Remove(index);
    }

    public static implicit operator OneOrMore<T>(T index) => new OneOrMore<T>(index);
    public static implicit operator OneOrMore<T>(T[] indices) => indices == null || indices.Length == 0 ? Null : new OneOrMore<T>(indices);

    public T Single()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 1)
            return Values[0];

        throw new InvalidOperationException("Expected exactly one item.");
    }
}