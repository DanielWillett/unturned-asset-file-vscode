using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

[Flags]
internal enum VectorTypeParseOptions
{
    Composite = 1,
    Object = 2,
    Legacy = 4
}