using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The directory name of a map, either installed locally or in workshop.
/// <para>Example: <c>ItemGunAsset.Magazine_Replacement_#_Map</c></para>
/// <code>
/// Prop Washington
/// </code>
/// </summary>
public sealed class MapNameSpecPropertyType : BasicSpecPropertyType<MapNameSpecPropertyType, string>
{
    public static readonly MapNameSpecPropertyType Instance = new MapNameSpecPropertyType();

    public override int GetHashCode() => 36;

    static MapNameSpecPropertyType() { }
    private MapNameSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "MapName";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Map Name";

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

        // todo: resolve map

        value = strValNode.Value;
        return true;
    }
}