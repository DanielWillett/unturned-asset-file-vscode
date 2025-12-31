namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An unsigned 16-bit integer.
/// <para>Example: <c>AnimalAsset.Health</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class UInt16Type : PrimitiveType<ushort, UInt16Type>
{
    public const string TypeId = "UInt16";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_UInt16;

    public override int GetHashCode() => 122958100;
}