namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An unsigned 64-bit integer.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class UInt64Type : PrimitiveType<ulong, UInt64Type>
{
    public const string TypeId = "UInt64";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_UInt64;

    public override int GetHashCode() => 1617289617;
}