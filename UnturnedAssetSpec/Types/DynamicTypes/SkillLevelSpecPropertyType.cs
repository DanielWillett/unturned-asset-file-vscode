using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public sealed class SkillLevelSpecPropertyType :
    BasicSpecPropertyType<SkillLevelSpecPropertyType, byte>,
    IStringParseableSpecPropertyType,
    IElementTypeSpecPropertyType,
    IAutoCompleteSpecPropertyType
{
    private ISpecDynamicValue? _skillValue;
    private ISpecDynamicValue? _specialityIndexValue;
    private ISpecDynamicValue? _skillIndexValue;
    private IAssetSpecDatabase? _cacheDb;

    public string SkillsetOrProperty { get; }

    public SkillLevelSpecPropertyType(string skillsetOrProperty)
    {
        SkillsetOrProperty = skillsetOrProperty;
    }

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string Type => "SkillLevel";

    /// <inheritdoc />
    public override SpecPropertyTypeKind Kind => SpecPropertyTypeKind.Number;

    /// <inheritdoc cref="ISpecPropertyType" />
    public override string DisplayName => "Skill Level";

    string IElementTypeSpecPropertyType.ElementType => SkillsetOrProperty;

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

        if (parse.Node is not AssetFileStringValueNode strValNode || !KnownTypeValueHelper.TryParseUInt8(strValNode.Value, out value))
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

        ISpecDynamicValue? skillValue;
        ISpecDynamicValue? specialityIndexValue;
        ISpecDynamicValue? skillIndexValue;
        lock (this)
        {
            if (_cacheDb == null || _cacheDb != db)
            {
                _skillIndexValue = null;
                _specialityIndexValue = null;
                _skillValue = null;
                skillValue = null;
                specialityIndexValue = null;
                skillIndexValue = null;
            }
            else
            {
                skillValue = _skillValue;
                specialityIndexValue = _specialityIndexValue;
                skillIndexValue = _skillIndexValue;
            }
        }

        skill = db.Information.Specialities
            ?.SelectMany(x => x?.Skills ?? Array.Empty<SkillInfo>())
            ?.FirstOrDefault(x => string.Equals(x.Skill, SkillsetOrProperty));

        if (skill != null)
        {
            return true;
        }

        if (skillValue == null && (specialityIndexValue == null || skillIndexValue == null))
        {
            int index = SkillsetOrProperty.IndexOf(':');

            lock (this)
            {
                if (index <= 0 || index >= SkillsetOrProperty.Length - 1)
                {
                    _skillValue = skillValue = new PropertyRef(SkillsetOrProperty.AsSpan(), SkillsetOrProperty);
                }
                else
                {
                    ReadOnlySpan<char> prop1 = SkillsetOrProperty.AsSpan(0, index).Trim();
                    ReadOnlySpan<char> prop2 = SkillsetOrProperty.AsSpan(index + 1).Trim();
                    if (!prop1.IsEmpty && !prop2.IsEmpty)
                    {
                        _specialityIndexValue = specialityIndexValue = new PropertyRef(prop1, null);
                        _skillIndexValue = skillIndexValue = new PropertyRef(prop2, null);
                    }
                    else
                    {
                        _specialityIndexValue = specialityIndexValue = null;
                        _skillIndexValue = skillIndexValue = null;
                    }
                }
                _cacheDb = db;
            }
        }

        if (skillValue != null)
        {
            if (!skillValue.TryEvaluateValue(in ctx, out string? value, out bool isNull) || value == null || isNull)
            {
                return false;
            }

            skill = db.Information.Specialities
                ?.SelectMany(x => x?.Skills ?? Array.Empty<SkillInfo>())
                ?.FirstOrDefault(x => string.Equals(x.Skill, value));
            return skill != null;
        }

        if (specialityIndexValue != null && skillIndexValue != null)
        {
            if (!specialityIndexValue.TryEvaluateValue(in ctx, out byte specialityIndex, out bool isNull) || isNull
                || !specialityIndexValue.TryEvaluateValue(in ctx, out byte skillIndex, out isNull) || isNull)
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