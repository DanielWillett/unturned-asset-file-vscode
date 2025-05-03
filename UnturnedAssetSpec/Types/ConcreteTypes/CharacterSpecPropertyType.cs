using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class CharacterSpecPropertyType : BasicSpecPropertyType<CharacterSpecPropertyType, char>
{
    public static readonly CharacterSpecPropertyType Instance = new CharacterSpecPropertyType();

    static CharacterSpecPropertyType() { }
    private CharacterSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "Character";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Character";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out char value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseCharacter(strValNode.Value, out value))
        {
            return FailedToParse(in parse, out value);
        }
        
        return true;
    }
}