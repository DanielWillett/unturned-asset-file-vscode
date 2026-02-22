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
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        EnsureValueExists(ctx.Services.Database);
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
        EnsureValueExists(ctx.Services.Database);
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(in ctx, out FileEvaluationContext crContext, out DatProperty? crProperty))
            {
                value = false;
                return false;
            }

            value = crProperty.IsIncluded(property.RequireValue, in crContext);
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
        EnsureValueExists(ctx.Services.Database);
        DatProperty prop = _value.Property;
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(in ctx, out FileEvaluationContext crContext, out _))
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

    /// <inheritdoc />
    protected override bool AcceptProperty(in KeyProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        EnsureValueExists(ctx.Services.Database);
        DatProperty prop = _value.Property;
        string? k;
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(in ctx, out FileEvaluationContext crContext, out _))
            {
                value = null;
                return false;
            }

            try
            {
                k = GetPropertyKey(prop, in crContext);
            }
            finally
            {
                cr.DisposeContext(in crContext);
            }
        }
        else
        {
            k = GetPropertyKey(prop, in ctx);
        }

        value = k;
        return k != null;
    }

    internal static string GetPropertyKey(DatProperty property, in FileEvaluationContext ctx)
    {
        return !ctx.File.TryGetProperty(property, in ctx, out IPropertySourceNode? propertyNode)
            ? property.Key
            : propertyNode.Key;
    }

    protected override bool AcceptProperty<TVisitor>(in IndicesProperty property, in FileEvaluationContext ctx, ref TVisitor visitor)
    {
        // todo
        return false;
    }

    protected override bool AcceptProperty(in IsLegacyProperty property, in FileEvaluationContext ctx, out bool value)
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

    protected override bool AcceptProperty(in ValueTypeProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        EnsureValueExists(ctx.Services.Database);
        DatProperty prop = _value.Property;
        SourceValueType k;
        if (_value is ICrossedPropertyReference cr)
        {
            if (!cr.TryResolveReference(in ctx, out FileEvaluationContext crContext, out _))
            {
                value = null;
                return false;
            }

            try
            {
                k = prop.GetValueType(in crContext);
            }
            finally
            {
                cr.DisposeContext(in crContext);
            }
        }
        else
        {
            k = prop.GetValueType(in ctx);
        }

        value = ValueTypeProperty.GetTypeName(k);
        return true;
    }

    protected override bool AcceptProperty(in CountProperty property, in FileEvaluationContext ctx, out int value)
    {
        // todo
        value = 0;
        return false;
    }
}