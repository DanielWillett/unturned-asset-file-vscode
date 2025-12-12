using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Stores key-value-pairs with string keys indexed by the first letter in the string.
/// </summary>
public class StringDictionary<T> : IDictionary<string, T>, IReadOnlyDictionary<string, T>, IDictionary
{
    private const int BucketCount = 28;

    private readonly bool _synchronized;
    private readonly IEqualityComparer<string> _keyComparer;
    private readonly IEqualityComparer<T>? _valueComparer;
    private readonly object? _sync;

    private int _version;

    private bool _readonly;

    // buckets sort values by first letter:
    // lower 32 bits: offset, higher 32 bits: count
    // 0-25: a-z letters, 26: numbers, 27: other (or empty)
    private readonly ulong[] _buckets;

    private KeyValuePair<string, T>[]? _entries;

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count { get; private set; }

    /// <inheritdoc cref="IDictionary{string,T}.Keys" />
    public KeyCollection Keys => new KeyCollection(this);

    /// <inheritdoc cref="IDictionary{string,T}.Values" />
    public ValueCollection Values => new ValueCollection(this);

    /// <inheritdoc />
    public bool IsSynchronized => _synchronized && !_readonly;

    /// <inheritdoc />
    public object SyncRoot => _sync!;

    /// <inheritdoc cref="ICollection{T}.IsReadOnly" />
    public bool IsReadOnly => _readonly;

    private StringDictionary(StringDictionary<T> copy)
    {
        _synchronized = copy._synchronized;
        _keyComparer = copy._keyComparer;
        _valueComparer = copy._valueComparer;
        _buckets = new ulong[BucketCount];
        Buffer.BlockCopy(copy._buckets, 0, _buckets, 0, sizeof(ulong) * BucketCount);
        _entries = copy._entries == null || copy._entries.Length == 0 ? null : new KeyValuePair<string, T>[copy._entries.Length];
        if (_entries != null)
        {
            Array.Copy(copy._entries, _entries, _entries.Length);
        }
        _sync = _synchronized ? _buckets : null;
        Count = copy.Count;
    }

    /// <summary>
    /// Create a new <see cref="StringDictionary{T}"/> object.
    /// </summary>
    /// <param name="capacity">Initial capacity for number of objects.</param>
    /// <param name="keyComparer">Comparer used to see if keys are equal and get their hash codes.</param>
    /// <param name="valueComparer">Comparer used to see if values are equal.</param>
    /// <param name="synchronized">If all operations are done using lock statements.</param>
    public StringDictionary(
        int capacity = 0,
        IEqualityComparer<string>? keyComparer = null,
        IEqualityComparer<T>? valueComparer = null,
        bool synchronized = false)
    {
        _synchronized = synchronized;
        _keyComparer = keyComparer ?? StringComparer.Ordinal;
        _valueComparer = valueComparer;
        _buckets = new ulong[BucketCount];
        _entries = capacity <= 0 ? null : new KeyValuePair<string, T>[capacity];
        _sync = _synchronized ? _buckets : null;
    }

    /// <summary>
    /// Mutates this dictionary, or a copy if frozen, and returns it.
    /// </summary>
    public StringDictionary<T> Mutate(Action<StringDictionary<T>> mutation)
    {
        if (_synchronized)
        {
            lock (_sync!)
            {
                if (_readonly)
                {
                    return MutateReadonlyIntl(mutation);
                }

                mutation(this);
                return this;
            }
        }
        else if (_readonly)
        {
            return MutateReadonlyIntl(mutation);
        }

        mutation(this);
        return this;
    }

    private StringDictionary<T> MutateReadonlyIntl(Action<StringDictionary<T>> mutation)
    {
        StringDictionary<T> copy = new StringDictionary<T>(this);
        mutation(copy);
        copy.Freeze();
        return copy;
    }

