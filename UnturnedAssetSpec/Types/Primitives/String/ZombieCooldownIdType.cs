using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Unique ID of a cooldown registration for zombie NPC rewards.
/// <para>Example: <c>Asset.NPCZombieReward.CooldownId</c></para>
/// <code>
/// Prop mega_zombie_spawn
/// </code>
/// </summary>
public sealed class ZombieCooldownIdType : PrimitiveType<string, ZombieCooldownIdType>, ITypeParser<string>
{
    public const string TypeId = "ZombieCooldownId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_ZombieCooldownId;

    public override ITypeParser<string> Parser => this;

    public override int GetHashCode() => 1959643143;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.String.TryParse(ref args, in ctx, out value))
            return false;

        if (args.DiagnosticSink == null || !ctx.TryGetRelevantMap(out _))
        {
            return true;
        }

        // todo check if spawnpoint ID exists.
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