using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current property.
/// </summary>
public sealed class SelfDataRef : RootDataRef<SelfDataRef>
{
    /// <summary>
    /// Singleton instance of <see cref="SelfDataRef"/>.
    /// </summary>
    public static readonly SelfDataRef Instance = new SelfDataRef();

    static SelfDataRef() { }
    private SelfDataRef() { }

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