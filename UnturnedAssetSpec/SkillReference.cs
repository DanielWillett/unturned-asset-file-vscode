using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data;

/// <summary>
/// A pair of speciality and skill index.
/// </summary>
public readonly struct SkillReference : IEquatable<SkillReference>, IComparable<SkillReference>
{
    /// <summary>
    /// A nil skill reference.
    /// </summary>
    public static readonly SkillReference None = new SkillReference(-1, -1);

    /// <summary>
    /// Index of the speciality this skill resides in.
    /// </summary>
    public int SpecialityIndex { get; }

    /// <summary>
    /// Index of this skill within it's speciality.
    /// </summary>
    public int SkillIndex { get; }

    /// <summary>
    /// Whether or not this value references a null skill (<see cref="None"/>). This property doesn't check that this value references a valid skill, just that one of its indices are negative.
    /// </summary>
    public bool IsNull => SpecialityIndex < 0 || SkillIndex < 0;

    /// <summary>
    /// Create a new <see cref="SkillReference"/> given a speciality and skill.
    /// </summary>
    /// <param name="specialityIndex">The index of the skill's speciality.</param>
    /// <param name="skillIndex">The index of the skill within it's speciality.</param>
    public SkillReference(int specialityIndex, int skillIndex)
    {
        SpecialityIndex = specialityIndex;
        SkillIndex = skillIndex;
    }

    /// <summary>
    /// Attempts to get the skill referenced by this <see cref="SkillReference"/> from the given <paramref name="information"/>.
    /// </summary>
    /// <param name="information">Dat specification information.</param>
    /// <param name="skillInfo">Dat specification information about this skill.</param>
    public bool TryGetSkillInfo(AssetInformation information, [NotNullWhen(true)] out SkillInfo? skillInfo)
    {
        return TryGetSkillInfo(information, out skillInfo, out _);
    }

    /// <summary>
    /// Attempts to get the skill referenced by this <see cref="SkillReference"/> from the given <paramref name="information"/>.
    /// </summary>
    /// <param name="information">Dat specification information.</param>
    /// <param name="skillInfo">Dat specification information about this skill.</param>
    /// <param name="specialityInfo">Dat specification information about this skill's speciality.</param>
    public bool TryGetSkillInfo(AssetInformation information, [NotNullWhen(true)] out SkillInfo? skillInfo, [NotNullWhen(true)] out SpecialityInfo? specialityInfo)
    {
        if (SpecialityIndex < 0 || SkillIndex < 0)
        {
            skillInfo = null;
            specialityInfo = null;
            return false;
        }

        SpecialityInfo?[]? specInfo = information.Specialities;
        if (specInfo == null || specInfo.Length <= SpecialityIndex)
        {
            skillInfo = null;
            specialityInfo = null;
            return false;
        }

        SpecialityInfo? speciality = specInfo[SpecialityIndex];
        specialityInfo = speciality;
        if (speciality?.Skills == null || speciality.Skills.Length <= SkillIndex)
        {
            skillInfo = null;
            return false;
        }

        skillInfo = speciality.Skills[SkillIndex];
        return skillInfo != null;
    }

    /// <summary>
    /// Attempts to parse any one of the 3 categories of skills into speciality and skill indices.
    /// </summary>
    /// <param name="str">The string to parse.</param>
    /// <param name="information">Dat specification information.</param>
    /// <param name="skill">The parsed skill.</param>
    /// <param name="ignoreCase">Whether or not character casing should be ignored.</param>
    /// <returns>Whether or not a valid skill could be found.</returns>
    public static bool TryParse(string str, AssetInformation information, out SkillReference skill, bool ignoreCase = true)
    {
        if (information.SkillCache != null)
        {
            return information.SkillCache.TryGetValue(str, out skill);
        }

        skill = None;
        str = str.Trim();
        SpecialityInfo?[]? specialities = information.Specialities;
        if (string.IsNullOrEmpty(str) || specialities == null || specialities.Length == 0)
            return false;

        ImmutableDictionary<string, SkillReference>.Builder bldr =
            ImmutableDictionary.CreateBuilder<string, SkillReference>(StringComparer.OrdinalIgnoreCase);

        StringComparison comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        bool found = false;
        for (int specialityIndex = 0; specialityIndex < specialities.Length; ++specialityIndex)
        {
            SpecialityInfo? specialityInfo = specialities[specialityIndex];
            SkillInfo?[]? skills = specialityInfo?.Skills;
            if (skills == null)
                continue;

            for (int skillIndex = 0; skillIndex < skills.Length; ++skillIndex)
            {
                SkillInfo? skillInfo = skills[skillIndex];
                if (skillInfo == null)
                    continue;

                if (!found && str.Equals(skillInfo.Skill, comparison))
                {
                    skill = new SkillReference(specialityIndex, skillIndex);
                    found = true;
                }

                bldr[skillInfo.Skill] = new SkillReference(specialityIndex, skillIndex);
            }
        }

        information.SkillCache = bldr.ToImmutable();
        return found;
    }

    /// <summary>
    /// Creates a <see cref="SkillReference"/> from a legacy EBlueprintSkill value.
    /// </summary>
    public static bool TryParseFromBlueprintSkill(string enumValue, IAssetSpecDatabase database, out SkillReference skill)
    {
        IDictionary<string, SkillReference>? bpSkills = database.BlueprintSkills;
        if (bpSkills != null && bpSkills.TryGetValue(enumValue, out skill))
            return true;

        skill = None;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(SkillReference other)
    {
        if (other.SpecialityIndex < 0 || other.SkillIndex < 0)
            return SpecialityIndex < 0 || SkillIndex < 0;
        
        if (SpecialityIndex < 0 || SkillIndex < 0)
            return false;

        return other.SpecialityIndex == SpecialityIndex && other.SkillIndex == SkillIndex;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"({SpecialityIndex}:{SkillIndex})";
    }

    /// <inheritdoc />
    public int CompareTo(SkillReference other)
    {
        if (other.SpecialityIndex < 0 || other.SkillIndex < 0)
            return SpecialityIndex < 0 || SkillIndex < 0 ? 0 : 1;

        if (SpecialityIndex < 0 || SkillIndex < 0)
            return -1;

        return SpecialityIndex == other.SpecialityIndex ? SkillIndex.CompareTo(other.SkillIndex) : SpecialityIndex.CompareTo(other.SpecialityIndex);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is SkillReference p && Equals(p);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (SpecialityIndex < 0 || SkillIndex < 0)
            return -1;

        return SpecialityIndex * 397 + SkillIndex;
    }
}