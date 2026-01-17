using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A data-ref root referencing another property.
/// </summary>
public sealed class PropertyDataRef : RootDataRef<PropertyDataRef>
{
    private readonly PropertyReference _propReference;
    private IPropertyReferenceValue? _value;

    /// <summary>
    /// The <see cref="Properties.PropertyReference"/> value describing the referencing property.
    /// </summary>
    public ref readonly PropertyReference PropertyReference => ref _propReference;

    /// <inheritdoc />
    [field: MaybeNull]
    public override string PropertyName => field ??= _propReference.ToString();

    internal PropertyDataRef(PropertyReference propRef)
    {
        _propReference = propRef;
    }

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        _value ??= _propReference.CreateValue(ctx.Self, ctx.Information);

        return _value.VisitValue(ref visitor, in ctx);
    }

    protected override bool Equals(PropertyDataRef other)
    {
        return _propReference.Equals(other._propReference);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return ~_propReference.GetHashCode();
    }

    protected override bool AcceptProperty(in IncludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        _value ??= _propReference.CreateValue(ctx.Self, ctx.Information);

        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(in ctx, out FileEvaluationContext crContext))
            {
                value = false;
                return false;
            }

            value = crContext.Self.IsIncluded(property.RequireValue, in crContext);
        }
        else
        {
            DatProperty prop = _value.Property;
            value = prop.IsIncluded(property.RequireValue, in ctx);
        }

        return true;
    }

    protected override bool AcceptProperty(in ExcludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        _value ??= _propReference.CreateValue(ctx.Self, ctx.Information);

        DatProperty prop = _value.Property;
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(in ctx, out FileEvaluationContext crContext))
            {
                value = false;
                return false;
            }

            try
            {
                value = prop.IsExcluded(in crContext);
            }
            finally
            {
                cr.DisposeContext(in crContext);
            }
        }
        else
        {
            value = prop.IsExcluded(in ctx);
        }

        return true;
    }
}