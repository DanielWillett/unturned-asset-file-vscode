using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Values;

public interface IPropertyReferenceValue : IValue
{
    /// <summary>
    /// The property being referenced.
    /// </summary>
    DatProperty Property { get; }

    /// <summary>
    /// The owner of the property reference (the property being referenced from).
    /// </summary>
    DatProperty Owner { get; }
}
public interface IPropertyReferenceValue<TValue> : IPropertyReferenceValue, IValue<TValue>
    where TValue : IEquatable<TValue>;