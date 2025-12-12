using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Runtime.CompilerServices;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Utilities for working with the <see cref="IValue"/> system.
/// </summary>
public static class Values
{
    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    public static IValue Null(IType type)
    {
        return new NullValue(type);
    }

    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    [OverloadResolutionPriority(1)]
    public static IValue<T> Null<T>(IType<T> type) where T : IEquatable<T>
    {
        return new NullValue<T>(type);
    }

    /// <summary>
    /// A <see langword="null"/> value of a given <paramref name="type"/>.
    /// </summary>
    [OverloadResolutionPriority(2)]
    public static IValue<T> Null<TType, T>(PrimitiveType<T, TType> type) where TType : PrimitiveType<T, TType>, new() where T : IEquatable<T>
    {
        return PrimitiveType<T, TType>.Null;
    }

    /// <summary>
    /// The <see langword="true"/> concrete value of type <see cref="BooleanType"/>.
    /// </summary>
    public static ConcreteValue<bool> True { get; } = new ConcreteValue<bool>(true, BooleanType.Instance);

    /// <summary>
    /// The <see langword="false"/> concrete value of type <see cref="BooleanType"/>.
    /// </summary>
    public static ConcreteValue<bool> False { get; } = new ConcreteValue<bool>(false, BooleanType.Instance);

    /// <summary>
    /// The <see langword="true"/> concrete value of type <see cref="FlagType"/>.
    /// </summary>
    public static ConcreteValue<bool> Included { get; } = new ConcreteValue<bool>(true, FlagType.Instance);

    /// <summary>
    /// The <see langword="false"/> concrete value of type <see cref="FlagType"/>.
    /// </summary>
    public static ConcreteValue<bool> Excluded { get; } = new ConcreteValue<bool>(false, FlagType.Instance);

    /// <summary>
    /// A value of type <see cref="FlagType"/>.
    /// </summary>
    public static ConcreteValue<bool> Flag(bool v) => v ? Included : Excluded;

    /// <summary>
    /// A value of type <see cref="BooleanType"/>.
    /// </summary>
    public static ConcreteValue<bool> Boolean(bool v) => v ? True : False;

    /// <summary>
    /// A boolean type which can be of type <see cref="KnownTypes.Flag"/>, <see cref="KnownTypes.Boolean"/>, or some other boolean type.
    /// </summary>
    public static ConcreteValue<bool> Boolean(bool v, IType<bool>? type)
    {
        if ((object?)type == FlagType.Instance)
        {
            return Flag(v);
        }

        if (type == null || (object?)type == BooleanType.Instance)
        {
            return Boolean(v);
        }

        return new ConcreteValue<bool>(v, type);
    }

    /// <summary>
    /// Creates a concrete value of a generic type.
    /// </summary>
    public static ConcreteValue<TValue> Create<TValue>(TValue v, IType<TValue> type) where TValue : IEquatable<TValue>
    {
        return new ConcreteValue<TValue>(v, type);
    }
}