    /// <summary>
    /// Clear the list and return an array of it's previous values.
    /// </summary>
    public KeyValuePair<string, T>[] Flush()
    {
        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                KeyValuePair<string, T>[] arr = ToArrayIntl();
                ClearIntl();
                return arr;
            }
        }
        else
        {
            AssertNotReadonly();

            KeyValuePair<string, T>[] arr = ToArrayIntl();
            ClearIntl();
            return arr;
        }
    }

    /// <summary>
    /// Copy the pairs in this list to an array.
    /// </summary>
    public KeyValuePair<string, T>[] ToArray()
    {
        if (_synchronized)
        {
            lock (_sync!)
            {
                return ToArrayIntl();
            }
        }
        else
        {
            return ToArrayIntl();
        }
    }

    private KeyValuePair<string, T>[] ToArrayIntl()
    {
        KeyValuePair<string, T>[] arr = new KeyValuePair<string, T>[Count];
        CopyTo(new ArraySegment<KeyValuePair<string, T>>(arr));
        return arr;
    }

    /// <inheritdoc cref="ICollection{T}.Clear" />
    public void Clear()
    {
        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                ClearIntl();
            }
        }
        else
        {
            AssertNotReadonly();

            ClearIntl();
        }
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey" />
    public bool ContainsKey(string key)
    {
        if (!_synchronized || _readonly)
        {
            return ContainsKeyIntl(key);
        }

        lock (_sync!)
        {
            return ContainsKeyIntl(key);
        }
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey" />
    public bool ContainsValue(T value)
    {
        if (!_synchronized || _readonly)
        {
            return ContainsValueIntl(value);
        }

        lock (_sync!)
        {
            return ContainsValueIntl(value);
        }
    }

    /// <inheritdoc />
    public bool Remove(string key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                return RemoveIntl(key);
            }
        }

        AssertNotReadonly();

        return RemoveIntl(key);
    }

    /// <summary>
    /// Gets the element with the given <paramref name="key"/>, or adds a new one using <paramref name="factory"/> if it's not present.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public T GetOrAdd(string key, Func<string, T> factory)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                return GetOrAddIntl(key, factory);
            }
        }

        AssertNotReadonly();

        return GetOrAddIntl(key, factory);
    }

    /// <summary>
    /// Gets the element with the given <paramref name="key"/>, or adds a new one using <paramref name="factory"/> if it's not present.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public T GetOrAdd<TState>(string key, TState state, Func<string, TState, T> factory)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                return GetOrAddIntl(key, in state, factory);
            }
        }

        AssertNotReadonly();

        return GetOrAddIntl(key, in state, factory);
    }


#pragma warning disable CS8767
    /// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue" />
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
    {
        if (!_synchronized || _readonly)
        {
            return TryGetIntl(key, out value);
        }

        lock (_sync!)
        {
            return TryGetIntl(key, out value);
        }
    }
