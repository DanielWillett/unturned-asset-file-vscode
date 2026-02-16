using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Returns the exact key used to specify this property, including aliases, casing, etc. This doens't include quotes.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#This</c></item>
///     <item><c>#Self</c></item>
///     <item>Any property reference.</item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#Target.Key</c>
/// </para>
/// </summary>
public readonly struct KeyProperty : IDataRefProperty, IEquatable<KeyProperty>
{
    /// <inheritdoc />
    public string PropertyName => "Key";

    /// <inheritdoc />
    public bool Equals(KeyProperty other) => true;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is KeyProperty;

    /// <inheritdoc />
    public override int GetHashCode() => 1426712611;

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
        return new DataRefProperty<KeyProperty>(target, default);
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
        return new DataRefProperty<KeyProperty, TValue>(type, target, default);
    }
}