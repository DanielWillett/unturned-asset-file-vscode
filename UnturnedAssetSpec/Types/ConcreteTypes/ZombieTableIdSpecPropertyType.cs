using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class ZombieTableIdSpecPropertyType : BasicSpecPropertyType<ZombieTableIdSpecPropertyType, int>
{
    public static readonly ZombieTableIdSpecPropertyType Instance = new ZombieTableIdSpecPropertyType();

    public override int GetHashCode() => 86;

    static ZombieTableIdSpecPropertyType() { }
    private ZombieTableIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ZombieTableId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc />
    public override string DisplayName => "Zombie Table ID";

    protected override ISpecDynamicValue CreateValue(int value) => new SpecDynamicConcreteConvertibleValue<int>(value, this);

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out int value)
    {
        if (parse.Node == null)
        {
            return MissingNode(in parse, out value);
        }

        if (parse.Node is not IValueSourceNode strValNode)
        {
            return FailedToParse(in parse, out value);
        }

        if (!KnownTypeValueHelper.TryParseInt32(strValNode.Value, out int val))
        {
            return FailedToParse(in parse, out value);
        }

        value = val;
        // todo: level zombie table ID test (ZombieTable.tableUniqueId)
        return true;
    }
}