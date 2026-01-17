using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

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
public struct IncludedProperty : IConfigurableDataRefProperty, IEquatable<IncludedProperty>
{
    /// <summary>
    /// Whether or not the property must also have a valid value to count as 'included'.
    /// </summary>
    public bool RequireValue { get; set; }

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
    public bool Equals(IncludedProperty other) => other.RequireValue == RequireValue;
}