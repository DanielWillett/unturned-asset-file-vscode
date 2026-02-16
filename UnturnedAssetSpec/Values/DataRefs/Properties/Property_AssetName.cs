using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Returns the 'name' property of the current asset. This is usually the file name.
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#This</c></item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#This.AssetName</c>
/// </para>
/// </summary>
public readonly struct AssetNameProperty : IDataRefProperty, IEquatable<AssetNameProperty>
{
    /// <inheritdoc />
    public string PropertyName => "AssetName";

    /// <inheritdoc />
    public bool Equals(AssetNameProperty other) => true;

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssetNameProperty;

    /// <inheritdoc />
    public override int GetHashCode() => 1873219462;

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
        return new DataRefProperty<AssetNameProperty>(target, default);
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
        return new DataRefProperty<AssetNameProperty, TValue>(type, target, default);
    }
}