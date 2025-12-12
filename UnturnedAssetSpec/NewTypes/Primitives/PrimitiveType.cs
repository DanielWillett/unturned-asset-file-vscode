using DanielWillett.UnturnedDataFileLspServer.Data.NewTypes;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A single-instance, value-only type which is represented by a built-in CLR type.
/// </summary>
/// <typeparam name="TValue">The CLR type to parse.</typeparam>
/// <typeparam name="TSelf">The implementing type.</typeparam>
public abstract class PrimitiveType<TValue, TSelf>
    : BaseType<TValue, TSelf>
    where TValue : IEquatable<TValue>
    where TSelf : PrimitiveType<TValue, TSelf>, new()
{
    /// <summary>
    /// Singleton instance of this type.
    /// </summary>
    public static TSelf Instance { get; } = new TSelf();
    static PrimitiveType() { }

    /// <summary>
    /// Singleton instance of the <see langword="null"/> value of this type.
    /// </summary>
    [field: MaybeNull]
    public static IValue<TValue> Null => field ??= new NullValue<TValue>(Instance);

    public override ITypeParser<TValue> Parser => TypeParsers.Get<TValue>();

#pragma warning disable CS0659

    protected override bool Equals(TSelf other)
    {
        return true;
    }

    public override bool Equals(object? other)
    {
        return other is TSelf;
    }

#pragma warning restore CS0659
}