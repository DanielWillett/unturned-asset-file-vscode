using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Data-ref referencing the current element in a list's index.
/// </summary>
public sealed class IndexDataRef<T> : RootDataRef<IndexDataRef<T>>, IValue<T>
    where T : IEquatable<T>
{
    /// <summary>
    /// Singleton instance of <see cref="IndexDataRef"/>.
    /// </summary>
    public static readonly IndexDataRef<T> Instance = new IndexDataRef<T>();

    /// <inheritdoc />
    public IType<T> Type { get; }

    static IndexDataRef() { }

    private IndexDataRef()
    {
        Type = TypeConverters.Get<T>().DefaultType;
    }

    /// <inheritdoc />
    public override string PropertyName => "Index";

    protected override bool IsPropertyNameKeyword => true;

    /// <inheritdoc />
    public override bool VisitValue<TVisitor>(ref TVisitor visitor, in FileEvaluationContext ctx)
    {
        return false;
    }

    /// <inheritdoc />
    protected override bool Equals(IndexDataRef<T> other)
    {
        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1989220716, typeof(T));
    }

    /// <inheritdoc />
    public bool TryGetConcreteValue(out Optional<T> value)
    {
        value = Optional<T>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryEvaluateValue(out Optional<T> value, in FileEvaluationContext ctx)
    {
        value = Optional<T>.Null;
        return false;
    }
}