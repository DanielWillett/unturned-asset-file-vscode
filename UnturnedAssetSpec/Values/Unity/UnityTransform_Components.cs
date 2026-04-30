using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

partial class UnityTransform
{
    private AssetTypeValueField? _componentsArray;
    private UnityComponent?[]? _components;

    private struct TryGetComponentByClassState
    {
        public UnityComponent? Component;
        public UnityTransform This;
        public AssetClassID Class;
    }
    private struct TryGetComponentByTypeState
    {
        public UnityComponent? Component;
        public UnityTransform This;
        public QualifiedType Type;
        public QueryComponentOptions Options;
    }
    private struct GetComponentState
    {
        public UnityComponent? Component;
        public UnityTransform This;
        public int Index;
    }

    /// <summary>
    /// Number of direct children this transform has.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public int ComponentCount
    {
        get
        {
            UnityComponent?[]? componentList = _components;
            while (componentList == null)
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(nameof(UnityTransform));

                BundleUtility.RunOperationOnBundle(
                    this, Bundle, Services,
                    static (_, _, manager, @this) =>
                    {
                        @this.CreateComponentListLocked(manager);
                    }
                );

                componentList = _components;
            }

            return componentList.Length;
        }
    }

    /// <summary>
    /// Get the component at the given index on this transform's object.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"/>
    /// <exception cref="FormatException"/>
    public UnityComponent GetComponent(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        GetComponentState state;
        state.This = this;
        state.Index = index;
        state.Component = null;
        BundleUtility.RunOperationOnBundle(ref state, Bundle, Services, static (bundle, _, manager, ref state) =>
        {
            state.Component = state.This.GetComponentLocked(state.Index, bundle, manager);
        });

        return state.Component ?? throw new FormatException("Invalid bundle format.");
    }

    private UnityComponent? GetComponentLocked(int index, DiscoveredBundle bundle, AssetsManager manager)
    {
        UnityComponent?[]? componentList = _components;
        while (componentList == null)
        {
            CreateComponentListLocked(manager);
            componentList = _components;
        }

        if (index < 0 || index >= componentList.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        UnityComponent? comp = Volatile.Read(ref componentList[index]);
        if (comp != null)
        {
            return comp;
        }

        AssetTypeValueField compArray = GetComponentsArrayLocked(manager);
        if (compArray.IsDummy)
        {
            return null;
        }

        List<AssetTypeValueField> children = compArray.Children;
        if (index >= children.Count)
        {
            return null;
        }

        AssetTypeValueField componentPair = children[index];
        if (componentPair.Children.Count < 1)
            return null;

        AssetTypeValueField componentPtr = componentPair.Children[0];
        if (componentPtr.Value != null)
            return null;

        if (!BundleUtility.TryReadPathId(componentPtr, out long pathId))
            return null;

        if (pathId == PathInfo.PathId)
        {
            return this;
        }

        if (!bundle.TryGetFileInfoFromPathId(pathId, out AssetFileInfo? componentFileInfo))
        {
            return null;
        }

        return GetComponentLocked(componentFileInfo, componentList, index);
    }

    private UnityComponent? GetComponentLocked(AssetFileInfo componentFileInfo, UnityComponent?[] componentList, int index)
    {
        AssetClassID classId = (AssetClassID)componentFileInfo.TypeId;
        if (!Services.Installation.KnownUnityClassTypes.TryGetValue(classId, out IBundleAssetType? type))
        {
            return null;
        }

        QualifiedType typeName = type.TypeName;
        UnityComponent comp = new UnityComponent(Object, File, PathInfo, Services, this, typeName);
        UnityComponent? oldComp = Interlocked.CompareExchange(ref componentList[index], comp, null);
        if (oldComp != null)
        {
            comp.Dispose();
            return oldComp;
        }

        return comp;
    }

    /// <summary>
    /// Attempt to get a component attached to this object from a <see cref="AssetClassID"/>.
    /// </summary>
    /// <param name="type">The known class ID type of the component.</param>
    /// <returns>Whether or not the component could be found.</returns>
    public bool TryGetComponent(
        AssetClassID type,
        [NotNullWhen(true)] out UnityComponent? component,
        QueryComponentOptions options = QueryComponentOptions.None)
    {
        switch (type)
        {
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
                if (options != QueryComponentOptions.None
                    && Services.Installation.KnownUnityClassTypes.TryGetValue(type, out IBundleAssetType? actualType))
                {
                    TryGetComponentByTypeState state2;
                    state2.Component = null;
                    state2.This = this;
                    state2.Type = actualType.TypeName;
                    state2.Options = options;
                    BundleUtility.RunOperationOnBundle(ref state2, Bundle, Services, static (bundle, _, manager, ref state) =>
                    {
                        if (!state.This.TryGetComponentIntlLocked(state.Type, bundle, manager, out state.Component))
                            state.Component = null;
                    });

                    component = state2.Component;
                    return component != null;
                }

                TryGetComponentByClassState state;
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

    /// <summary>
    /// Enumerate all components.
    /// </summary>
    /// <param name="options">Changes how components are searched.</param>
    /// <returns>An enumerator that steps through matching components, loading them as the enumerator progresses.</returns>
    public ComponentEnumerator EnumerateComponents(QueryComponentOptions options = QueryComponentOptions.None)
    {
        return EnumerateComponents(QualifiedType.None, options);
    }

    /// <summary>
    /// Enumerate all components of the given type (including sub-classes, unless specified in <paramref name="options"/>).
    /// </summary>
    /// <param name="type">Base type of all components.</param>
    /// <param name="options">Changes how components are searched.</param>
    /// <returns>An enumerator that steps through matching components, loading them as the enumerator progresses.</returns>
    public ComponentEnumerator EnumerateComponents(QualifiedType type, QueryComponentOptions options = QueryComponentOptions.None)
    {
        ComponentEnumerator enumerator = new ComponentEnumerator(this, type, options);

        try
        {
            if (!BundleUtility.TryLockBundle(Bundle, Services, ref enumerator.Lock, ref enumerator.HasLock, out enumerator.Bundle, out _, out enumerator.Manager))
            {
                throw new InvalidOperationException("Unable to enumerate children. Bundle could not be loaded.");
            }

            if (_components == null)
            {
                CreateComponentListLocked(enumerator.Manager);
            }

            return enumerator;
        }
        catch
        {
            enumerator.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Attempt to get a component attached to this object from a <see cref="QualifiedType"/>.
    /// Only works on types that can be mapped to a <see cref="AssetClassID"/>.
    /// </summary>
    /// <param name="type">The fully-qualified type name of the component to get.</param>
    /// <returns>Whether or not the component could be found.</returns>
    public bool TryGetComponent(
        QualifiedType type,
        [NotNullWhen(true)] out UnityComponent? component,
        QueryComponentOptions options = QueryComponentOptions.None)
    {
        VerifyOptions(options);

        if (!type.IsCaseInsensitive)
            type = type.CaseInsensitive;

        if (type.Equals("UnityEngine.Object, UnityEngine.CoreModule")
            || type.Equals("UnityEngine.Component, UnityEngine.CoreModule")
            || type.Equals("UnityEngine.Transform, UnityEngine.CoreModule"))
        {
            component = this;
            return true;
        }

        if (type.Equals("UnityEngine.RectTransform, UnityEngine.CoreModule"))
        {
            if (Class == AssetClassID.RectTransform)
            {
                component = this;
                return true;
            }

            component = null;
            return false;
        }

        TryGetComponentByTypeState state;
        state.This = this;
        state.Type = type;
        state.Component = null;
        state.Options = options;

        BundleUtility.RunOperationOnBundle(ref state, Bundle, Services, static (bundle, _, manager, ref state) =>
        {
            if (!state.This.TryGetComponentIntlLocked(state.Type, bundle, manager, out state.Component, state.Options))
                state.Component = null;
        });

        component = state.Component;
        return state.Component != null;
    }

    /// <summary>
    /// Attempt to get a component attached to this object from a <see cref="QualifiedType"/>.
    /// Only works on types that can be mapped to a <see cref="AssetClassID"/>.
    /// </summary>
    /// <param name="type">The fully-qualified type name of the component to get.</param>
    /// <returns>Whether or not the component could be found.</returns>
    private bool TryGetComponentIntlLocked(
        QualifiedType type,
        DiscoveredBundle bundle,
        AssetsManager manager,
        [NotNullWhen(true)] out UnityComponent? component,
        QueryComponentOptions options = QueryComponentOptions.None)
    {
        if (Services.Installation.KnownClassIdsByTypes.TryGetValue(type, out AssetClassID classId))
        {
            if (TryGetComponentIntlLocked(classId, bundle, manager, out component))
                return true;
        }

        if ((options & QueryComponentOptions.ExactTypeOnly) == 0 && Services.Database.Information.TryGetHierarchy(type, out TypeHierarchy? hierarchy))
        {
            foreach (KeyValuePair<QualifiedType, TypeHierarchy> h in hierarchy.ChildTypes)
            {
                if (TryGetComponentIntlLocked(h.Value, bundle, manager, out component))
                    return true;
            }
        }

        if ((options & QueryComponentOptions.InChildren) != 0)
        {
            using ChildEnumerator childEnumerator = EnumerateChildrenLocked(bundle, manager);
            while (childEnumerator.MoveNext())
            {
                if (childEnumerator.Current.TryGetComponentIntlLocked(type, bundle, manager, out component, options))
                    return true;
            }
        }
        else if ((options & QueryComponentOptions.InParents) != 0 && _parent != null)
        {
            if (_parent.TryGetComponentIntlLocked(type, bundle, manager, out component, options))
                return true;
        }

        component = null;
        return false;
    }

    private bool TryGetComponentIntlLocked(
        TypeHierarchy type,
        DiscoveredBundle bundle,
        AssetsManager manager,
        [NotNullWhen(true)] out UnityComponent? component)
    {
        if (!type.IsAbstract && Services.Installation.KnownClassIdsByTypes.TryGetValue(type.Type, out AssetClassID classId))
        {
            if (TryGetComponentIntlLocked(classId, bundle, manager, out component))
                return true;
        }

        foreach (KeyValuePair<QualifiedType, TypeHierarchy> h in type.ChildTypes)
        {
            if (TryGetComponentIntlLocked(h.Value, bundle, manager, out component))
                return true;
        }

        component = null;
        return false;
    }

    private bool TryGetComponentIntlLocked(
        AssetClassID type,
        DiscoveredBundle bundle,
        AssetsManager manager,
        [NotNullWhen(true)] out UnityComponent? component)
    {
        component = null;

        UnityComponent?[]? componentList = _components;
        while (componentList == null)
        {
            CreateComponentListLocked(manager);
            componentList = _components;
        }

        for (int i = 0; i < componentList.Length; ++i)
        {
            UnityComponent? comp = Volatile.Read(ref componentList[i]);
            if (comp == null || comp.Class != type)
                continue;

            component = comp;
            return true;
        }

        AssetTypeValueField components = GetComponentsArrayLocked(manager);
        if (components.IsDummy)
            return false;

        for (int i = 0; i < components.Children.Count; i++)
        {
            AssetTypeValueField componentPair = components.Children[i];
            if (componentPair.Children.Count < 1)
                continue;

            AssetTypeValueField componentPtr = componentPair.Children[0];
            if (componentPtr.Value != null)
                continue;

            if (!BundleUtility.TryReadPathId(componentPtr, out long pathId))
                continue;

            if (!bundle.TryGetFileInfoFromPathId(pathId, out AssetFileInfo? componentFileInfo)
                || (AssetClassID)componentFileInfo.TypeId != type
                || !Services.Installation.KnownUnityClassTypes.TryGetValue(type, out IBundleAssetType? clrType))
            {
                continue;
            }

            QualifiedType typeName = clrType.TypeName;
            UnityComponent comp = new UnityComponent(Object, File, componentFileInfo, Services, this, typeName);

            UnityComponent? oldValue = Interlocked.CompareExchange(ref componentList[i], comp, null);
            if (oldValue != null)
            {
                comp.Dispose();
                component = oldValue;
                return true;
            }

            if (IsDisposed)
            {
                comp.Dispose();
                throw new ObjectDisposedException(nameof(UnityObject));
            }

            component = comp;
            return true;
        }

        return false;
    }

    private AssetTypeValueField GetComponentsArrayLocked(AssetsManager manager)
    {
        AssetTypeValueField? componentsArray = _componentsArray;
        while (componentsArray == null)
        {
            AssetTypeValueField? baseField = _objectBaseField;
            while (baseField == null)
            {
                CacheObjectBaseFieldLocked(manager);
                baseField = _objectBaseField;
            }

            componentsArray = !baseField.IsDummy
                ? baseField["m_Component"]
                : AssetTypeValueField.DUMMY_FIELD;

            componentsArray = !componentsArray.IsDummy
                ? componentsArray["Array"]
                : AssetTypeValueField.DUMMY_FIELD;

            if (componentsArray.IsDummy)
            {
                Interlocked.CompareExchange(ref _componentsArray, AssetTypeValueField.DUMMY_FIELD, null);
                return _componentsArray ?? AssetTypeValueField.DUMMY_FIELD;
            }

            _componentsArray = componentsArray;
        }

        return componentsArray;
    }

    [MemberNotNull(nameof(_components))]
    private void CreateComponentListLocked(AssetsManager manager)
    {
        if (_components != null)
            return;

        AssetTypeValueField componentArray = GetComponentsArrayLocked(manager);
        if (componentArray.IsDummy || componentArray.Children.Count == 0)
        {
            _components = Array.Empty<UnityComponent>();
            return;
        }
        
        List<AssetTypeValueField> components = componentArray.Children;
        int componentCount = components.Count;

        UnityComponent?[] arr = new UnityComponent?[componentCount];

        // find the index of the current transform and add it to the list.
        for (int i = 0; i < componentCount; ++i)
        {
            AssetTypeValueField componentPair = components[i];
            if (componentPair.Children.Count < 1)
                continue;

            AssetTypeValueField componentPtr = componentPair.Children[0];
            if (!BundleUtility.TryReadPathId(componentPtr, out long pathId) || pathId != PathInfo.PathId)
            {
                continue;
            }

            arr[i] = this;
            break;
        }

        _components = arr;
    }

    private static void VerifyOptions(QueryComponentOptions options)
    {
        if ((options & (QueryComponentOptions.InChildren | QueryComponentOptions.InParents)) == (QueryComponentOptions.InChildren | QueryComponentOptions.InParents))
        {
            throw new ArgumentException("Can not search both children and parents in one query.", nameof(options));
        }
    }

    /// <summary>
    /// Options for component queries on <see cref="UnityTransform"/> objects.
    /// </summary>
    [Flags]
    public enum QueryComponentOptions
    {
        /// <summary>
        /// Find the given component type and all subtypes in the transform only.
        /// </summary>
        None,

        /// <summary>
        /// Include components in parent objects.
        /// </summary>
        InParents = 1,

        /// <summary>
        /// Include components in child objects. Uses a depth-first-search to match the behavior exhibited by Unity's GetComponentsInChildren method.
        /// </summary>
        InChildren = 2,

        /// <summary>
        /// Only match components with the exact type (don't use the type hierarchy).
        /// </summary>
        ExactTypeOnly = 4
    }

    /// <summary>
    /// Enumerates components related to a <see cref="UnityTransform"/>.
    /// <para>
    /// Must be disposed after usage to avoid deadlocks.
    /// </para>
    /// </summary>
    /// <remarks>Children are created lazily so they will only be read as the enumerator progresses.</remarks>
    public sealed class ComponentEnumerator : IEnumerator<UnityComponent>, IEnumerable<UnityComponent>
    {
        internal TfmLock? Lock;
        internal bool HasLock;
        internal DiscoveredBundle? Bundle;
        internal AssetsManager? Manager;

        private readonly UnityTransform _seed;
        private readonly QualifiedType _type;
        private readonly bool _includeChildren;
        private readonly bool _includeParents;
        private readonly QueryComponentOptions _options;

        // current parent
        private UnityTransform _current;
        private UnityComponent? _currentComponent;
        private AssetFileInfo? _currentFileInfo;

        // index within current object
        private int _index;

        // child stack (DFS)
        private Stack<UnityTransform>? _stack;

        public QualifiedType CurrentType { get; private set; }
        public AssetClassID CurrentClass { get; private set; }

#nullable disable
        /// <inheritdoc />
        public UnityComponent Current => _currentComponent ?? CreateComponentObject()!;

        object IEnumerator.Current => Current;
#nullable restore

        internal ComponentEnumerator(UnityTransform seed, QualifiedType type, QueryComponentOptions options)
        {
            _seed = seed;
            _type = type;
            _includeChildren = (options & QueryComponentOptions.InChildren) != 0;
            _includeParents = (options & QueryComponentOptions.InParents) != 0;
            _options = options;
            _index = -1;
            _current = _seed;
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if (Bundle == null)
                return false;

            if (_includeChildren)
            {
                return MoveNextWithChildren();
            }

            if (_includeParents)
            {
                return MoveNextWithParents();
            }

            return MoveNextStandard();
        }

        private bool MoveNextStandard()
        {
            UnityComponent?[] components = _current._components ?? throw new ObjectDisposedException(nameof(UnityTransform));
            do
            {
                ++_index;
                if (_index >= components.Length)
                    return false;

            } while (!SetComponent(_current, components, _index));
            return true;
        }

        private bool MoveNextWithParents()
        {
            UnityComponent?[] components = _current._components ?? throw new ObjectDisposedException(nameof(UnityTransform));
            do
            {
                ++_index;
                if (_index < components.Length)
                    continue;

                nextParent:
                UnityTransform? parent = _current._parent;
                if (parent == null)
                    return false;

                if (parent._components == null)
                {
                    parent.CreateComponentListLocked(Manager!);
                }

                _current = parent;
                _index = 0;
                if (components.Length == 0)
                    goto nextParent;

                components = parent._components ?? throw new ObjectDisposedException(nameof(UnityTransform));

            } while (!SetComponent(_current, components, _index));
            return true;
        }

        private bool MoveNextWithChildren()
        {
            if (_index == -1 && _current == _seed)
            {
                _stack ??= new Stack<UnityTransform>(16);
                ChildEnumerator enumerator = _seed.EnumerateChildrenLocked(Bundle!, Manager!);
                while (enumerator.MoveNext())
                {
                    _stack.Push(enumerator.Current);
                }
                enumerator.Dispose();
            }

            UnityComponent?[] components = _current._components ?? throw new ObjectDisposedException(nameof(UnityTransform));
            do
            {
                ++_index;
                if (_index < components.Length)
                    continue;

                if (_stack!.Count == 0)
                {
                    return false;
                }

                nextChild:
                UnityTransform next = _stack.Pop();
                ChildEnumerator enumerator = next.EnumerateChildrenLocked(Bundle!, Manager!);
                while (enumerator.MoveNext())
                {
                    _stack.Push(enumerator.Current);
                }

                enumerator.Dispose();
                if (next._components == null)
                {
                    next.CreateComponentListLocked(Manager!);
                }

                _current = next;
                _index = 0;
                if (components.Length == 0)
                    goto nextChild;

                components = next._components ?? throw new ObjectDisposedException(nameof(UnityTransform));

            } while (!SetComponent(_current, components, _index));
            return true;
        }

        private bool SetComponent(UnityTransform obj, UnityComponent?[] components, int index)
        {
            UnityComponent? current = components[index];
            _currentComponent = current;
            if (current != null)
            {
                _currentFileInfo = current.PathInfo;

                CurrentType = current.IsMonoBehaviourType
                    ? new QualifiedType("UnityEngine.MonoBehaviour, UnityEngine.CoreModule", true)
                    : current.Type;
                CurrentClass = current.Class;

                _currentComponent = current;
                return true;
            }

            AssetTypeValueField value = obj.GetComponentsArrayLocked(Manager!);
            AssetTypeValueField componentPair = value.Children[index];
            if (componentPair.Children.Count < 1)
                return false;

            AssetTypeValueField componentPtr = componentPair.Children[0];
            if (componentPtr.Value != null)
                return false;

            if (!BundleUtility.TryReadPathId(componentPtr, out long pathId)
                || !Bundle!.TryGetFileInfoFromPathId(pathId, out _currentFileInfo))
            {
                return false;
            }

            AssetClassID classId = (AssetClassID)_currentFileInfo.TypeId;
            if (!_current.Services.Installation.KnownUnityClassTypes.TryGetValue(classId, out IBundleAssetType? type))
            {
                return false;
            }

            QualifiedType typeName = type.TypeName;
            if (!_type.IsNull)
            {
                if ((_options & QueryComponentOptions.ExactTypeOnly) != 0)
                {
                    if (!typeName.Equals(_type))
                        return false;
                }
                else
                {
                    if (!_current.Services.Database.Information.IsAssignableFrom(_type, typeName))
                        return false;
                }
            }

            CurrentType = typeName;
            CurrentClass = classId;
            return true;
        }

        private UnityComponent? CreateComponentObject()
        {
            if (_currentFileInfo == null)
                return null;

            UnityComponent?[] components = _current._components ?? throw new ObjectDisposedException(nameof(UnityTransform));
            UnityComponent? current = components[_index];
            if (current != null)
            {
                _currentComponent = current;
                return current;
            }

            UnityComponent component = new UnityComponent(
                _current.Object,
                _current.File,
                _currentFileInfo,
                _current.Services,
                _current,
                CurrentType
            );

            UnityComponent? oldComponent = Interlocked.CompareExchange(ref components[_index], component, null);
            if (oldComponent != null)
            {
                _currentComponent = oldComponent;
                component.Dispose();
                return oldComponent;
            }

            _currentComponent = component;
            return component;
        }

        /// <exception cref="NotSupportedException"/>
        public void Reset()
        {
            _current = _seed;
            _index = -1;
            _stack?.Clear();
            _currentComponent = null;
            _currentFileInfo = null;
            CurrentType = QualifiedType.None;
            CurrentClass = AssetClassID.Object;
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
        public ComponentEnumerator GetEnumerator()
        {
            if ((_stack == null || _stack.Count == 0) && _index == -1 && _current == _seed)
                return this;

            throw new InvalidOperationException("Can not call GetEnumerator on a used enumerator.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!HasLock)
                return;

            PlatformLockHelper.ExitLock(Lock!);
            HasLock = false;
        }

        IEnumerator<UnityComponent> IEnumerable<UnityComponent>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}