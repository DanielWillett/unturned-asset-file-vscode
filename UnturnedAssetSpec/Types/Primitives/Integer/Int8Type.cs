namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A signed 8-bit integer.
/// <para>Example: <c>ItemConsumableAsset.Oxygen</c></para>
/// <code>
/// Prop 123
/// Prop -123
/// </code>
/// </summary>
public sealed class Int8Type : PrimitiveType<sbyte, Int8Type>
{
    public const string TypeId = "Int8";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Int8;

    public override int GetHashCode() => 862102374;
}