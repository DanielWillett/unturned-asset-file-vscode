using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// A skill name or blueprint skill name.
/// <para>Example: <c>LevelAsset.SkillRule.Id</c></para>
/// <code>
/// // normal skill
/// Prop Engineer
///
/// // blueprint skill
/// Prop Craft
/// </code>
/// </summary>
internal class SkillType : BaseType<SkillReference, SkillType>, ITypeParser<SkillReference>
{
    private readonly AssetInformation _info;

    internal const string BlueprintSkillEnumType = "SDG.Unturned.EBlueprintSkill, Assembly-CSharp";

    private static readonly string[] DisplayNames =
    [
        Resources.Type_Name_Skill,
        Resources.Type_Name_BlueprintSkill
    ];

    /// <inheritdoc />
    public override string Id => TypeIds[(int)Kind];

    /// <inheritdoc />
    public override string DisplayName => DisplayNames[(int)Kind];

    /// <inheritdoc />
    public override ITypeParser<SkillReference> Parser => this;

    /// <summary>
    /// Type IDs of this type indexed by <see cref="SkillKind"/>.
    /// </summary>
    public static readonly ImmutableArray<string> TypeIds = ImmutableArray.Create<string>
    (
        "Skill",
        "BlueprintSkill"
    );

    /// <summary>
    /// The type factory for the skill type.
    /// </summary>
    public static ITypeFactory Factory { get; } = new TypeFactoryById
    (
        (
            TypeIds[(int)SkillKind.Skill],
            (ctx, _, _) => new SkillType(SkillKind.Skill, ctx)
        ),
        (
            TypeIds[(int)SkillKind.BackwardsCompatibleBlueprintSkill],
            (ctx, _, _) => new SkillType(SkillKind.BackwardsCompatibleBlueprintSkill, ctx)
        )
    );

    /// <summary>
    /// The type of skill to parse.
    /// </summary>
    public SkillKind Kind { get; }

    public SkillType(SkillKind kind, IDatSpecificationReadContext context)
    {
        if (kind is < SkillKind.Skill or > SkillKind.BackwardsCompatibleBlueprintSkill)
            throw new InvalidEnumArgumentException(nameof(kind), (int)kind, typeof(SkillKind));

        if (context == null)
            throw new ArgumentNullException(nameof(context));

        _info = context.Information;
        Kind = kind;
    }

    /// <inheritdoc />
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Id);
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<SkillReference> args, in FileEvaluationContext ctx, out Optional<SkillReference> value)
    {
        value = Optional<SkillReference>.Null;

        args.CreateSubTypeParserArgs(out TypeParserArgs<string> stringParseArgs, args.ValueNode, args.ParentNode, StringType.Instance, LegacyExpansionFilter.Either);
        
        if (!TypeParsers.String.TryParse(ref stringParseArgs, in ctx, out Optional<string> valueAsString) || string.IsNullOrEmpty(valueAsString.Value))
        {
            return false;
        }

        if (Kind == SkillKind.BackwardsCompatibleBlueprintSkill)
        {
            if (ctx.Information.BlueprintSkills is not { Count: > 0 })
            {
                args.DiagnosticSink?.UNT2005(ref args, BlueprintSkillEnumType);
            }
            else if (SkillReference.TryParseFromBlueprintSkill(valueAsString.Value, ctx.Information, out SkillReference blueprintEnumValue))
            {
                value = blueprintEnumValue;
                return true;
            }
        }

        if (!SkillReference.TryParse(valueAsString.Value, _info, out SkillReference skillRef))
        {
            args.DiagnosticSink?.UNT1014(ref args, valueAsString.Value);
            return false;
        }

        value = new Optional<SkillReference>(skillRef);
        return true;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<SkillReference> value, IType<SkillReference> valueType)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.Null:
                value = Optional<SkillReference>.Null;
                return true;

            case JsonValueKind.String when SkillReference.TryParse(json.GetString()!, _info, out SkillReference skillRef):
                value = skillRef;
                return true;

            default:
                value = Optional<SkillReference>.Null;
                return false;
        }
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, SkillReference value, IType<SkillReference> valueType, JsonSerializerOptions options)
    {
        if (value.TryGetSkillInfo(_info, out SkillInfo? skillInfo))
        {
            writer.WriteStringValue(skillInfo.Skill);
        }
        else
        {
            writer.WriteNullValue();
        }
    }

    /// <inheritdoc />
    protected override bool Equals(SkillType other)
    {
        return _info == other._info && other.Kind == Kind;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1935388404, Kind);
    }
}

/// <summary>
/// The kind of skill to reference.
/// </summary>
public enum SkillKind
{
    /// <summary>
    /// The name of any skill in any speciality.
    /// </summary>
    Skill,

    /// <summary>
    /// The name of any skill in any speciality, or a member of the legacy EBlueprintSkill enum.
    /// </summary>
    BackwardsCompatibleBlueprintSkill
}