using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

#pragma warning disable CA2231

/// <summary>
/// Checks whether or not the given property has a value.
/// <para>
/// Supported properties:
/// <list type="bullet">
///     <item><see cref="bool"/> RequireValue - Whether or not the property must also have a valid value to count as 'included'.</item>
/// </list>
/// </para>
/// <para>
/// Supported targets:
/// <list type="bullet">
///     <item><c>#Self</c></item>
///     <item>Any property reference.</item>
/// </list>
/// </para>
/// <para>
/// Syntax:<br/>
/// <c>#Target.Included</c><br/>
/// <c>#Target.Included{RequireValue=true}</c>
/// </para>
/// </summary>
public readonly struct IncludedProperty : IConfigurableDataRefProperty, IEquatable<IncludedProperty>
{
    /// <summary>
    /// Whether or not the property must also have a valid value to count as 'included'.
    /// </summary>
    public bool RequireValue { get; }

    /// <inheritdoc />
    public string PropertyName => "Included";

    public IncludedProperty(bool requireValue)
    {
        RequireValue = requireValue;
    }

    /// <inheritdoc />
    public OneOrMore<KeyValuePair<string, object>> Options
    {
        get
        {
            if (RequireValue)
            {
                return new OneOrMore<KeyValuePair<string, object>>(
                    new KeyValuePair<string, object>(nameof(RequireValue), BoxedPrimitives.True)
                );
            }

            return OneOrMore<KeyValuePair<string, object>>.Null;
        }
    }

    /// <inheritdoc />
    public bool Equals(IncludedProperty other)
    {
        return other.RequireValue == RequireValue;
    }

    /// <inheritdoc />
    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is IncludedProperty prop && Equals(prop);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1178292574, RequireValue);
    }

    internal static IncludedProperty Create(OneOrMore<KeyValuePair<string, object?>> properties)
    {
        return new IncludedProperty(
            requireValue: (bool)properties.GetValueOrDefault(nameof(RequireValue), BoxedPrimitives.False, StringComparison.OrdinalIgnoreCase)
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
        return new DataRefProperty<IncludedProperty>(target, Create(properties));
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
        return new DataRefProperty<IncludedProperty, TValue>(type, target, Create(properties));
    }
}