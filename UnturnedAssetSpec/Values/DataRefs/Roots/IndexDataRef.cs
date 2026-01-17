using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current element in a list's index.
/// </summary>
public sealed class IndexDataRef : RootDataRef<IndexDataRef>
{
    /// <summary>
    /// Singleton instance of <see cref="IndexDataRef"/>.
    /// </summary>
    public static readonly IndexDataRef Instance = new IndexDataRef();

    static IndexDataRef() { }
    private IndexDataRef() { }

    /// <inheritdoc />
    public override string PropertyName => "Index";

    protected override bool IsPropertyNameKeyword => true;

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        return false;
    }

    /// <inheritdoc />
    protected override bool Equals(IndexDataRef other)
    {
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return 1989220716;
    }
}
