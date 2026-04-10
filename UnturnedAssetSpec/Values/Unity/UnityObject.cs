using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An object from a unity bundle at a given path.
/// </summary>
public sealed class UnityObject : IValue<UnityObject>, IEquatable<UnityObject?>, IDisposable
{
    private readonly AssetsFileInstance _file;
    private readonly AssetFileInfo _pathInfo;

    private AssetTypeValueField? _baseField;
    private readonly IParsingServices _services;
    private readonly int _level;
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

            if (_baseField != null)
                return _baseField;

            CacheBaseField();
            return _baseField ?? AssetTypeValueField.DUMMY_FIELD;
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

    public UnityObject(
        IBundleAssetType type,
        string path,
        IBundleProxy bundle,
        AssetsFileInstance file,
        AssetFileInfo fileInfo,
        IParsingServices services,
        int level = 0
    )
    {
        _file = file;
        _pathInfo = fileInfo;
        _services = services;
        _level = level;
        Type = type;
        Bundle = bundle;
        Path = path;
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
            
            if (bundle.FilePreloadCache == null)
            {
                return;
            }

            AssetTypeValueField baseField = @this.BaseField;
            if (baseField.IsDummy)
            {
                return;
            }

            AssetTypeValueField component = baseField["m_Component"];
            if (component.IsDummy)
            {
                return;
            }

            foreach (AssetTypeValueField componentSet in component.Children)
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

                        if (!bundle.FilePreloadCache.TryGetValue(pathId, out AssetFileInfo? componentFileInfo))
                            continue;

                        @this._transform = new UnityTransform(null, @this, @this._file, componentFileInfo, @this._level, @this._services);
                        @this._hasTransform = true;

                        if (@this._disposed)
                        {
                            @this._hasTransform = false;
                            Interlocked.Exchange(ref @this._transform, null).Dispose();
                            throw new ObjectDisposedException(nameof(UnityObject));
                        }

                        return;
                    }
                }
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
    }
}