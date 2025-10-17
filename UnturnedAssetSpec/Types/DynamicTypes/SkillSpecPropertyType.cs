using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class SkillSpecPropertyType :
    BasicSpecPropertyType<SkillSpecPropertyType, string>,
    IStringParseableSpecPropertyType,
    IAutoCompleteSpecPropertyType
{
    private ISpecType? _blueprintSkillType;
    private IAssetSpecDatabase? _cacheDb;

    private const string BlueprintSkill = "SDG.Unturned.EBlueprintSkill, Assembly-CSharp";

    public bool AllowBlueprintSkills { get; }
    public bool AllowStandardSkills { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type { get; }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName { get; }

    protected override ISpecDynamicValue CreateValue(string? value) => new SpecDynamicConcreteConvertibleValue<string>(value, this);

    public SkillSpecPropertyType(bool allowStandardSkills = true, bool allowBlueprintSkills = false)
    {
        AllowStandardSkills = allowStandardSkills;
        AllowBlueprintSkills = allowBlueprintSkills;
        if (allowBlueprintSkills)
        {
            if (allowStandardSkills)
            {
                Type = "BlueprintSkill";
                DisplayName = "Skill (or Blueprint Skill)";
            }
            else
            {
                Type = "SDG.Unturned.EBlueprintSkill, Assembly-CSharp";
                DisplayName = "Blueprint Skill";
            }
        }
        else
        {
            Type = "Skill";
            DisplayName = "Skill";
        }
    }

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Enum;

    public string? ToString(ISpecDynamicValue value)
    {
        return value.AsConcrete<string>();
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> span, string? stringValue, out ISpecDynamicValue dynamicValue)
    {
        if (span.IsEmpty)
        {
            dynamicValue = null!;
            return false;
        }

        stringValue ??= span.ToString();
        dynamicValue = SpecDynamicValue.String(stringValue, this);
        return false;
    }

    /// <inheritdoc />
    public override bool TryParseValue(in SpecPropertyTypeParseContext parse, out string? value)
    {
        if (parse.Node == null)
            return MissingNode(in parse, out value);

        if (parse.Node is not IValueSourceNode strValNode)
            return FailedToParse(in parse, out value);

        string skill = strValNode.Value;
        if (AllowBlueprintSkills)
        {
            ISpecType? blueprintSkillType = null;
            lock (this)
            {
                if (_cacheDb == parse.Database)
                {
                    blueprintSkillType = _blueprintSkillType;
                }

                if (blueprintSkillType == null)
                {
                    _blueprintSkillType = blueprintSkillType = parse.Database.FindType(BlueprintSkill, AssetFileType.AssetBaseType(parse.Database));
                    _cacheDb = parse.Database;
                }
            }

            if (blueprintSkillType is not EnumSpecType enumType)
            {
                if (parse.HasDiagnostics)
                {
                    parse.Log(new DatDiagnosticMessage
                    {
                        Diagnostic = DatDiagnostics.UNT2005,
                        Message = string.Format(DiagnosticResources.UNT2005, BlueprintSkill),
                        Range = parse.Node.Range
                    });
                }
            }
            else if (enumType.TryParse(strValNode.Value.AsSpan(), out EnumSpecTypeValue enumValue) && enumValue.AdditionalProperties.TryGetValue("Skill", out object? val) && val is string str)
            {
                if (!AllowStandardSkills)
                {
                    value = str;
                    return true;
                }

                skill = str;
            }
            else if (!AllowStandardSkills)
            {
                value = null;
                return false;
            }
        }

        value = skill;

        if (!parse.HasDiagnostics || string.Equals(value, "NONE", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        SkillInfo? skillInfo = parse.Database.Information.Specialities
            ?.SelectMany(x => x?.Skills ?? Array.Empty<SkillInfo>())
            ?.FirstOrDefault(x => string.Equals(x.Skill, skill));

        if (skillInfo == null)
        {
            parse.Log(new DatDiagnosticMessage
            {
                Diagnostic = DatDiagnostics.UNT1014,
                Message = string.Format(DiagnosticResources.UNT1014, skill),
                Range = parse.Node.Range
            });
        }

        return true;
    }

    public Task<AutoCompleteResult[]> GetAutoCompleteResults(in AutoCompleteParameters parameters, in FileEvaluationContext context)
    {
        SpecialityInfo?[]? specialities = context.Information.Information.Specialities;
        if (specialities == null)
        {
            return Task.FromResult(Array.Empty<AutoCompleteResult>());
        }

        return Task.FromResult(
            specialities
                .Where(x => x != null)
                .SelectMany(x => x!.Skills)
                .Select(x => new AutoCompleteResult(x.Skill, x.Description))
                .ToArray()
        );
    }
}