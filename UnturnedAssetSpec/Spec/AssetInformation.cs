using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Contains generic information about asset types.
/// </summary>
public class AssetInformation
{
    /// <summary>
    /// The GitHub commit (SHA) where this information was taken from, if any. Note that some information may have been pulled from other places.
    /// </summary>
    public string? Commit { get; set; }

#nullable disable
    public Dictionary<string, QualifiedType> AssetAliases { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<string>))]
    public Dictionary<QualifiedType, string> AssetCategories { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<TypeHierarchy>))]
    public Dictionary<QualifiedType, TypeHierarchy> Types { get; set; }

    public Dictionary<string, QualifiedType> KnownFileNames { get; set; }

    [JsonIgnore] // generated at runtime from Types
    [field: MaybeNull]
    public Dictionary<QualifiedType, InverseTypeHierarchy> ParentTypes => field ??= RegenParentTypes();

    public int FaceCount { get; set; } = 32;
    public int BeardCount { get; set; } = 16;
    public int HairCount { get; set; } = 23;


#nullable restore

    public AssetBundleVersionInfo?[]? AssetBundleVersions { get; set; }
    public SkillsetInfo?[]? Skillsets { get; set; }
    public SpecialityInfo?[]? Specialities { get; set; }
    public Color32[]? SkinColors { get; set; }
    public int[]? EmissiveFaces { get; set; }
    public string? FaceTextureTemplate { get; set; }
    public string? EmissiveFaceTextureTemplate { get; set; }
    public string? BeardTextureTemplate { get; set; }
    public string? HairTextureTemplate { get; set; }
    public string? StatusJsonFallbackUrl { get; set; }
    public string? PlayerDashboardInventoryLocalizationFallbackUrl { get; set; }
    public string? SkillsLocalizationFallbackUrl { get; set; }
    public string? SkillsetsLocalizationFallbackUrl { get; set; }

    public bool TryGetAssetBundleVersionInfo(int assetBundleVersion, out UnityEngineVersion version, out string displayName)
    {
        version = default;
        displayName = null!;
        if (AssetBundleVersions == null || assetBundleVersion < 0 || assetBundleVersion >= AssetBundleVersions.Length)
        {
            return false;
        }

        AssetBundleVersionInfo? info = AssetBundleVersions[assetBundleVersion];
        if (info?.DisplayName == null || info.EndVersion.Status == null)
        {
            return false;
        }

        version = info.EndVersion;
        displayName = info.DisplayName;
        return true;
    }

    public bool TryGetFaceUrl(int face, out string url)
    {
        if (face < 0 || face >= FaceCount)
        {
            url = null!;
            return false;
        }

        if (EmissiveFaces != null && Array.IndexOf(EmissiveFaces, face) >= 0)
        {
            if (EmissiveFaceTextureTemplate == null)
            {
                url = null!;
                return false;
            }

            url = string.Format(EmissiveFaceTextureTemplate, face.ToString(CultureInfo.InvariantCulture));
            return true;
        }

        if (FaceTextureTemplate == null)
        {
            url = null!;
            return false;
        }

        url = string.Format(FaceTextureTemplate, face.ToString(CultureInfo.InvariantCulture));
        return true;
    }

    public bool TryGetBeardUrl(int beard, out string url)
    {
        if (beard < 0 || beard >= BeardCount)
        {
            url = null!;
            return false;
        }

        if (BeardTextureTemplate == null)
        {
            url = null!;
            return false;
        }

        url = string.Format(BeardTextureTemplate, beard.ToString(CultureInfo.InvariantCulture));
        return true;
    }

    public bool TryGetHairUrl(int hair, out string url)
    {
        if (hair < 0 || hair >= HairCount)
        {
            url = null!;
            return false;
        }

        if (HairTextureTemplate == null)
        {
            url = null!;
            return false;
        }

        url = string.Format(HairTextureTemplate, hair.ToString(CultureInfo.InvariantCulture));
        return true;
    }

    public bool IsAssignableTo(QualifiedType type, QualifiedType to)
    {
        return type.Equals(to) || Array.IndexOf(GetParentTypes(type).ParentTypes, to) != -1;
    }

    public bool IsAssignableFrom(QualifiedType type, QualifiedType from)
    {
        return IsAssignableTo(from, type);
    }

    public bool IsAbstract(QualifiedType type)
    {
        return GetParentTypes(type).IsAbstract;
    }

    public TypeHierarchy GetAssetHierarchy()
    {
        return GetHierarchy(TypeHierarchy.AssetBaseType);
    }

    public TypeHierarchy GetUseableHierarchy()
    {
        return GetHierarchy(TypeHierarchy.UseableBaseType);
    }

    public TypeHierarchy GetHierarchy(QualifiedType baseType)
    {
        if (baseType.IsCaseInsensitive)
            baseType = baseType.CaseSensitive;

        if (Types == null || !Types.TryGetValue(baseType, out TypeHierarchy? hierarchy))
            return new TypeHierarchy();

        return hierarchy;
    }

    public InverseTypeHierarchy GetParentTypes(QualifiedType type)
    {
        if (type.IsCaseInsensitive)
            type = type.CaseSensitive;

        return ParentTypes.TryGetValue(type, out InverseTypeHierarchy? typeHierarchy)
            ? typeHierarchy
            : new InverseTypeHierarchy(new TypeHierarchy { Type = type }, Array.Empty<QualifiedType>(), false);
    }

    private Dictionary<QualifiedType, InverseTypeHierarchy> RegenParentTypes()
    {
        Stack<QualifiedType> typeStack = new Stack<QualifiedType>();
        Dictionary<QualifiedType, InverseTypeHierarchy> parentTypes = new Dictionary<QualifiedType, InverseTypeHierarchy>(96);
        QualifiedType[]? arrNull = null;
        foreach (KeyValuePair<QualifiedType, TypeHierarchy> h in Types)
        {
            h.Value.Type = h.Key;
            RegenParentTypesRecursive(h.Value, typeStack, ref arrNull, parentTypes);
        }

        return parentTypes;
    }

    private static void RegenParentTypesRecursive(
        TypeHierarchy hierarchy,
        Stack<QualifiedType> typeStack,
        ref QualifiedType[]? arr,
        Dictionary<QualifiedType, InverseTypeHierarchy> parentTypes)
    {
        if (hierarchy.Type.IsNull)
            return;

        arr ??= typeStack.ToArray();
        parentTypes[hierarchy.Type] = new InverseTypeHierarchy(hierarchy, arr, true);
        if (hierarchy.ChildTypes is not { Count: > 0 })
            return;

        typeStack.Push(hierarchy.Type);
        QualifiedType[]? arrNull = null;
        foreach (TypeHierarchy h in hierarchy.ChildTypes.Values)
        {
            h.HasDataFiles |= hierarchy.HasDataFiles;
            h.Parent = hierarchy;
            RegenParentTypesRecursive(h, typeStack, ref arrNull, parentTypes);
        }

        typeStack.Pop();
    }

    [DebuggerDisplay("{DisplayName} ({EndVersion,nq})")]
    public class AssetBundleVersionInfo
    {
        public required UnityEngineVersion EndVersion { get; set; }
        public required string DisplayName { get; set; }
    }

    /// <summary>
    /// Gets the index of the first asset category in the list.
    /// </summary>
    /// <returns>
    /// -1 if more than one asset category could be searched, 0 if no asset categories could be searched,
    /// otherwise the index of the category in <see cref="AssetCategory.TypeOf"/>'s values.
    /// </returns>
    public int GetAssetCategory(OneOrMore<QualifiedType> elementTypes)
    {
        return GetAssetCategory(QualifiedType.None, elementTypes);
    }

    /// <summary>
    /// Gets the index of the first asset category in the list.
    /// </summary>
    /// <returns>
    /// -1 if more than one asset category could be searched, 0 if no asset categories could be searched,
    /// otherwise the index of the category in <see cref="AssetCategory.TypeOf"/>'s values.
    /// </returns>
    public int GetAssetCategory(QualifiedType elementType, OneOrMore<QualifiedType> otherElementTypes)
    {
        int category;
        if (elementType.Type != null)
        {
            int c = AssetCategory.GetCategoryFromType(elementType, this);
            if (c == -1)
                return -1;

            category = c;
        }
        else
        {
            category = 0;
        }

        foreach (QualifiedType qt in otherElementTypes)
        {
            int c = AssetCategory.GetCategoryFromType(qt, this);
            switch (c)
            {
                case -1: return -1;
                case 0: continue;
            }

            if (category == 0)
            {
                category = c;
            }
            else if (category != c)
            {
                return -1;
            }
        }

        return 0;
    }
}

