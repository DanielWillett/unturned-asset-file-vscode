using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// The contextual difficulty of the current file. Usually this is used in a server config to get the correct default values.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#This</c></item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#This.Difficulty</c>
/// </para>
/// </summary>
public readonly struct DifficultyProperty : IDataRefProperty, IEquatable<DifficultyProperty>
{
    /// <inheritdoc />
    public string PropertyName => "Difficulty";

    /// <inheritdoc />
    public bool Equals(DifficultyProperty other) => true;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is DifficultyProperty;

    /// <inheritdoc />
    public override int GetHashCode() => 976892135;

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    public static IDataRef CreateDataRef(
#else
    public IDataRef CreateDataRef(
#endif
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties)
    {
        return new DataRefProperty<DifficultyProperty>(target, default);
    }

    /// <inheritdoc />
#if NET7_0_OR_GREATER
    public static IDataRef<TValue> CreateDataRef<TValue>(
#else
    public IDataRef<TValue> CreateDataRef<TValue>(
#endif
        IType<TValue> type,
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties
    ) where TValue : IEquatable<TValue>
    {
        return new DataRefProperty<DifficultyProperty, TValue>(type, target, default);
    }
}