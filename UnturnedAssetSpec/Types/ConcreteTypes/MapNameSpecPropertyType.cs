using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class MapNameSpecPropertyType : BasicSpecPropertyType<MapNameSpecPropertyType, string>
{
    public static readonly MapNameSpecPropertyType Instance = new MapNameSpecPropertyType();

    static MapNameSpecPropertyType() { }
    private MapNameSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "MapName";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Map Name";

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not AssetFileStringValueNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        // todo: resolve map

        value = strValNode.Value;
        return true;
    }
}