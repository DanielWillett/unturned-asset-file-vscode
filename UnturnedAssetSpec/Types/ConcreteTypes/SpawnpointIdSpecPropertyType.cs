using DanielWillett.UnturnedDataFileLspServer.Data.Files;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class SpawnpointIdSpecPropertyType : BasicSpecPropertyType<SpawnpointIdSpecPropertyType, string>
{
    public static readonly SpawnpointIdSpecPropertyType Instance = new SpawnpointIdSpecPropertyType();

    static SpawnpointIdSpecPropertyType() { }
    private SpawnpointIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "SpawnpointId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Spawnpoint ID";

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

        value = strValNode.Value;
        // todo: spawnpoint test
        return true;
    }
}