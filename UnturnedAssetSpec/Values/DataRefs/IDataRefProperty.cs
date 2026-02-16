using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Base interface for properties of data-ref targets (<see cref="IDataRefTarget"/>).
/// </summary>
public interface IDataRefProperty
{
    string PropertyName { get; }

#if NET7_0_OR_GREATER

    static abstract IDataRef CreateDataRef(
#else
    IDataRef CreateDataRef(
#endif
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties
    );

#if NET7_0_OR_GREATER

    static abstract IDataRef<TValue> CreateDataRef<TValue>(
#else
    IDataRef<TValue> CreateDataRef<TValue>(
#endif
        IType<TValue> type,
        IDataRefTarget target,
        OneOrMore<int> indices,
        OneOrMore<KeyValuePair<string, object?>> properties
    ) where TValue : IEquatable<TValue>;
}

public interface IIndexableDataRefProperty : IDataRefProperty
{
    OneOrMore<int> Indices { get; }
}

public interface IConfigurableDataRefProperty : IDataRefProperty
{
    OneOrMore<KeyValuePair<string, object>> Options { get; }
}