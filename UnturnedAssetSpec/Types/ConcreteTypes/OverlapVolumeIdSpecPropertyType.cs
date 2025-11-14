using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class OverlapVolumeIdSpecPropertyType : BasicSpecPropertyType<OverlapVolumeIdSpecPropertyType, string>
{
    public static readonly OverlapVolumeIdSpecPropertyType Instance = new OverlapVolumeIdSpecPropertyType();

    public override int GetHashCode() => 85;

    static OverlapVolumeIdSpecPropertyType() { }
    private OverlapVolumeIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "OverlapVolumeId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "NPC Overlap Volume ID";

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        value = strValNode.Value;
        // todo: volume ID test
        return true;
    }
}