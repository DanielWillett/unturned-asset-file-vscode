namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A signed 16-bit integer.
/// <para>Example: <c>Asset.NPCZombieKillsCondition.Value</c></para>
/// <code>
/// Prop 123
/// Prop -123
/// </code>
/// </summary>
public sealed class Int16Type : PrimitiveType<short, Int16Type>
{
    public const string TypeId = "Int16";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Int16;

    public override int GetHashCode() => 1714554487;
}