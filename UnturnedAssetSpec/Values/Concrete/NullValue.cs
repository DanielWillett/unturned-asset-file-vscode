using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// A <see langword="null"/> value of any type.
/// </summary>
public sealed class NullValue(IType type) : IValue
{
    public IType Type { get; } = type;
    public bool IsNull => true;
}

/// <summary>
/// A <see langword="null"/> value of any strong type.
/// </summary>
public sealed class NullValue<T>(IType<T> type) : IValue<T> where T : IEquatable<T>
{
    public IType<T> Type { get; } = type;
    public bool IsNull => true;

    public bool TryGetConcreteValue(out Optional<T> value)
    {
        value = Optional<T>.Null;
        return true;
    }

    public bool TryEvaluateValue(out Optional<T> value, in FileEvaluationContext ctx)
    {
        value = Optional<T>.Null;
        return true;
    }

    IType IValue.Type => Type;
}