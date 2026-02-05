using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Unique ID of a zombie table as set in the level editor. Values less than zero are usually interpreted as all zombie tables.
/// <para>Example: <c>Asset.NPCZombieReward.LevelTableOverride</c></para>
/// <code>
/// Prop 1
/// </code>
/// </summary>
public sealed class ZombieTableIdType : PrimitiveType<byte, ZombieTableIdType>, ITypeParser<byte>
{
    public const string TypeId = "ZombieTableId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_ZombieTableId;

    public override ITypeParser<byte> Parser => this;

    public override int GetHashCode() => 1181215100;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<byte> args, in FileEvaluationContext ctx, out Optional<byte> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.UInt8.TryParse(ref args, in ctx, out value))
            return false;

        if (args.DiagnosticSink == null || !ctx.TryGetRelevantMap(out _))
        {
            return true;
        }

        // todo: level zombie table ID test (ZombieTable.tableUniqueId)
        return true;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<byte> value, IType<byte> valueType)
    {
        return TypeParsers.UInt8.TryReadValueFromJson(in json, out value, valueType);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, byte value, IType<byte> valueType, JsonSerializerOptions options)
    {
        TypeParsers.UInt8.WriteValueToJson(writer, value, valueType, options);
    }
}