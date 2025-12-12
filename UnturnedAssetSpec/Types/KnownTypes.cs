using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

/// <summary>
/// Directory of common types used throughout the spec.
/// </summary>
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
        { "Path", () => Path },
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
        { "Color32RGBString", () => Color32RGBString },
        { "Color32RGBAString", () => Color32RGBAString },
        { "Color32RGBStrictHex", () => Color32RGBStrictHex },
        { "Color32RGBAStrictHex", () => Color32RGBAStrictHex },
        { "ColorRGB", () => ColorRGB },
        { "ColorRGBA", () => ColorRGBA },
        { "ColorRGBLegacy", () => ColorRGBLegacy },
        { "ColorRGBALegacy", () => ColorRGBALegacy },
        { "ColorRGBString", () => ColorRGBString },
        { "ColorRGBAString", () => ColorRGBAString },
        { "ColorRGBStrictHex", () => ColorRGBStrictHex },
        { "ColorRGBAStrictHex", () => ColorRGBAStrictHex },
        { "AudioReference", () => AudioReference },
        { "TranslationReference", () => TranslationReference },
        { "NavId", () => NavId },
        { "SpawnpointId", () => SpawnpointId },
        { "OverlapVolumeId", () => OverlapVolumeId },
        { "ZombieTableId", () => ZombieTableId },
        { "ZombieCooldownId", () => ZombieCooldownId },
        { "FlagId", () => FlagId },
        { "NPCAchievementId", () => NPCAchievementId },
        { "DateTime", () => DateTime },
        { "TimeSpan", () => TimeSpan },
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
        { "Vector2", () => Vector2 },
        { "MasterBundleName", () => MasterBundleName },
        { "LegacyBundleName", () => LegacyBundleName },
        { "AssetBundleVersion", () => AssetBundleVersion },
        { "MapName", () => MapName },
        { "ActionKey", () => ActionKey },
        { "LocalizableString", () => LocalizableString },
        { "LocalizableRichString", () => LocalizableRichString },
        { "LocalizableTargetString", () => LocalizableTargetString },
        { "LocalizableTargetRichString", () => LocalizableTargetRichString },
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

    /// <summary>
    /// Parse the type from a type name.
    /// </summary>
    /// <remarks>
    /// This method will not return all types.
    /// Use <see cref="GetType(IAssetSpecDatabase?,string,string?,OneOrMore{string},bool)"/> instead if you have element types.
    /// </remarks>
    public static ISpecPropertyType? GetType(IAssetSpecDatabase? database, string knownType)
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

        if (type == null || !typeof(ISpecPropertyType).IsAssignableFrom(type) || typeof(ISecondPassSpecPropertyType).IsAssignableFrom(type))
            return null;

        ConstructorInfo? ctor = type.GetConstructor(System.Type.EmptyTypes);
        if (ctor != null)
            return (ISpecPropertyType)ctor.Invoke(Array.Empty<object>());


        ctor ??= type.GetConstructor([typeof(IAssetSpecDatabase)]);
        if (ctor == null)
            return null;

        if (database == null)
        {
            return new UnresolvedSpecPropertyType(knownType, isKnownType: true);
        }

        return (ISpecPropertyType)ctor.Invoke([ database ]);
    }

    /// <summary>
    /// Parse the type from a type name and element types.
    /// </summary>
    /// <remarks>If possible use <see cref="GetType(IAssetSpecDatabase?,string,string?,OneOrMore{string},bool)"/> instead.</remarks>
    public static ISpecPropertyType? GetType(
        IAssetSpecDatabase? database,
        string knownType,
        string? elementType,
        OneOrMore<string> specialTypes,
        IAdditionalPropertyProvider? additionalPropertiesProvider = null,
        bool resolvedOnly = false)
    {
        if (string.IsNullOrEmpty(knownType))
            return null;

        ISpecPropertyType? t = GetType(database, knownType);

        if (t != null)
        {
            return t;
        }

        //if (knownType.Equals("FilePathString", StringComparison.Ordinal))
        //{
        //    return FilePathString(elementType);
        //}

        if (knownType.Equals("TypeOrEnum", StringComparison.Ordinal))
        {
            string? elementType2 = specialTypes.FirstOrDefault();
            if (string.IsNullOrEmpty(elementType))
                return Type;

            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);

            return TypeOrEnum(database, elementType2 == null ? default : new QualifiedType(elementType2), new QualifiedType(elementType!));
        }

        if (knownType.Equals("AssetReference", StringComparison.Ordinal))
        {
            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);
            
            return AssetReference(database, string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                specialTypes
            );
        }

        if (knownType.Equals("BlueprintId", StringComparison.Ordinal))
        {
            return database == null
                ? new UnresolvedSpecPropertyType(knownType, elementType, specialTypes)
                : BlueprintId(database);
        }
        if (knownType.Equals("BlueprintIdString", StringComparison.Ordinal))
        {
            return database == null
                ? new UnresolvedSpecPropertyType(knownType, elementType, specialTypes)
                : BlueprintIdString(database);
        }

        if (knownType.Equals("BcAssetReference", StringComparison.Ordinal))
        {
            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);

            return BackwardsCompatibleAssetReference(database, string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                specialTypes
            );
        }

        if (knownType.Equals("BcAssetReferenceString", StringComparison.Ordinal))
        {
            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);

            return BackwardsCompatibleAssetReferenceString(database, string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                specialTypes
            );
        }

        if (knownType.Equals("AssetReferenceString", StringComparison.Ordinal))
        {
            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);

            return AssetReferenceString(database, string.IsNullOrEmpty(elementType)
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
                    : GetType(database, elementType!, elementType2, specialTypes.Remove(elementType2!), additionalPropertiesProvider) ?? new UnresolvedSpecPropertyType(elementType!));
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
                : GetType(database, elementType!, elementType2, specialTypes.Remove(elementType2!), additionalPropertiesProvider, resolvedOnly);

            if (resolvedOnly && resolvedElementType == null)
            {
                return null;
            }

            return List(resolvedElementType ?? new UnresolvedSpecPropertyType(elementType!), allowSingle);
        }

        if (knownType.Equals("Dictionary", StringComparison.Ordinal))
        {
            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);

            string? elementType2 = specialTypes.FirstOrDefault();

            ISpecPropertyType? resolvedElementType = string.IsNullOrEmpty(elementType)
                ? String
                : GetType(database, elementType!, elementType2, specialTypes.Remove(elementType2!), additionalPropertiesProvider, resolvedOnly);

            if (resolvedOnly && resolvedElementType == null)
            {
                return null;
            }

            return Dictionary(database, resolvedElementType ?? new UnresolvedSpecPropertyType(elementType!));
        }

        if (knownType.Equals("LegacyCompatibleList", StringComparison.Ordinal))
        {
            if (elementType == null)
                return null;

            string? elementType2 = specialTypes.FirstOrDefault();

            ISpecPropertyType? resolvedElementType = string.IsNullOrEmpty(elementType)
                ? null
                : GetType(database, elementType, elementType2, specialTypes.Remove(elementType2!), additionalPropertiesProvider, resolvedOnly);

            if (resolvedOnly && resolvedElementType == null)
                return null;

            bool allowSingleModern = false, allowSingleLegacy = false;

            additionalPropertiesProvider?.TryGetAdditionalProperty("AllowSingleModern", out allowSingleModern);
            additionalPropertiesProvider?.TryGetAdditionalProperty("AllowSingleLegacy", out allowSingleLegacy);
            
            return resolvedElementType == null
                ? LegacyCompatibleList(new UnresolvedSpecPropertyType(elementType), allowSingleModern, allowSingleLegacy)
                : LegacyCompatibleList(resolvedElementType, allowSingleModern, allowSingleLegacy);
        }

        if (knownType.Equals("Skill", StringComparison.Ordinal))
        {
            return database == null
                ? new UnresolvedSpecPropertyType(knownType, isKnownType: true)
                : Skill(database);
        }
        if (knownType.Equals("BlueprintSkill", StringComparison.Ordinal))
        {
            return database == null
                ? new UnresolvedSpecPropertyType(knownType, isKnownType: true)
                : Skill(database, true, true);
        }
        if (knownType.Equals("SkillLevel", StringComparison.Ordinal))
        {
            if (specialTypes.FirstOrDefault() is not { } skillProperty)
                return UInt8;

            if (database == null)
                return new UnresolvedSpecPropertyType(knownType, elementType, specialTypes);

            return SkillLevel(database, skillProperty);
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
    public static ISpecPropertyType<string> Path => PathSpecPropertyType.Instance;
    public static ISpecPropertyType<string> RegEx => StringSpecPropertyType.Instance; // todo

    //public static ISpecPropertyType<string> FilePathString(string? globPattern = null) =>
    //    string.IsNullOrEmpty(globPattern)
    //        ? FilePathStringSpecPropertyType.Instance
    //        : new FilePathStringSpecPropertyType(globPattern);

    public static ISpecPropertyType<string> RichTextString => RichTextStringSpecPropertyType.Instance;
    public static ISpecPropertyType<char> Character => CharacterSpecPropertyType.Instance;

    public static ISpecPropertyType<float> Float32 => Float32SpecPropertyType.Instance;
    public static ISpecPropertyType<double> Float64 => Float64SpecPropertyType.Instance;
    public static ISpecPropertyType<decimal> Float128 => Float128SpecPropertyType.Instance;

    public static ISpecPropertyType<QualifiedType> Type => TypeSpecPropertyType.Instance;
    public static ISpecPropertyType<QualifiedType> TypeOrEnum(IAssetSpecDatabase database, QualifiedType elementType, QualifiedType enumType)
        => new TypeOrEnumSpecPropertyType(database, elementType, enumType);

    public static ISpecPropertyType<Guid> Guid => GuidSpecPropertyType.Instance;

    public static ISpecPropertyType<Color32> Color32RGB => Color32RGBSpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBA => Color32RGBASpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBLegacy => Color32RGBLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBALegacy => Color32RGBALegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBString => Color32RGBStringSpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBAString => Color32RGBAStringSpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBStrictHex => Color32RGBStrictHexSpecPropertyType.Instance;
    public static ISpecPropertyType<Color32> Color32RGBAStrictHex => Color32RGBAStrictHexSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGB => ColorRGBSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBA => ColorRGBASpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBLegacy => ColorRGBLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBALegacy => ColorRGBALegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBString => ColorRGBStringSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBAString => ColorRGBAStringSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBStrictHex => ColorRGBStrictHexSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBAStrictHex => ColorRGBAStrictHexSpecPropertyType.Instance;

    public static ISpecPropertyType<Guid> AssetReference(IAssetSpecDatabase database, QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new AssetReferenceSpecPropertyType(database, elementType, true, specialTypes);
    public static ISpecPropertyType<Guid> AssetReferenceString(IAssetSpecDatabase database, QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new AssetReferenceSpecPropertyType(database, elementType, false, specialTypes);
    public static ISpecPropertyType<GuidOrId> BackwardsCompatibleAssetReference(IAssetSpecDatabase database, QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new BackwardsCompatibleAssetReferenceSpecPropertyType(database, elementType, true, specialTypes);
    public static ISpecPropertyType<GuidOrId> BackwardsCompatibleAssetReferenceString(IAssetSpecDatabase database, QualifiedType elementType, OneOrMore<string> specialTypes = default)
        => new BackwardsCompatibleAssetReferenceSpecPropertyType(database, elementType, false, specialTypes);
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
    public static ISpecPropertyType<string> OverlapVolumeId => OverlapVolumeIdSpecPropertyType.Instance;
    public static ISpecPropertyType<int> ZombieTableId => ZombieTableIdSpecPropertyType.Instance;
    public static ISpecPropertyType<string> ZombieCooldownId => ZombieCooldownIdSpecPropertyType.Instance;

    public static ISpecPropertyType<ushort> FlagId => FlagIdSpecPropertyType.Instance;

    public static ISpecPropertyType<GuidOrId> BlueprintId(IAssetSpecDatabase database)
        => new BlueprintIdSpecPropertyType(database, true);
    public static ISpecPropertyType<GuidOrId> BlueprintIdString(IAssetSpecDatabase database)
        => new BlueprintIdSpecPropertyType(database, false);

    public static ISpecPropertyType<string> NPCAchievementId => NPCAchievementIdSpecPropertyType.Instance;

    public static ISpecPropertyType<DateTime> DateTime => DateTimeSpecPropertyType.Instance;
    public static ISpecPropertyType<TimeSpan> TimeSpan => TimeSpanSpecPropertyType.Instance;
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
    public static ISpecPropertyType<Vector2> Vector2 => Vector2SpecPropertyType.Instance;

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

    public static ISpecPropertyType<EquatableArray<DictionaryPair<TValue>>> Dictionary<TValue>(IAssetSpecDatabase database, ISpecPropertyType<TValue> innerType) where TValue : IEquatable<TValue>
        => new DictionarySpecPropertyType<TValue>(database, innerType ?? throw new ArgumentNullException(nameof(innerType)));

    public static ISpecPropertyType Dictionary(IAssetSpecDatabase database, ISpecPropertyType innerType)
    {
        if (innerType == null)
            throw new ArgumentNullException(nameof(innerType));

        if (innerType is ISecondPassSpecPropertyType secondPassType)
        {
            return new UnresolvedDictionarySpecPropertyType(database, secondPassType);
        }

        Type type = typeof(DictionarySpecPropertyType<>).MakeGenericType(innerType.ValueType);
        return (ISpecPropertyType)Activator.CreateInstance(type, database, innerType);
    }

    public static ISpecPropertyType<string> MasterBundleName => MasterBundleNameSpecPropertyType.Instance;
    public static ISpecPropertyType<string> LegacyBundleName => LegacyBundleNameSpecPropertyType.Instance;
    public static ISpecPropertyType<int> AssetBundleVersion => AssetBundleVersionSpecPropertyType.Instance;
    public static ISpecPropertyType<string> MapName => MapNameSpecPropertyType.Instance;
    public static ISpecPropertyType<string> ActionKey => ActionKeySpecPropertyType.Instance;
    public static ISpecPropertyType<string> LocalizableString => LocalizableStringSpecPropertyType.Instance;
    public static ISpecPropertyType<string> LocalizableRichString => LocalizableStringSpecPropertyType.RichInstance;
    public static ISpecPropertyType<string> LocalizableTargetString => LocalizableStringSpecPropertyType.TargetInstance;
    public static ISpecPropertyType<string> LocalizableTargetRichString => LocalizableStringSpecPropertyType.TargetRichInstance;

    public static ISpecPropertyType<byte> SkillLevel(IAssetSpecDatabase database, string skillsetOrPropertyName)
        => new SkillLevelSpecPropertyType(database, skillsetOrPropertyName);

    public static ISpecPropertyType<string> Skill(IAssetSpecDatabase database, bool allowStandardSkills = true, bool allowBlueprintSkills = false)
        => new SkillSpecPropertyType(database, allowStandardSkills, allowBlueprintSkills);

    public static ISpecPropertyType<EquatableArray<TValue>> LegacyCompatibleList<TValue>(ISpecPropertyType<TValue> innerType, bool allowSingleModern = false, bool allowSingleLegacy = false) where TValue : IEquatable<TValue>
        => new LegacyCompatibleListSpecPropertyType<TValue>(innerType ?? throw new ArgumentNullException(nameof(innerType)), allowSingleModern, allowSingleLegacy);

    public static ISpecPropertyType LegacyCompatibleList(ISpecPropertyType innerType, bool allowSingleModern = false, bool allowSingleLegacy = false)
    {
        if (innerType == null)
            throw new ArgumentNullException(nameof(innerType));

        if (innerType is ISecondPassSpecPropertyType secondPassType)
        {
            return new UnresolvedLegacyCompatibleListSpecPropertyType(secondPassType, allowSingleModern, allowSingleLegacy);
        }

        Type type = typeof(LegacyCompatibleListSpecPropertyType<>).MakeGenericType(innerType.ValueType);
        return (ISpecPropertyType)Activator.CreateInstance(type, innerType, allowSingleModern, allowSingleLegacy);
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