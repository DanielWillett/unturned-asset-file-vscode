using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Indicates that a type can be used to implement custom logic for parsing enums or custom types.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class StringParseableTypeAttribute : Attribute;