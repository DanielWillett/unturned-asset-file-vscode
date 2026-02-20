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
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        // NOTE: it doesn't make sense to return the value of the current property
        //       since that's what's being evaluated in this function.

        return false;
    }

    protected override bool AcceptProperty(in IncludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        value = Owner.IsIncluded(property.RequireValue, in ctx);
        return true;
    }

    protected override bool AcceptProperty(in ExcludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        value = Owner.IsExcluded(in ctx);
        return true;
    }

    protected override bool AcceptProperty(in KeyProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = PropertyDataRef.GetPropertyKey(Owner, in ctx);
        return value != null;
    }

    protected override bool AcceptProperty<TVisitor>(in IndicesProperty property, in FileEvaluationContext ctx, ref TVisitor visitor)
    {
        // todo
        return false;
    }

    protected override bool AcceptProperty(in IsLegacyProperty property, in FileEvaluationContext ctx, out bool value)
    {
        // todo
        value = false;
        return false;
    }

    /// <inheritdoc />
    protected override bool AcceptProperty(in ValueTypeProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        value = ValueTypeProperty.GetTypeName(Owner.GetValueType(in ctx));
        return true;
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