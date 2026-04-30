using AssetsTools.NET.Extra;
using DanielWillett.UnturnedDataFileLspServer.Data.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    private int? _maxSkillLevelCached;

    internal ImmutableDictionary<string, SkillReference>? SkillCache;

#nullable disable
    public Dictionary<string, QualifiedType> AssetAliases { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<string>))]
    public Dictionary<QualifiedType, string> AssetCategories { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<TypeHierarchy>))]
    public Dictionary<QualifiedType, TypeHierarchy> Types { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<string[]>))]
    public Dictionary<QualifiedType, string[]> BundleValidFileExtensions { get; set; }

    public Dictionary<string, QualifiedType> KnownFileNames { get; set; }

    public Dictionary<string, QualifiedType> KnownUnityClassTypes { get; set; }

    [JsonIgnore] // generated at runtime from Types
    [field: MaybeNull]
    public Dictionary<QualifiedType, InverseTypeHierarchy> ParentTypes => field ??= RegenParentTypes();

    public int FaceCount { get; set; } = 32;
    public int BeardCount { get; set; } = 16;
    public int HairCount { get; set; } = 23;


#nullable restore

    /// <summary>
    /// List of files in the <c>Localization/English</c> directory that are parsed using the legacy (line-by-line) format.
    /// </summary>
    public string?[]? LegacyParsedLocalizationFiles { get; set; }

    /// <summary>
    /// List of files in the <c>Localization/English</c> directory that do not allow values.
    /// </summary>
    public string?[]? KeyOnlyLocalizationFiles { get; set; }
    public AssetBundleVersionInfo?[]? AssetBundleVersions { get; set; }
    public SkillsetInfo?[]? Skillsets { get; set; }

    public string?[]? RelevantBundleAssetClasses { get; set; }

    public SpecialityInfo?[]? Specialities
    {
        get;
        set
        {
            field = value;
            _maxSkillLevelCached = null;
        }
    }

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

    /// <summary>
    /// Calculates and caches the maximum level of all known skills.
    /// </summary>
    /// <returns>The highest reachable level of at least one skill under normal circumstances.</returns>
    public int GetMaximumSkillLevel()
    {
        int? maxSkillLevel = _maxSkillLevelCached;
        if (maxSkillLevel.HasValue)
            return maxSkillLevel.Value;

        int max = GetMaxSkillLevelIntl(Specialities);
        _maxSkillLevelCached = max;
        return max;
    }

    private static int GetMaxSkillLevelIntl(SpecialityInfo?[]? specialities)
    {
        if (specialities == null)
            return 0;

        int max = 0;
        for (int spec = 0; spec < specialities.Length; ++spec)
        {
            SpecialityInfo? specInfo = specialities[spec];
            SkillInfo[]? skills = specInfo?.Skills;
            if (skills == null)
                continue;

            for (int skill = 0; skill < skills.Length; ++skill)
            {
                SkillInfo skillInfo = skills[skill];
                if (skillInfo == null || skillInfo.MaximumLevel < max)
                    continue;

                max = skillInfo.MaximumLevel;
            }
        }

        return max;
    }

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
        if (!baseType.IsCaseInsensitive)
            baseType = baseType.CaseInsensitive;

        if (Types == null || !Types.TryGetValue(baseType, out TypeHierarchy? hierarchy))
            return new TypeHierarchy();

        return hierarchy;
    }

    public bool TryGetHierarchy(QualifiedType baseType, [NotNullWhen(true)] out TypeHierarchy? hierarchy)
    {
        if (!baseType.IsCaseInsensitive)
            baseType = baseType.CaseInsensitive;

        if (Types != null && Types.TryGetValue(baseType, out hierarchy))
            return true;

        hierarchy = null;
        return false;
    }

    public InverseTypeHierarchy GetParentTypes(QualifiedType type)
    {
        if (!type.IsCaseInsensitive)
            type = type.CaseInsensitive;

        return ParentTypes.TryGetValue(type, out InverseTypeHierarchy typeHierarchy)
            ? typeHierarchy
            : new InverseTypeHierarchy(type);
    }

    /// <remarks>Use <see cref="EnumerateTypeHierarchyDescending"/> to go from requested type to base type.</remarks>
    /// <inheritdoc cref="TypeHierarchyAscendingEnumerator"/>
    public TypeHierarchyAscendingEnumerator EnumerateTypeHierarchyAscending(QualifiedType type)
    {
        if (!type.IsCaseInsensitive)
            type = type.CaseInsensitive;

        if (!ParentTypes.TryGetValue(type, out InverseTypeHierarchy typeHierarchy))
        {
            return new TypeHierarchyAscendingEnumerator(type, Array.Empty<QualifiedType>());
        }

        return new TypeHierarchyAscendingEnumerator(type, typeHierarchy.ParentTypes);
    }

    /// <remarks>Use <see cref="EnumerateTypeHierarchyAscending"/> to go from requested type to base type.</remarks>
    /// <inheritdoc cref="TypeHierarchyDescendingEnumerator"/>
    public TypeHierarchyDescendingEnumerator EnumerateTypeHierarchyDescending(QualifiedType type)
    {
        if (!type.IsCaseInsensitive)
            type = type.CaseInsensitive;

        if (!ParentTypes.TryGetValue(type, out InverseTypeHierarchy typeHierarchy))
        {
            return new TypeHierarchyDescendingEnumerator(type, Array.Empty<QualifiedType>());
        }

        return new TypeHierarchyDescendingEnumerator(type, typeHierarchy.ParentTypes);
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
        parentTypes[hierarchy.Type] = new InverseTypeHierarchy(hierarchy, arr);
        if (hierarchy.ChildTypes is not { Count: > 0 })
            return;

        typeStack.Push(hierarchy.Type);
        QualifiedType[]? arrNull = null;
        foreach (KeyValuePair<QualifiedType, TypeHierarchy> h in hierarchy.ChildTypes)
        {
            h.Value.HasDataFiles |= hierarchy.HasDataFiles;
            h.Value.Parent = hierarchy;
            RegenParentTypesRecursive(h.Value, typeStack, ref arrNull, parentTypes);
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
    /// otherwise the index of the category in <see cref="AssetCategory.Instance"/>'s values.
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
    /// otherwise the index of the category in <see cref="AssetCategory.Instance"/>'s values.
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
    public const string ObjectType = "System.Object, mscorlib";
    internal const string ItemAssetType = "SDG.Unturned.ItemAsset, Assembly-CSharp";

    public const string UseableBaseType = "SDG.Unturned.Useable, Assembly-CSharp";

    public bool HasDataFiles { get; set; }
    public bool IsAbstract { get; set; }
    public QualifiedType Type { get; set; }
    public TypeHierarchy? Parent { get; set; }
#nullable disable
    public ImmutableDictionary<QualifiedType, TypeHierarchy> ChildTypes { get; set; }
#nullable restore
}

[DebuggerDisplay("{Type.GetTypeName()}")]
public readonly struct InverseTypeHierarchy
{
    public TypeHierarchy? Hierarchy { get; }
    public QualifiedType Type { get; }

    // [0] = direct parent, [^1] = lowest
    public QualifiedType[] ParentTypes { get; }
    public bool IsValid => Hierarchy != null;
    public bool IsAbstract => Hierarchy is { IsAbstract: true };

    public InverseTypeHierarchy(TypeHierarchy hierarchy, QualifiedType[] parentTypes)
    {
        Hierarchy = hierarchy;
        Type = hierarchy.Type;
        ParentTypes = parentTypes;
    }

    public InverseTypeHierarchy(QualifiedType type)
    {
        Type = type;
        ParentTypes = Array.Empty<QualifiedType>();
    }

    /// <remarks>Use <see cref="EnumerateDescending"/> to go from requested type to base type.</remarks>
    /// <inheritdoc cref="TypeHierarchyAscendingEnumerator"/>
    public TypeHierarchyAscendingEnumerator EnumerateAscending()
    {
        return new TypeHierarchyAscendingEnumerator(Type, ParentTypes);
    }

    /// <remarks>Use <see cref="EnumerateAscending"/> to go from requested type to base type.</remarks>
    /// <inheritdoc cref="TypeHierarchyDescendingEnumerator"/>
    public TypeHierarchyDescendingEnumerator EnumerateDescending()
    {
        return new TypeHierarchyDescendingEnumerator(Type, ParentTypes);
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

/// <summary>
/// Enumerates a type's hierarchy from base type to requested type.
/// Includes the requested type but not base types like <see cref="object"/>, <see cref="ValueType"/>, or <see cref="Enum"/>.
/// </summary>
/// <remarks>Use <see cref="TypeHierarchyDescendingEnumerator"/> to go from requested type to base type.</remarks>
public struct TypeHierarchyAscendingEnumerator : IEnumerator<QualifiedType>, IEnumerable<QualifiedType>
{
    private readonly QualifiedType _type;
    private readonly QualifiedType[] _parentTypes;
    private int _index;

    /// <inheritdoc />
    public QualifiedType Current { get; private set; }

    internal TypeHierarchyAscendingEnumerator(QualifiedType type, QualifiedType[] parentTypes)
    {
        _type = type;
        _parentTypes = parentTypes;
        _index = -1;
    }

    /// <inheritdoc cref="IEnumerable{QualifiedType}.GetEnumerator()"/>
    public readonly TypeHierarchyAscendingEnumerator GetEnumerator()
    {
        return _index < 0 ? this : new TypeHierarchyAscendingEnumerator(_type, _parentTypes);
    }

    /// <inheritdoc />
    public bool MoveNext()
    {
        int index = ++_index;
        int l = _parentTypes.Length;
        if (index == l)
        {
            Current = _type;
            return true;
        }

        if (index > l)
            return false;

        Current = _parentTypes[l - index - 1];
        return true;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _index = -1;
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public readonly void Dispose() { }

    readonly object IEnumerator.Current => Current;
    readonly IEnumerator<QualifiedType> IEnumerable<QualifiedType>.GetEnumerator() => GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Enumerates a type's hierarchy from requested type to base type.
/// Includes the requested type but not base types like <see cref="object"/>, <see cref="ValueType"/>, or <see cref="Enum"/>.
/// </summary>
/// <remarks>Use <see cref="TypeHierarchyAscendingEnumerator"/> to go from base type to requested type.</remarks>
public struct TypeHierarchyDescendingEnumerator : IEnumerator<QualifiedType>, IEnumerable<QualifiedType>
{
    private readonly QualifiedType _type;
    private readonly QualifiedType[] _parentTypes;
    private int _index;

    /// <inheritdoc />
    public QualifiedType Current { get; private set; }

    internal TypeHierarchyDescendingEnumerator(QualifiedType type, QualifiedType[] parentTypes)
    {
        _type = type;
        _parentTypes = parentTypes;
        _index = -2;
    }

    /// <inheritdoc cref="IEnumerable{QualifiedType}.GetEnumerator()"/>
    public readonly TypeHierarchyDescendingEnumerator GetEnumerator()
    {
        return _index < -1 ? this : new TypeHierarchyDescendingEnumerator(_type, _parentTypes);
    }

    /// <inheritdoc />
    public bool MoveNext()
    {
        int index = ++_index;
        if (index == -1)
        {
            Current = _type;
            return true;
        }

        int l = _parentTypes.Length;
        if (index >= l)
        {
            return false;
        }

        Current = _parentTypes[index];
        return true;
    }

    /// <inheritdoc />
    public void Reset()
    {
        _index = -2;
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public readonly void Dispose() { }

    readonly object IEnumerator.Current => Current;
    readonly IEnumerator<QualifiedType> IEnumerable<QualifiedType>.GetEnumerator() => GetEnumerator();
    readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}