namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A signed 64-bit integer.
/// <para>Example: <c>Asset.NPCDateCounterCondition.Value</c></para>
/// <code>
/// Prop 123
/// Prop -123
/// </code>
/// </summary>
public sealed class Int64Type : PrimitiveType<long, Int64Type>
{
    public const string TypeId = "Int64";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Int64;

    public override int GetHashCode() => 367049358;
}