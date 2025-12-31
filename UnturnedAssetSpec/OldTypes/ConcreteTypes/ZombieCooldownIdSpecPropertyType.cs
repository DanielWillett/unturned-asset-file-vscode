using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Unique ID of a cooldown registration for zombie NPC rewards.
/// <para>Example: <c>Asset.NPCZombieReward.CooldownId</c></para>
/// <code>
/// Prop mega_zombie_spawn
/// </code>
/// </summary>
public sealed class ZombieCooldownIdSpecPropertyType : BaseSpecPropertyType<ZombieCooldownIdSpecPropertyType, string>
{
    public static readonly ZombieCooldownIdSpecPropertyType Instance = new ZombieCooldownIdSpecPropertyType();

    public override int GetHashCode() => 87;

    static ZombieCooldownIdSpecPropertyType() { }
    private ZombieCooldownIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "ZombieCooldownId";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

    /// <inheritdoc />
    public override string DisplayName => "Zombie Cooldown ID";

    protected override ISpecDynamicValue CreateValue(string value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

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
        // todo: level zombie table ID test (ZombieTable.tableUniqueId)
        return true;
    }
}