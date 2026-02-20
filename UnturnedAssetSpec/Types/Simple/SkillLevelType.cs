using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The skill level of another property's skill.
/// <para>Example: <c>ItemAsset.Blueprint.Skill_Level</c></para>
/// <code>
/// Skill Sharpshooter
/// Prop 6
///
/// // or
/// Speciality 0
/// Skill 1
/// Prop 6
/// </code>
/// <para>
/// Supports the following properties:
/// <list type="bullet">
///     <item><c><see cref="IValue"/> Skill</c> - Property reference to a skill name property. Should not be set with <c>SpecialityIndex</c> and <c>SkillIndex</c>.</item>
///     <item><c><see cref="IValue"/> SpecialityIndex</c> - Property reference to a speciality index property. Also requires that <c>SkillIndex</c> is set.</item>
///     <item><c><see cref="IValue"/> SkillIndex</c> - Property reference to a skill index property. Also requires that <c>SpecialityIndex</c> is set.</item>
///     <item><c><see cref="bool"/> AllowNegatives</c> - Whether or not negative values can be used to imply no skill.</item>
/// </list>
/// </para>
/// </summary>
internal class SkillLevelType : BaseType<int, SkillLevelType>, ITypeParser<int>, ITypeFactory
{
    private readonly AssetInformation _info;

    // property-ref to a Skill string property
    private readonly IValue? _skillValue;

    // property-refs to speciality and skill index properties
    private readonly IValue? _specialityIndexValue;
    private readonly IValue? _skillIndexValue;
    
    /// <summary>
    /// Type ID of this type.
    /// </summary>
    public const string TypeId = "SkillLevel";

    /// <inheritdoc />
    public override string Id => TypeId;

    /// <inheritdoc />
    public override string DisplayName => Resources.Type_Name_SkillLevel;

    /// <inheritdoc />
    public override ITypeParser<int> Parser => this;

    /// <summary>
    /// The type factory for the skill level type.
    /// </summary>
    public static ITypeFactory Factory { get; } = new SkillLevelType();

    /// <summary>
    /// Whether or not negative values can be used to indicate some other state, such as no skill.
    /// </summary>
    public bool AllowNegatives { get; init; }

    private SkillLevelType() { _info = null!; }

    public SkillLevelType(IDatSpecificationReadContext context)
    {
        _info = context.Information;
    }

    public SkillLevelType(IValue<SkillReference> skillValue, IDatSpecificationReadContext context) : this(context)
    {
        _skillValue = skillValue ?? throw new ArgumentNullException(nameof(skillValue));
    }

    public SkillLevelType(IValue specialityIndexValue, IValue skillIndexValue, IDatSpecificationReadContext context) : this(context)
    {
        _specialityIndexValue = specialityIndexValue ?? throw new ArgumentNullException(nameof(specialityIndexValue));
        _skillIndexValue = skillIndexValue ?? throw new ArgumentNullException(nameof(skillIndexValue));
    }

    /// <inheritdoc />
    public override void WriteToJson(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        if (_skillValue == null && _specialityIndexValue == null && _skillIndexValue == null && !AllowNegatives)
        {
            writer.WriteStringValue(Id);
            return;
        }

        writer.WriteStartObject();
        
        WriteTypeName(writer);

        if (AllowNegatives)
        {
            writer.WriteBoolean("AllowNegatives"u8, true);
        }

        if (_skillValue != null)
        {
            writer.WritePropertyName("Skill"u8);
            _skillValue.WriteToJson(writer, options);
        }
        else if (_specialityIndexValue != null && _skillIndexValue != null)
        {
            writer.WritePropertyName("SpecialityIndex"u8);
            _specialityIndexValue.WriteToJson(writer, options);
            writer.WritePropertyName("SkillIndex"u8);
            _skillIndexValue.WriteToJson(writer, options);
        }

        writer.WriteEndObject();
    }

