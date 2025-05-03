using DanielWillett.UnturnedDataFileLspServer.Data.Types.DynamicTypes;
using System;
using System.Numerics;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;
public static class KnownTypes
{
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
    public static ISpecPropertyType<string> RichTextString => RichTextStringSpecPropertyType.Instance;
    public static ISpecPropertyType<char> Character => CharacterSpecPropertyType.Instance;

    public static ISpecPropertyType<float> Float32 => Float32SpecPropertyType.Instance;
    public static ISpecPropertyType<double> Float64 => Float64SpecPropertyType.Instance;
    public static ISpecPropertyType<decimal> Float128 => Float128SpecPropertyType.Instance;

    public static ISpecPropertyType<QualifiedType> Type => TypeSpecPropertyType.Instance;
    public static ISpecPropertyType<QualifiedType> TypeOrEnum(QualifiedType elementType) => new TypeOrEnumSpecPropertyType(elementType);

    public static ISpecPropertyType<Guid> Guid => GuidSpecPropertyType.Instance;

    public static ISpecPropertyType<DateTime> DateTime => DateTimeSpecPropertyType.Instance;

    public static ISpecPropertyType<Vector3> Position => PositionSpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> PositionOrLegacy => PositionOrLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> LegacyPosition => LegacyPositionSpecPropertyType.Instance;

    public static ISpecPropertyType<Vector3> Scale => ScaleSpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> ScaleOrLegacy => ScaleOrLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> LegacyScale => LegacyScaleSpecPropertyType.Instance;

    public static ISpecPropertyType<Vector3> EulerRotation => EulerRotationSpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> EulerRotationOrLegacy => EulerRotationOrLegacySpecPropertyType.Instance;
    public static ISpecPropertyType<Vector3> LegacyEulerRotation => LegacyEulerRotationSpecPropertyType.Instance;

    public static ISpecPropertyType<Guid> AssetReference(QualifiedType elementType)
        => new AssetReferenceSpecPropertyType(elementType);
    public static ISpecPropertyType<BundleReference> MasterBundleReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleReference);
    public static ISpecPropertyType<BundleReference> MasterBundleReferenceString(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.MasterBundleReferenceString);
    public static ISpecPropertyType<BundleReference> ContentReference(QualifiedType elementType)
        => new MasterBundleReferenceSpecPropertyType(elementType, MasterBundleReferenceType.ContentReference);
    public static ISpecPropertyType<BundleReference> AudioReference => MasterBundleReferenceSpecPropertyType.AudioReference;
}