using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// The skill level of another property's skill.
/// <para>Example: <c>ItemAsset.Blueprint.Skill_Level</c></para>
/// <code>
/// Skill Sharpshooter
/// Prop 6
/// </code>
/// </summary>
public sealed class SkillLevelSpecPropertyType :
    BaseSpecPropertyType<SkillLevelSpecPropertyType, byte>,
    IStringParseableSpecPropertyType,
    IElementTypeSpecPropertyType,
    IAutoCompleteSpecPropertyType
{
    private readonly IAssetSpecDatabase _database;
    private PropertyRef? _skillValue;
    private PropertyRef? _specialityIndexValue;
    private PropertyRef? _skillIndexValue;
    private EnumSpecType? _blueprintSkillEnumType;

    public string SkillsetOrProperty { get; }

    protected override ISpecDynamicValue CreateValue(byte value) => new SpecDynamicConcreteConvertibleValue<byte>(value, this);

    public override int GetHashCode()
    {
        return 77 ^ (SkillsetOrProperty?.GetHashCode() ?? 0);
    }

    public SkillLevelSpecPropertyType(IAssetSpecDatabase database, string skillsetOrProperty)
    {
        _database = database.ResolveFacade();
        SkillsetOrProperty = skillsetOrProperty;
    }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "SkillLevel";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName => "Skill Level";

    string IElementTypeSpecPropertyType.ElementType => SkillsetOrProperty;

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcreteNullable<byte>()?.ToString();
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (byte.TryParse(stringValue ?? span.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out byte result))
        {
            dynamicValue = SpecDynamicValue.UInt8(result, this);
            return true;
        }

        dynamicValue = null!;
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out byte value)
    {
        if (parse.Node == null)
            return MissingNode(in parse, out value);

        if (parse.Node is not IValueSourceNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
            return FailedToParse(in parse, out value);

        if (!parse.HasDiagnostics)
            return true;

        if (!TryGetSkill(in parse.EvaluationContext, out SkillInfo? skill))
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1014,
                Message = string.Format(DiagnosticResources.UNT1014, strValNode.Value),
                Range = parse.Node.Range
            };

            parse.Log(message);
            return false;
        }

        if (value == 0 || value > skill!.MaximumLevel)
        {
            DatDiagnosticMessage message = new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1016,
                Message = string.Format(DiagnosticResources.UNT1016, value, skill!.DisplayName ?? skill.Skill),
                Range = parse.Node.Range
            };

            parse.Log(message);
        }

        return true;
    }

    private bool TryGetSkill(in FileEvaluationContext ctx, out SkillInfo? skill)
    {
        IAssetSpecDatabase db = ctx.Information;

        skill = db.FindSkillByName(SkillsetOrProperty);

        if (skill != null)
        {
            return true;
        }

        if (_skillValue == null && (_specialityIndexValue == null || _skillIndexValue == null))
        {
            int index = SkillsetOrProperty.IndexOf(':');

            if (index <= 0 || index >= SkillsetOrProperty.Length - 1)
            {
                _skillValue = new PropertyRef(SkillsetOrProperty.AsSpan(), SkillsetOrProperty);
            }
            else
            {
                ReadOnlySpan<char> prop1 = SkillsetOrProperty.AsSpan(0, index).Trim();
                ReadOnlySpan<char> prop2 = SkillsetOrProperty.AsSpan(index + 1).Trim();
                if (!prop1.IsEmpty && !prop2.IsEmpty)
                {
                    _specialityIndexValue = new PropertyRef(prop1, null);
                    _skillIndexValue = new PropertyRef(prop2, null);
                }
                else
                {
                    _specialityIndexValue = null;
                    _skillIndexValue = null;
                }
            }
        }

        if (_skillValue != null)
        {
            if (!_skillValue.TryEvaluateValue(in ctx, out string? value, out bool isNull) || value == null || isNull)
            {
                return false;
            }

            skill = db.FindSkillByName(value);

            if (skill != null)
                return true;

            SpecProperty? prop = _skillValue.ResolveProperty(in ctx);
            if (prop?.Type.GetType(in ctx) is not SkillSpecPropertyType { AllowBlueprintSkills: true }
                || _database.FindType(SkillSpecPropertyType.BlueprintSkillEnumType, ctx.FileType) is not EnumSpecType enumType)
            {
                return false;
            }

            _blueprintSkillEnumType = enumType;
            if (!enumType.TryParse(value, out int index)
                || !enumType.Values[index].TryGetAdditionalProperty("Skill", out string? skillName)
                || skillName == null)
            {
                return false;
            }

            skill = db.FindSkillByName(skillName);
            return skill != null;
        }

        if (_specialityIndexValue != null && _skillIndexValue != null)
        {
            if (!_specialityIndexValue.TryEvaluateValue(in ctx, out byte specialityIndex, out bool isNull) || isNull
                || !_specialityIndexValue.TryEvaluateValue(in ctx, out byte skillIndex, out isNull) || isNull)
            {
                return false;
            }

            SpecialityInfo? spec = db.Information.Specialities == null
                ? null
                : Array.Find(db.Information.Specialities, x => x != null && x.Index == specialityIndex);
            skill = spec?.Skills == null
                ? null
                : Array.Find(spec.Skills, x => x != null && x.Index == skillIndex);

            return skill != null;
        }

        return false;
    }

    private static readonly AutoCompleteResult[] Zero = [ new AutoCompleteResult("0", "No skill.") ];

    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context)
    {
        if (!TryGetSkill(in context, out SkillInfo? skill) || skill == null || skill.MaximumLevel == 0)
        {
            return Task.FromResult(Zero);
        }

        AutoCompleteResult[] results = new AutoCompleteResult[skill.MaximumLevel + 1];
        for (int i = 0; i < results.Length; ++i)
        {
            ref AutoCompleteResult result = ref results[i];
            string? description;
            if (i == 0)
                description = "Not required.";
            else
                description = skill.GetLevelDescription(i) ?? $"Level {i}";

            result = new AutoCompleteResult(i.ToString(CultureInfo.InvariantCulture), description);
        }

        return Task.FromResult(results);
    }
}