namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A true or false value.
/// <para>Example: <c>ItemBarricadeAsset.Can_Be_Damaged</c></para>
/// <code>
/// Prop True
/// Prop False
/// </code>
/// </summary>
public sealed class BooleanType : PrimitiveType<bool, BooleanType>
{
    public const string TypeId = "Boolean";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Boolean;

    public override int GetHashCode() => 164523370;
}