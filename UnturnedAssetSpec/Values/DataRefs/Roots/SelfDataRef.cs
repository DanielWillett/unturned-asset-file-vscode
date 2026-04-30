using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current property.
/// </summary>
public sealed class SelfDataRef : RootDataRef<SelfDataRef>
{
    /// <summary>
    /// The property being referred to by this data-ref.
    /// </summary>
    public DatProperty Owner { get; }

    public SelfDataRef(DatProperty owner)
    {
        Owner = owner;
    }

    /// <inheritdoc />
    public override string PropertyName => "Self";

    protected override bool IsPropertyNameKeyword => true;

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, ref FileEvaluationContext ctx)
    {
        // NOTE: it doesn't make sense to return the value of the current property
        //       since that's what's being evaluated in this function.

        return false;
    }

    protected override bool AcceptProperty(in IncludedProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        value = Owner.IsIncluded(property.RequireValue, ref ctx);
        return true;
    }

    protected override bool AcceptProperty(in ExcludedProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        value = Owner.IsExcluded(ref ctx);
        return true;
    }

    protected override bool AcceptProperty(in KeyProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = PropertyDataRef.GetPropertyKey(Owner, ref ctx);
        return value != null;
    }

    protected override bool AcceptProperty<TVisitor>(in IndicesProperty property, ref FileEvaluationContext ctx, ref TVisitor visitor)
    {
        // todo
        return false;
    }

    protected override bool AcceptProperty(in IsLegacyProperty property, ref FileEvaluationContext ctx, out bool value)
    {
        // todo
        value = false;
        return false;
    }

    protected override bool AcceptProperty(in ValueTypeProperty property, ref FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = ValueTypeProperty.GetTypeName(Owner.GetValueType(ref ctx));
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
        if (Owner is not DatBundleAsset bundleAsset)
        {
            return false;
        }

        return property.GetValue(bundleAsset, ref ctx, ref visitor);
    }

    /// <inheritdoc />
    protected override bool Equals(SelfDataRef other)
    {
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return 1176347863;
    }
}