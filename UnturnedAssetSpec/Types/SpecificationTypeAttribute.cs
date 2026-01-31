using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Indicates that a type can be created dynamically when referenced from the asset spec by assembly-qualified name.
/// </summary>
/// <remarks>Override the creation code using a <see cref="FactoryMethod"/>.</remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class SpecificationTypeAttribute : Attribute
{
    /// <summary>
    /// The name of a static method that will create this object given an <see cref="SpecificationTypeFactoryArgs"/>.
    /// It should have the following signature:
    /// <code>
    /// <see langword="static"/> TSelf(<see langword="in"/> <see cref="SpecificationTypeFactoryArgs"/> args);
    /// </code>
    /// </summary>
    public string? FactoryMethod { get; set; }
}