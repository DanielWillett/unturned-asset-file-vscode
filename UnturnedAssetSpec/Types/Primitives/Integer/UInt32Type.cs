namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An unsigned 32-bit integer.
/// <para>Example: <c>AnimalAsset.Reward_XP</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class UInt32Type : PrimitiveType<uint, UInt32Type>
{
    public const string TypeId = "UInt32";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_UInt32;

    public override int GetHashCode() => 937264964;
}