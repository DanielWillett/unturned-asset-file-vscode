using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public class SpecTypeResolveException : Exception
{
    public SpecTypeResolveException(string message) : base(message) { }
}