using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Represents an object under the hierarchy of a prefab <see cref="UnityObject"/>.
/// </summary>
public class UnityTransform : IEnumerable<UnityTransform>, IDisposable
{
    private readonly UnityTransform? _parent;
    private readonly AssetsFileInstance _file;
    private readonly AssetFileInfo _pathInfo;

    private readonly IParsingServices _services;
    private readonly IBundleProxy _bundle;

    private readonly int _level;

    private AssetTypeValueField? _baseField;
    private AssetTypeValueField? _objectBaseField;

    private UnityTransform?[]? _children;
    private int? _childCount;

    private bool _disposed;

    private AssetTypeValueField ChildrenField => field ??= BaseField["m_Children"];

    /// <summary>
    /// The root object this transform belongs to. This is not the same as a GameObject, as a <see cref="UnityObject"/> references a prefab, not a GameObject.
    /// </summary>
    public UnityObject Object { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal AssetTypeValueField BaseField
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            if (_baseField != null)
                return _baseField;

            CacheBaseField();
            return _baseField ?? AssetTypeValueField.DUMMY_FIELD;
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal AssetTypeValueField GameObjectBaseField
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            if (_objectBaseField != null)
                return _objectBaseField;

            CacheObjectBaseField();
            return _objectBaseField ?? AssetTypeValueField.DUMMY_FIELD;
        }
    }

    /// <summary>
    /// Number of direct children this transform has.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public int ChildCount
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            return _childCount ?? CountChildren();
        }
    }

    /// <summary>
    /// Local position to parent, or world position if this object has no parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public Vector3 Position
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? baseField = _baseField;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = _baseField;
            }

            return BundleUtility.ConstructVector3(baseField, "m_LocalPosition");
        }
    }

    /// <summary>
    /// Local rotation to parent, or world rotation if this object has no parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public Quaternion Rotation
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? baseField = _baseField;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = _baseField;
            }

            return BundleUtility.ConstructQuaternion(baseField, "m_LocalRotation");
        }
    }

    /// <summary>
    /// Local scale to parent, or global scale if this object has no parent.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public Vector3 Scale
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? baseField = _baseField;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = _baseField;
            }

            return BundleUtility.ConstructVector3(baseField, "m_LocalScale");
        }
    }

    /// <summary>
    /// The name of the object this transform represents.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string? Name
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? objectBaseField = _objectBaseField;
            while (objectBaseField == null)
            {
                CacheObjectBaseField();
                objectBaseField = _objectBaseField;
            }

            AssetTypeValueField nameField = objectBaseField["m_Name"];
            if (nameField.IsDummy || nameField.Value.ValueType != AssetValueType.String)
            {
                return null;
            }

            return nameField.Value.AsString;
        }
    }

    /// <summary>
    /// Number of hierarchical levels from being a root object.
    /// </summary>
    /// <remarks>A value of <c>0</c> indicates the transform belongs to a root object, <c>1</c> indicates a child of a root object, etc.</remarks>
    public int Depth => _level;

    public UnityTransform(UnityTransform? parent, UnityObject rootObject, AssetsFileInstance file, AssetFileInfo transformPathInfo, int level, IParsingServices services)
    {
        Object = rootObject;
        _bundle = rootObject.Bundle;

        _parent = parent;
        _file = file;
        _pathInfo = transformPathInfo;
        _level = level;
        _services = services;
    }

    public UnityTransform(UnityTransform parent, UnityObject rootObject, AssetsFileInstance file, AssetFileInfo transformPathInfo)
    {
        _parent = parent;
        _bundle = parent._bundle;
        _file = file;
        _pathInfo = transformPathInfo;
        _level = parent._level + 1;
        _services = parent._services;
        Object = rootObject;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        UnityTransform?[]? children = Interlocked.Exchange(ref _children, null);
        if (children != null)
        {
            for (int i = 0; i < children.Length; ++i)
            {
                children[i]?.Dispose();
            }
        }

        _disposed = true;

        _baseField = null;
        _objectBaseField = null;
    }

    /// <summary>
    /// Get the child at the given index.
    /// </summary>
    /// <param name="index">A zero-based index of a direct child.</param>
    /// <returns>A <see cref="UnityTransform"/> wrapper for the child at index <paramref name="index"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Index is negative or too large.</exception>
    public UnityTransform this[int index]
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (_children != null)
            {
                if (index >= _children.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                UnityTransform? child = Volatile.Read(ref _children[index]);
                if (child != null)
                {
                    return child;
                }
            }

            CreateChild(index);
            return Volatile.Read(ref _children[index])!;
        }
    }

    private struct FindState
    {
        public UnityTransform? Transform;
        public string Path;
    }

    /// <summary>
    /// Find a child object by it's name or path. Behaves the same as Unity's <c>Transform.Find</c> method.
    /// </summary>
    public UnityTransform? Find(string path)
    {
        FindState state;
        state.Transform = this;
        state.Path = path;

        BundleUtility.RunOperationOnBundle(ref state, _bundle, _services, static (bundle, _, manager, ref state) =>
        {
            state.Transform = state.Transform!.FindLocked(state.Path, bundle, manager);
        });

        return state.Transform == this ? null : state.Transform;
    }

    internal UnityTransform? FindLocked(ReadOnlySpan<char> path, DiscoveredBundle bundle, AssetsManager manager)
    {
        for (UnityTransform transform = this;;)
        {
            int nextSlashIndex = path.IndexOf('/');
            ReadOnlySpan<char> name = path;
            if (nextSlashIndex != -1)
            {
                name = path[..nextSlashIndex];
                path = path.Slice(nextSlashIndex + 1);
            }
            else
            {
                path = ReadOnlySpan<char>.Empty;
            }

            int childCt = ChildCount;

            if (transform._children == null)
            {
                transform.CreateChildListLocked(manager);
            }

            bool found = false;
            for (int i = 0; i < childCt; ++i)
            {
                if (transform._children![i] is not { } child)
                {
                    transform.CreateChildLocked(bundle, manager, i);
                    child = transform._children[i];
                }

                if (!name.Equals(child!.Name, StringComparison.Ordinal))
                {
                    continue;
                }

                transform = child;
                found = true;
                break;
            }

            if (!found)
            {
                return null;
            }

            if (path.IsEmpty)
            {
                return transform;
            }
        }
    }

    [MemberNotNull(nameof(_baseField))]
    private void CacheBaseField()
    {
        BundleUtility.RunOperationOnBundle(
            this, _bundle, _services,
            static (_, _, manager, @this) =>
            {
                @this.CacheBaseFieldLocked(manager);
            }
        );

        _baseField ??= AssetTypeValueField.DUMMY_FIELD;
    }

    [MemberNotNull(nameof(_baseField))]
    private void CacheBaseFieldLocked(AssetsManager manager)
    {
        _baseField ??= manager.GetBaseField(_file, _pathInfo) ?? AssetTypeValueField.DUMMY_FIELD;
        if (!_disposed)
            return;

        _baseField = null;
        throw new ObjectDisposedException(nameof(UnityTransform));
    }

    [MemberNotNull(nameof(_objectBaseField))]
    private void CacheObjectBaseField()
    {
        BundleUtility.RunOperationOnBundle(this, _bundle, _services, static (_, _, manager, @this) =>
        {
            if (@this._objectBaseField != null)
            {
                return;
            }

            @this.CacheBaseFieldLocked(manager);
            if (@this._baseField.IsDummy)
            {
                return;
            }

            AssetTypeValueField gameObjectPtr = @this.BaseField["m_GameObject"];
            if (!BundleUtility.TryReadPathId(gameObjectPtr, out long gameObjectPathId))
            {
                return;
            }

            @this._objectBaseField = manager.GetBaseField(@this._file, gameObjectPathId);
        });

        _objectBaseField ??= AssetTypeValueField.DUMMY_FIELD;
        if (!_disposed)
            return;

        _objectBaseField = null;
        throw new ObjectDisposedException(nameof(UnityTransform));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_parent == null)
        {
            return Object.Path;
        }

        StringBuilder sb = new StringBuilder(Object.Path.Length + _level * 15);
        sb.Append(Object.Path);

        for (UnityTransform? transform = this; transform is { _parent: not null }; transform = transform._parent)
        {
            AssetTypeValueField nameField = transform.GameObjectBaseField["m_Name"];
            if (nameField.Value.ValueType == AssetValueType.String)
            {
                sb.Append('/').Append(nameField.Value.AsString);
            }
        }

        return sb.ToString();
    }

    [MemberNotNull(nameof(_childCount))]
    private int CountChildren()
    {
        BundleUtility.RunOperationOnBundle(this, _bundle, _services, static (_, _, manager, @this) =>
        {
            @this.CountChildrenLocked(manager);
        });

        if (_childCount.HasValue)
            return _childCount.Value;

        _childCount = 0;
        return 0;
    }

    [MemberNotNull(nameof(_childCount))]
    private int CountChildrenLocked(AssetsManager manager)
    {
        if (_childCount.HasValue)
        {
            return _childCount.Value;
        }

        CacheBaseFieldLocked(manager);
        if (_baseField.IsDummy)
        {
            _childCount = 0;
            return 0;
        }

        int childCt = 0;
        AssetTypeValueField childField = ChildrenField;
        if (!childField.IsDummy)
        {
            foreach (AssetTypeValueField childSet in childField.Children)
            {
                if (childSet.Value.ValueType != AssetValueType.Array)
                    continue;

                childCt += childSet.AsArray.size;
            }
        }

        _childCount = childCt;
        return childCt;
    }

    [MemberNotNull(nameof(_children))]
    private void CreateChildListLocked(AssetsManager manager)
    {
        if (_children != null)
            return;

        int childCount = _childCount ?? CountChildrenLocked(manager);
        _children = new UnityTransform[childCount];
    }

    private struct CreateChildState
    {
        public UnityTransform This;
        public int Index;
    }

    [MemberNotNull(nameof(_children))]
    private void CreateChild(int index)
    {
        CreateChildState state;
        state.This = this;
        state.Index = index;

        BundleUtility.RunOperationOnBundle(ref state, _bundle, _services, static (bndl, _, manager, ref state) =>
        {
            state.This.CreateChildLocked(bndl, manager, state.Index);
        });

        throw new ArgumentOutOfRangeException(nameof(index));
    }

    [MemberNotNull(nameof(_children))]
    private void CreateChildLocked(DiscoveredBundle bndl, AssetsManager manager, int index)
    {
        if (_children == null)
        {
            CreateChildListLocked(manager);
        }

        UnityTransform?[] children = _children;
        if (children == null)
            throw new ObjectDisposedException(nameof(UnityTransform));
        
        if (index >= children.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        ref UnityTransform? childElem = ref children[index];
        if (childElem != null)
        {
            return;
        }

        if (!TryGetChildAtIndex(bndl, manager, index, out AssetFileInfo? transformPathInfo))
        {
            throw new FormatException($"Child #{index} not defined. Likely caused by a corrupted bundle.");
        }

        UnityTransform child = new UnityTransform(this, Object, _file, transformPathInfo);

        UnityTransform? oldValue = Interlocked.CompareExchange(ref childElem, child, null);
        if (oldValue != null)
        {
            // didn't replace
            child.Dispose();
        }
        else if (_disposed && _children != children)
        {
            child.Dispose();
        }
    }

    private bool TryGetChildAtIndex(DiscoveredBundle bndl, AssetsManager manager, int index, [NotNullWhen(true)] out AssetFileInfo? transformPathInfo)
    {
        if (_baseField == null)
            CacheBaseFieldLocked(manager);

        AssetTypeValueField field = ChildrenField;
        if (!field.IsDummy)
        {
            foreach (AssetTypeValueField childSet in field.Children)
            {
                if (childSet.Value.ValueType != AssetValueType.Array)
                    continue;

                AssetTypeArrayInfo arrayInfo = childSet.AsArray;
                if (index >= arrayInfo.size)
                {
                    index -= arrayInfo.size;
                    continue;
                }

                AssetTypeValueField childField = childSet.Children[index];
                if (childField.IsDummy
                    || !BundleUtility.TryReadPathId(childField, out long transformBaseFieldPathId)
                    || bndl.FilePreloadCache == null
                    || !bndl.FilePreloadCache.TryGetValue(transformBaseFieldPathId, out transformPathInfo))
                {
                    transformPathInfo = null;
                    return false;
                }

                return true;
            }
        }

        // children not defined, treat as no children
        throw new ArgumentOutOfRangeException(nameof(index));
    }

    /// <inheritdoc cref="ChildEnumerator"/>
    public ChildEnumerator GetEnumerator()
    {
        ChildEnumerator enumerator = new ChildEnumerator(this);
        try
        {
            if (!BundleUtility.TryLockBundle(_bundle, _services, ref enumerator.Lock, ref enumerator.HasLock, out enumerator.Bundle, out _, out enumerator.Manager))
            {
                throw new InvalidOperationException("Unable to enumerate children. Bundle could not be loaded.");
            }

            if (_children == null)
            {
                CreateChildListLocked(enumerator.Manager);
            }

            return enumerator;
        }
        catch
        {
            if (enumerator.HasLock)
                PlatformLockHelper.ExitLock(enumerator.Lock!);
            throw;
        }
    }

    IEnumerator<UnityTransform> IEnumerable<UnityTransform>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Enumerates child transforms of a <see cref="UnityTransform"/>.
    /// <para>
    /// Must be disposed after usage to avoid deadlocks.
    /// </para>
    /// </summary>
    /// <remarks>Children are created lazily so they will only be read as the enumerator progresses.</remarks>
    public struct ChildEnumerator : IEnumerator<UnityTransform>
    {
        private readonly UnityTransform _parent;

        internal TfmLock? Lock;
        internal bool HasLock;
        internal DiscoveredBundle? Bundle;
        internal AssetsManager? Manager;

        private int _index;

        internal ChildEnumerator(UnityTransform parent)
        {
            _parent = parent;
        }

#nullable disable

        /// <inheritdoc />
        public UnityTransform Current { get; private set; }

        readonly object IEnumerator.Current => Current;

#nullable restore

        /// <inheritdoc />
        public void Reset()
        {
            _index = 0;
        }

        /// <inheritdoc />
        [MemberNotNullWhen(true, nameof(Current))]
        public bool MoveNext()
        {
            int index = _index;
            ++_index;
            if (index >= _parent._children!.Length)
                return false;

            ref UnityTransform? transform = ref _parent._children[index];
            if (transform == null)
            {
                _parent.CreateChildLocked(Bundle!, Manager!, index);
            }

            Current = transform!;
#pragma warning disable CS8775
            return true;
#pragma warning restore CS8775
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!HasLock)
                return;

            PlatformLockHelper.ExitLock(Lock!);
            HasLock = false;
        }
    }
}