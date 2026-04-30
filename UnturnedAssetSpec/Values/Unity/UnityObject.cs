using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An object from a unity bundle at a given path.
/// </summary>
[DebuggerDisplay("{ObjectType,nq} - {Name,nq}")]
public sealed class UnityObject : IValue<UnityObject>, IEquatable<UnityObject?>, IDisposable
{
    private readonly AssetsFileInstance _file;
    private readonly AssetFileInfo _pathInfo;

    private AssetTypeValueField? _baseField;
    private readonly IParsingServices _services;
    private bool _disposed;

    private UnityTransform? _transform;
    private bool _hasTransform;

    public bool IsNull => false;

    public IBundleAssetType Type { get; }

    public IBundleProxy Bundle { get; }

    public AssetClassID ObjectType => (AssetClassID)_pathInfo.TypeId;

    public string Path { get; }

    internal AssetTypeValueField BaseField
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityObject));

            AssetTypeValueField? baseField = _baseField;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = _baseField;
            }

            return baseField;
        }
    }

    /// <summary>
    /// Allows access to the object's hierarchy.
    /// </summary>
    public UnityTransform? Transform
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityObject));

            if (ObjectType != AssetClassID.GameObject)
                return null;

            return _hasTransform ? _transform : CreateTransform();
        }
    }

    /// <summary>
    /// The name of this object.
    /// </summary>
    public string? Name
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(UnityObject));

            AssetTypeValueField? baseField = _baseField;
            while (baseField == null)
            {
                CacheBaseField();
                baseField = _baseField;
            }

            if (baseField.IsDummy)
            {
                return null;
            }

            AssetTypeValueField nameField = baseField["m_Name"];
            if (nameField.IsDummy || nameField.Value.ValueType != AssetValueType.String)
            {
                return null;
            }

            return nameField.Value.AsString;
        }
    }

    public UnityObject(
        IBundleAssetType type,
        string path,
        IBundleProxy bundle,
        AssetsFileInstance file,
        AssetFileInfo fileInfo,
        IParsingServices services
    )
    {
        _file = file;
        _pathInfo = fileInfo;
        _services = services;
        Type = type;
        Bundle = bundle;
        Path = path;
    }

    /// <summary>
    /// Attempts to get this object's type as a <see cref="IBundleAssetType"/>.
    /// </summary>
    /// <param name="type">A known CLR type associated with this object's <see cref="AssetClassID"/> (stored in <see cref="ObjectType"/>).</param>
    /// <returns>Whether or not this object has a known CLR type associated with it.</returns>
    public bool TryGetBundleAssetType([NotNullWhen(true)] out IBundleAssetType? type)
    {
        if (_services.Installation.KnownUnityClassTypes.TryGetValue(ObjectType, out IBundleAssetType? value))
        {
            type = value;
            return true;
        }

        type = null;
        return false;
    }

    [MemberNotNull(nameof(_baseField))]
    private void CacheBaseField()
    {
        BundleUtility.RunOperationOnBundle(
            this, Bundle, _services,
            static (_, _, manager, @this) =>
            {
                if (@this._baseField != null)
                    return;

                @this._baseField = manager.GetBaseField(@this._file, @this._pathInfo);
            }
        );

        _baseField ??= AssetTypeValueField.DUMMY_FIELD;
    }

    private UnityTransform? CreateTransform()
    {
        BundleUtility.RunOperationOnBundle(this, Bundle, _services, static (bundle, _, _, @this) =>
        {
            if (@this._hasTransform)
                return;
            
            AssetTypeValueField baseField = @this.BaseField;
            if (baseField.IsDummy)
            {
                return;
            }

            AssetTypeValueField componentsArray = baseField["m_Component"];

            componentsArray = !componentsArray.IsDummy
                ? componentsArray["Array"]
                : AssetTypeValueField.DUMMY_FIELD;

            if (componentsArray.IsDummy)
            {
                return;
            }

            for (int i = 0; i < componentsArray.Children.Count; i++)
            {
                AssetTypeValueField componentPair = componentsArray.Children[i];
                if (componentPair.Children.Count < 1)
                    continue;

                AssetTypeValueField componentPtr = componentPair.Children[0];
                if (componentPtr.Value != null)
                    continue;

                if (!BundleUtility.TryReadPathId(componentPtr, out long pathId))
                    continue;

                if (!bundle.TryGetFileInfoFromPathId(pathId, out AssetFileInfo? componentFileInfo)
                    || (AssetClassID)componentFileInfo.TypeId is not AssetClassID.Transform
                    and not AssetClassID.RectTransform)
                {
                    continue;
                }

                @this._transform = new UnityTransform(null, @this, @this._file, componentFileInfo, 0, -1, @this._services);
                @this._hasTransform = true;

                if (@this._disposed)
                {
                    @this._hasTransform = false;
                    Interlocked.Exchange(ref @this._transform, null)?.Dispose();
                    throw new ObjectDisposedException(nameof(UnityObject));
                }

                return;
            }

            @this._hasTransform = true;
            if (@this._disposed)
            {
                @this._hasTransform = false;
                Interlocked.Exchange(ref @this._transform, null)?.Dispose();
                throw new ObjectDisposedException(nameof(UnityObject));
            }
        });

        return _transform;
    }

    public bool Equals(IValue? other)
    {
        return other is UnityObject obj && Equals(obj);
    }

    public bool Equals(UnityObject? other)
    {
        if (other == null)
            return false;

        return other.Type.Equals(Type)
               && Bundle.Equals(other.Bundle)
               && string.Equals(Path, other.Path, StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Path;
    }

    void IValue.WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    bool IValue.VisitConcreteValue<TVisitor>(ref TVisitor visitor)
    {
        visitor.Accept(Type, this);
        return true;
    }

    bool IValue.VisitValue<TVisitor>(ref TVisitor visitor, ref FileEvaluationContext ctx)
    {
        visitor.Accept(Type, this);
        return true;
    }

    bool IValue<UnityObject>.TryGetConcreteValue(out Optional<UnityObject> value)
    {
        value = new Optional<UnityObject>(this);
        return true;
    }

    bool IValue<UnityObject>.TryEvaluateValue(out Optional<UnityObject> value, ref FileEvaluationContext ctx)
    {
        value = new Optional<UnityObject>(this);
        return true;
    }
    IType<UnityObject> IValue<UnityObject>.Type => Type;

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
        _hasTransform = false;
        Interlocked.Exchange(ref _transform, null)?.Dispose();
        _baseField = null;
    }
}