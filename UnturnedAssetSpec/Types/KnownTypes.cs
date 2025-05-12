using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public static class KnownTypes
{
    private static readonly Dictionary<string, Func<ISpecPropertyType>> ConcreteTypes
        = new Dictionary<string, Func<ISpecPropertyType>>(64, StringComparer.Ordinal)
    {
        { "Flag", () => Flag },
        { "Boolean", () => Boolean },
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
        { "ColorRGB", () => ColorRGB },
        { "ColorRGBA", () => ColorRGBA },
        { "ColorRGBLegacy", () => ColorRGBLegacy },
        { "ColorRGBALegacy", () => ColorRGBALegacy },
        { "AudioReference", () => AudioReference },
        { "NavId", () => NavId },
        { "SpawnpointId", () => SpawnpointId },
        { "FlagId", () => FlagId },
        { "BlueprintSupplyId", () => BlueprintSupplyId },
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
        { "LocalizableString", () => LocalizableString }
    };

    public static ISpecPropertyType? GetType(string knownType, SpecProperty property, string? elementType)
    {
        if (string.IsNullOrEmpty(knownType))
            return null;

        if (ConcreteTypes.TryGetValue(knownType, out Func<ISpecPropertyType> func))
        {
            return func();
        }

        if (knownType.Equals("FilePathString", StringComparison.Ordinal))
        {
            return FilePathString(elementType);
        }

        if (knownType.Equals("TypeOrEnum", StringComparison.Ordinal))
        {
            return string.IsNullOrEmpty(elementType) || property.SpecialTypes is not { Length: 1 }
                ? Type
                : TypeOrEnum(new QualifiedType(elementType!), new QualifiedType(property.SpecialTypes[0]));
        }

        if (knownType.Equals("AssetReference", StringComparison.Ordinal))
        {
            return AssetReference(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!),
                property.SpecialTypes
            );
        }

        if (knownType.Equals("AssetReferenceString", StringComparison.Ordinal))
        {
            return AssetReferenceString(string.IsNullOrEmpty(elementType)
                ? new QualifiedType(TypeHierarchy.AssetBaseType)
                : new QualifiedType(elementType!), property.SpecialTypes);
        }

        if (knownType.Equals("MasterBundleReference", StringComparison.Ordinal))
        {
            return MasterBundleReference(string.IsNullOrEmpty(elementType)
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
                : new QualifiedType(elementType!), property.SpecialTypes);
        }

        if (knownType.Equals("Id", StringComparison.Ordinal))
        {
            if (string.IsNullOrEmpty(elementType))
            {
                return Id(TypeHierarchy.AssetBaseType, property.SpecialTypes);
            }

            if (AssetCategory.TryParse(elementType, out EnumSpecTypeValue category))
            {
                return Id(category, property.SpecialTypes);
            }

            return Id(new QualifiedType(elementType!), property.SpecialTypes);
        }

        if (knownType.Equals("CommaDelimtedString", StringComparison.Ordinal))
        {
            string? elementType2 = property.SpecialTypes?.FirstOrDefault();
            return CommaDelimtedString(
                string.IsNullOrEmpty(elementType)
                    ? String
                    : GetType(elementType!, property, elementType2) ?? new UnresolvedSpecPropertyType(elementType!));
        }

        if (knownType.Equals("List", StringComparison.Ordinal))
        {
            string? elementType2 = property.SpecialTypes?.FirstOrDefault();
            return List(
                string.IsNullOrEmpty(elementType)
                    ? String
                    : GetType(elementType!, property, elementType2) ?? new UnresolvedSpecPropertyType(elementType!));
        }

        Type? type = System.Type.GetType(knownType, false, false);
        if (type != null && typeof(ISpecPropertyType).IsAssignableFrom(type))
        {
            return (ISpecPropertyType)Activator.CreateInstance(type);
        }

        return null;
    }

    public static ISpecPropertyType<bool> Flag => FlagSpecPropertyType.Instance;

    public static ISpecPropertyType<bool> Boolean => BooleanSpecPropertyType.Instance;

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
    public static ISpecPropertyType<Color> ColorRGB => ColorRGBSpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBA => ColorRGBASpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBLegacy => ColorRGBLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Color> ColorRGBALegacy => ColorRGBALegacySpecPropertyType.Instance;

    public static ISpecPropertyType<Guid> AssetReference(QualifiedType elementType, string[]? specialTypes = null)
        => new AssetReferenceSpecPropertyType(elementType, true, specialTypes);
    public static ISpecPropertyType<Guid> AssetReferenceString(QualifiedType elementType, string[]? specialTypes = null)
        => new AssetReferenceSpecPropertyType(elementType, false, specialTypes);
    public static ISpecPropertyType<BundleReference> MasterBundleReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleReference);
    public static ISpecPropertyType<BundleReference> MasterBundleReferenceString(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleReferenceString);
    public static ISpecPropertyType<BundleReference> ContentReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.ContentReference);
    public static ISpecPropertyType<BundleReference> AudioReference => MasterBundleReferenceSpecPropertyType.AudioReference;

    public static ISpecPropertyType<GuidOrId> GuidOrId(QualifiedType elementType, string[]? specialTypes = null)
        => new GuidOrIdSpecPropertyType(elementType, specialTypes);

    public static ISpecPropertyType<ushort> Id(QualifiedType elementType, string[]? specialTypes = null)
        => new IdSpecPropertyType(elementType, specialTypes);

    public static ISpecPropertyType<ushort> Id(EnumSpecTypeValue assetCategory, string[]? specialTypes = null)
        => new IdSpecPropertyType(assetCategory, specialTypes);

    public static ISpecPropertyType<byte> NavId => NavIdSpecPropertyType.Instance;
    public static ISpecPropertyType<string> SpawnpointId => SpawnpointIdSpecPropertyType.Instance;

    public static ISpecPropertyType<ushort> FlagId => FlagIdSpecPropertyType.Instance;

    public static ISpecPropertyType<ushort> BlueprintSupplyId => BlueprintSupplyIdSpecPropertyType.Instance;

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

    public static ISpecPropertyType<string> CommaDelimtedString(ISpecPropertyType innerType)
        => new CommaDelimtedStringSpecPropertyType(innerType ?? throw new ArgumentNullException(nameof(innerType)));

    public static ISpecPropertyType<EquatableArray<TValue>> List<TValue>(ISpecPropertyType<TValue> innerType) where TValue : IEquatable<TValue>
        => new ListSpecPropertyType<TValue>(innerType ?? throw new ArgumentNullException(nameof(innerType)));

    public static ISpecPropertyType List(ISpecPropertyType innerType)
    {
        if (innerType == null)
            throw new ArgumentNullException(nameof(innerType));

        Type type = typeof(ListSpecPropertyType<>).MakeGenericType(innerType.ValueType);
        return (ISpecPropertyType)Activator.CreateInstance(type, innerType);
    }

    public static ISpecPropertyType<string> MasterBundleName => MasterBundleNameSpecPropertyType.Instance;
    public static ISpecPropertyType<string> LegacyBundleName => LegacyBundleNameSpecPropertyType.Instance;
    public static ISpecPropertyType<int> AssetBundleVersion => AssetBundleVersionSpecPropertyType.Instance;
    public static ISpecPropertyType<string> MapName => MapNameSpecPropertyType.Instance;
    public static ISpecPropertyType<string> ActionKey => ActionKeySpecPropertyType.Instance;
    public static ISpecPropertyType<string> LocalizableString => LocalizableStringSpecPropertyType.Instance;
}