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

    public DatProperty Owner { get; }

    internal PropertyDataRef(PropertyReference propRef, DatProperty owner)
    {
        Owner = owner;
        _propReference = propRef;
    }

    [MemberNotNull(nameof(_value))]
    private void EnsureValueExists(IAssetSpecDatabase database)
    {
        _value ??= _propReference.CreateValue(Owner, database);
    }

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, ref FileEvaluationContext ctx)
    {
        EnsureValueExists(ctx.Services.Database);
        return _value.VisitValue(ref visitor, ref ctx);
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

    protected override bool AcceptProperty(in IncludedProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        EnsureValueExists(ctx.Services.Database);
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(ref ctx, out FileEvaluationContext crContext, out DatProperty? crProperty))
            {
                value = false;
                return false;
            }

            try
            {
                value = crProperty.IsIncluded(property.RequireValue, ref crContext);
            }
            finally
            {
                cr.DisposeContext(ref crContext);
            }
        }
        else
        {
            DatProperty prop = _value.Property;
            value = prop.IsIncluded(property.RequireValue, ref ctx);
        }

        return true;
    }

    protected override bool AcceptProperty(in ExcludedProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        EnsureValueExists(ctx.Services.Database);
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(ref ctx, out FileEvaluationContext crContext, out DatProperty? prop))
            {
                value = false;
                return false;
            }

            try
            {
                value = prop.IsExcluded(ref crContext);
            }
            finally
            {
                cr.DisposeContext(ref crContext);
            }
        }
        else
        {
            DatProperty prop = _value.Property;
            value = prop.IsExcluded(ref ctx);
        }

        return true;
    }

    /// <inheritdoc />
    protected override bool AcceptProperty(in KeyProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        EnsureValueExists(ctx.Services.Database);
        string? k;
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(ref ctx, out FileEvaluationContext crContext, out DatProperty? prop))
            {
                value = null;
                return false;
            }

            try
            {
                k = GetPropertyKey(prop, ref crContext);
            }
            finally
            {
                cr.DisposeContext(ref crContext);
            }
        }
        else
        {
            DatProperty prop = _value.Property;
            k = GetPropertyKey(prop, ref ctx);
        }

        value = k;
        return k != null;
    }

    internal static string GetPropertyKey(DatProperty property, ref FileEvaluationContext ctx)
    {
        return !ctx.File.TryGetProperty(property, ref ctx, out IPropertySourceNode? propertyNode)
            ? property.Key
            : propertyNode.Key;
    }

    protected override bool AcceptProperty<TVisitor>(in IndicesProperty property, ref FileEvaluationContext ctx, ref TVisitor visitor)
    {
        // todo
        return false;
    }

    protected override bool AcceptProperty(in IsLegacyProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        if (_propReference.IsCrossReference)
        {
            value = false;
            return false;
        }

        // todo
        EnsureValueExists(ctx.Services.Database);
        value = false;
        return false;
    }

    protected override bool AcceptProperty(in ValueTypeProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        EnsureValueExists(ctx.Services.Database);
        SourceValueType k;
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(ref ctx, out FileEvaluationContext crContext, out DatProperty? prop))
            {
                value = null;
                return false;
            }

            try
            {
                k = prop.GetValueType(ref crContext);
            }
            finally
            {
                cr.DisposeContext(ref crContext);
            }
        }
        else
        {
            DatProperty prop = _value.Property;
            k = prop.GetValueType(ref ctx);
        }

        value = ValueTypeProperty.GetTypeName(k);
        return true;
    }

    protected override bool AcceptProperty(in CountProperty property, ref FileEvaluationContext ctx, out int value)
    {
        // todo
        value = 0;
        return false;
    }

    protected override bool AcceptProperty<TVisitor>(in ComponentProperty property, ref FileEvaluationContext ctx, ref TVisitor visitor)
    {
        if (_propReference.IsCrossReference)
        {
            // not supported.
            return false;
        }

        EnsureValueExists(ctx.Services.Database);
        if (_value.Property is not DatBundleAsset bundleAsset)
        {
            return false;
        }

        return property.GetValue(bundleAsset, ref ctx, ref visitor);
    }
}