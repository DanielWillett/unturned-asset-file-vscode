using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// An included or excluded value. Just the presence of the property counts as a <see langword="true"/> value, even if the value is set to 'False'.
/// <para>Example: <c>ItemBarricadeAsset.Vulnerable</c></para>
/// </summary>
public sealed class FlagType : PrimitiveType<bool, FlagType>, IFlagType
{
    public const string TypeId = "Flag";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_Flag;
    public override ITypeParser<bool> Parser => TypeParsers.Flag;

    public override int GetHashCode() => 521491847;
}