    /// <inheritdoc />
    public IType CreateType(in JsonElement typeDefinition, string typeId, IDatSpecificationReadContext spec,
        DatProperty owner, string context = "")
    {
        if (typeDefinition.ValueKind == JsonValueKind.String)
            return new SkillLevelType(spec);

        bool allowNegatives = false;
        if (typeDefinition.TryGetProperty("AllowNegatives"u8, out JsonElement element) && element.ValueKind != JsonValueKind.Null)
        {
            allowNegatives = element.GetBoolean();
        }

        if (typeDefinition.TryGetProperty("Skill"u8, out element))
        {
            IValue<SkillReference> skill = spec.ReadValue(in element, new SkillType(SkillKind.BackwardsCompatibleBlueprintSkill, spec), owner, context, options: ValueReadOptions.Default | ValueReadOptions.AssumeProperty);
            return new SkillLevelType(skill, spec) { AllowNegatives = allowNegatives };
        }

        IValue<int>? specIndex = null, skillIndex = null;
        if (typeDefinition.TryGetProperty("SpecialityIndex"u8, out element))
        {
            specIndex = spec.ReadValue(in element, Int32Type.Instance, owner, context, options: ValueReadOptions.Default | ValueReadOptions.AssumeProperty);
        }

        if (typeDefinition.TryGetProperty("SkillIndex"u8, out element))
        {
            skillIndex = spec.ReadValue(in element, Int32Type.Instance, owner, context, options: ValueReadOptions.Default | ValueReadOptions.AssumeProperty);
        }

        if (specIndex != null && skillIndex != null)
        {
            return new SkillLevelType(specIndex, skillIndex, spec) { AllowNegatives = allowNegatives };
        }

        return new SkillLevelType(spec) { AllowNegatives = allowNegatives };
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<int> args, in FileEvaluationContext ctx, out Optional<int> value)
    {
        if (TypeParsers.TryApplyMissingValueBehavior(ref args, in ctx, out value, out bool rtn))
        {
            return rtn;
        }

        if (!TypeParsers.Int32.TryParse(ref args, in ctx, out value))
        {
            return false;
        }

        if (!value.HasValue || !AllowNegatives && value.Value < 0)
        {
            return false;
        }

        int maxValue = -1;

        SkillInfo? skillInfo = null;
        if (args.DiagnosticSink != null && value.Value >= 0)
        {
            if (TryGetCurrentSkillReference(in ctx, out SkillReference currentSkillRef)
                && currentSkillRef.TryGetSkillInfo(_info, out skillInfo))
            {
                maxValue = skillInfo.MaximumLevel;
            }
            else if (_skillValue != null)
            {
                args.DiagnosticSink.UNT109(ref args, _skillValue.ToString()!);
            }
            else if (_specialityIndexValue != null && _skillIndexValue != null)
            {
                args.DiagnosticSink.UNT109(ref args, _specialityIndexValue.ToString()!, _skillIndexValue.ToString()!);
            }

            if (maxValue < 0)
            {
                maxValue = _info.GetMaximumSkillLevel();
            }

            if (value.Value > maxValue)
            {
                if (skillInfo == null)
                    args.DiagnosticSink?.UNT1016(ref args, value.Value, maxValue);
                else
                    args.DiagnosticSink?.UNT1016(ref args, value.Value, skillInfo);
            }
        }

        return true;
    }

    private bool TryGetCurrentSkillReference(in FileEvaluationContext ctx, out SkillReference skillRef)
    {
        skillRef = SkillReference.None;

        if (_skillValue != null)
        {
            SkillReferenceVisitor v;
            v.Value = SkillReference.None;
            v.Information = ctx.Services.Database;
            v.Success = false;

            if (!_skillValue.VisitValue(ref v, in ctx) || !v.Success)
                return false;

            skillRef = v.Value;
            return true;
        }

        if (_specialityIndexValue != null && _skillIndexValue != null
            && _specialityIndexValue.TryGetValueAs(in ctx, out Optional<int> specialityIndex) && specialityIndex.HasValue
            && _skillIndexValue.TryGetValueAs(in ctx, out Optional<int> skillIndex) && skillIndex.HasValue)
        {
            skillRef = new SkillReference(specialityIndex.Value, skillIndex.Value);
            return true;
        }

        return false;
    }

    private struct SkillReferenceVisitor : IValueVisitor
    {
        public SkillReference Value;
        public IAssetSpecDatabase Information;
        public bool Success;
        public void Accept<TValue>(Optional<TValue> value) where TValue : IEquatable<TValue>
        {
            if (!value.HasValue)
                return;
            
            if (typeof(TValue) == typeof(SkillReference))
            {
                Value = Unsafe.As<TValue, SkillReference>(ref Unsafe.AsRef(in value.Value));
                Success = true;
            }
            else if (ConvertVisitor<string>.TryConvert(value.Value, out string? str) && !string.IsNullOrEmpty(str))
            {
                Success = SkillReference.TryParse(str, Information.Information, out Value)
                          || SkillReference.TryParseFromBlueprintSkill(str, Information, out Value);
            }
        }
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson<TDataRefReadContext>(
        in JsonElement json,
        out Optional<int> value,
        IType<int> valueType,
        ref TDataRefReadContext dataRefContext
    ) where TDataRefReadContext : IDataRefReadContext?
    {
        return TypeParsers.Int32.TryReadValueFromJson(in json, out value, valueType, ref dataRefContext);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, int value, IType<int> valueType, JsonSerializerOptions options)
    {
        TypeParsers.Int32.WriteValueToJson(writer, value, valueType, options);
    }

    /// <inheritdoc />
    protected override bool Equals(SkillLevelType other)
    {
        return _info == other._info
               && AllowNegatives == other.AllowNegatives
               && (_skillValue?.Equals(other._skillValue) ?? other._skillValue == null)
               && (_specialityIndexValue?.Equals(other._specialityIndexValue) ?? other._specialityIndexValue == null)
               && (_skillIndexValue?.Equals(other._skillIndexValue) ?? other._skillIndexValue == null);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(1537143670, AllowNegatives, _skillValue, _specialityIndexValue, _skillIndexValue);
    }
}