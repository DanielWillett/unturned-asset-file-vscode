using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The unique ID of an NPCOverlapVolume as set in the level editor.
/// <para>Example: <c>Asset.NPCVolumeOverlapCondition.VolumeID</c></para>
/// <code>
/// Prop overlap_give_gun
/// </code>
/// </summary>
public sealed class OverlapVolumeIdType : PrimitiveType<string, OverlapVolumeIdType>, ITypeParser<string>
{
    public const string TypeId = "OverlapVolumeId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_OverlapVolumeId;

    public override ITypeParser<string> Parser => this;

    public override int GetHashCode() => 1517786790;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (!TypeParsers.String.TryParse(ref args, in ctx, out value))
            return false;

        if (args.DiagnosticSink == null || !ctx.TryGetRelevantMap(out _))
        {
            return true;
        }

        // todo check if overlap volume ID exists.
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