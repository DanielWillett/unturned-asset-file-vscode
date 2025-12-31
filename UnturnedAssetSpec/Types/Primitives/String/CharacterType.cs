namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A single text character.
/// <para>Currently unused by Unturned.</para>
/// <code>
/// Prop a
/// </code>
/// </summary>
public sealed class CharacterType : PrimitiveType<char, CharacterType>
{
    public const string TypeId = "Character";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Character;

    public override int GetHashCode() => 111534601;
}