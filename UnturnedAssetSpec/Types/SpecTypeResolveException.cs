using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An exception thrown when a property type resolution fails.
/// </summary>
public class SpecTypeResolveException : Exception
{
    internal SpecTypeResolveException(string message) : base(message) { }
}