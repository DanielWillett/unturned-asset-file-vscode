using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values.Expressions;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Base data-ref target for all data-refs that can contain properties.
/// </summary>
public abstract class BaseDataRefTarget : IDataRefTarget
{
    /// <inheritdoc cref="IDataRefExpressionNode.DataRef" />
    protected abstract IDataRef DataRef { get; }

    /// <inheritdoc />
    public bool AcceptProperty<TProperty, TVisitor>(in TProperty property, ref TVisitor valueVisitor, in FileEvaluationContext ctx)
        where TProperty : IDataRefProperty
        where TVisitor : IValueVisitor
    #if NET9_0_OR_GREATER
        , allows ref struct
    #endif
    {
        if (typeof(TProperty) == typeof(IncludedProperty))
        {
            if (!AcceptProperty(in Unsafe.As<TProperty, IncludedProperty>(ref Unsafe.AsRef(in property)), in ctx, out bool isIncluded))
                return false;

            valueVisitor.Accept(new Optional<bool>(isIncluded));
            return true;
        }

        return AcceptUnknownProperty(in property, ref valueVisitor, in ctx);
    }

    /// <summary>
    /// Invoked when a property that isn't already given a known virtual method (see other methods in <see cref="BaseDataRefTarget"/>) is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptUnknownProperty<TProperty, TVisitor>(in TProperty property, ref TVisitor valueVisitor, in FileEvaluationContext ctx)
        where TProperty : IDataRefProperty
        where TVisitor : IValueVisitor
#if NET9_0_OR_GREATER
        , allows ref struct
#endif
    {
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="IncludedProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in IncludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        value = false;
        return false;
    }

    /// <summary>
    /// Invoked when a <see cref="ExcludedProperty"/> is accessed on this target.
    /// </summary>
    /// <inheritdoc cref="IDataRefTarget.AcceptProperty"/>.
    protected virtual bool AcceptProperty(in ExcludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        value = false;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(IDataRefTarget? other) => Equals(other as IExpressionNode);

    /// <inheritdoc />
    public abstract bool Equals(IExpressionNode? other);

    IDataRef IDataRefExpressionNode.DataRef => DataRef;
}