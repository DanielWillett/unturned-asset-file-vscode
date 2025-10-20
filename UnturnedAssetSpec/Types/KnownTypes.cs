using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public static class KnownTypes
{
    private static readonly Dictionary<string, Func<ISpecPropertyType>> ConcreteTypes
        = new Dictionary<string, Func<ISpecPropertyType>>(64, StringComparer.Ordinal)
    {
        { "Flag", () => Flag },
        { "Boolean", () => Boolean },
        { "BooleanOrFlag", () => BooleanOrFlag },
        { "FaceIndex", () => FaceIndex },
        { "BeardIndex", () => BeardIndex },
        { "HairIndex", () => HairIndex },
        { "UInt8", () => UInt8 },
        { "UInt16", () => UInt16 },
        { "UInt32", () => UInt32 },
        { "UInt64", () => UInt64 },
        { "Int8", () => Int8 },
        { "Int16", () => Int16 },
        { "Int32", () => Int32 },
        { "Int64", () => Int64 },
        { "String", () => String },
        { "RegEx", () => RegEx },
        { "RichTextString", () => RichTextString },
        { "Character", () => Character },
        { "Float32", () => Float32 },
        { "Float64", () => Float64 },
        { "Float128", () => Float128 },
        { "Type", () => Type },
        { "Guid", () => Guid },
        { "Color32RGB", () => Color32RGB },
        { "Color32RGBA", () => Color32RGBA },
        { "Color32RGBLegacy", () => Color32RGBLegacy },
        { "Color32RGBALegacy", () => Color32RGBALegacy },
        { "Color32RGBStrictHex", () => Color32RGBStrictHex },
        { "Color32RGBAStrictHex", () => Color32RGBAStrictHex },
        { "ColorRGB", () => ColorRGB },
        { "ColorRGBA", () => ColorRGBA },
        { "ColorRGBLegacy", () => ColorRGBLegacy },
        { "ColorRGBALegacy", () => ColorRGBALegacy },
        { "ColorRGBStrictHex", () => ColorRGBStrictHex },
        { "ColorRGBAStrictHex", () => ColorRGBAStrictHex },
        { "AudioReference", () => AudioReference },
        { "TranslationReference", () => TranslationReference },
        { "NavId", () => NavId },
        { "SpawnpointId", () => SpawnpointId },
        { "FlagId", () => FlagId },
        { "BlueprintId", () => BlueprintId },
        { "BlueprintIdString", () => BlueprintIdString },
        { "NPCAchievementId", () => NPCAchievementId },
        { "DateTime", () => DateTime },
        { "DateTimeOffset", () => DateTimeOffset },
        { "Position", () => Position },
        { "PositionOrLegacy", () => PositionOrLegacy },
        { "LegacyPosition", () => LegacyPosition },
        { "Scale", () => Scale },
        { "ScaleOrLegacy", () => ScaleOrLegacy },
        { "LegacyScale", () => LegacyScale },
        { "EulerRotation", () => EulerRotation },
        { "EulerRotationOrLegacy", () => EulerRotationOrLegacy },
        { "LegacyEulerRotation", () => LegacyEulerRotation },
        { "MasterBundleName", () => MasterBundleName },
        { "LegacyBundleName", () => LegacyBundleName },
        { "AssetBundleVersion", () => AssetBundleVersion },
        { "MapName", () => MapName },
        { "ActionKey", () => ActionKey },
        { "LocalizableString", () => LocalizableString },
        { "LocalizableTargetString", () => LocalizableTargetString },
        { "Skill", () => Skill() },
        { "BlueprintSkill", () => Skill(true, true) },
        { "SteamItemDef", () => SteamItemDef },
        { "CaliberId", () => CaliberId },
        { "BladeId", () => BladeId },
        { "PhysicsMaterial", () => String }, // todo
        { "PhysicsMaterialLegacy", () => String }, // todo
        { TypeOf<AssetCategory>(), () => AssetCategory.TypeOf },
        { "SDG.Unturned.EAssetType, Assembly-CSharp", () => AssetCategory.TypeOf },
        { "IPv4Range", () => IPv4Range },
        { "Steam64ID", () => Steam64ID },
    };

    private static string TypeOf<T>()
    {
        return Assembly.CreateQualifiedName(typeof(T).Assembly.GetName().Name, typeof(T).FullName);
    }

    public static ISpecPropertyType? GetType(string knownType)
    {
        if (string.IsNullOrEmpty(knownType))
            return null;

        if (ConcreteTypes.TryGetValue(knownType, out Func<ISpecPropertyType> func))
        {
            return func();
        }

        Type? type;
        try
        {
            type = System.Type.GetType(knownType, false, false);
        }
        catch
        {
            type = null;
        }

        if (type != null && typeof(ISpecPropertyType).IsAssignableFrom(type) && !typeof(ISecondPassSpecPropertyType).IsAssignableFrom(type))
        {
            return (ISpecPropertyType)Activator.CreateInstance(type);
        }

        return null;
    }

    public static ISpecPropertyType? GetType(string knownType, string? elementType, OneOrMore<string> specialTypes, bool resolvedOnly = false)
    {
        if (string.IsNullOrEmpty(knownType))
            return null;

        ISpecPropertyType? t = GetType(knownType);

        if (t != null)
        {
            return t;
        }

        if (knownType.Equals("FilePathString", StringComparison.Ordinal))
        {
            return FilePathString(elementType);
        }

        if (knownType.Equals("TypeOrEnum", StringComparison.Ordinal))
        {
            string? elementType2 = specialTypes.FirstOrDefault();
            return string.IsNullOrEmpty(elementType)
                ? Type
                : TypeOrEnum(elementType2 == null ? default : new QualifiedType(elementType2), new QualifiedType(elementType!));
        }

        if (knownType.Equals("AssetReference", StringComparison.Ordinal))
        {
            return AssetReference(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                specialTypes
            );
        }

        if (knownType.Equals("BcAssetReference", StringComparison.Ordinal))
        {
            return BackwardsCompatibleAssetReference(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                specialTypes
            );
        }

        if (knownType.Equals("BcAssetReferenceString", StringComparison.Ordinal))
        {
            return BackwardsCompatibleAssetReferenceString(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                specialTypes
            );
        }

        if (knownType.Equals("AssetReferenceString", StringComparison.Ordinal))
        {
            return AssetReferenceString(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!), specialTypes);
        }

        if (knownType.Equals("MasterBundleReference", StringComparison.Ordinal))
        {
            return MasterBundleReference(string.IsNullOrEmpty(elementType)
                ? new QualifiedType("UnityEngine.Object, UnityEngine.CoreModule")
                : new QualifiedType(elementType!));
        }

        if (knownType.Equals("MasterBundleOrContentReference", StringComparison.Ordinal))
        {
            return MasterBundleOrContentReference(string.IsNullOrEmpty(elementType)
                ? new QualifiedType("UnityEngine.Object, UnityEngine.CoreModule")
                : new QualifiedType(elementType!));
        }

        if (knownType.Equals("MasterBundleReferenceString", StringComparison.Ordinal))
        {
            return MasterBundleReferenceString(string.IsNullOrEmpty(elementType)
                ? new QualifiedType("UnityEngine.Object, UnityEngine.CoreModule")
                : new QualifiedType(elementType!));
        }

        if (knownType.Equals("ContentReference", StringComparison.Ordinal))
        {
            return ContentReference(string.IsNullOrEmpty(elementType)
                ? new QualifiedType("UnityEngine.Object, UnityEngine.CoreModule")
                : new QualifiedType(elementType!));
        }

        if (knownType.Equals("GuidOrId", StringComparison.Ordinal))
        {
            return GuidOrId(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!), specialTypes);
        }

        if (knownType.Equals("Id", StringComparison.Ordinal))
        {
            if (string.IsNullOrEmpty(elementType))
            {
                return Id(TypeHierarchy.AssetBaseType, specialTypes);
            }

            if (AssetCategory.TryParse(elementType, out EnumSpecTypeValue category))
            {
                return Id(category, specialTypes);
            }

            return Id(new QualifiedType(elementType!), specialTypes);
        }

        if (knownType.Equals("DefaultableId", StringComparison.Ordinal))
        {
            if (string.IsNullOrEmpty(elementType))
            {
                return DefaultableId(TypeHierarchy.AssetBaseType, specialTypes);
            }

            if (AssetCategory.TryParse(elementType, out EnumSpecTypeValue category))
            {
                return DefaultableId(category, specialTypes);
            }

            return DefaultableId(new QualifiedType(elementType!), specialTypes);
        }

        if (knownType.Equals("CommaDelimitedString", StringComparison.Ordinal))
        {
            string? elementType2 = specialTypes.FirstOrDefault();
            return CommaDelimitedString(
                string.IsNullOrEmpty(elementType)
                    ? String
                    : GetType(elementType!, elementType2, specialTypes.Remove(elementType2!)) ?? new UnresolvedSpecPropertyType(elementType!));
        }

        if (knownType.Equals("TypeReference", StringComparison.Ordinal))
        {
            return TypeReference(string.IsNullOrEmpty(elementType)
                ? QualifiedType.None
                : new QualifiedType(elementType!, false)
            );
        }

        bool allowSingle = knownType.Equals("ListOrSingle", StringComparison.Ordinal);
        if (allowSingle || knownType.Equals("List", StringComparison.Ordinal))
        {
            string? elementType2 = specialTypes.FirstOrDefault();

            ISpecPropertyType? resolvedElementType = string.IsNullOrEmpty(elementType)
                ? String
                : GetType(elementType!, elementType2, specialTypes.Remove(elementType2!), resolvedOnly);

            if (resolvedOnly && resolvedElementType == null)
            {
                return null;
            }

            return List(resolvedElementType ?? new UnresolvedSpecPropertyType(elementType!), allowSingle);
        }

        if (knownType.Equals("Dictionary", StringComparison.Ordinal))
        {
            string? elementType2 = specialTypes.FirstOrDefault();

            ISpecPropertyType? resolvedElementType = string.IsNullOrEmpty(elementType)
                ? String
                : GetType(elementType!, elementType2, specialTypes.Remove(elementType2!), resolvedOnly);

            if (resolvedOnly && resolvedElementType == null)
            {
                return null;
            }

            return Dictionary(resolvedElementType ?? new UnresolvedSpecPropertyType(elementType!));
        }

        if (knownType.Equals("LegacyCompatibleList", StringComparison.Ordinal))
        {
            if (elementType == null)
                return null;

            string? elementType2 = specialTypes.FirstOrDefault();

            ISpecType? resolvedElementType = string.IsNullOrEmpty(elementType)
                ? null
                : GetType(elementType, elementType2, specialTypes.Remove(elementType2!), resolvedOnly) as ISpecType;

            if (resolvedOnly && resolvedElementType == null)
                return null;
            
            return resolvedElementType == null
                ? LegacyCompatibleList(new UnresolvedSpecPropertyType(elementType))
                : LegacyCompatibleList(resolvedElementType);
        }

        if (knownType.Equals("SkillLevel", StringComparison.Ordinal))
        {
            return specialTypes.FirstOrDefault() is { } skillProperty
                ? SkillLevel(skillProperty)
                : UInt8;
        }

        if (knownType.Equals("FormatString", StringComparison.Ordinal))
        {
            int argCount = 1;
            bool allowRichText = false;
            if (elementType != null)
            {
                string num, rt;
                int index = elementType.IndexOf('|');
                if (index == -1)
                {
                    num = elementType;
                    rt = string.Empty;
                }
                else
                {
                    num = elementType.Substring(0, index);
                    rt = index == elementType.Length - 1 ? string.Empty : elementType.Substring(index + 1);
                }

                if (!int.TryParse(num, NumberStyles.Number, CultureInfo.InvariantCulture, out argCount))
                    argCount = 1;
                if (rt.StartsWith("T", StringComparison.OrdinalIgnoreCase))
                    allowRichText = true;
            }

            return FormatString(argCount, allowRichText);
        }

        if (knownType.Equals("Url", StringComparison.Ordinal))
        {
            return elementType == null && specialTypes.IsNull ? Url : new UrlSpecPropertyType(elementType, specialTypes);
        }

        return null;
    }

    public static ISpecPropertyType<bool> Flag => FlagSpecPropertyType.Instance;

    public static ISpecPropertyType<bool> Boolean => BooleanSpecPropertyType.Instance;
    public static ISpecPropertyType<bool> BooleanOrFlag => BooleanOrFlagSpecPropertyType.Instance;

    public static ISpecPropertyType<byte> FaceIndex => FaceIndexSpecPropertyType.Instance;
    public static ISpecPropertyType<byte> BeardIndex => BeardIndexSpecPropertyType.Instance;
    public static ISpecPropertyType<byte> HairIndex => HairIndexSpecPropertyType.Instance;

    public static ISpecPropertyType<byte> UInt8 => UInt8SpecPropertyType.Instance;
    public static ISpecPropertyType<ushort> UInt16 => UInt16SpecPropertyType.Instance;
    public static ISpecPropertyType<uint> UInt32 => UInt32SpecPropertyType.Instance;
    public static ISpecPropertyType<ulong> UInt64 => UInt64SpecPropertyType.Instance;
    public static ISpecPropertyType<sbyte> Int8 => Int8SpecPropertyType.Instance;
    public static ISpecPropertyType<short> Int16 => Int16SpecPropertyType.Instance;
    public static ISpecPropertyType<int> Int32 => Int32SpecPropertyType.Instance;
    public static ISpecPropertyType<long> Int64 => Int64SpecPropertyType.Instance;

    public static ISpecPropertyType<string> String => StringSpecPropertyType.Instance;
    public static ISpecPropertyType<string> RegEx => StringSpecPropertyType.Instance; // todo

    public static ISpecPropertyType<string> FilePathString(string? globPattern = null) =>
        string.IsNullOrEmpty(globPattern)
            ? FilePathStringSpecPropertyType.Instance
            : new FilePathStringSpecPropertyType(globPattern);

    public static ISpecPropertyType<string> RichTextString => RichTextStringSpecPropertyType.Instance;
    public static ISpecPropertyType<char> Character => CharacterSpecPropertyType.Instance;

    public static ISpecPropertyType<float> Float32 => Float32SpecPropertyType.Instance;
    public static ISpecPropertyType<double> Float64 => Float64SpecPropertyType.Instance;
    public static ISpecPropertyType<decimal> Float128 => Float128SpecPropertyType.Instance;

    public static ISpecPropertyType<QualifiedType> Type => TypeSpecPropertyType.Instance;
    public static ISpecPropertyType<QualifiedType> TypeOrEnum(QualifiedType elementType, QualifiedType enumType) => new TypeOrEnumSpecPropertyType(elementType, enumType);

    public static ISpecPropertyType<Guid> Guid => GuidSpecPropertyType.Instance;

    public static ISpecPropertyType<Color32> Color32RGB => Color32RGBSpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBA => Color32RGBASpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBLegacy => Color32RGBLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBALegacy => Color32RGBALegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBStrictHex => Color32RGBStrictHexSpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBAStrictHex => Color32RGBAStrictHexSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGB => ColorRGBSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBA => ColorRGBASpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBLegacy => ColorRGBLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBALegacy => ColorRGBALegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBStrictHex => ColorRGBStrictHexSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBAStrictHex => ColorRGBAStrictHexSpecPropertyType.Instance;

    public static ISpecPropertyType<Guid> AssetReference(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new AssetReferenceSpecPropertyType(elementType, true, specialTypes);
    public static ISpecPropertyType<Guid> AssetReferenceString(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new AssetReferenceSpecPropertyType(elementType, false, specialTypes);
    public static ISpecPropertyType<GuidOrId> BackwardsCompatibleAssetReference(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new BackwardsCompatibleAssetReferenceSpecPropertyType(elementType, true, specialTypes);
    public static ISpecPropertyType<GuidOrId> BackwardsCompatibleAssetReferenceString(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new BackwardsCompatibleAssetReferenceSpecPropertyType(elementType, false, specialTypes);
    public static ISpecPropertyType<BundleReference> MasterBundleReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleReference);
    public static ISpecPropertyType<BundleReference> MasterBundleReferenceString(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleReferenceString);
    public static ISpecPropertyType<BundleReference> ContentReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.ContentReference);
    public static ISpecPropertyType<BundleReference> AudioReference => MasterBundleReferenceSpecPropertyType.AudioReference;
    public static ISpecPropertyType<BundleReference> MasterBundleOrContentReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleOrContentReference);

    public static ISpecPropertyType<GuidOrId> GuidOrId(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new GuidOrIdSpecPropertyType(elementType, specialTypes);

    public static ISpecPropertyType<ushort> Id(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new IdSpecPropertyType(elementType, specialTypes);

    public static ISpecPropertyType<ushort> Id(EnumSpecTypeValue assetCategory, OneOrMore<string> specialTypes = default)
        => new IdSpecPropertyType(assetCategory, specialTypes);

    public static ISpecPropertyType<int> DefaultableId(QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new DefaultableIdSpecPropertyType(elementType, specialTypes);

    public static ISpecPropertyType<int> DefaultableId(EnumSpecTypeValue assetCategory, OneOrMore<string> specialTypes = default)
        => new DefaultableIdSpecPropertyType(assetCategory, specialTypes);

    public static ISpecPropertyType<byte> NavId => NavIdSpecPropertyType.Instance;
    public static ISpecPropertyType<string> SpawnpointId => SpawnpointIdSpecPropertyType.Instance;

    public static ISpecPropertyType<ushort> FlagId => FlagIdSpecPropertyType.Instance;

    public static ISpecPropertyType<GuidOrId> BlueprintId => BlueprintIdSpecPropertyType.Instance;
    public static ISpecPropertyType<GuidOrId> BlueprintIdString => BlueprintIdSpecPropertyType.StringInstance;

    public static ISpecPropertyType<string> NPCAchievementId => NPCAchievementIdSpecPropertyType.Instance;

    public static ISpecPropertyType<DateTime> DateTime => DateTimeSpecPropertyType.Instance;
    public static ISpecPropertyType<DateTimeOffset> DateTimeOffset => DateTimeOffsetSpecPropertyType.Instance;

    public static ISpecPropertyType<Vector3> Position => PositionSpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> PositionOrLegacy => PositionOrLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> LegacyPosition => LegacyPositionSpecPropertyType.Instance;

    public static ISpecPropertyType<Vector3> Scale => ScaleSpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> ScaleOrLegacy => ScaleOrLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> LegacyScale => LegacyScaleSpecPropertyType.Instance;

    public static ISpecPropertyType<Vector3> EulerRotation => EulerRotationSpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> EulerRotationOrLegacy => EulerRotationOrLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> LegacyEulerRotation => LegacyEulerRotationSpecPropertyType.Instance;

    public static ISpecPropertyType<string> CommaDelimitedString(ISpecPropertyType innerType)
        => new CommaDelimitedStringSpecPropertyType(innerType ?? throw new ArgumentNullException(nameof(innerType)));

    public static ISpecPropertyType<EquatableArray<TValue>> List<TValue>(ISpecPropertyType<TValue> innerType, bool allowSingle) where TValue : IEquatable<TValue>
        => new ListSpecPropertyType<TValue>(innerType ?? throw new ArgumentNullException(nameof(innerType)), allowSingle);

    public static ISpecPropertyType List(ISpecPropertyType innerType, bool allowSingle)
    {
        if (innerType == null)
            throw new ArgumentNullException(nameof(innerType));

        if (innerType is ISecondPassSpecPropertyType secondPassType)
        {
            return new UnresolvedListSpecPropertyType(secondPassType, allowSingle);
        }

        Type type = typeof(ListSpecPropertyType<>).MakeGenericType(innerType.ValueType);
        return (ISpecPropertyType)Activator.CreateInstance(type, innerType, allowSingle);
    }

    public static ISpecPropertyType<EquatableArray<DictionaryPair<TValue>>> Dictionary<TValue>(ISpecPropertyType<TValue> innerType) where TValue : IEquatable<TValue>
        => new DictionarySpecPropertyType<TValue>(innerType ?? throw new ArgumentNullException(nameof(innerType)));

    public static ISpecPropertyType Dictionary(ISpecPropertyType innerType)
    {
        if (innerType == null)
            throw new ArgumentNullException(nameof(innerType));

        if (innerType is ISecondPassSpecPropertyType secondPassType)
        {
            return new UnresolvedDictionarySpecPropertyType(secondPassType);
        }

        Type type = typeof(DictionarySpecPropertyType<>).MakeGenericType(innerType.ValueType);
        return (ISpecPropertyType)Activator.CreateInstance(type, innerType);
    }

    public static ISpecPropertyType<string> MasterBundleName => MasterBundleNameSpecPropertyType.Instance;
    public static ISpecPropertyType<string> LegacyBundleName => LegacyBundleNameSpecPropertyType.Instance;
    public static ISpecPropertyType<int> AssetBundleVersion => AssetBundleVersionSpecPropertyType.Instance;
    public static ISpecPropertyType<string> MapName => MapNameSpecPropertyType.Instance;
    public static ISpecPropertyType<string> ActionKey => ActionKeySpecPropertyType.Instance;
    public static ISpecPropertyType<string> LocalizableString => LocalizableStringSpecPropertyType.Instance;
    public static ISpecPropertyType<string> LocalizableTargetString => LocalizableStringSpecPropertyType.TargetInstance;

    public static ISpecPropertyType<byte> SkillLevel(string skillsetOrPropertyName)
        => new SkillLevelSpecPropertyType(skillsetOrPropertyName);

    public static ISpecPropertyType<string> Skill(bool allowStandardSkills = true, bool allowBlueprintSkills = false)
        => new SkillSpecPropertyType(allowStandardSkills, allowBlueprintSkills);

    public static ISpecPropertyType<EquatableArray<CustomSpecTypeInstance>> LegacyCompatibleList(ISpecType type)
    {
        return new LegacyCompatibleListSpecPropertyType(type);
    }
    public static ISpecPropertyType LegacyCompatibleList(ISecondPassSpecPropertyType type)
    {
        return new UnresolvedLegacyCompatibleListSpecPropertyType(type);
    }

    public static ISpecPropertyType<int> SteamItemDef => SteamItemDefSpecPropertyType.Instance;
    public static ISpecPropertyType<ushort> CaliberId => CaliberIdSpecPropertyType.Instance;
    public static ISpecPropertyType<byte> BladeId => BladeIdSpecPropertyType.Instance;
    public static ISpecPropertyType<string> FormatString(int argCount, bool allowRichText)
    {
        return argCount switch
        {
            1 => allowRichText ? FormatStringSpecPropertyType.OneRichText : FormatStringSpecPropertyType.OneNoRichText,
            2 => allowRichText ? FormatStringSpecPropertyType.TwoRichText : FormatStringSpecPropertyType.TwoNoRichText,
            _ => new FormatStringSpecPropertyType(argCount, allowRichText)
        };
    }
    public static ISpecPropertyType<QualifiedType> TypeReference(QualifiedType elementType)
        => new TypeReferenceSpecPropertyType(elementType);

    public static ISpecPropertyType<string> Url => UrlSpecPropertyType.Instance;
    public static ISpecPropertyType<string> IPv4Range => StringSpecPropertyType.Instance; // todo
    public static ISpecPropertyType<ulong> Steam64ID => UInt64SpecPropertyType.Instance; // todo
    public static ISpecPropertyType<BundleReference> TranslationReference => MasterBundleReferenceSpecPropertyType.TranslationReference;
}