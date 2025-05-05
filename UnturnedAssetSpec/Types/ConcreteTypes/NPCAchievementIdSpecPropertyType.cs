using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class NPCAchievementIdSpecPropertyType :
    BasicSpecPropertyType<NPCAchievementIdSpecPropertyType, string>
{
    public static readonly NPCAchievementIdSpecPropertyType Instance = new NPCAchievementIdSpecPropertyType();
    static NPCAchievementIdSpecPropertyType() { }

    /// <inheritdoc />
    public override string Type => "NPCAchievementId";

    /// <inheritdoc />
    public override string DisplayName => "NPC Achievement ID";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.String;

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