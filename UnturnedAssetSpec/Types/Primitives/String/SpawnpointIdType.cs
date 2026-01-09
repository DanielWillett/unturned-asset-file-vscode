using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The unique ID of an NPC spawnpoint node as set in the level editor.
/// <para>Example: <c>Asset.NPCVehicleReward.Spawnpoint</c></para>
/// <code>
/// Prop liberator_spawn_jet
/// </code>
/// </summary>
public sealed class SpawnpointIdType : PrimitiveType<string, SpawnpointIdType>, ITypeParser<string>
{
    public const string TypeId = "SpawnpointId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_SpawnpointId;

    public override ITypeParser<string> Parser => this;

    public override int GetHashCode() => 839766959;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
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