using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A reference to a navmesh placed in the map editor.
/// <para>A value of 255 means no navmesh (or all regions) in most cases.</para>
/// <para>Example: <c>Asset.NPCZombieKillsCondition.Nav</c></para>
/// <code>
/// Prop 14
/// Prop 255
/// </code>
/// </summary>
public sealed class NavIdType : PrimitiveType<byte, NavIdType>, ITypeParser<byte>
{
    public const string TypeId = "NavId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_NavId;

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

        // todo check if nav ID is within range.
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