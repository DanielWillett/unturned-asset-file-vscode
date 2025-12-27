using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Utility;

public interface IGenericVisitor
{
    void Accept<T>(T? value) where T : IEquatable<T>;
}
