using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Helpers and extensions for <see cref="IDataRef"/> values.
/// </summary>
public static class DataRefs
{
    public delegate IDataRef DataRefPropertyFactory(IDataRefTarget target, OneOrMore<KeyValuePair<string, object>> properties, OneOrMore<int> indices);

    /// <summary>
    /// Read-only, case-insensitive set of all reserved keywords for data-refs.
    /// </summary>
    public static IImmutableSet<string> Keywords { get; }

    /// <summary>
    /// All built-in data-ref properties.
    /// </summary>
    public static IImmutableDictionary<string, DataRefPropertyFactory> Properties { get; }

    static DataRefs()
    {
        ImmutableHashSet<string>.Builder bldr = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);

        bldr.Add("This");
        bldr.Add("Self");
        bldr.Add("Index");
        bldr.Add("Value");

        Keywords = bldr.ToImmutable();

        ImmutableDictionary<string, DataRefPropertyFactory>.Builder properties
            = ImmutableDictionary.CreateBuilder<string, DataRefPropertyFactory>(StringComparer.OrdinalIgnoreCase);

        properties["Included"] = (target, properties, _) => new DataRefProperty<IncludedProperty>(
            target,
            new IncludedProperty(
                requireValue: (bool)properties.GetValueOrDefault("RequireValue", BoxedPrimitives.False, StringComparison.OrdinalIgnoreCase)
            )
        );

        properties["Excluded"] = (target, _, _) => new DataRefProperty<ExcludedProperty>(target, new ExcludedProperty());

        Properties = properties.ToImmutable();
    }

    public static bool TryReadDataRef(string text, IType? type, [NotNullWhen(true)] out IDataRef? dataRef)
    {
        dataRef = SelfDataRef.Instance;
        return true;
    }

    public static bool TryReadDataRef<TValue>(string text, IType<TValue> type, [NotNullWhen(true)] out IDataRef? dataRef)
        where TValue : IEquatable<TValue>
    {
        dataRef = SelfDataRef.Instance;
        return true;
    }
}