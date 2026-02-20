using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Unique flag ID used for quest conditions and rewards.
/// <para>Example: <c>ObjectNPCAsset.PlayerKnowsNameFlagID</c></para>
/// <code>
/// Prop 123
/// </code>
/// </summary>
public sealed class FlagIdType : PrimitiveType<ushort, FlagIdType>, ITypeParser<ushort>
{
    public const string TypeId = "FlagId";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_FlagId;

    public override ITypeParser<ushort> Parser => this;

    public override int GetHashCode() => 1882775473;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<ushort> args, in FileEvaluationContext ctx, out Optional<ushort> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.UInt16.TryParse(ref args, in ctx, out value))
            return false;

        if (args.DiagnosticSink == null || !ctx.TryGetRelevantMap(out _))
        {
            return true;
        }

        // todo check if flag ID is valid.
        // note: doesn't actually have to be valid but if it's not referenced anywhere else we can show a warning or something
        return true;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<ushort> value,
        IType<ushort> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        return TypeParsers.UInt16.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, ushort value, IType<ushort> valueType, JsonSerializerOptions options)
    {
        TypeParsers.UInt16.WriteValueToJson(writer, value, valueType, options);
    }
}