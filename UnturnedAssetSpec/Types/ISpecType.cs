using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public interface ISpecType : IEquatable<ISpecType>
{
    QualifiedType Parent { get; }
    QualifiedType Type { get; }
    string DisplayName { get; }
    string? Docs { get; }

    SpecProperty? FindProperty(string propertyName, SpecPropertyContext context);
}