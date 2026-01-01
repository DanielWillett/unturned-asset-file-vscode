using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;

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
        knownTypes["Guid"]                              = () => GuidType.Instance;
        knownTypes["GuidOrId"]                          = () => GuidOrIdType.Instance;
        knownTypes["Color32"]                           = () => Color32Type.Instance;
        knownTypes["Color"]                             = () => ColorType.Instance;
        knownTypes["AssetReference"]                    = () => AssetReferenceType.Factory;
        knownTypes["BcAssetReference"]                  = () => StringType.Instance;    // todo
        knownTypes["AssetReferenceString"]              = () => AssetReferenceType.Factory;
        knownTypes["BcAssetReferenceString"]            = () => StringType.Instance;    // todo
        knownTypes["ContentReference"]                  = () => BundleReferenceType.Factory;
        knownTypes["AudioReference"]                    = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleReference"]             = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleOrContentReference"]    = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleReferenceString"]       = () => BundleReferenceType.Factory;
        knownTypes["TranslationReference"]              = () => BundleReferenceType.Factory;
        knownTypes["FaceIndex"]                         = () => UInt8Type.Instance;     // todo
        knownTypes["BeardIndex"]                        = () => UInt8Type.Instance;     // todo
        knownTypes["HairIndex"]                         = () => UInt8Type.Instance;     // todo
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
        knownTypes["DateTime"]                          = () => DateTimeType.Instance;
        knownTypes["TimeSpan"]                          = () => TimeSpanType.Instance;
        knownTypes["DateTimeOffset"]                    = () => DateTimeOffsetType.Instance;
        knownTypes["Vector4"]                           = () => Vector4Type.Instance;
        knownTypes["Vector3"]                           = () => Vector3Type.Instance;
        knownTypes["Vector2"]                           = () => Vector2Type.Instance;
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
        knownTypes["IPv4Filter"]                        = () => IPv4FilterType.Instance;
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
        if (typeof(TCountType) == typeof(GuidOrId))
            return (IType<TCountType>)(object)GuidOrIdType.Instance;
        if (typeof(TCountType) == typeof(sbyte))
            return (IType<TCountType>)(object)Int8Type.Instance;
        if (typeof(TCountType) == typeof(long))
            return (IType<TCountType>)(object)Int64Type.Instance;
        if (typeof(TCountType) == typeof(ulong))
            return (IType<TCountType>)(object)UInt64Type.Instance;
        
        throw new InvalidOperationException(string.Format(Resources.InvalidOperationException_InvalidCountType, typeof(TCountType).Name));
    }

    /// <summary>
    /// Gets the default type from <typeparamref name="TValueType"/>, returning <see langword="null"/> if the type doesn't have a default type.
    /// </summary>
    /// <typeparam name="TValueType">Any common value type.</typeparam>
    public static IType<TValueType>? TryGetDefaultValueType<TValueType>() where TValueType : IEquatable<TValueType>
    {
        if (TypeConverters.TryGet<TValueType>() is { } tc)
        {
            return tc.DefaultType;
        }

        return null;
    }

    /// <summary>
    /// Gets the type suffix of a value of the given type (i.e. 'ul', 'u', 'l', 'd', 'f', 'm', etc).
    /// </summary>
    public static string? GetTypeSuffix<TValue>() where TValue : IEquatable<TValue>
    {
        if (typeof(TValue) == typeof(ulong))
            return "ul";
        if (typeof(TValue) == typeof(long))
            return "l";
        if (typeof(TValue) == typeof(uint))
            return "u";
        if (typeof(TValue) == typeof(float))
            return "f";
        if (typeof(TValue) == typeof(double))
            return "d";
        if (typeof(TValue) == typeof(decimal))
            return "m";

        return null;
    }

    /// <summary>
    /// Loads an integer value into a generic parameter if it's a compatible type.
    /// </summary>
    public static bool TryLoadInteger<TResult>(long i, [MaybeNullWhen(false)] out TResult result)
    {
        if (typeof(TResult) == typeof(int))
        {
            if (i is < int.MinValue or > int.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<int, TResult>((int)i);
        }
        else if (typeof(TResult) == typeof(uint))
        {
            if (i is < 0 or > uint.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<uint, TResult>((uint)i);
        }
        else if (typeof(TResult) == typeof(long))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<long, TResult>(i);
        }
        else if (typeof(TResult) == typeof(ushort))
        {
            if (i is < 0 or > ushort.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<ushort, TResult>((ushort)i);
        }
        else if (typeof(TResult) == typeof(GuidOrId))
        {
            if (i is < 0 or > ushort.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<GuidOrId, TResult>(new GuidOrId((ushort)i));
        }
        else if (typeof(TResult) == typeof(char))
        {
            if (i is < 0 or > 9)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<char, TResult>((char)(i + '0'));
        }
        else if (typeof(TResult) == typeof(ulong))
        {
            if (i < 0)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<ulong, TResult>((ulong)i);
        }
        else if (typeof(TResult) == typeof(short))
        {
            if (i is < short.MinValue or > short.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<short, TResult>((short)i);
        }
        else if (typeof(TResult) == typeof(sbyte))
        {
            if (i is < sbyte.MinValue or > sbyte.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<sbyte, TResult>((sbyte)i);
        }
        else if (typeof(TResult) == typeof(byte))
        {
            if (i is < 0 or > byte.MaxValue)
            {
                result = default;
                return false;
            }

            result = SpecDynamicExpressionTreeValueHelpers.As<byte, TResult>((byte)i);
        }
        else if (typeof(TResult) == typeof(float))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<float, TResult>(i);
        }
        else if (typeof(TResult) == typeof(double))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<double, TResult>(i);
        }
        else if (typeof(TResult) == typeof(decimal))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<decimal, TResult>(i);
        }
        else if (typeof(TResult) == typeof(string))
        {
            result = SpecDynamicExpressionTreeValueHelpers.As<string, TResult>(i.ToString(CultureInfo.InvariantCulture));
        }
        else
        {
            result = default;
            return false;
        }


        return true;
    }

    /// <summary>
    /// Attempts to read a value of the given <paramref name="type"/> from a JSON <paramref name="element"/>.
    /// </summary>
    /// <returns>Whether or not the value was successfully parsed.</returns>
    public static bool TryReadFromJson<TValue>(this IType<TValue> type, in JsonElement element, [NotNullWhen(true)] out IValue<TValue>? value)
        where TValue : IEquatable<TValue>

    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (typeof(TValue) == typeof(bool) && Conditions.TryReadComplexOrBasicConditionFromJson(in element, out IValue<bool>? condition))
            {
                value = Unsafe.As<IValue<bool>, IValue<TValue>>(ref condition);
                return true;
            }
        }

        if (type.Parser.TryReadValueFromJson(in element, out Optional<TValue> optionalValue, type))
        {
            value = type.CreateValue(optionalValue);
        }
        else if (TypeConverters.TryGet<TValue>() is { } typeConverter)
        {
            TypeConverterParseArgs<TValue> args = default;
            args.Type = type;

            if (!typeConverter.TryReadJson(in element, out optionalValue, ref args))
            {
                value = null;
                return false;
            }

            value = type.CreateValue(optionalValue);
        }
        else
        {
            value = null;
            return false;
        }

        return true;
    }
}
