using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Checks whether or not the given property has a value.
/// <para>
/// Supports one index parameter.
/// </para>
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#Self</c></item>
///     <item><c>#This</c></item>
///     <item>Any property reference.</item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#Target.Indices</c><br/>
/// <c>#Target.Indices[0]</c><br/>
/// <c>#Target.Indices[-1]</c> (starts from end of array)
/// </para>
/// </summary>
public readonly struct IndicesProperty : IIndexableDataRefProperty, IEquatable<IndicesProperty>
{
    /// <summary>
    /// The index being referred to, or <see langword="null"/> to reference the entire array.
    /// </summary>
    public int? Index { get; }

    /// <summary>
    /// When used with <c>ValueTemplateGroupReference</c>, makes it an error to reference the current object.
    /// </summary>
    public bool PreventSelfReference { get; }

    /// <inheritdoc />
    public OneOrMore<int> Indices => Index.HasValue ? new OneOrMore<int>(Index.Value) : OneOrMore<int>.Null;

    /// <inheritdoc />
    public string PropertyName => "Indices";

    public IndicesProperty(int? index, bool preventSelfReference)
    {
        Index = index;
        PreventSelfReference = preventSelfReference;
    }

    /// <summary>
    /// Gets the target index for an array of the given length, or -1 to reference the whole array.
    /// </summary>
    /// <remarks>The returned index may be out of range, in which case a <see langword="null"/> value should be returned.</remarks>
    public int GetIndex(int length)
    {
        if (!Index.HasValue)
            return -1;

        int index = Index.Value;
        if (index < 0)
        {
            index = length + index;
            if (index < 0)
                return length;
        }

        return index;
    }

    /// <inheritdoc />
    public bool Equals(IndicesProperty other)
    {
        return other.Index == Index;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IndicesProperty prop && Equals(prop);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(767241444, Index);
    }

    internal static IndicesProperty Create(OneOrMore<int> indices, OneOrMore<KeyValuePair<string, object?>> properties)
    {
        return new IndicesProperty(
            index: indices.Length > 0 ? indices[0] : null,
            preventSelfReference: (bool)properties.GetValueOrDefault(nameof(PreventSelfReference), BoxedPrimitives.False, StringComparison.OrdinalIgnoreCase)
        );
    }

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
        return new DataRefProperty<IndicesProperty>(target, Create(indices, properties));
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
        return new DataRefProperty<IndicesProperty, TValue>(type, target, Create(indices, properties));
    }
}