[JsonConverter(typeof(TypeHierarchyConverter))]
[DebuggerDisplay("{Type.GetTypeName()}")]
public class TypeHierarchy
{
    public const string AssetBaseType = "SDG.Unturned.Asset, Assembly-CSharp";
    internal const string ItemAssetType = "SDG.Unturned.ItemAsset, Assembly-CSharp";

    public const string UseableBaseType = "SDG.Unturned.Useable, Assembly-CSharp";

    public bool HasDataFiles { get; set; }
    public bool IsAbstract { get; set; }
    public QualifiedType Type { get; set; }
    public TypeHierarchy? Parent { get; set; }
#nullable disable
    public Dictionary<QualifiedType, TypeHierarchy> ChildTypes { get; set; }
#nullable restore
}

[DebuggerDisplay("{Type.GetTypeName()}")]
public class InverseTypeHierarchy
{
    public TypeHierarchy Hierarchy { get; }
    public QualifiedType Type { get; }

    // [0] = direct parent, [^1] = lowest
    public QualifiedType[] ParentTypes { get; }
    public bool IsValid { get; }
    public bool IsAbstract { get; }
    public InverseTypeHierarchy(TypeHierarchy hierarchy, QualifiedType[] parentTypes, bool isValid)
    {
        Hierarchy = hierarchy;
        Type = hierarchy.Type;
        IsAbstract = hierarchy.IsAbstract;
        ParentTypes = parentTypes;
        IsValid = isValid;
    }
}

