using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

/// <summary>
/// Base interface for properties of data-ref targets (<see cref="IDataRefTarget"/>).
/// </summary>
public interface IDataRefProperty
{
    string PropertyName { get; }
}

public interface IIndexableDataRefProperty : IDataRefProperty
{
    OneOrMore<int> Indices { get; }
}

public interface IConfigurableDataRefProperty : IDataRefProperty
{
    OneOrMore<KeyValuePair<string, object>> Options { get; }
}