using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Stores one or more generic values efficiently.
/// </summary>
[CollectionBuilder(typeof(OneOrMoreExtensions), nameof(OneOrMoreExtensions.Create))]
public readonly struct OneOrMore<T> : IEquatable<OneOrMore<T>>, IEquatable<T>, IEnumerable<T>
{
    /// <summary>
    /// A container with no elements.
    /// </summary>
    public static readonly OneOrMore<T> Null = new OneOrMore<T>(Array.Empty<T>());

    public delegate TValue SelectWithStateByRef<out TValue, TState>(T input, in TState state) where TState : struct;
    public delegate bool WhereWithStateByRef<TState>(T input, in TState state) where TState : struct;

#nullable disable
    /// <summary>
    /// A single value represented by this container.
    /// </summary>
    /// <remarks>When <see cref="Values"/> is <see langword="null"/>, this field always represents a single value (even if this value is <see langword="null"/>).</remarks>
    public readonly T Value;

    /// <summary>
    /// The backing array of values, may be null if there is only one value.
    /// </summary>
    /// <remarks>A value of null for this field indicates a length of one, where the only element is <see cref="Value"/> (even if <see cref="Value"/> is <see langword="null"/>).</remarks>
    public readonly T[] Values;
#nullable restore

    /// <summary>
    /// If there are no values stored in this container.
    /// </summary>
    public bool IsNull => Values != null && Values.Length == 0;

    /// <summary>
    /// If there is exactly one value stored in this container.
    /// </summary>
    public bool IsSingle => Values == null || Values.Length == 1;

    /// <summary>
    /// Number of elements stored in this container.
    /// </summary>
    public int Length => Values?.Length ?? 1;

    /// <summary>
    /// Create a container surrounding one value.
    /// </summary>
    public OneOrMore(T value)
    {
        Value = value;
    }

    private OneOrMore(T[] array, T? value)
    {
        Value = value;
        Values = array;
    }

    internal static OneOrMore<T> CreateUnsafe(T[] array, T? value)
    {
        return new OneOrMore<T>(array, value);
    }

    /// <summary>
    /// Create a container surrounding multiple values.
    /// <para>
    /// This does not copy <paramref name="values"/>, so changes made to the array outside this container may corrupt it's state.
    /// </para>
    /// </summary>
    /// <remarks>Duplicate values will be removed.</remarks>
    public OneOrMore(T[] values)
    {
        if (values == null || values.Length == 0)
        {
            Values = Array.Empty<T>();
            return;
        }
        if (values.Length == 1)
        {
            Value = values[0];
            return;
        }

        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        if (values.Length == 2)
        {
            if (comparer.Equals(values[0], values[1]))
                Value = values[0];
            else
                Values = values;
            return;
        }

        // remove duplicates
        int count = values.Length;
        LightweightBitArray dupeMask = new LightweightBitArray(count);

        for (int i = 0; i < values.Length; ++i)
        {
            if (i > 0 && dupeMask[i])
                continue;

            T val = values[i];
            for (int j = i + 1; j < values.Length; ++j)
            {
                if (i != 0 && dupeMask[j] || !comparer.Equals(val, values[j]))
                    continue;

                dupeMask[j] = true;
                --count;
            }
        }

        if (count == values.Length)
        {
            Values = values;
            return;
        }

        if (count == 1)
        {
            Value = values[0];
            return;
        }

        T[] newArray = new T[count];
        count = 0;
        newArray[0] = values[0];
        for (int i = 1; i < values.Length; ++i)
        {
            if (dupeMask[i])
                continue;

            newArray[++count] = values[i];
        }

        Values = newArray;
    }

    /// <summary>
    /// Create a container surrounding multiple values.
    /// </summary>
    /// <remarks>Duplicate values will be removed.</remarks>
    public OneOrMore(IList<T> values)
    {
        if (values == null || values.Count == 0)
        {
            Values = Array.Empty<T>();
            return;
        }

        if (values.Count == 1)
        {
            Value = values[0];
            return;
        }

        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        if (values.Count == 2)
        {
            T i0 = values[0], i1 = values[1];

            if (comparer.Equals(i0, i1))
                Value = i0;
            else
                Values = [ i0, i1 ];
            return;
        }

        // remove duplicates
        int count = values.Count;
        LightweightBitArray dupeMask = new LightweightBitArray(count);

        for (int i = 0; i < values.Count; ++i)
        {
            if (i > 0 && dupeMask[i])
                continue;

            T val = values[i];
            for (int j = i + 1; j < values.Count; ++j)
            {
                if (i != 0 && dupeMask[j] || !comparer.Equals(val, values[j]))
                    continue;

                dupeMask[j] = true;
                --count;
            }
        }

        if (count == values.Count)
        {
            T[] array = new T[count];
            values.CopyTo(array, 0);
            Values = array;
            return;
        }

        if (count == 1)
        {
            Value = values[0];
            return;
        }

        T[] newArray = new T[count];
        count = 0;
        newArray[0] = values[0];
        for (int i = 1; i < values.Count; ++i)
        {
            if (dupeMask[i])
                continue;

            newArray[++count] = values[i];
        }

        Values = newArray;
    }

    /// <summary>
    /// Gets the value at a certain index in this container.
    /// </summary>
    public T this[int index]
    {
        get
        {
            if (Values == null)
            {
                if (index != 0)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return Value;
            }

            if (index < 0 || index >= Values.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            return Values[index];
        }
    }

    /// <summary>
    /// Copy this container to an array.
    /// </summary>
    public T[] ToArray()
    {
        if (Values == null)
            return [ Value ];
        if (Values.Length == 0)
            return Array.Empty<T>();

        T[] arr = new T[Values.Length];
        for (int i = 0; i < arr.Length; ++i)
            arr[i] = Values[i];
        return arr;
    }

    /// <summary>
    /// Gets this container as an array, possibly using the underlying array in use by this container.
    /// </summary>
    public T[] GetArray()
    {
        if (Values == null)
            return [Value];
        if (Values.Length == 0)
            return Array.Empty<T>();

        return Values;
    }

    /// <summary>
    /// Copy this container to a new list.
    /// </summary>
    public List<T> ToList()
    {
        if (Values == null)
            return new List<T>(1) { Value };
        if (Values.Length == 0)
            return new List<T>(0);

        return [ ..Values ];
    }

    /// <summary>
    /// Gets the first value added, or throws an exception if there are no items in this container.
    /// </summary>
    /// <exception cref="InvalidOperationException">There are no items in this collection.</exception>
    public T First()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 0)
            throw new InvalidOperationException("No values in collection.");

        return Values[0];
    }

    /// <summary>
    /// Gets the first value added, or <see langword="default"/> if there are no items in this container.
    /// </summary>
    public T? FirstOrDefault()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 0)
            return default;

        return Values[0];
    }

    /// <summary>
    /// Gets the first value added matching <paramref name="selector"/>, or <see langword="default"/> if there are no matching items in this container.
    /// </summary>
    public T? FirstOrDefault(Func<T, bool> selector)
    {
        if (Values == null)
        {
            return selector(Value) ? Value : default;
        }

        if (Values.Length == 0)
            return default;

        for (int i = 0; i < Values.Length; ++i)
        {
            T v = Values[0];
            if (selector(v))
                return v;
        }

        return default;
    }

    /// <summary>
    /// Checks if there are any values matching <paramref name="selector"/>.
    /// </summary>
    public bool Any(Func<T, bool> selector)
    {
        if (Values == null)
        {
            return selector(Value);
        }

        if (Values.Length == 0)
            return false;

        for (int i = 0; i < Values.Length; ++i)
        {
            T v = Values[0];
            if (selector(v))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if all values match <paramref name="selector"/>.
    /// </summary>
    /// <remarks>Returns <see langword="true"/> if there are no elements.</remarks>
    public bool All(Func<T, bool> selector)
    {
        if (Values == null)
        {
            return selector(Value);
        }

        if (Values.Length == 0)
            return true;

        for (int i = 0; i < Values.Length; ++i)
        {
            T v = Values[0];
            if (!selector(v))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the first value added matching <paramref name="selector"/>, or throws an exception if there are no matching items in this container.
    /// </summary>
    /// <exception cref="InvalidOperationException">There are no matching items in this collection.</exception>
    public T? First(Func<T, bool> selector)
    {
        if (Values == null)
        {
            return selector(Value) ? Value : throw new InvalidOperationException("No values in collection.");
        }

        if (Values.Length == 0)
            throw new InvalidOperationException("No values in collection.");

        for (int i = 0; i < Values.Length; ++i)
        {
            T v = Values[0];
            if (selector(v))
                return v;
        }

        throw new InvalidOperationException("No values in collection.");
    }

    /// <summary>
    /// Gets the last value added, or throws an exception if there are no items in this container.
    /// </summary>
    /// <exception cref="InvalidOperationException">There are no items in this collection.</exception>
    public T Last()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 0)
            throw new InvalidOperationException("No values in collection.");

        return Values[Values.Length - 1];
    }

    /// <summary>
    /// Gets the last value added, or <see langword="default"/> if there are no items in this container.
    /// </summary>
    public T? LastOrDefault()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 0)
            return default;

        return Values[Values.Length - 1];
    }

    /// <summary>
    /// Gets the single value in this container, or throws an exception if there is not exactly one value in this container.
    /// </summary>
    /// <exception cref="InvalidOperationException">There is not exactly one item.</exception>
    public T Single()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 1)
            return Values[0];

        throw new InvalidOperationException("Expected exactly one item.");
    }

    /// <summary>
    /// Gets the single value in this container, or <see langword="default"/> if there are no values or more than one value.
    /// </summary>
    public T? SingleOrDefault()
    {
        if (Values == null)
            return Value;

        if (Values.Length == 1)
            return Values[0];

        return default;
    }

    /// <summary>
    /// Gets a new container with only values in this container matching a selector.
    /// </summary>
    public OneOrMore<T> Where(Func<T, bool> selector)
    {
        if (Values == null)
            return selector(Value) ? this : default;

        if (Values.Length == 1)
            return selector(Values[0]) ? new OneOrMore<T>(Values[0]) : default;

        LightweightBitArray mask = new LightweightBitArray(Values.Length);
        int ct = 0;
        T? single = default;
        for (int i = 0; i < Values.Length; ++i)
        {
            bool match = selector(Values[i]);
            if (!match)
                continue;

            if (ct == 0)
                single = Values[i];
            ++ct;
            mask[i] = true;
        }

        if (ct == Values.Length)
            return this;

        if (ct == 0)
            return Null;

        if (ct == 1)
            return new OneOrMore<T>(single!);

        T[] outArray = new T[ct];
        int index = -1;
        for (int i = 0; i < Values.Length; ++i)
        {
            if (!mask[i])
                continue;

            outArray[++index] = Values[i];
        }

        return new OneOrMore<T>(outArray);
    }

    /// <summary>
    /// Gets a new container with only values in this container matching a selector.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations.</param>
    public OneOrMore<T> Where<TState>(Func<T, TState, bool> selector, TState state)
    {
        if (Values == null)
            return selector(Value, state) ? this : default;

        if (Values.Length == 1)
            return selector(Values[0], state) ? new OneOrMore<T>(Values[0]) : default;

        LightweightBitArray mask = new LightweightBitArray(Values.Length);
        int ct = 0;
        T? single = default;
        for (int i = 0; i < Values.Length; ++i)
        {
            bool match = selector(Values[i], state);
            if (!match)
                continue;

            if (ct == 0)
                single = Values[i];
            ++ct;
            mask[i] = true;
        }

        if (ct == Values.Length)
            return this;

        if (ct == 0)
            return Null;

        if (ct == 1)
            return new OneOrMore<T>(single!);

        T[] outArray = new T[ct];
        int index = -1;
        for (int i = 0; i < Values.Length; ++i)
        {
            if (!mask[i])
                continue;

            outArray[++index] = Values[i];
        }

        return new OneOrMore<T>(outArray);
    }

    /// <summary>
    /// Gets a new container with only values in this container matching a selector.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations. Passed by-ref for larger structs.</param>
    public OneOrMore<T> Where<TState>(WhereWithStateByRef<TState> selector, TState state) where TState : struct
    {
        if (Values == null)
            return selector(Value, in state) ? this : default;

        if (Values.Length == 1)
            return selector(Values[0], in state) ? new OneOrMore<T>(Values[0]) : default;

        LightweightBitArray mask = new LightweightBitArray(Values.Length);
        int ct = 0;
        T? single = default;
        for (int i = 0; i < Values.Length; ++i)
        {
            bool match = selector(Values[i], in state);
            if (!match)
                continue;

            if (ct == 0)
                single = Values[i];
            ++ct;
            mask[i] = true;
        }

        if (ct == Values.Length)
            return this;

        if (ct == 0)
            return Null;

        if (ct == 1)
            return new OneOrMore<T>(single!);

        T[] outArray = new T[ct];
        int index = -1;
        for (int i = 0; i < Values.Length; ++i)
        {
            if (!mask[i])
                continue;

            outArray[++index] = Values[i];
        }

        return new OneOrMore<T>(outArray);
    }

    /// <summary>
    /// Check if a value is contained in this container.
    /// </summary>
    [Pure]
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

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, returning a new container.
    /// </summary>
    [Pure]
    public OneOrMore<TValue> Select<TValue>(Func<T, TValue> modify)
    {
        if (Values == null)
        {
            return new OneOrMore<TValue>(modify(Value));
        }

        if (Values.Length == 0)
        {
            return OneOrMore<TValue>.Null;
        }

        TValue[] outValues = new TValue[Values.Length];
        for (int i = 0; i < Values.Length; ++i)
        {
            outValues[i] = modify(Values[i]);
        }

        return new OneOrMore<TValue>(outValues);
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, returning a new container.
    /// </summary>
    [Pure]
    public OneOrMore<T> Select(Func<T, T> modify)
    {
        return Select<T>(modify);
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, returning a new container.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations.</param>
    [Pure]
    public OneOrMore<TValue> Select<TValue, TState>(SelectWithStateByRef<TValue, TState> select, in TState state) where TState : struct
    {
        if (Values == null)
        {
            return new OneOrMore<TValue>(select(Value, in state));
        }

        if (Values.Length == 0)
            return OneOrMore<TValue>.Null;

        if (Values.Length == 1)
        {
            return new OneOrMore<TValue>(select(Values[0], in state));
        }

        TValue[] newArray = new TValue[Values.Length];
        for (int i = 0; i < Values.Length; ++i)
        {
            newArray[i] = select(Values[i], state);
        }

        return new OneOrMore<TValue>(newArray);
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, returning a new container.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations. Passed by-ref for larger structs.</param>
    [Pure]
    public OneOrMore<T> Select<TState>(SelectWithStateByRef<T, TState> modify, in TState state) where TState : struct
    {
        return Select<T, TState>(modify, in state);
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, returning a new container.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations. Passed by-ref for larger structs.</param>
    [Pure]
    public OneOrMore<TValue> Select<TValue, TState>(Func<T, TState, TValue> select, TState state)
    {
        if (Values == null)
        {
            return new OneOrMore<TValue>(select(Value, state));
        }

        if (Values.Length == 0)
            return OneOrMore<TValue>.Null;

        if (Values.Length == 1)
        {
            return new OneOrMore<TValue>(select(Values[0], state));
        }

        TValue[] newArray = new TValue[Values.Length];
        for (int i = 0; i < Values.Length; ++i)
        {
            newArray[i] = select(Values[i], state);
        }

        return new OneOrMore<TValue>(newArray);
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, returning a new container.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations.</param>
    [Pure]
    public OneOrMore<T> Select<TState>(Func<T, TState, T> modify, TState state)
    {
        return Select<T, TState>(modify, state);
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, but will use the same array if there are more than one values in this container.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations. Passed by-ref for larger structs.</param>
    public OneOrMore<T> SelectInPlace<TState>(SelectWithStateByRef<T, TState> modify, in TState state) where TState : struct
    {
        if (Values == null)
        {
            return new OneOrMore<T>(modify(Value, in state));
        }

        if (Values.Length == 0)
            return Null;

        if (Values.Length == 1)
        {
            return new OneOrMore<T>(modify(Values[0], in state));
        }

        for (int i = 0; i < Values.Length; ++i)
        {
            Values[i] = modify(Values[i], in state);
        }

        return this;
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, but will use the same array if there are more than one values in this container.
    /// </summary>
    /// <param name="state">An arbitrary value passed to the lambda function to avoid closure allocations.</param>
    public OneOrMore<T> SelectInPlace<TState>(Func<T, TState, T> modify, TState state)
    {
        if (Values == null)
        {
            return new OneOrMore<T>(modify(Value, state));
        }

        if (Values.Length == 0)
            return Null;

        if (Values.Length == 1)
        {
            return new OneOrMore<T>(modify(Values[0], state));
        }

        for (int i = 0; i < Values.Length; ++i)
        {
            Values[i] = modify(Values[i], state);
        }

        return this;
    }

    /// <summary>
    /// Performs a select operation, mapping elements 1:1 using a conversion lambda function, but will use the same array if there are more than one values in this container.
    /// </summary>
    public OneOrMore<T> SelectInPlace(Func<T, T> modify)
    {
        if (Values == null)
        {
            return new OneOrMore<T>(modify(Value));
        }

        if (Values.Length == 0)
            return Null;

        if (Values.Length == 1)
        {
            return new OneOrMore<T>(modify(Values[0]));
        }

        for (int i = 0; i < Values.Length; ++i)
        {
            Values[i] = modify(Values[i]);
        }

        return this;
    }

    /// <summary>
    /// Appends an element to the end of the values in this container and returns a new container.
    /// </summary>
    [Pure]
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

    /// <summary>
    /// Removes an element from the values in this container and returns a new container.
    /// </summary>
    [Pure]
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


    /// <summary>
    /// Checks if the lists contain the same set of values (in any order) using the default <see cref="IEqualityComparer{T}"/>.
    /// </summary>
    /// <param name="other">The other list.</param>
    [Pure]
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

        if (ReferenceEquals(Values, other.Values))
        {
            return true;
        }

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

    /// <summary>
    /// Checks if the lists contain the same set of values (in any order) using an equality function.
    /// </summary>
    /// <param name="other">The other list.</param>
    /// <param name="equality">A function that defines whether or not two values are equal.</param>
    [Pure]
    public bool Equals(OneOrMore<T> other, Func<T, T, bool> equality)
    {
        if (Values == null)
        {
            if (other.Values == null)
                return equality(Value, other.Value);
            if (other.Values.Length == 1)
                return equality(Value, other.Values[0]);
            return false;
        }

        if (other.Values == null)
        {
            return Values.Length == 1 && equality(Value, Values[0]);
        }

        if (Values.Length == 0)
            return other.Values.Length == 0;
        if (other.Values.Length == 0)
            return false;

        if (Values.Length == 1)
            return other.Values.Length == 1 && equality(other.Values[0], Values[0]);

        if (other.Values.Length == 1)
            return Values.Length == 1 && equality(Values[0], other.Values[0]);

        if (other.Values.Length != Values.Length)
            return false;

        if (ReferenceEquals(Values, other.Values))
        {
            return true;
        }

        int n = Values.Length;
        for (int i = 0; i < n; ++i)
        {
            T ind = Values[i];
            bool foundOne = false;
            for (int j = 0; j < n; ++j)
            {
                if (!equality(other.Values[(j + i) % n], ind))
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
    [Pure]
    public bool Equals(T other)
    {
        if (Values == null)
            return EqualityComparer<T>.Default.Equals(Value, other);

        return Values.Length == 1 && EqualityComparer<T>.Default.Equals(Values[0], other);
    }

    [Pure]
    public OneOrMoreEnumerator<T> GetEnumerator() => new OneOrMoreEnumerator<T>(this);

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj) => obj is OneOrMore<T> i && Equals(i);

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        if (Values == null)
            return EqualityComparer<T>.Default.GetHashCode(Value);
        if (Values.Length == 0)
            return 0;

        IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        int hashCode = 0;
        unchecked
        {
            for (int i = 0; i < Values.Length; ++i)
            {
                hashCode = (hashCode * 397) ^ comparer.GetHashCode(Values[i]);
            }
        }

        return hashCode;
    }

    /// <summary>
    /// Get the hash code of this collection with a custom hash code function.
    /// </summary>
    [Pure]
    public int GetHashCode(Func<T, int> getHashCode)
    {
        if (Values == null)
            return getHashCode(Value);
        if (Values.Length == 0)
            return 0;

        int hashCode = 0;
        unchecked
        {
            for (int i = 0; i < Values.Length; ++i)
            {
                hashCode = (hashCode * 397) ^ getHashCode(Values[i]);
            }
        }

        return hashCode;
    }

    /// <inheritdoc />
    [Pure]
    public override string ToString()
    {
        if (Values == null)
            return Value?.ToString() ?? string.Empty;

        if (Values.Length == 0)
            return "-";

        if (Values.Length == 1)
            return Values[0]?.ToString() ?? string.Empty;

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
    public static bool operator true(OneOrMore<T> container) => !container.IsNull;
    public static bool operator false(OneOrMore<T> container) => container.IsNull;
    public static OneOrMore<T> operator |(OneOrMore<T> left, OneOrMore<T> right)
    {
        if (!left.IsNull)
            return left;
        if (!right.IsNull)
            return right;

        return Null;
    }

    public static OneOrMore<T> operator &(OneOrMore<T> left, OneOrMore<T> right)
    {
        if (left.IsNull || right.IsNull)
            return Null;
        return left;
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new OneOrMoreEnumerator<T>(this);
    IEnumerator IEnumerable.GetEnumerator() => new OneOrMoreEnumerator<T>(this);
}

/// <summary>
/// Enumerates through a <see cref="OneOrMore{T}"/> container.
/// </summary>
public struct OneOrMoreEnumerator<T> : IEnumerator<T>
{
    private readonly OneOrMore<T> _container;
    private int _index;

    /// <inheritdoc />
    public readonly T Current => _container[_index];

    public OneOrMoreEnumerator(OneOrMore<T> container)
    {
        _container = container;
        _index = -1;
    }

    /// <inheritdoc />
    public bool MoveNext()
    {
        ++_index;
        return _index < _container.Length;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _index = -1;
    }

    /// <summary>
    /// Unused.
    /// </summary>
    public readonly void Dispose() { }

    /// <inheritdoc />
    readonly object? IEnumerator.Current => _container[_index];
}

public static class OneOrMoreExtensions
{
    /// <summary>
    /// Create a <see cref="OneOrMore{T}"/> using a read-only span. Used by the <see cref="CollectionBuilderAttribute"/> for <see cref="OneOrMore{T}"/>.
    /// </summary>
    [SkipLocalsInit, Pure]
    public static OneOrMore<T> Create<T>(ReadOnlySpan<T> span)
    {
        IEqualityComparer<T> comparer;
        switch (span.Length)
        {
            case 0:
                return OneOrMore<T>.Null;

            case 1:
                return new OneOrMore<T>(span[0]);

            case 2:
                comparer = EqualityComparer<T>.Default;
                return comparer.Equals(span[0], span[1])
                    ? new OneOrMore<T>(span[0])
                    : new OneOrMore<T>([ span[0], span[1] ]);
        }

        comparer = EqualityComparer<T>.Default;
        // remove duplicates
        int count = span.Length;
        LightweightBitArray dupeMask = new LightweightBitArray(count);

        for (int i = 0; i < span.Length; ++i)
        {
            if (i > 0 && dupeMask[i])
                continue;

            T val = span[i];
            for (int j = i + 1; j < span.Length; ++j)
            {
                if (i != 0 && dupeMask[j] || !comparer.Equals(val, span[j]))
                    continue;

                dupeMask[j] = true;
                --count;
            }
        }

        if (count == span.Length)
        {
            return new OneOrMore<T>(span.ToArray());
        }

        if (count == 1)
        {
            return new OneOrMore<T>(span[0]);
        }

        T[] newArray = new T[count];
        count = 0;
        newArray[0] = span[0];
        for (int i = 1; i < span.Length; ++i)
        {
            if (dupeMask[i])
                continue;

            newArray[++count] = span[i];
        }

        return new OneOrMore<T>(newArray);
    }

    /// <summary>
    /// Adds all values from a <see cref="OneOrMore{T}"/> to <paramref name="list"/>.
    /// </summary>
    public static void AddToList<T>(this OneOrMore<T> oneOrMore, List<T> list)
    {
        int capacity = list.Count + oneOrMore.Length;
        if (capacity < list.Capacity)
            list.Capacity = capacity;

        if (oneOrMore.Values == null)
        {
            list.Add(oneOrMore.Value);
            return;
        }

        if (oneOrMore.Values.Length != 0)
            list.AddRange(oneOrMore.Values);
    }

    /// <summary>
    /// Attempts to get a value from a <see cref="OneOrMore{T}"/> acting as a dictionary.
    /// </summary>
    public static bool TryGetValue<TKey, TValue>(this OneOrMore<KeyValuePair<TKey, TValue>> dictionary, TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        foreach (KeyValuePair<TKey, TValue> pair in dictionary)
        {
            if (!EqualityComparer<TKey>.Default.Equals(pair.Key, key))
                continue;
            
            value = pair.Value;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to get a value from a <see cref="OneOrMore{T}"/> acting as a dictionary based on a string comparison type.
    /// </summary>
    public static bool TryGetValue<TValue>(this OneOrMore<KeyValuePair<string, TValue>> dictionary, string key, [MaybeNullWhen(false)] out TValue value, StringComparison stringComparison)
    {
        foreach (KeyValuePair<string, TValue> pair in dictionary)
        {
            if (!string.Equals(pair.Key, key, stringComparison))
                continue;
            
            value = pair.Value;
            return true;
        }

        value = default!;
        return false;
    }

    /// <summary>
    /// Attempts to get a value from a <see cref="OneOrMore{T}"/> acting as a dictionary based on a string comparison type.
    /// </summary>
    [return: NotNullIfNotNull(nameof(defaultValue))]
    public static TValue? GetValueOrDefault<TValue>(this OneOrMore<KeyValuePair<string, TValue>> dictionary, string key, TValue? defaultValue, StringComparison stringComparison)
    {
        return dictionary.TryGetValue(key, out TValue? value, stringComparison) ? value : defaultValue;
    }

    /// <summary>
    /// Attempts to get a value from a <see cref="OneOrMore{T}"/> acting as a dictionary based on a string comparison type.
    /// </summary>
    public static TValue? GetValueOrDefault<TValue>(this OneOrMore<KeyValuePair<string, TValue>> dictionary, string key, StringComparison stringComparison)
    {
        return dictionary.TryGetValue(key, out TValue? value, stringComparison) ? value : default;
    }

    /// <summary>
    /// Check if a string is contained in this container based on a string comparison type.
    /// </summary>
    [Pure]
    public static bool Contains(this OneOrMore<string> stringList, string value, StringComparison comparisonType)
    {
        if (stringList.Values == null)
        {
            return string.Equals(stringList.Value, value, comparisonType);
        }
        if (stringList.Values.Length == 0)
        {
            return false;
        }

        string[] values = stringList.Values;
        for (int i = 0; i < values.Length; ++i)
        {
            if (string.Equals(values[i], value, comparisonType))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Adds a string to a container based on a string comparison type and returns the new container.
    /// </summary>
    [Pure]
    public static OneOrMore<string> Add(this OneOrMore<string> stringList, string value, StringComparison comparisonType)
    {
        if (stringList.Values == null)
        {
            return string.Equals(stringList.Value, value, comparisonType)
                ? stringList
                : new OneOrMore<string>([ stringList.Value, value ]);
        }
        if (stringList.Values.Length == 0)
        {
            return new OneOrMore<string>(value);
        }

        if (stringList.Values.Length == 1)
        {
            return string.Equals(stringList.Values[0], value, comparisonType)
                ? new OneOrMore<string>(value)
                : new OneOrMore<string>([ stringList.Values[0], value ]);
        }

        for (int i = 0; i < stringList.Values.Length; ++i)
        {
            if (string.Equals(stringList.Values[i], value, comparisonType))
                return stringList;
        }

        string[] arr = new string[stringList.Values.Length + 1];
        for (int i = 0; i < stringList.Values.Length; ++i)
            arr[i] = stringList.Values[i];
        arr[stringList.Values.Length] = value;
        return new OneOrMore<string>(arr);
    }

    /// <summary>
    /// Remove a string from a container based on a string comparison type and returns the new container.
    /// </summary>
    [Pure]
    public static OneOrMore<string> Remove(this OneOrMore<string> stringList, string value, StringComparison comparisonType)
    {
        RemoveStringState state = new RemoveStringState
        {
            Value = value,
            Comparison = comparisonType
        };

        return stringList.Where(
            static (string x, in RemoveStringState state) => !string.Equals(x, state.Value, state.Comparison),
            state
        );
    }

    /// <summary>
    /// Removes duplicate strings from a container based on a string comparison type and returns the new container.
    /// </summary>
    [Pure]
    public static OneOrMore<string> RemoveDuplicates(this OneOrMore<string> stringList, StringComparison comparisonType)
    {
        string[]? values = stringList.Values;
        string? value = stringList.Value;

        if (values == null)
        {
            return new OneOrMore<string>(value);
        }
        if (values.Length == 1)
        {
            return new OneOrMore<string>(values);
        }
        if (values.Length == 0)
        {
            return OneOrMore<string>.Null;
        }

        if (values.Length == 2)
        {
            return string.Equals(values[0], values[1], comparisonType)
                ? new OneOrMore<string>(values[0])
                : stringList;
        }

        // remove duplicates
        int count = values.Length;
        LightweightBitArray dupeMask = new LightweightBitArray(count);

        for (int i = 0; i < values.Length; ++i)
        {
            if (i > 0 && dupeMask[i])
                continue;

            string val = values[i];
            for (int j = i + 1; j < values.Length; ++j)
            {
                if (i != 0 && dupeMask[j] || !string.Equals(val, values[j], comparisonType))
                    continue;

                dupeMask[j] = true;
                --count;
            }
        }

        if (count == values.Length)
        {
            return stringList;
        }

        string[] newArray = new string[count];
        count = 0;
        newArray[0] = values[0];
        for (int i = 1; i < values.Length; ++i)
        {
            if (dupeMask[i])
                continue;

            newArray[++count] = values[i];
        }

        return new OneOrMore<string>(newArray);
    }

    private struct RemoveStringState
    {
        public required StringComparison Comparison;
        public required string Value;
    }

    /// <summary>
    /// Compare two different container's contents, taking a <see cref="StringComparison"/> into account.
    /// </summary>
    [Pure]
    public static bool Equals(this OneOrMore<string> stringList, OneOrMore<string> other, StringComparison comparisonType)
    {
        if (stringList.Values == null)
        {
            if (other.Values == null)
                return string.Equals(stringList.Value, other.Value, comparisonType);
            if (other.Values.Length == 1)
                return string.Equals(stringList.Value, other.Values[0], comparisonType);
            return false;
        }

        if (other.Values == null)
        {
            return stringList.Values.Length == 1 && string.Equals(stringList.Value, stringList.Values[0], comparisonType);
        }

        if (stringList.Values.Length == 0)
            return other.Values.Length == 0;
        if (other.Values.Length == 0)
            return false;

        if (stringList.Values.Length == 1)
            return other.Values.Length == 1 && string.Equals(other.Values[0], stringList.Values[0], comparisonType);

        if (other.Values.Length == 1)
            return stringList.Values.Length == 1 && string.Equals(stringList.Values[0], other.Values[0], comparisonType);

        if (other.Values.Length != stringList.Values.Length)
            return false;

        if (ReferenceEquals(stringList.Values, other.Values))
        {
            return true;
        }

        int n = stringList.Values.Length;
        for (int i = 0; i < n; ++i)
        {
            string ind = stringList.Values[i];
            bool foundOne = false;
            for (int j = 0; j < n; ++j)
            {
                if (!string.Equals(other.Values[(j + i) % n], ind, comparisonType))
                    continue;

                foundOne = true;
                break;
            }

            if (!foundOne)
                return false;
        }

        return true;
    }
    
    /// <summary>
    /// Get a hash code of this container's contents, taking a <see cref="StringComparison"/> into account.
    /// </summary>
    [Pure]
    public static int GetHashCode(this OneOrMore<string> stringList, StringComparison comparisonType)
    {
        if (stringList.Values is { Length: 0 })
            return 0;

        StringComparer comparer = comparisonType switch
        {
            StringComparison.CurrentCulture => StringComparer.CurrentCulture,
            StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
            StringComparison.InvariantCulture => StringComparer.InvariantCulture,
            StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
            StringComparison.Ordinal => StringComparer.Ordinal,
            StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
            _ => StringComparer.Ordinal
        };

        if (stringList.Values == null)
            return comparer.GetHashCode(stringList.Value);

        int hashCode = 0;
        unchecked
        {
            for (int i = 0; i < stringList.Values.Length; ++i)
            {
                hashCode = (hashCode * 397) ^ comparer.GetHashCode(stringList.Values[i]);
            }
        }

        return hashCode;
    }
}