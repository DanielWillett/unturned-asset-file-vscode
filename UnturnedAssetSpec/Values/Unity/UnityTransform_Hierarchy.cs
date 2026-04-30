using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

partial class UnityTransform : IEnumerable<UnityTransform>
{
    private readonly UnityTransform? _parent;

    private AssetTypeValueField? _childrenField;
    private UnityTransform?[]? _children;
    private int? _childCount;

    /// <summary>
    /// Number of direct children this transform has.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public int ChildCount
    {
        get
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            return _childCount ?? CountChildren();
        }
    }

    /// <summary>
    /// Number of hierarchical levels from being a root object.
    /// </summary>
    /// <remarks>A value of <c>0</c> indicates the transform belongs to a root object, <c>1</c> indicates a child of a root object, etc.</remarks>
    public int Depth { get; }

    /// <summary>
    /// Index of this transform within it's siblings.
    /// <c>-1</c> if this transform is attached to a root object such as a prefab.
    /// </summary>
    public int SiblingIndex { get; }

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
            if (IsDisposed)
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

    /// <inheritdoc cref="ChildEnumerator"/>
    public ChildEnumerator GetEnumerator()
    {
        ChildEnumerator enumerator = new ChildEnumerator(this);
        try
        {
            if (!BundleUtility.TryLockBundle(Bundle, Services, ref enumerator.Lock, ref enumerator.HasLock, out enumerator.Bundle, out _, out enumerator.Manager))
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

        BundleUtility.RunOperationOnBundle(ref state, Bundle, Services, static (bundle, _, manager, ref state) =>
        {
            state.Transform = state.Transform!.FindLocked(state.Path, bundle, manager);
        });

        return state.Transform == this ? null : state.Transform;
    }

    internal UnityTransform? FindLocked(ReadOnlySpan<char> path, DiscoveredBundle bundle, AssetsManager manager)
    {
        for (UnityTransform transform = this; ;)
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

    private AssetTypeValueField GetChildrenFieldLocked(AssetsManager manager)
    {
        AssetTypeValueField? childrenField = _childrenField;
        while (childrenField == null)
        {
            AssetTypeValueField? baseField = BaseFieldIntl;
            while (baseField == null)
            {
                CacheBaseFieldLocked(manager);
                baseField = BaseFieldIntl;
            }

            childrenField = !baseField.IsDummy
                ? baseField["m_Children"]
                : AssetTypeValueField.DUMMY_FIELD;

            if (childrenField.IsDummy)
            {
                Interlocked.CompareExchange(ref _childrenField, AssetTypeValueField.DUMMY_FIELD, null);
                return _childrenField ?? AssetTypeValueField.DUMMY_FIELD;
            }

            _childrenField = childrenField;
        }

        return childrenField;
    }

    [MemberNotNull(nameof(_childCount))]
    private int CountChildren()
    {
        BundleUtility.RunOperationOnBundle(this, Bundle, Services, static (_, _, manager, @this) =>
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

        int childCt = 0;

        AssetTypeValueField childField = GetChildrenFieldLocked(manager);
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

        BundleUtility.RunOperationOnBundle(ref state, Bundle, Services, static (bndl, _, manager, ref state) =>
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

        if (!TryGetChildAtIndexLocked(bndl, manager, index, out AssetFileInfo? transformPathInfo))
        {
            throw new FormatException($"Child #{index} not defined. Likely caused by a corrupted bundle.");
        }

        UnityTransform child = new UnityTransform(this, Object, File, index, transformPathInfo);

        UnityTransform? oldValue = Interlocked.CompareExchange(ref childElem, child, null);
        if (oldValue != null)
        {
            // didn't replace
            child.Dispose();
        }
        else if (IsDisposed && _children != children)
        {
            child.Dispose();
        }
    }

    private bool TryGetChildAtIndexLocked(
        DiscoveredBundle bndl,
        AssetsManager manager,
        int index,
        [NotNullWhen(true)] out AssetFileInfo? transformPathInfo)
    {
        AssetTypeValueField field = GetChildrenFieldLocked(manager);
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
                    || !bndl.TryGetFileInfoFromPathId(transformBaseFieldPathId, out transformPathInfo))
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

    private ChildEnumerator EnumerateChildrenLocked(DiscoveredBundle bundle, AssetsManager manager)
    {
        ChildEnumerator enumerator = new ChildEnumerator(this)
        {
            Bundle = bundle,
            Manager = manager
        };

        if (_children == null)
        {
            CreateChildListLocked(enumerator.Manager);
        }

        return enumerator;
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
            UnityTransform?[] children = _parent._children ?? throw new ObjectDisposedException(nameof(UnityTransform));
            if (index >= children.Length)
                return false;

            ref UnityTransform? transform = ref children[index];
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