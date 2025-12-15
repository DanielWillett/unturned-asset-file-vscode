using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Used to allow lookup of other types while reading type info from JSON.
/// </summary>
public interface IDatSpecificationReadContext
{
    /// <summary>
    /// Reads a type from a JSON object or string.
    /// </summary>
    IType ReadType(in JsonElement root, IDatSpecificationObject readObject, string context = "");

    /// <summary>
    /// Reads a property reference from a JSON object or string.
    /// </summary>
    IValue<T> ReadValue<T>(in JsonElement root, IType<T> valueType, IDatSpecificationObject readObject, string context = "") where T : IEquatable<T>;
}
