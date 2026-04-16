using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

public class UnityComponent : IDisposable
{
    private protected readonly AssetsFileInstance File;
    private protected readonly AssetFileInfo PathInfo;
    private protected readonly IParsingServices Services;
    private protected readonly IBundleProxy Bundle;
    private protected bool IsDisposed;
    private protected AssetTypeValueField? BaseFieldIntl;

    /// <summary>
    /// The root asset this component is a part of.
    /// This is not the same as a GameObject, as a <see cref="UnityObject"/> references a prefab, not a GameObject.
    /// </summary>
    public UnityObject Object { get; }
    
    /// <summary>
    /// The transform this component is attached to. For <see cref="UnityTransform"/> objects, this is equal to <see langword="this"/>.
    /// </summary>
    public UnityTransform Transform { get; }

    /// <summary>
    /// The fully-qualified CLR type of component this object represents.
    /// </summary>
    public QualifiedType Type { get; }

    /// <summary>
    /// The known type of component this object represents.
    /// </summary>
    public AssetClassID Class { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    protected internal AssetTypeValueField BaseField
    {
        get
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(UnityTransform));

            if (BaseFieldIntl != null)
                return BaseFieldIntl;

            CacheBaseField();
            return BaseFieldIntl ?? AssetTypeValueField.DUMMY_FIELD;
        }
    }

    internal UnityComponent(UnityObject rootObject, AssetsFileInstance file, AssetFileInfo pathInfo, IParsingServices services)
        : this(
            rootObject,
            file,
            pathInfo,
            services,
            null!,
            (AssetClassID)pathInfo.TypeId == AssetClassID.RectTransform
                ? new QualifiedType("UnityEngine.RectTransform, UnityEngine.CoreModule")
                : new QualifiedType("UnityEngine.Transform, UnityEngine.CoreModule")
        ) { }

    internal UnityComponent(
        UnityObject rootObject,
        AssetsFileInstance file,
        AssetFileInfo pathInfo,
        IParsingServices services,
        UnityTransform transform,
        QualifiedType type)
    {
        if (transform == null)
        {
            if (this is UnityTransform t)
            {
                Transform = t;
            }
            else
            {
                throw new ArgumentNullException(nameof(transform));
            }
        }
        else
        {
            Transform = transform;
        }

        Object = rootObject;
        Bundle = rootObject.Bundle;
        PathInfo = pathInfo;
        File = file;
        Services = services;

        Type = type.CaseInsensitive.Normalized;
        Class = (AssetClassID)pathInfo.TypeId;
    }

#pragma warning disable CS8774 //  BaseField -> BaseFieldIntl
    [MemberNotNull(nameof(BaseFieldIntl))]
    [MemberNotNull(nameof(BaseField))]
    private protected void CacheBaseField()
    {
        BundleUtility.RunOperationOnBundle(
            this, Bundle, Services,
            static (_, _, manager, @this) =>
            {
                @this.CacheBaseFieldLocked(manager);
            }
        );

        BaseFieldIntl ??= AssetTypeValueField.DUMMY_FIELD;
    }
#pragma warning restore CS8774

    [MemberNotNull(nameof(BaseFieldIntl))]
    private protected void CacheBaseFieldLocked(AssetsManager manager)
    {
        BaseFieldIntl ??= manager.GetBaseField(File, PathInfo) ?? AssetTypeValueField.DUMMY_FIELD;
        if (!IsDisposed)
            return;

        BaseFieldIntl = null;
        throw new ObjectDisposedException(nameof(UnityTransform));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            IsDisposed = true;
            BaseFieldIntl = null;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}