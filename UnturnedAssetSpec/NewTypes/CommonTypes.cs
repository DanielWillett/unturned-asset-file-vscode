using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Collections.Immutable;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public static class CommonTypes
{
    /// <summary>
    /// Case-sensitive dictionary of all known property types by their ID.
    /// </summary>
    public static ImmutableDictionary<string, Func<ITypeFactory>> TypeFactories { get; }

    static CommonTypes()
    {
        ImmutableDictionary<string, Func<ITypeFactory>>.Builder knownTypes = ImmutableDictionary.CreateBuilder<string, Func<ITypeFactory>>(StringComparer.Ordinal);
        
        knownTypes["Flag"]                              = () => FlagType.Instance;
        knownTypes["UInt8"]                             = () => UInt8Type.Instance;
        knownTypes["UInt16"]                            = () => UInt16Type.Instance;
        knownTypes["UInt32"]                            = () => UInt32Type.Instance;
        knownTypes["UInt64"]                            = () => UInt64Type.Instance;
        knownTypes["Int8"]                              = () => Int8Type.Instance;
        knownTypes["Int16"]                             = () => Int16Type.Instance;
        knownTypes["Int32"]                             = () => Int32Type.Instance;
        knownTypes["Int64"]                             = () => Int64Type.Instance;
        knownTypes["String"]                            = () => StringType.Instance;
        knownTypes["RegEx"]                             = () => RegexStringType.Instance;
        knownTypes["Float32"]                           = () => Float32Type.Instance;
        knownTypes["Float64"]                           = () => Float64Type.Instance;
        knownTypes["Float128"]                          = () => Float128Type.Instance;
        knownTypes["Boolean"]                           = () => BooleanType.Instance;
        knownTypes["BooleanOrFlag"]                     = () => BooleanOrFlagType.Instance;
        knownTypes["Character"]                         = () => CharacterType.Instance;
        knownTypes["List"]                              = () => ListType.Factory;
        knownTypes["Type"]                              = () => StringType.Instance;    // todo
        knownTypes["TypeOrEnum"]                        = () => StringType.Instance;    // todo
        knownTypes["Guid"]                              = () => StringType.Instance;    // todo
        knownTypes["Color32RGB"]                        = () => StringType.Instance;    // todo
        knownTypes["Color32RGBA"]                       = () => StringType.Instance;    // todo
        knownTypes["Color32RGBLegacy"]                  = () => StringType.Instance;    // todo
        knownTypes["Color32RGBALegacy"]                 = () => StringType.Instance;    // todo
        knownTypes["Color32RGBString"]                  = () => StringType.Instance;    // todo
        knownTypes["Color32RGBAString"]                 = () => StringType.Instance;    // todo
        knownTypes["Color32RGBStrictHex"]               = () => StringType.Instance;    // todo
        knownTypes["Color32RGBAStrictHex"]              = () => StringType.Instance;    // todo
        knownTypes["Color32RGBAStrictHex"]              = () => StringType.Instance;    // todo
        knownTypes["ColorRGB"]                          = () => StringType.Instance;    // todo
        knownTypes["ColorRGBA"]                         = () => StringType.Instance;    // todo
        knownTypes["ColorRGBLegacy"]                    = () => StringType.Instance;    // todo
        knownTypes["ColorRGBALegacy"]                   = () => StringType.Instance;    // todo
        knownTypes["ColorRGBString"]                    = () => StringType.Instance;    // todo
        knownTypes["ColorRGBAString"]                   = () => StringType.Instance;    // todo
        knownTypes["ColorRGBStrictHex"]                 = () => StringType.Instance;    // todo
        knownTypes["ColorRGBAStrictHex"]                = () => StringType.Instance;    // todo
        knownTypes["ColorRGBAStrictHex"]                = () => StringType.Instance;    // todo
        knownTypes["AssetReference"]                    = () => StringType.Instance;    // todo
        knownTypes["BcAssetReference"]                  = () => StringType.Instance;    // todo
        knownTypes["AssetReferenceString"]              = () => StringType.Instance;    // todo
        knownTypes["BcAssetReferenceString"]            = () => StringType.Instance;    // todo
        knownTypes["ContentReference"]                  = () => StringType.Instance;    // todo
        knownTypes["AudioReference"]                    = () => StringType.Instance;    // todo
        knownTypes["MasterBundleReference"]             = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleOrContentReference"]    = () => StringType.Instance;    // todo
        knownTypes["MasterBundleReferenceString"]       = () => StringType.Instance;    // todo
        knownTypes["TranslationReference"]              = () => StringType.Instance;    // todo
        knownTypes["FaceIndex"]                         = () => UInt8Type.Instance;     // todo
        knownTypes["BeardIndex"]                        = () => UInt8Type.Instance;     // todo
        knownTypes["HairIndex"]                         = () => UInt8Type.Instance;     // todo
        knownTypes["GuidOrId"]                          = () => StringType.Instance;    // todo
        knownTypes["Id"]                                = () => UInt16Type.Instance;    // todo
        knownTypes["DefaultableId"]                     = () => Int32Type.Instance;     // todo
        knownTypes["NavId"]                             = () => UInt8Type.Instance;     // todo
        knownTypes["SpawnpointId"]                      = () => StringType.Instance;    // todo
        knownTypes["OverlapVolumeId"]                   = () => StringType.Instance;    // todo
        knownTypes["ZombieTableId"]                     = () => Int32Type.Instance;     // todo
        knownTypes["ZombieCooldownId"]                  = () => StringType.Instance;    // todo
        knownTypes["FlagId"]                            = () => Int16Type.Instance;     // todo
        knownTypes["BlueprintId"]                       = () => StringType.Instance;    // todo
        knownTypes["NPCAchievementId"]                  = () => StringType.Instance;    // todo
        knownTypes["DateTime"]                          = () => StringType.Instance;    // todo
        knownTypes["TimeSpan"]                          = () => StringType.Instance;    // todo
        knownTypes["DateTimeOffset"]                    = () => StringType.Instance;    // todo
        knownTypes["Position"]                          = () => StringType.Instance;    // todo
        knownTypes["PositionOrLegacy"]                  = () => StringType.Instance;    // todo
        knownTypes["LegacyPosition"]                    = () => StringType.Instance;    // todo
        knownTypes["Scale"]                             = () => StringType.Instance;    // todo
        knownTypes["ScaleOrLegacy"]                     = () => StringType.Instance;    // todo
        knownTypes["LegacyScale"]                       = () => StringType.Instance;    // todo
        knownTypes["EulerRotation"]                     = () => StringType.Instance;    // todo
        knownTypes["EulerRotationOrLegacy"]             = () => StringType.Instance;    // todo
        knownTypes["LegacyEulerRotation"]               = () => StringType.Instance;    // todo
        knownTypes["Vector2"]                           = () => StringType.Instance;    // todo
        knownTypes["CommaDelimitedString"]              = () => StringType.Instance;    // todo
        knownTypes["List"]                              = () => StringType.Instance;    // todo
        knownTypes["Dictionary"]                        = () => StringType.Instance;    // todo
        knownTypes["ListOrSingle"]                      = () => StringType.Instance;    // todo
        knownTypes["MasterBundleName"]                  = () => StringType.Instance;    // todo
        knownTypes["LegacyBundleName"]                  = () => StringType.Instance;    // todo
        knownTypes["AssetBundleVersion"]                = () => Int32Type.Instance;     // todo
        knownTypes["MapName"]                           = () => StringType.Instance;    // todo
        knownTypes["ActionKey"]                         = () => StringType.Instance;    // todo
        knownTypes["LocalizableString"]                 = () => StringType.Instance;    // todo
        knownTypes["LocalizableRichString"]             = () => StringType.Instance;    // todo
        knownTypes["LocalizableTargetString"]           = () => StringType.Instance;    // todo
        knownTypes["LocalizableTargetRichString"]       = () => StringType.Instance;    // todo
        knownTypes["SkillLevel"]                        = () => Int32Type.Instance ;    // todo
        knownTypes["LegacyCompatibleList"]              = () => StringType.Instance;    // todo
        knownTypes["Skill"]                             = () => StringType.Instance;    // todo
        knownTypes["BlueprintSkill"]                    = () => StringType.Instance;    // todo
        knownTypes["SkillLevel"]                        = () => StringType.Instance;    // todo
        knownTypes["SteamItemDef"]                      = () => Int32Type.Instance;     // todo
        knownTypes["CaliberId"]                         = () => UInt16Type.Instance;    // todo
        knownTypes["BladeId"]                           = () => UInt8Type.Instance;     // todo
        knownTypes["PhysicsMaterial"]                   = () => StringType.Instance;    // todo
        knownTypes["PhysicsMaterialLegacy"]             = () => StringType.Instance;    // todo
        knownTypes["TypeReference"]                     = () => StringType.Instance;    // todo
        knownTypes["IPv4Filter"]                        = () => StringType.Instance;    // todo
        knownTypes["Steam64ID"]                         = () => UInt64Type.Instance;    // todo
        knownTypes["Url"]                               = () => StringType.Instance;    // todo
        knownTypes["Path"]                              = () => StringType.Instance;    // todo

        TypeFactories = knownTypes.ToImmutable();
    }

    /// <summary>
    /// Gets the default integer type from <typeparamref name="TCountType"/>, throwing an exception if the type isn't an integer type. Doesn't support native integers.
    /// </summary>
    /// <typeparam name="TCountType">An integer type.</typeparam>
    /// <exception cref="InvalidOperationException"><typeparamref name="TCountType"/> is not an integer type.</exception>
    public static IType<TCountType> GetIntegerType<TCountType>() where TCountType : IEquatable<TCountType>
    {
        if (typeof(TCountType) == typeof(int))
            return (IType<TCountType>)(object)Int32Type.Instance;
        if (typeof(TCountType) == typeof(byte))
            return (IType<TCountType>)(object)UInt8Type.Instance;
        if (typeof(TCountType) == typeof(uint))
            return (IType<TCountType>)(object)UInt32Type.Instance;
        if (typeof(TCountType) == typeof(short))
            return (IType<TCountType>)(object)Int16Type.Instance;
        if (typeof(TCountType) == typeof(ushort))
            return (IType<TCountType>)(object)UInt16Type.Instance;
        if (typeof(TCountType) == typeof(sbyte))
            return (IType<TCountType>)(object)Int8Type.Instance;
        if (typeof(TCountType) == typeof(long))
            return (IType<TCountType>)(object)Int64Type.Instance;
        if (typeof(TCountType) == typeof(ulong))
            return (IType<TCountType>)(object)UInt64Type.Instance;
        
        throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_InvalidCountType, typeof(TCountType).Name));
    }
}
