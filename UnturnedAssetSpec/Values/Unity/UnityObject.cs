using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// An object from a unity bundle at a given path.
/// </summary>
public sealed class UnityObject : IValue<UnityObject>, IEquatable<UnityObject?>
{
    internal readonly AssetFileInfo FileInfo;
    internal readonly AssetsFileInstance File;
    internal readonly AssetTypeValueField BaseField;
    private readonly IParsingServices _services;

    private UnityTransform? _transform;
    private bool _hasTransform;

    public bool IsNull => false;

    public IBundleAssetType Type { get; }

    public IBundleProxy Bundle { get; }

    public AssetClassID ObjectType { get; }

    public string Path { get; }

    /// <summary>
    /// Allows access to the object's hierarchy.
    /// </summary>
    public UnityTransform? Transform
    {
        get
        {
            if (ObjectType != AssetClassID.GameObject)
                return null;

            return _hasTransform ? _transform : CreateTransform();
        }
    }

    public UnityObject(
        IBundleAssetType type,
        string path,
        IBundleProxy bundle,
        AssetFileInfo fileInfo,
        AssetTypeValueField baseField,
        IParsingServices services
    )
    {
        FileInfo = fileInfo;
        BaseField = baseField;
        _services = services;
        ObjectType = (AssetClassID)FileInfo.TypeId;
        Type = type;
        Bundle = bundle;
        Path = path;
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

    private UnityTransform? CreateTransform()
    {

        bool hasLock = false;
        TfmLock? @lock = null;

        try
        {
            DiscoveredBundle? bndl;
            while (true)
            {
                bndl = Bundle.Bundle;
                if (bndl == null)
                    break;

                @lock = bndl.GetLock(_services);
                PlatformLockHelper.EnterLock(@lock, ref hasLock);
                if (bndl != Bundle.Bundle)
                {
                    PlatformLockHelper.ExitLock(@lock);
                    hasLock = false;
                    continue;
                }

                break;
            }

            if (bndl?.FilePreloadCache == null)
            {
                return null;
            }

            AssetsManager? assetsManager = _services.Installation.AssetBundleManager;
            if (assetsManager == null)
            {
                return null;
            }

            AssetTypeValueField component = BaseField["m_Component"];
            if (component.IsDummy)
            {
                return null;
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
                        if (!TryReadPathId(componentPtr, out long pathId))
                            continue;

                        if (!bndl.FilePreloadCache.TryGetValue(pathId, out AssetFileInfo? componentPath))
                            continue;

                        AssetTypeValueField transformField = assetsManager.GetBaseField(File, componentPath);

                        _transform = new UnityTransform(null, this, transformField);
                        _hasTransform = true;
                        return _transform;
                    }
                }
            }

            _hasTransform = true;
            return null;
        }
        finally
        {
            if (hasLock)
                PlatformLockHelper.ExitLock(@lock!);
        }
    }

    private static bool TryReadPathId(AssetTypeValueField? childGameObject, out long pathId)
    {
        pathId = 0;
        if (childGameObject == null)
            return false;

        AssetTypeValueField pathIdField = childGameObject["m_PathID"];
        if (pathIdField.IsDummy || pathIdField.Value.ValueType != AssetValueType.Int64)
            return false;

        pathId = pathIdField.AsLong;
        return true;
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
}
