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
/// <remarks>Extends <see cref="UnityComponent"/>.</remarks>
public class UnityTransform : UnityComponent, IEnumerable<UnityTransform>
{
    private readonly UnityTransform? _parent;

    private readonly int _level;

    private AssetTypeValueField? _objectBaseField;
    private AssetTypeValueField? _childrenField;

    private UnityTransform?[]? _children;
    private int? _childCount;

    private Dictionary<AssetClassID, UnityComponent>? _components;

    private AssetTypeValueField ChildrenField => _childrenField ??= BaseField["m_Children"];

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    internal AssetTypeValueField GameObjectBaseField
    {
        get
        {
            if (IsDisposed)
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
            if (IsDisposed)
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
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? baseField = BaseFieldIntl;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = BaseFieldIntl;
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
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? baseField = BaseFieldIntl;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = BaseFieldIntl;
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
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            AssetTypeValueField? baseField = BaseFieldIntl;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = BaseFieldIntl;
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
            if (IsDisposed)
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

    internal UnityTransform(
        UnityTransform? parent,
        UnityObject rootObject,
        AssetsFileInstance file,
        AssetFileInfo transformPathInfo,
        int level,
        IParsingServices services)
    : base(rootObject, file, transformPathInfo, services)
    {
        _parent = parent;
        _level = level;
    }

    internal UnityTransform(
        UnityTransform parent,
        UnityObject rootObject,
        AssetsFileInstance file,
        AssetFileInfo transformPathInfo)
    : base(rootObject, file, transformPathInfo, parent.Services)
    {
        _parent = parent;
        _level = parent._level + 1;
    }

    private struct TryGetComponentState
    {
        public UnityComponent? Component;
        public UnityTransform This;
        public AssetClassID Class;
    }

    /// <summary>
    /// Attempt to get a component attached to this object from a <see cref="AssetClassID"/>.
    /// </summary>
    /// <param name="type">The known class ID type of the component.</param>
    /// <returns>Whether or not the component could be found.</returns>
    public bool TryGetComponent(AssetClassID type, [NotNullWhen(true)] out UnityComponent? component)
    {
        switch (type)
        {
            case AssetClassID.GameObject:
            case AssetClassID.AssetBundle:
            case AssetClassID.AssetBundleManifest:
            case AssetClassID.@bool:
            case AssetClassID.@float:
            case AssetClassID.@int:
            case AssetClassID.@void:
                component = null;
                return false;

            case AssetClassID.Object:
            case AssetClassID.Component:
            case AssetClassID.Transform:
                component = this;
                return true;

            case AssetClassID.RectTransform:
                if (Class == AssetClassID.RectTransform)
                {
                    component = this;
                    return true;
                }

                component = null;
                return false;

            default:

                Dictionary<AssetClassID, UnityComponent>? compCache = _components;
                if (compCache != null)
                {
                    lock (compCache)
                    {
                        if (compCache.TryGetValue(type, out UnityComponent comp))
                        {
                            component = comp;
                            return true;
                        }
                    }
                }

                TryGetComponentState state;
                state.Component = null;
                state.This = this;
                state.Class = type;
                BundleUtility.RunOperationOnBundle(ref state, Bundle, Services, static (bundle, _, manager, ref state) =>
                {
                    if (!state.This.TryGetComponentIntlLocked(state.Class, bundle, manager, out state.Component))
                        state.Component = null;
                });

                component = state.Component;
                return component != null;
        }
    }

    private bool TryGetComponentIntlLocked(
        AssetClassID type,
        DiscoveredBundle bundle,
        AssetsManager manager,
        [NotNullWhen(true)] out UnityComponent? component)
    {
        component = null;

        AssetTypeValueField? gameObject = _objectBaseField;
        if (gameObject == null)
        {
            CacheObjectBaseFieldLocked(manager);
            gameObject = _objectBaseField ?? AssetTypeValueField.DUMMY_FIELD;
        }

        if (gameObject.IsDummy || bundle.FilePreloadCache == null)
            return false;

        AssetTypeValueField components = gameObject["m_Component"];
        if (components.IsDummy)
            return false;

        foreach (AssetTypeValueField componentSet in components.Children)
        {
            if (componentSet.Value.ValueType != AssetValueType.Array)
                continue;

            foreach (AssetTypeValueField componentPair in componentSet.Children)
            {
                if (componentPair.Value != null)
                    continue;

                foreach (AssetTypeValueField componentPtr in componentPair.Children)
                {
                    if (!BundleUtility.TryReadPathId(componentPtr, out long pathId))
                        continue;

                    if (!bundle.FilePreloadCache.TryGetValue(pathId, out AssetFileInfo? componentFileInfo)
                        || (AssetClassID)componentFileInfo.TypeId != type)
                    {
                        continue;
                    }

                    UnityComponent comp = new UnityComponent(Object, File, componentFileInfo, Services);

                    Dictionary<AssetClassID, UnityComponent> dictionary = ComponentCache;

                    lock (dictionary)
                    {
                        dictionary[type] = comp;
                    }

                    if (IsDisposed)
                    {
                        lock (dictionary)
                        {
                            if (dictionary.TryGetValue(type, out UnityComponent comp2) && comp == comp2)
                            {
                                dictionary.Remove(type);
                            }
                        }
                        comp.Dispose();
                        throw new ObjectDisposedException(nameof(UnityObject));
                    }

                    component = comp;
                    return true;
                }
            }
        }

        return false;
    }

    private Dictionary<AssetClassID, UnityComponent> ComponentCache
    {
        get
        {
            Dictionary<AssetClassID, UnityComponent>? dictionary = _components;
            if (dictionary == null)
            {
                Dictionary<AssetClassID, UnityComponent> newDict = new Dictionary<AssetClassID, UnityComponent>(1);
                dictionary = Interlocked.CompareExchange(ref _components, newDict, null) ?? newDict;
            }

            return dictionary;
        }
    }

    /// <summary>
    /// Attempt to get a component attached to this object from a <see cref="QualifiedType"/>.
    /// Only works on types that can be mapped to a <see cref="AssetClassID"/>.
    /// </summary>
    /// <param name="type">The fully-qualified type name of the component to get.</param>
    /// <returns>Whether or not the component could be found.</returns>
    public bool TryGetComponent(QualifiedType type, [NotNullWhen(true)] out UnityComponent? component)
    {
        if (!type.IsCaseInsensitive)
            type = type.CaseInsensitive;

        if (Services.Installation.KnownClassIdsByTypes.TryGetValue(type, out AssetClassID classId))
        {
            return TryGetComponent(classId, out component);
        }

        component = null;
        return false;

    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        UnityTransform?[]? children = Interlocked.Exchange(ref _children, null);
        if (children != null)
        {
            for (int i = 0; i < children.Length; ++i)
            {
                children[i]?.Dispose();
            }
        }

        if (disposing)
        {
            _childrenField = null;
            _objectBaseField = null;
        }
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

    [MemberNotNull(nameof(_objectBaseField))]
    private void CacheObjectBaseField()
    {
        BundleUtility.RunOperationOnBundle(this, Bundle, Services, static (_, _, manager, @this) =>
        {
            if (@this._objectBaseField != null)
            {
                return;
            }

            @this.CacheObjectBaseFieldLocked(manager);
        });

        _objectBaseField ??= AssetTypeValueField.DUMMY_FIELD;
        if (!IsDisposed)
            return;

        _objectBaseField = null;
        throw new ObjectDisposedException(nameof(UnityTransform));
    }

    private void CacheObjectBaseFieldLocked(AssetsManager manager)
    {
        CacheBaseFieldLocked(manager);
        if (BaseFieldIntl.IsDummy)
            return;

        AssetTypeValueField gameObjectPtr = BaseField["m_GameObject"];
        if (BundleUtility.TryReadPathId(gameObjectPtr, out long gameObjectPathId))
        {
            _objectBaseField = manager.GetBaseField(File, gameObjectPathId);
        }
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

        CacheBaseFieldLocked(manager);
        if (BaseFieldIntl.IsDummy)
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

        if (!TryGetChildAtIndex(bndl, manager, index, out AssetFileInfo? transformPathInfo))
        {
            throw new FormatException($"Child #{index} not defined. Likely caused by a corrupted bundle.");
        }

        UnityTransform child = new UnityTransform(this, Object, File, transformPathInfo);

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

    private bool TryGetChildAtIndex(DiscoveredBundle bndl, AssetsManager manager, int index, [NotNullWhen(true)] out AssetFileInfo? transformPathInfo)
    {
        if (BaseFieldIntl == null)
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