#pragma warning restore CS8767

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
    public T this[string key]
    {
        get
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            bool success;
            T? value;
            if (_synchronized && !_readonly)
            {
                lock (_sync!)
                {
                    success = TryGetIntl(key, out value);
                }
            }
            else
            {
                success = TryGetIntl(key, out value);
            }

            return success ? value! : throw new KeyNotFoundException($"Unable to find a value with key \"{key}\".");
        }
        set
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (_synchronized)
            {
                lock (_sync!)
                {
                    AssertNotReadonly();

                    AddOrUpdateIntl(key, value, update: true);
                }
            }
            else
            {
                AssertNotReadonly();

                AddOrUpdateIntl(key, value, update: true);
            }
        }
    }

    /// <inheritdoc />
    public void Add(string key, T value)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        bool success;
        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                success = AddOrUpdateIntl(key, value, update: false);
            }
        }
        else
        {
            AssertNotReadonly();

            success = AddOrUpdateIntl(key, value, update: false);
        }

        if (!success)
            throw new ArgumentException($"Duplicate key \"{key}\" already exists in dictionary.");
    }

    /// <summary>
    /// Locks this <see cref="StringDictionary{T}"/> so it can no longer be modified.
    /// </summary>
    /// <param name="compress">Reduces the size of the internal array so it's as small as possible given the current data.</param>
    public void Freeze(bool compress = true)
    {
        if (_synchronized)
        {
            lock (_sync!)
            {
                FreezeIntl(compress);
            }
        }
        else
        {
            FreezeIntl(compress);
        }
    }

    /// <summary>
    /// Copy as many elements as possible to the <paramref name="array"/>.
    /// </summary>
    /// <returns>The number of elements copied.</returns>
    public int CopyTo(ArraySegment<KeyValuePair<string, T>> array)
    {
        if (array.Count == 0)
            return 0;

        if (!_synchronized || _readonly)
        {
            return CopyToIntl(array.Array!, array.Offset, array.Count);
        }

        lock (_sync!)
        {
            return CopyToIntl(array.Array!, array.Offset, array.Count);
        }
    }


    private void AssertNotReadonly()
    {
        if (_readonly)
            throw new NotSupportedException("Unable to modify dictionary while frozen.");
    }

    private bool TryGetIntl(string key, [MaybeNullWhen(false)] out T value)
    {
        ParseBucket(_buckets[GetBucketIndex(key)], out int offset, out int count);
        for (int i = 0; i < count; ++i)
        {
            ref KeyValuePair<string, T> entry = ref _entries![i + offset];
            if (_keyComparer.Equals(entry.Key, key))
            {
                value = entry.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private bool ContainsKeyIntl(string key)
    {
        ParseBucket(_buckets[GetBucketIndex(key)], out int offset, out int count);
        for (int i = 0; i < count; ++i)
        {
            ref KeyValuePair<string, T> entry = ref _entries![i + offset];
            if (_keyComparer.Equals(entry.Key, key))
            {
                return true;
            }
        }

        return false;
    }

    private bool ContainsValueIntl(T value)
    {
        IEqualityComparer<T> comparer = _valueComparer ?? EqualityComparer<T>.Default;
        for (int i = 0; i < Count; ++i)
        {
            ref KeyValuePair<string, T> entry = ref _entries![i];
            if (comparer.Equals(entry.Value, value))
                return true;
        }

        return false;
    }

    private bool AddOrUpdateIntl(string key, T value, bool update)
    {
        int bucketIndex = GetBucketIndex(key);
        ref ulong bucket = ref _buckets[bucketIndex];
        ParseBucket(bucket, out int offset, out int count);

        KeyValuePair<string, T> pair = new KeyValuePair<string, T>(key, value);

        if (_entries == null || _entries.Length == 0)
        {
            _entries = new KeyValuePair<string, T>[4];
            _entries[0] = pair;
            bucket = CreateBucket(0, 1);
            Count = 1;
            unchecked { ++_version; }
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
            Array.Fill(_buckets, 1ul, bucketIndex + 1, BucketCount - bucketIndex - 1);
#else
            for (int i = bucketIndex + 1; i < BucketCount; ++i)
                _buckets[i] = 1ul;
#endif
            return true;
        }

        for (int i = 0; i < count; ++i)
        {
            ref KeyValuePair<string, T> entryPair = ref _entries[i + offset];
            if (!_keyComparer.Equals(entryPair.Key, key))
                continue;

            if (update)
            {
                entryPair = pair;
                unchecked { ++_version; }
            }

            return false;
        }

        int amtIncludingCurrentBucket = offset + count;
        if (_entries.Length <= Count)
        {
            // array needs to grow
            KeyValuePair<string, T>[] newArr = new KeyValuePair<string, T>[Math.Max(Count + 1, _entries.Length * 2)];
            if (amtIncludingCurrentBucket != 0)
            {
                Array.Copy(_entries, 0, newArr, 0, amtIncludingCurrentBucket);
            }
            if (amtIncludingCurrentBucket < _entries.Length)
            {
                Array.Copy(_entries, amtIncludingCurrentBucket, newArr, amtIncludingCurrentBucket + 1, Count - amtIncludingCurrentBucket);
            }
            _entries = newArr;
        }
        else if (amtIncludingCurrentBucket < _entries.Length)
        {
            Array.Copy(_entries, amtIncludingCurrentBucket, _entries, amtIncludingCurrentBucket + 1, Count - amtIncludingCurrentBucket);
        }

        for (int b = bucketIndex + 1; b < BucketCount; ++b)
            ++_buckets[b];
        _entries[amtIncludingCurrentBucket] = pair;
        ++Count;
        bucket = CreateBucket(offset, count + 1);
        unchecked { ++_version; }
        return true;
    }

    private bool RemoveIntl(string key)
    {
        int bucketIndex = GetBucketIndex(key);
        ref ulong bucket = ref _buckets[bucketIndex];
        ParseBucket(bucket, out int offset, out int count);
        if (count <= 0)
            return false;

        for (int i = 0; i < count; ++i)
        {
            int index = i + offset;
            ref KeyValuePair<string, T> entry = ref _entries![index];
            if (!_keyComparer.Equals(entry.Key, key))
            {
                continue;
            }

            bucket = CreateBucket(offset, count - 1);

            if (index != Count - 1)
                Array.Copy(_entries, index + 1, _entries, index, Count - index - 1);

            for (int b = bucketIndex + 1; b < BucketCount; ++b)
                --_buckets[b];
            --Count;
            _entries[Count] = default;
            unchecked { ++_version; }
            return true;
        }

        return false;
    }

    private bool RemoveIfIntl(string key, T val)
    {
        int bucketIndex = GetBucketIndex(key);
        ref ulong bucket = ref _buckets[bucketIndex];
        ParseBucket(bucket, out int offset, out int count);
        if (count <= 0)
            return false;

        for (int i = 0; i < count; ++i)
        {
            int index = i + offset;
            ref KeyValuePair<string, T> entry = ref _entries![index];
            if (!_keyComparer.Equals(entry.Key, key))
            {
                continue;
            }

            if (!(_valueComparer ?? EqualityComparer<T>.Default).Equals(entry.Value, val))
            {
                return false;
            }

            bucket = CreateBucket(offset, count - 1);

            if (index != Count - 1)
                Array.Copy(_entries, index + 1, _entries, index, Count - index - 1);

            for (int b = bucketIndex + 1; b < BucketCount; ++b)
                --_buckets[b];

            --Count;
            _entries[Count] = default;
            unchecked { ++_version; }
            return true;
        }

        return false;
    }

    private void ClearIntl()
    {
        if (_entries != null)
        {
            Array.Clear(_entries, 0, Count);
        }

        Count = 0;
        Unsafe.InitBlock(ref Unsafe.As<ulong, byte>(ref _buckets[0]), 0, BucketCount * sizeof(ulong));
        unchecked { ++_version; }
    }

    private T GetOrAddIntl(string key, Func<string, T> factory)
    {
        if (TryGetValue(key, out T? value))
            return value;

        value = factory(key);
        Add(key, value);
        return value;
    }

    private T GetOrAddIntl<TState>(string key, in TState state, Func<string, TState, T> factory)
    {
        if (TryGetValue(key, out T? value))
            return value;

        value = factory(key, state);
        Add(key, value);
        return value;
    }

    private static void ParseBucket(ulong entry, out int offset, out int count)
    {
        unchecked
        {
            offset = (int)entry;
            count = (int)(entry >>> 32);
        }
    }

    private static ulong CreateBucket(int offset, int count)
    {
        unchecked
        {
            return (uint)offset | ((ulong)count << 32);
        }
    }

    private void FreezeIntl(bool compress)
    {
        if (_readonly)
            throw new InvalidOperationException("Already frozen.");

        if (compress && _entries != null && Count < _entries.Length)
        {
            if (Count == 0)
            {
                _entries = null;
            }
            else
            {
                KeyValuePair<string, T>[] newArr = new KeyValuePair<string, T>[Count];
                Array.Copy(_entries, 0, newArr, 0, Count);
                _entries = newArr;
            }
        }

        _readonly = true;
    }

    private static int GetBucketIndex(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return 27;
        }

        char letter = str[0];
        return letter switch
        {
            >= '0' and <= '9' => 26,
            >= 'a' and <= 'z' => letter - 'a',
            >= 'A' and <= 'Z' => letter - 'A',
            _ => 27
        };
    }

    private int CopyToIntl(KeyValuePair<string, T>[] array, int index, int count)
    {
        if (_entries == null || Count == 0)
            return 0;

        count = Math.Min(count, Count);
        Array.Copy(_entries, 0, array, index, count);
        return count;
    }

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
    {
        if (arrayIndex >= array.Length)
        {
            if (arrayIndex == array.Length)
                return;

            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        CopyTo(new ArraySegment<KeyValuePair<string, T>>(array, arrayIndex, array.Length - arrayIndex));
    }

    /// <inheritdoc />
    void IDictionary.Add(object key, object value)
    {
        if (key is not string str)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            throw new ArgumentException("Expected key of type \"String\".");
        }
        if (value is not T t)
        {
            if (typeof(T).IsValueType || value != null)
                throw new ArgumentException($"Expected value of type \"{typeof(T)}\".");

            t = default!;
        }

        Add(str, t);
    }

    /// <inheritdoc />
    void IDictionary.Remove(object key)
    {
        if (key is string str)
        {
            Remove(str);
            return;
        }

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        throw new ArgumentException("Expected key of type \"String\".");
    }

    /// <inheritdoc />
    bool IDictionary.Contains(object key)
    {
        if (key is string str)
            return ContainsKey(str);

        if (key == null)
            throw new ArgumentNullException(nameof(key));

        throw new ArgumentException("Expected key of type \"String\".");
    }

    /// <inheritdoc />
    object? IDictionary.this[object key]
    {
        get
        {
            if (key is string str)
                return this[str];

            if (key == null)
                throw new ArgumentNullException(nameof(key));

            throw new ArgumentException("Expected key of type \"String\".");

        }
        set
        {
            if (key is not string str)
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                throw new ArgumentException("Expected key of type \"String\".");
            }
            if (value is not T t)
            {
                if (typeof(T).IsValueType || value != null)
                    throw new ArgumentException($"Expected value of type \"{typeof(T)}\".");

                t = default!;
            }

            this[str] = t;
        }
    }

    /// <inheritdoc />
    void ICollection.CopyTo(Array array, int arrayIndex)
    {
        if (arrayIndex >= array.Length)
        {
            if (arrayIndex == array.Length)
                return;

            throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        }

        if (!_synchronized || _readonly)
        {
            CopyToUntyped(array, arrayIndex, array.Length - arrayIndex);
            return;
        }

        lock (_sync!)
        {
            CopyToUntyped(array, arrayIndex, array.Length - arrayIndex);
        }

        return;

        void CopyToUntyped(Array array, int index, int count)
        {
            if (_entries == null || Count == 0)
                return;

            count = Math.Min(count, Count);
            Array.Copy(_entries, 0, array, index, count);
        }
    }

    /// <inheritdoc />
    void ICollection<KeyValuePair<string, T>>.Add(KeyValuePair<string, T> item)
    {
        Add(item.Key, item.Value);
    }

    /// <inheritdoc />
    bool ICollection<KeyValuePair<string, T>>.Remove(KeyValuePair<string, T> item)
    {
        if (item.Key == null)
            return false;

        if (_synchronized)
        {
            lock (_sync!)
            {
                AssertNotReadonly();

                return RemoveIfIntl(item.Key, item.Value);
            }
        }

        AssertNotReadonly();

        return RemoveIfIntl(item.Key, item.Value);
    }

    /// <inheritdoc />
    bool ICollection<KeyValuePair<string, T>>.Contains(KeyValuePair<string, T> item)
    {
        return TryGetValue(item.Key, out T? value)
               && (_valueComparer ?? EqualityComparer<T>.Default).Equals(value, item.Value);
    }

    /// <inheritdoc />
    bool IDictionary.IsFixedSize => false;

    /// <inheritdoc />
    ICollection IDictionary.Keys => Values;

    /// <inheritdoc />
    ICollection<string> IDictionary<string, T>.Keys => Keys;

    /// <inheritdoc />
    IEnumerable<string> IReadOnlyDictionary<string, T>.Keys => Keys;

    /// <inheritdoc />
    ICollection IDictionary.Values => Keys;

    /// <inheritdoc />
    ICollection<T> IDictionary<string, T>.Values => Values;

    /// <inheritdoc />
    IEnumerable<T> IReadOnlyDictionary<string, T>.Values => Values;

    /// <inheritdoc />
    IEnumerator<KeyValuePair<string, T>> IEnumerable<KeyValuePair<string, T>>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    IDictionaryEnumerator IDictionary.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    /// <inheritdoc cref="IEnumerable{KeyValuePair{string,T}}.GetEnumerator" />
    public Enumerator GetEnumerator() => new Enumerator(this);


    public readonly struct ValueCollection : ICollection<T>, IReadOnlyCollection<T>, ICollection
    {
        private readonly StringDictionary<T> _dictionary;

        public ValueCollection(StringDictionary<T> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
        public ValueEnumerator GetEnumerator() => new ValueEnumerator(_dictionary);

        /// <inheritdoc />
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        void ICollection<T>.Add(T item)
        {
            _dictionary.AssertNotReadonly();
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        void ICollection<T>.Clear()
        {
            _dictionary.AssertNotReadonly();
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains(T item) => _dictionary.ContainsValue(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (_dictionary is { _synchronized: true, _readonly: false })
            {
                lock (_dictionary._sync!)
                {
                    CopyIntl(array, arrayIndex);
                }
            }
            else
            {
                CopyIntl(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int arrayIndex)
        {
            if (arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (_dictionary is { _synchronized: true, _readonly: false })
            {
                lock (_dictionary._sync!)
                {
                    CopyIntl(array, arrayIndex);
                }
            }
            else
            {
                CopyIntl(array, arrayIndex);
            }
        }

        private void CopyIntl(T[] array, int arrayIndex)
        {
            int count = Math.Min(_dictionary.Count, array.Length - arrayIndex);
            for (int i = 0; i < count; ++i)
            {
                array[i + arrayIndex] = _dictionary._entries![i].Value;
            }
        }

        private void CopyIntl(Array array, int arrayIndex)
        {
            int count = Math.Min(_dictionary.Count, array.Length - arrayIndex);
            for (int i = 0; i < count; ++i)
            {
                array.SetValue(_dictionary._entries![i].Value, i + arrayIndex);
            }
        }

        /// <inheritdoc />
        bool ICollection<T>.Remove(T item)
        {
            _dictionary.AssertNotReadonly();
            throw new NotSupportedException();
        }

        /// <inheritdoc cref="ICollection{T}.Count" />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public bool IsSynchronized => _dictionary.IsSynchronized;

        /// <inheritdoc />
        public object SyncRoot => _dictionary._sync!;

        /// <inheritdoc />
        public bool IsReadOnly => true;

        public struct ValueEnumerator : IEnumerator<T>
        {
            private readonly StringDictionary<T> _dictionary;
            private int _version;
            private int _index;
            private bool _lockTaken;

            /// <inheritdoc />
            public T Current => _dictionary._entries![_index].Value;

            public ValueEnumerator(StringDictionary<T> dictionary)
            {
                _dictionary = dictionary;
                _version = -1;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_version == -1)
                {
                    _index = -1;
                    if (_dictionary is { _synchronized: true, _readonly: false })
                    {
                        if (!_lockTaken)
                            Monitor.Enter(_dictionary._sync, ref _lockTaken);
                    }

                    _version = _dictionary._version;
                }
                else if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException("Dictionary was updated while enumerating, concurrent operations are not supported while enumerating a collection.");
                }

                ++_index;
                return _index < _dictionary.Count;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _version = -1;
            }

            /// <inheritdoc />
            object? IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            {
                if (_lockTaken)
                    Monitor.Exit(_dictionary._sync);
            }
        }
    }

    public readonly struct KeyCollection : ICollection<string>, IReadOnlyCollection<string>, ICollection
    {
        private readonly StringDictionary<T> _dictionary;

        public KeyCollection(StringDictionary<T> dictionary)
        {
            _dictionary = dictionary;
        }

        /// <inheritdoc cref="IEnumerable{string}.GetEnumerator" />
        public KeyEnumerator GetEnumerator() => new KeyEnumerator(_dictionary);

        /// <inheritdoc />
        IEnumerator<string> IEnumerable<string>.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        void ICollection<string>.Add(string item)
        {
            _dictionary.AssertNotReadonly();
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        void ICollection<string>.Clear()
        {
            _dictionary.AssertNotReadonly();
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public bool Contains(string item) => _dictionary.ContainsKey(item);

        /// <inheritdoc />
        public void CopyTo(string[] array, int arrayIndex)
        {
            if (arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (_dictionary is { _synchronized: true, _readonly: false })
            {
                lock (_dictionary._sync!)
                {
                    CopyIntl(array, arrayIndex);
                }
            }
            else
            {
                CopyIntl(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int arrayIndex)
        {
            if (arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));

            if (_dictionary is { _synchronized: true, _readonly: false })
            {
                lock (_dictionary._sync!)
                {
                    CopyIntl(array, arrayIndex);
                }
            }
            else
            {
                CopyIntl(array, arrayIndex);
            }
        }

        private void CopyIntl(string[] array, int arrayIndex)
        {
            int count = Math.Min(_dictionary.Count, array.Length - arrayIndex);
            for (int i = 0; i < count; ++i)
            {
                array[i + arrayIndex] = _dictionary._entries![i].Key;
            }
        }

        private void CopyIntl(Array array, int arrayIndex)
        {
            int count = Math.Min(_dictionary.Count, array.Length - arrayIndex);
            for (int i = 0; i < count; ++i)
            {
                array.SetValue(_dictionary._entries![i].Key, i + arrayIndex);
            }
        }

        /// <inheritdoc />
        bool ICollection<string>.Remove(string item)
        {
            _dictionary.AssertNotReadonly();
            throw new NotSupportedException();
        }

        /// <inheritdoc cref="ICollection{T}.Count" />
        public int Count => _dictionary.Count;

        /// <inheritdoc />
        public bool IsSynchronized => _dictionary.IsSynchronized;

        /// <inheritdoc />
        public object SyncRoot => _dictionary._sync!;

        /// <inheritdoc />
        public bool IsReadOnly => true;

        public struct KeyEnumerator : IEnumerator<string>
        {
            private readonly StringDictionary<T> _dictionary;
            private int _version;
            private int _index;
            private bool _lockTaken;

            /// <inheritdoc />
            public string Current => _dictionary._entries![_index].Key;

            public KeyEnumerator(StringDictionary<T> dictionary)
            {
                _dictionary = dictionary;
                _version = -1;
            }

            /// <inheritdoc />
            public bool MoveNext()
            {
                if (_version == -1)
                {
                    _index = -1;
                    if (_dictionary is { _synchronized: true, _readonly: false })
                    {
                        if (!_lockTaken)
                            Monitor.Enter(_dictionary._sync, ref _lockTaken);
                    }

                    _version = _dictionary._version;
                }
                else if (_version != _dictionary._version)
                {
                    throw new InvalidOperationException("Dictionary was updated while enumerating, concurrent operations are not supported while enumerating a collection.");
                }

                ++_index;
                return _index < _dictionary.Count;
            }

            /// <inheritdoc />
            public void Reset()
            {
                _version = -1;
            }

            /// <inheritdoc />
            object IEnumerator.Current => Current;

            /// <inheritdoc />
            public void Dispose()
            {
                if (_lockTaken)
                    Monitor.Exit(_dictionary._sync);
            }
        }
    }

    public struct Enumerator : IEnumerator<KeyValuePair<string, T>>, IDictionaryEnumerator
    {
        private readonly StringDictionary<T> _dictionary;
        private int _version;
        private int _index;
        private bool _lockTaken;

        /// <inheritdoc />
        public KeyValuePair<string, T> Current => _dictionary._entries![_index];
        public string Key => _dictionary._entries![_index].Key;
        public T Value => _dictionary._entries![_index].Value;

        public Enumerator(StringDictionary<T> dictionary)
        {
            _dictionary = dictionary;
            _version = -1;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (_version == -1)
            {
                _index = -1;
                if (_dictionary is { _synchronized: true, _readonly: false })
                {
                    if (!_lockTaken)
                        Monitor.Enter(_dictionary._sync, ref _lockTaken);
                }

                _version = _dictionary._version;
            }
            else if (_version != _dictionary._version)
            {
                throw new InvalidOperationException("Dictionary was updated while enumerating, concurrent operations are not supported while enumerating a collection.");
            }

            ++_index;
            return _index < _dictionary.Count;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _version = -1;
        }

        /// <inheritdoc />
        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose()
        {
            if (_lockTaken)
                Monitor.Exit(_dictionary._sync);
        }

        /// <inheritdoc />
        DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(Key, Value);

        /// <inheritdoc />
        object IDictionaryEnumerator.Key => Key;

        /// <inheritdoc />
        object? IDictionaryEnumerator.Value => Value;
    }
}