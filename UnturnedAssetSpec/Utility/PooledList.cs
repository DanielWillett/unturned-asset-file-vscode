using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// A disposable list that works from the array pool.
/// </summary>
/// <typeparam name="T"></typeparam>
public class PooledList<T> : IList<T>, IReadOnlyList<T>, IList, IDisposable
{
    private readonly IEqualityComparer<T>? _equalityComparer;
    private readonly ArrayPool<T>? _arrayPool;

#nullable disable
    private T[] _items;
#nullable restore
    private int _size;
    private int _version;

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => _size;

    public PooledList() : this(0) { }

    public PooledList(int capacity, IEqualityComparer<T>? equalityComparer = null, ArrayPool<T>? arrayPool = null)
    {
        _equalityComparer = equalityComparer;
        _arrayPool = arrayPool;

        _items = capacity <= 0 ? Array.Empty<T>() : ArrayPool<T>.Shared.Rent(capacity);
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator" />
    public Enumerator GetEnumerator() => new Enumerator(this);

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => new Enumerator(this);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this);

    /// <inheritdoc />
    public void Add(T item)
    {
        Extend(1);
        _items[_size] = item;
        ++_size;
        ++_version;
    }

    /// <inheritdoc cref="ICollection{T}.Clear" />
    public void Clear()
    {
        int size = _size;
        _size = 0;
        ++_version;
        if (size > 0)
        {
            Array.Clear(_items, 0, size);
        }
    }

    /// <inheritdoc />
    public bool Contains(T item) => IndexOf(item) >= 0;

    private void Extend(int i)
    {
        if (_size + i <= _items.Length)
            return;

        ArrayPool<T> pool = _arrayPool ?? ArrayPool<T>.Shared;
        T[] newArray = pool.Rent(Math.Max(_size + i, _items.Length * 2));
        T[] oldArray = Interlocked.Exchange(ref _items, newArray);
        try
        {
            if (oldArray.Length <= 0)
                return;

            if (_size > 0)
            {
                Array.Copy(oldArray, 0, newArray, 0, _size);
                Array.Clear(oldArray, 0, _size);
            }
        }
        finally
        {
            pool.Return(oldArray);
        }
    }

    public T[] ToArray()
    {
        if (_size == 0)
            return Array.Empty<T>();

        T[] newArray = new T[_size];
        Array.Copy(_items, 0, newArray, 0, _size);
        return newArray;
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (arrayIndex > array.Length || arrayIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(arrayIndex));

        int len = Math.Min(_size, array.Length - arrayIndex);
        if (len == 0)
            return;

        Array.Copy(_items, 0, array, arrayIndex, len);
    }


    /// <inheritdoc />
    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index < 0)
            return false;

        RemoveAt(index);
        return true;
    }

    /// <inheritdoc />
    public int IndexOf(T item)
    {
        IEqualityComparer<T> comparer = _equalityComparer ?? EqualityComparer<T>.Default;

        for (int i = 0; i < _size; ++i)
        {
            if (comparer.Equals(item, _items[i]))
                return i;
        }

        return -1;
    }

    /// <inheritdoc />
    public void Insert(int index, T item)
    {
        if (index > _size || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index == _size)
        {
            Add(item);
            return;
        }

        Extend(1);
        Array.Copy(_items, index, _items, index + 1, _size - index);
        _items[index] = item;
        ++_size;
        ++_version;
    }

    /// <inheritdoc cref="IList{T}.RemoveAt" />
    public void RemoveAt(int index)
    {
        if (index >= _size || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        if (index == _size - 1)
        {
            --_size;
            ++_version;
            _items[_size] = default;
            return;
        }

        int size = _size;
        --_size;
        ++_version;
        Array.Copy(_items, index + 1, _items, index, size - index - 1);
        _items[_size] = default;
    }


    /// <inheritdoc cref="IList{T}.this" />
    public T this[int index]
    {
        get
        {
            if (index >= _items.Length || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _items[index];
        }
        set
        {
            if (index >= _items.Length || index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            _items[index] = value;
            ++_version;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Clear();
        T[] arr = Interlocked.Exchange(ref _items, Array.Empty<T>());
        if (arr.Length > 0)
        {
            (_arrayPool ?? ArrayPool<T>.Shared).Return(arr);
        }
    }

    bool ICollection.IsSynchronized => false;
    object ICollection.SyncRoot => throw new NotSupportedException();
    bool ICollection<T>.IsReadOnly => false;
    bool IList.IsFixedSize => false;
    bool IList.IsReadOnly => false;
    int IList.Add(object? value)
    {
        Extend(1);
        int size = _size;
        _items[size] = (T?)value;
        ++_size;
        ++_version;
        return size;
    }
    void ICollection.CopyTo(Array array, int index)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (index > array.Length || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        int len = Math.Min(_size, array.Length - index);
        if (len == 0)
            return;

        Array.Copy(_items, 0, array, index, len);
    }
    /// <inheritdoc />
#nullable disable
    object IList.this[int index]
    {
        get => this[index];
        set => this[index] = (T)value;
    }
#nullable restore
    bool IList.Contains(object? value)
    {
        return ((IList)this).IndexOf(value) >= 0;
    }
    int IList.IndexOf(object value)
    {
        if (default(T) == null)
        {
            if (value == null)
                return IndexOf(default!);
        }
        else if (value == null)
            return -1;

        return value is T t ? IndexOf(t) : -1;
    }
    void IList.Insert(int index, object value)
    {
        Insert(index, (T)value);
    }
    void IList.Remove(object value)
    {
        if (default(T) == null)
        {
            if (value == null)
            {
                Remove(default!);
                return;
            }
        }
        else if (value == null)
            return;

        if (value is T t)
            Remove(t);
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly PooledList<T> _pooledList;
        private int _version;
        private int _index;

        public Enumerator(PooledList<T> pooledList)
        {
            _pooledList = pooledList;
            _version = _pooledList._version;
            _index = -1;
        }

        /// <inheritdoc />
        public T Current => _pooledList._items[_index];

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (++_index >= _pooledList.Count)
                return false;

            if (_version != _pooledList._version)
                throw new InvalidOperationException();

            return true;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _index = -1;
            _version = _pooledList._version;
        }

        /// <inheritdoc />
        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }
    }
}