#if !NET5_0_OR_GREATER
#pragma warning disable IDE0130
namespace System.Runtime.CompilerServices;
#pragma warning restore IDE0130

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Interface |
    AttributeTargets.Method | AttributeTargets.Module | AttributeTargets.Property | AttributeTargets.Struct,
    Inherited = false)]
internal sealed class SkipLocalsInitAttribute : Attribute;
#endif