[DebuggerDisplay("{Skillset}")]
public class SkillsetInfo
{
    public required string Skillset { get; set; }
    public required int Index { get; set; }
    public required SkillsetSkillInfo[] Skills { get; set; }

    // [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DisplayName { get; set; }
}

[DebuggerDisplay("{Speciality}:{Skill}")]
public struct SkillsetSkillInfo
{
    public int Speciality { get; set; }
    public int Skill { get; set; }

    public SkillsetSkillInfo() { }
    public SkillsetSkillInfo(int speciality, int skill)
    {
        Speciality = speciality;
        Skill = skill;
    }

    public override string ToString() => $"{Speciality.ToString(CultureInfo.InvariantCulture)}:{Skill.ToString(CultureInfo.InvariantCulture)}";
}

[DebuggerDisplay("{Speciality}")]
public class SpecialityInfo
{
    public required string Speciality { get; set; }
    public required int Index { get; set; }
    public required SkillInfo[] Skills { get; set; }
    public string? DisplayName { get; set; }
}

[DebuggerDisplay("{Skill}")]
public class SkillInfo
{
    public required string Skill { get; set; }
    public required int Index { get; set; }
    public required uint Cost { get; set; }
    public required float Difficulty { get; set; }
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public string?[]? Levels { get; set; }
    public int MaximumLevel { get; set; }
    public double? LevelMultiplier { get; set; }
    public bool LevelMultiplierInverse { get; set; }

    public string? GetLevelDescription(int i)
    {
        if (i <= 0 || i > MaximumLevel)
            return null;

        double normalizedLevel = i >= MaximumLevel ? 1f : i / (float)MaximumLevel;

        if (LevelMultiplier.HasValue)
        {
            double value = normalizedLevel * i;
            if (LevelMultiplierInverse)
                value = 1 - value;

            string valueStr = value.ToString(CultureInfo.InvariantCulture);

            return Levels is not { Length: 1 } || Levels[0] == null ? valueStr : string.Format(Levels[0]!, valueStr);
        }

        --i;
        return Levels != null && i < Levels.Length ? Levels[i] : null;
    }
}