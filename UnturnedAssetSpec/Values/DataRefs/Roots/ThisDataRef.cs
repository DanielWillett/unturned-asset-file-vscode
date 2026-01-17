using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current <see cref="DatTypeWithProperties"/> or <see cref="IDatTypeWithLocalizationProperties"/> object.
/// </summary>
public sealed class ThisDataRef : RootDataRef<ThisDataRef>
{
    /// <summary>
    /// Singleton instance of <see cref="ThisDataRef"/>.
    /// </summary>
    public static readonly ThisDataRef Instance = new ThisDataRef();

    static ThisDataRef() { }
    private ThisDataRef() { }

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
