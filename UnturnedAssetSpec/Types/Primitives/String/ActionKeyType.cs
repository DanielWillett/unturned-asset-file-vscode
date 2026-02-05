using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Collections.Generic;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// References a valid action key for blueprint actions which are loaded from the <c>Localization\English\Player\PlayerDashboardInventory.dat</c> file.
/// <para>Example: <c>ItemAsset.Action.CommonTextId</c></para>
/// <code>
/// Prop Store
/// </code>
/// </summary>
/// <remarks>Any lines ending in <c>_Button</c> and <c>_Button_Tooltip</c> are valid.</remarks>
public sealed class ActionKeyType : PrimitiveType<string, ActionKeyType>, ITypeParser<string>
{
    public const string TypeId = "ActionKey";

    public override string Id => TypeId;

    public override string DisplayName => Properties.Resources.Type_Name_ActionKey;

    public override ITypeParser<string> Parser => this;

    public override int GetHashCode() => 1338896614;

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<string> args, in FileEvaluationContext ctx, out Optional<string> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.String.TryParse(ref args, in ctx, out value))
            return false;

        if (args.DiagnosticSink == null || !value.HasValue)
            return true;

        // ActionKeys usually implemented with ImmutableHashSet
        IDictionary<string, ActionButton>? achIds = ctx.Services.Database.ValidActionButtons;
        if (achIds == null || !achIds.ContainsKey(value.Value))
        {
            args.DiagnosticSink?.UNT1014(ref args, value.Value);
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