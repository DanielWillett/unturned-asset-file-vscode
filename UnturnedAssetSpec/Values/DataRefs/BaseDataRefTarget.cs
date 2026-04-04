using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Base data-ref target for all data-refs that can contain properties.
/// </summary>
public abstract class BaseDataRefTarget : IDataRefTarget
{
    /// <inheritdoc cref="IDataRefExpressionNode.DataRef" />
    protected abstract IDataRef DataRef { get; }

    /// <inheritdoc />
    public bool AcceptProperty<TProperty, TVisitor>(in TProperty property, ref TVisitor valueVisitor, ref FileEvaluationContext ctx)
        where TProperty : IDataRefProperty
        where TVisitor : IValueVisitor
    #if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        if (typeof(TProperty) == typeof(ExcludedProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, ExcludedProperty>(ref Unsafe.AsRef(in property)), ref ctx, out bool isExcluded))
                return false;

            valueVisitor.Accept(BooleanType.Instance, new Optional<bool>(isExcluded));
            return true;
        }
        if (typeof(TProperty) == typeof(IncludedProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, IncludedProperty>(ref Unsafe.AsRef(in property)), ref ctx, out bool isIncluded))
                return false;

            valueVisitor.Accept(BooleanType.Instance, new Optional<bool>(isIncluded));
            return true;
        }
        if (typeof(TProperty) == typeof(KeyProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, KeyProperty>(ref Unsafe.AsRef(in property)), ref ctx, out string? key))
                return false;

            valueVisitor.Accept(StringType.Instance, key == null ? Optional<string>.Null : new Optional<string>(key));
            return true;
        }
        if (typeof(TProperty) == typeof(AssetNameProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, AssetNameProperty>(ref Unsafe.AsRef(in property)), ref ctx, out string? assetName))
                return false;

            valueVisitor.Accept(StringType.Instance, assetName == null ? Optional<string>.Null : new Optional<string>(assetName));
            return true;
        }
        if (typeof(TProperty) == typeof(DifficultyProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, DifficultyProperty>(ref Unsafe.AsRef(in property)), ref ctx, out string? difficulty))
                return false;

            valueVisitor.Accept(StringType.Instance, difficulty == null ? Optional<string>.Null : new Optional<string>(difficulty));
            return true;
        }
        if (typeof(TProperty) == typeof(IsSingleplayerProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, IsSingleplayerProperty>(ref Unsafe.AsRef(in property)), ref ctx, out bool isSingleplayer))
                return false;

            valueVisitor.Accept(BooleanType.Instance, new Optional<bool>(isSingleplayer));
            return true;
        }
        if (typeof(TProperty) == typeof(IndicesProperty))
        {
            return AcceptProperty(in Unsafe.As<TProperty, IndicesProperty>(ref Unsafe.AsRef(in property)), ref ctx, ref valueVisitor);
        }
        if (typeof(TProperty) == typeof(IsLegacyProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, IsLegacyProperty>(ref Unsafe.AsRef(in property)), ref ctx, out bool isLegacy))
                return false;

            valueVisitor.Accept(BooleanType.Instance, new Optional<bool>(isLegacy));
            return true;
        }
        if (typeof(TProperty) == typeof(ValueTypeProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, ValueTypeProperty>(ref Unsafe.AsRef(in property)), ref ctx, out string? valueType))
                return false;

            valueVisitor.Accept(StringType.Instance, valueType == null ? Optional<string>.Null : new Optional<string>(valueType));
            return true;
        }
        if (typeof(TProperty) == typeof(CountProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, CountProperty>(ref Unsafe.AsRef(in property)), ref ctx, out int count))
                return false;

            valueVisitor.Accept(Int32Type.Instance, new Optional<int>(count));
            return true;
        }
        if (typeof(TProperty) == typeof(ComponentProperty))
        {
            return AcceptProperty(in Unsafe.As<TProperty, ComponentProperty>(ref Unsafe.AsRef(in property)), ref ctx, ref valueVisitor);
        }

        return AcceptUnknownProperty(in property, ref valueVisitor, ref ctx);
    }

    /// <summary>
    /// Invoked when a property that isn't already given a known virtual method (see other methods in <see cref="BaseDataRefTarget"/>) is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptUnknownProperty<TProperty, TVisitor>(in TProperty property, ref TVisitor valueVisitor, ref FileEvaluationContext ctx)
        where TProperty : IDataRefProperty
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="ExcludedProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in ExcludedProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        value = false;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="IncludedProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in IncludedProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        value = false;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="KeyProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in KeyProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="AssetNameProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in AssetNameProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="DifficultyProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in DifficultyProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="IsSingleplayerProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in IsSingleplayerProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        value = false;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="IndicesProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty<TVisitor>(in IndicesProperty property, ref FileEvaluationContext ctx, ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="IsLegacyProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in IsLegacyProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        value = false;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="ValueTypeProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in ValueTypeProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = null;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="CountProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in CountProperty property, ref FileEvaluationContext ctx, out int value)
    {
        value = 0;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="ComponentProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty<TVisitor>(in ComponentProperty property, ref FileEvaluationContext ctx, ref TVisitor visitor)
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return false;
    }

    /// <inheritdoc />
    public bool Equals(IDataRefTarget? other) => Equals(other as IExpressionNode);

    /// <inheritdoc />
    public abstract bool Equals(IExpressionNode? other);

    IDataRef IDataRefExpressionNode.DataRef => DataRef;
}