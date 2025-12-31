using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Properties;

/// <summary>
/// A type that can be used as a property type.
/// </summary>
public interface IPropertyType : IEquatable<IPropertyType?>
{
    /// <summary>
    /// Attempts to resolve the property's current type without context.
    /// </summary>
    /// <returns>Whether or not the type could be resolved successfully without context.</returns>
    bool TryGetConcreteType([NotNullWhen(true)] out IType? type);

    /// <summary>
    /// Attempts to resolve the property's current type with context.
    /// </summary>
    /// <returns>Whether or not the type could be resolved successfully.</returns>
    bool TryEvaluateType([NotNullWhen(true)] out IType? type, in FileEvaluationContext ctx);
}