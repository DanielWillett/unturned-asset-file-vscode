using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public interface ISpecType : IEquatable<ISpecType>, IComparable<ISpecType>
{
    string Type { get; }
    string DisplayName { get; }
    string? Docs { get; }
}