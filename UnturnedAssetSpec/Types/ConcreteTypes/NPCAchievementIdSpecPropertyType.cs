using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The name of an NPC achievement. Valid NPC achievements are listed in the Status.json file under "Achievements"."NPC_Achievement_IDs".
/// <para>Example: <c>Asset.NPCAchievementReward.ID</c></para>
/// <code>
/// Prop Soulcrystal
/// </code>
/// </summary>
public sealed class NPCAchievementIdSpecPropertyType :
    BaseSpecPropertyType<NPCAchievementIdSpecPropertyType, string>
{
    public static readonly NPCAchievementIdSpecPropertyType Instance = new NPCAchievementIdSpecPropertyType();

    public override int GetHashCode() => 39;

    static NPCAchievementIdSpecPropertyType() { }
    private NPCAchievementIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "NPCAchievementId";

    /// <inheritdoc />
    public override string DisplayName => "NPC Achievement ID";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

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

        string val = strValNode.Value;
        value = val;
        if (parse.HasDiagnostics
            && parse.Database.NPCAchievementIds != null
            && Array.IndexOf(parse.Database.NPCAchievementIds, val) < 0)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Range = strValNode.Range,
                Diagnostic = DatDiagnostics.UNT1013,
                Message = DiagnosticResources.UNT1013
            });
        }

        return true;
    }
}