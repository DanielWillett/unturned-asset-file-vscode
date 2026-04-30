using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Represents an object under the hierarchy of a prefab <see cref="UnityObject"/>.
/// </summary>
/// <remarks>Extends <see cref="UnityComponent"/>.</remarks>
public partial class UnityTransform : UnityComponent
{
    private AssetTypeValueField? _objectBaseField;

    /// <summary>
    /// Local position to parent, or world position if this object has no parent.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>
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
    /// <exception cref="ObjectDisposedException"/>
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
    /// <exception cref="ObjectDisposedException"/>
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
    /// <exception cref="ObjectDisposedException"/>
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
    /// Whether or not this transform represents a <c>RectTransform</c> component.
    /// </summary>
    public bool IsRectTransform => Class == AssetClassID.RectTransform;

    internal UnityTransform(
        UnityTransform? parent,
        UnityObject rootObject,
        AssetsFileInstance file,
        AssetFileInfo transformPathInfo,
        int level,
        int siblingIndex,
        IParsingServices services)
    : base(rootObject, file, transformPathInfo, services)
    {
        _parent = parent;
        Depth = level;
        SiblingIndex = siblingIndex;
    }

    internal UnityTransform(
        UnityTransform parent,
        UnityObject rootObject,
        AssetsFileInstance file,
        int siblingIndex,
        AssetFileInfo transformPathInfo)
    : base(rootObject, file, transformPathInfo, parent.Services)
    {
        _parent = parent;
        Depth = parent.Depth + 1;
        SiblingIndex = siblingIndex;
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

        UnityComponent?[]? components = Interlocked.Exchange(ref _components, null);
        if (components != null)
        {
            for (int i = 0; i < components.Length; ++i)
            {
                UnityComponent? comp = components[i];
                if (comp == null || comp == this)
                    continue;

                comp.Dispose();
            }
        }

        base.Dispose(disposing);

        if (disposing)
        {
            _childrenField = null;
            _objectBaseField = null;
            _componentsArray = null;
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

    [MemberNotNull(nameof(_objectBaseField))]
    private void CacheObjectBaseFieldLocked(AssetsManager manager)
    {
        AssetTypeValueField? baseField = BaseFieldIntl;
        while (baseField == null)
        {
            CacheBaseFieldLocked(manager);
            baseField = BaseFieldIntl;
        }

        if (baseField.IsDummy)
        {
            _objectBaseField = AssetTypeValueField.DUMMY_FIELD;
            return;
        }

        AssetTypeValueField gameObjectPtr = BaseField["m_GameObject"];
        if (BundleUtility.TryReadPathId(gameObjectPtr, out long gameObjectPathId))
        {
            _objectBaseField = manager.GetBaseField(File, gameObjectPathId);
        }
        else
        {
            Interlocked.CompareExchange(ref _objectBaseField, AssetTypeValueField.DUMMY_FIELD, null);
        }
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_parent == null)
        {
            return Object.Path;
        }

        StringBuilder sb = new StringBuilder(Object.Path.Length + Depth * 15);
        sb.Append(Object.Path);

        for (UnityTransform? transform = this; transform is { _parent: not null }; transform = transform._parent)
        {
            string? name = Name;
            if (!string.IsNullOrEmpty(name))
            {
                sb.Append('/').Append(name);
            }
        }

        return sb.ToString();
    }
}