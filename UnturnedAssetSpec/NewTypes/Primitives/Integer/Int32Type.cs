namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A signed 32-bit integer.
/// <para>Example: <c>ItemGunAsset.Bursts</c></para>
/// <code>
/// Prop 123
/// Prop -123
/// </code>
/// </summary>
public sealed class Int32Type : PrimitiveType<int, Int32Type>
{
    public const string TypeId = "Int32";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Int32;

    public override int GetHashCode() => 2085051757;
}