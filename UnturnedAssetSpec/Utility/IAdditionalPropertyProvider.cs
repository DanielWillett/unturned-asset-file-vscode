using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

/// <summary>
/// Allows a type to provide a dictionary of extra properties.
/// </summary>
public interface IAdditionalPropertyProvider
{
    /// <summary>
    /// Key-value dictionary of extra properties from JSON data.
    /// </summary>
    OneOrMore<KeyValuePair<string, object?>> AdditionalProperties { get; }
}

public static class AdditionalPropertyProviderExtensions
{
    /// <summary>
    /// Attempts to get an additional property as a type of value.
    /// </summary>
    public static bool TryGetAdditionalProperty<T>(this IAdditionalPropertyProvider provider, string name, out T? val)
    {
        if (provider == null)
            throw new ArgumentNullException(nameof(provider));

        if (name == null || !provider.AdditionalProperties.TryGetValue(name, out object? valBox, StringComparison.OrdinalIgnoreCase))
        {
            val = default;
            return false;
        }

        if (valBox is T t)
        {
            val = t;
            return true;
        }

        if (valBox == null && !typeof(T).IsValueType)
        {
            val = default;
            return true;
        }

        try
        {
            object? obj2 = Convert.ChangeType(valBox!, typeof(T));
            val = (T)obj2;
            return true;
        }
        catch
        {
            val = default;
            return false;
        }
    }
}