namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An unsigned 8-bit integer.
/// <para>Example: <c>ItemAsset.Size_X</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class UInt8Type : PrimitiveType<byte, UInt8Type>
{
    public const string TypeId = "UInt8";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_UInt8;

    public override int GetHashCode() => 382943854;
}