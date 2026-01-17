using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Generic;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The name of an NPC achievement. Valid NPC achievements are listed in the Status.json file under "Achievements"."NPC_Achievement_IDs".
/// <para>Example: <c>Asset.NPCAchievementReward.ID</c></para>
/// <code>
/// Prop Soulcrystal
/// </code>
/// </summary>
public sealed class NPCAchievementIdType : PrimitiveType<string, NPCAchievementIdType>, ITypeParser<string>
{
    public const string TypeId = "NPCAchievementId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_NPCAchievementId;

    public override ITypeParser<string> Parser => this;

    public override int GetHashCode() => 2031038088;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (!TypeParsers.String.TryParse(ref args, in ctx, out value))
            return false;

        if (args.DiagnosticSink == null || !value.HasValue)
            return true;

        // NPCAchievementIds usually implemented with ImmutableHashSet
        ICollection<string>? achIds = ctx.Information.NPCAchievementIds;
        if (achIds == null || !achIds.Contains(value.Value))
        {
            args.DiagnosticSink?.UNT1013(ref args, value.Value);
        }

        return true;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<string> value, IType<string> valueType)
    {
        return TypeParsers.String.TryReadValueFromJson(in json, out value, valueType);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, string value, IType<string> valueType, JsonSerializerOptions options)
    {
        TypeParsers.String.WriteValueToJson(writer, value, valueType, options);
    }
}