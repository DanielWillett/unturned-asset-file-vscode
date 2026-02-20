using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current <see cref="DatTypeWithProperties"/> or <see cref="IDatTypeWithLocalizationProperties"/> object.
/// </summary>
public sealed class ThisDataRef : RootDataRef<ThisDataRef>
{
    /// <summary>
    /// The property being defining this data-ref.
    /// </summary>
    public DatProperty Owner { get; }

    public ThisDataRef(DatProperty owner)
    {
        Owner = owner;
    }

    /// <inheritdoc />
    public override string PropertyName => "This";

    protected override bool IsPropertyNameKeyword => true;

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        // NOTE: while we could return the 'this' object (created from DatCustomType),
        //       that could create an infinite loop when trying to resolve
        //       the value of the current property if we're not careful.
        //
        //       I don't really see a use for '#This' as a property value
        //       so better to just not support it.

        return false;
    }

    protected override bool AcceptProperty(in IncludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        // TODO: check if object is included instead
        value = Owner.IsIncluded(property.RequireValue, in ctx);
        return true;
    }

    protected override bool AcceptProperty(in ExcludedProperty property, in FileEvaluationContext ctx, out bool value)
    {
        // TODO: check if object is excluded instead
        value = Owner.IsExcluded(in ctx);
        return true;
    }

    protected override bool AcceptProperty(in KeyProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        // TODO: get object key
        value = PropertyDataRef.GetPropertyKey(Owner, in ctx);
        return value != null;
    }

    protected override bool AcceptProperty(in AssetNameProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        switch (ctx.File)
        {
            case ILocalizationSourceFile lcl:
                value = lcl.Asset.AssetName;
                return true;

            case IAssetSourceFile asset:
                value = asset.AssetName;
                return true;

            default:
                value = null;
                return false;
        }
    }

    protected override bool AcceptProperty(in DifficultyProperty property, in FileEvaluationContext ctx, [NotNullWhen(true)] out string? value)
    {
        if (!DifficultyProperty.TryGetFileDifficultyContext(in ctx, out ServerDifficulty diff))
        {
            value = null;
            return false;
        }

        value = DifficultyProperty.GetDifficultyName(diff);
        return true;
    }

    protected override bool AcceptProperty<TVisitor>(in IndicesProperty property, in FileEvaluationContext ctx, ref TVisitor visitor)
    {
        // todo
        return false;
    }

    /// <inheritdoc />
    protected override bool Equals(ThisDataRef other)
    {
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return 1528824009;
    }
}