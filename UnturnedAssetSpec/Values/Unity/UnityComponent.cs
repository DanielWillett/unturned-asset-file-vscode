using AssetsTools.NET;
using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
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

    internal UnityComponent(
        UnityObject rootObject,
        AssetsFileInstance file,
        AssetFileInfo pathInfo,
        IParsingServices services)
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

    /// <summary>
    /// Gets a property path within this component.
    /// Paths can include indexers and multiple properties separated by periods.
    /// </summary>
    /// <param name="value">Read value (or <see langword="null"/>) converted to <typeparamref name="TValue"/>.</param>
    /// <returns>Whether or not the property was found and the value could be converted to <typeparamref name="TValue"/>.</returns>
    /// <inheritdoc cref="TryReadProperty{TVisitor}(string,ref TVisitor)"/>
    public bool TryReadProperty<TValue>(string propertyPath, out Optional<TValue> value)
        where TValue : IEquatable<TValue>
    {
        ConvertVisitor<TValue> visitor = new ConvertVisitor<TValue>();
        if (!TryReadProperty(propertyPath, ref visitor) || !visitor.WasSuccessful)
        {
            value = default;
            return false;
        }
        
        value = visitor.IsNull ? Optional<TValue>.Null : new Optional<TValue>(visitor.Result);
        return true;
    }

    /// <summary>
    /// Visits a property path within this component.
    /// Paths can include indexers and multiple properties separated by periods.
    /// </summary>
    /// <typeparam name="TVisitor">Visitor type.</typeparam>
    /// <param name="propertyPath">Property to read.
    /// Can optionally include multiple properties separated by periods, and indexers for arrays.
    /// Example: <c>m_Prop1.m_Prop2[^1].m_Prop3</c>.</param>
    /// <param name="visitor">Visitor</param>
    /// <returns>Whether or not the property was found and the <paramref name="visitor"/> was invoked.</returns>
    public bool TryReadProperty<TVisitor>(string propertyPath, ref TVisitor visitor)
        where TVisitor : IGenericVisitor
    {
        TryReadPropertyState state = default;
        state.PropertyPath = propertyPath;
        state.This = this;
        state.Field = null;
        BundleUtility.RunOperationOnBundle(
            ref state, Bundle, Services,
            static (_, _, manager, ref state) =>
            {
                AssetTypeValueField? baseField = state.This.BaseFieldIntl;
                while (baseField == null)
                {
                    state.This.CacheBaseFieldLocked(manager);
                    baseField = state.This.BaseFieldIntl;
                }

                UnityPropertyPathEnumerator enumerator = new UnityPropertyPathEnumerator(state.PropertyPath);
                while (enumerator.MoveNext())
                {
                    string prop = enumerator.Property;

                    AssetTypeValueField fld = baseField[prop];
                    if (fld.IsDummy)
                    {
                        return;
                    }

                    if (enumerator.Index.HasValue && fld.Value.ValueType == AssetValueType.Array)
                    {
                        List<AssetTypeValueField> children = fld.Children;
                        int index = enumerator.Index.Value.GetOffset(children.Count);
                        if (index < 0 || index >= children.Count)
                            return;

                        baseField = children[index];
                        if (baseField.IsDummy)
                            return;
                    }
                    else
                    {
                        baseField = fld;
                    }
                }

                state.Field = baseField;
            }
        );

        return state.Field is { IsDummy: false } && BundleUtility.VisitFieldValue(state.Field, ref visitor);
    }

    private struct TryReadPropertyState
    {
        public string PropertyPath;
        public UnityComponent This;
        public AssetTypeValueField? Field;
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