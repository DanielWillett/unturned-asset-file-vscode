using DanielWillett.UnturnedDataFileLspServer.Data.TypeConverters;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Contains generic information about asset types.
/// </summary>
public class AssetInformation
{

#nullable disable
    public Dictionary<string, QualifiedType> AssetAliases { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<string>))]
    public Dictionary<QualifiedType, string> AssetCategories { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<TypeHierarchy>))]
    public Dictionary<QualifiedType, TypeHierarchy> Types { get; set; }

    [JsonConverter(typeof(TypeDictionaryConverter<InverseTypeHierarchy>))]
    public Dictionary<QualifiedType, InverseTypeHierarchy> ParentTypes { get; set; }

    public int FaceCount { get; set; } = 32;
    public int BeardCount { get; set; } = 16;
    public int HairCount { get; set; } = 23;


#nullable restore

    public AssetBundleVersionInfo?[]? AssetBundleVersions { get; set; }
    public int[]? EmissiveFaces { get; set; }
    public string? FaceTextureTemplate { get; set; }
    public string? EmissiveFaceTextureTemplate { get; set; }
    public string? BeardTextureTemplate { get; set; }
    public string? HairTextureTemplate { get; set; }
    public string? StatusJsonFallbackUrl { get; set; }

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
        if (Types == null || !Types.TryGetValue(baseType, out TypeHierarchy? hierarchy))
            return new TypeHierarchy();

        return hierarchy;
    }

    public InverseTypeHierarchy GetParentTypes(QualifiedType type)
    {
        if (ParentTypes == null)
        {
            if (Types == null || Types.Count == 0)
                return new InverseTypeHierarchy(new TypeHierarchy { Type = type }, Array.Empty<QualifiedType>(), false);

            Stack<QualifiedType> typeStack = new Stack<QualifiedType>();
            Dictionary<QualifiedType, InverseTypeHierarchy> parentTypes = new Dictionary<QualifiedType, InverseTypeHierarchy>(96);
            foreach (KeyValuePair<QualifiedType, TypeHierarchy> h in Types)
            {
                h.Value.Type = h.Key;
                QualifiedType[]? arrNull = null;
                RegenParentTypes(h.Value, typeStack, ref arrNull, parentTypes);
            }

            ParentTypes = parentTypes;
        }

        return ParentTypes.TryGetValue(type, out InverseTypeHierarchy? typeHierarchy)
            ? typeHierarchy
            : new InverseTypeHierarchy(new TypeHierarchy { Type = type }, Array.Empty<QualifiedType>(), false);
    }

    private static void RegenParentTypes(
        TypeHierarchy hierarchy,
        Stack<QualifiedType> typeStack,
        ref QualifiedType[]? arr,
        Dictionary<QualifiedType, InverseTypeHierarchy> parentTypes)
    {
        if (hierarchy.Type.IsNull)
            return;

        arr ??= typeStack.ToArray();
        parentTypes[hierarchy.Type] = new InverseTypeHierarchy(hierarchy, typeStack.ToArray(), true);
        if (hierarchy.ChildTypes is not { Count: > 0 })
            return;

        typeStack.Push(hierarchy.Type);
        foreach (TypeHierarchy h in hierarchy.ChildTypes.Values)
        {
            QualifiedType[]? arrNull = null;
            h.HasDataFiles |= hierarchy.HasDataFiles;
            h.Parent = hierarchy;
            RegenParentTypes(h, typeStack, ref arrNull, parentTypes);
        }

        typeStack.Pop();
    }

    public class AssetBundleVersionInfo
    {
        public required UnityEngineVersion EndVersion { get; set; }
        public required string DisplayName { get; set; }
    }
}

[JsonConverter(typeof(TypeHierarchyConverter))]
public class TypeHierarchy
{
    public const string AssetBaseType = "SDG.Unturned.Asset, Assembly-CSharp";

    public const string UseableBaseType = "SDG.Unturned.Useable, Assembly-CSharp";

    public bool HasDataFiles { get; set; }
    public bool IsAbstract { get; set; }
    public QualifiedType Type { get; set; }
    public TypeHierarchy? Parent { get; set; }
#nullable disable
    public IReadOnlyDictionary<QualifiedType, TypeHierarchy> ChildTypes { get; set; }
#nullable restore
}

public class InverseTypeHierarchy
{
    public TypeHierarchy Hierarchy { get; }
    public QualifiedType Type { get; }
    public QualifiedType[] ParentTypes { get; }
    public bool IsValid { get; }
    public InverseTypeHierarchy(TypeHierarchy hierarchy, QualifiedType[] parentTypes, bool isValid)
    {
        Hierarchy = hierarchy;
        Type = hierarchy.Type;
        ParentTypes = parentTypes;
        IsValid = isValid;
    }
}