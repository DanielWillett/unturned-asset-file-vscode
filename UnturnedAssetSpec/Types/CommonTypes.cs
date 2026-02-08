using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using DanielWillett.UnturnedDataFileLspServer.Data.Values;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

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
        
        knownTypes[FlagType.TypeId]                     = () => FlagType.Instance;
        knownTypes[UInt8Type.TypeId]                    = () => UInt8Type.Instance;
        knownTypes[UInt16Type.TypeId]                   = () => UInt16Type.Instance;
        knownTypes[UInt32Type.TypeId]                   = () => UInt32Type.Instance;
        knownTypes[UInt64Type.TypeId]                   = () => UInt64Type.Instance;
        knownTypes[Int8Type.TypeId]                     = () => Int8Type.Instance;
        knownTypes[Int16Type.TypeId]                    = () => Int16Type.Instance;
        knownTypes[Int32Type.TypeId]                    = () => Int32Type.Instance;
        knownTypes[Int64Type.TypeId]                    = () => Int64Type.Instance;
        knownTypes[StringType.TypeId]                   = () => StringType.Instance;
        knownTypes[RegexStringType.TypeId]              = () => RegexStringType.Instance;
        knownTypes[Float32Type.TypeId]                  = () => Float32Type.Instance;
        knownTypes[Float64Type.TypeId]                  = () => Float64Type.Instance;
        knownTypes[Float128Type.TypeId]                 = () => Float128Type.Instance;
        knownTypes[BooleanType.TypeId]                  = () => BooleanType.Instance;
        knownTypes[BooleanOrFlagType.TypeId]            = () => BooleanOrFlagType.Instance;
        knownTypes[CharacterType.TypeId]                = () => CharacterType.Instance;
        knownTypes[ListType.TypeId]                     = () => ListType.Factory;
        knownTypes[QualifiedTypeType.TypeId]            = () => QualifiedTypeType.Instance;
        knownTypes[GuidType.TypeId]                     = () => GuidType.Instance;
        knownTypes[GuidOrIdType.TypeId]                 = () => GuidOrIdType.Instance;
        knownTypes[Color32Type.TypeId]                  = () => Color32Type.Instance;
        knownTypes[ColorType.TypeId]                    = () => ColorType.Instance;
        knownTypes["AssetReference"]                    = () => AssetReferenceType.Factory;
        knownTypes["AssetReferenceString"]              = () => AssetReferenceType.Factory;
        knownTypes["BcAssetReference"]                  = () => BackwardsCompatibleAssetReferenceType.Factory;
        knownTypes["BcAssetReferenceString"]            = () => BackwardsCompatibleAssetReferenceType.Factory;
        knownTypes["LegacyAssetReferenceString"]        = () => BackwardsCompatibleAssetReferenceType.Factory;
        knownTypes["ContentReference"]                  = () => BundleReferenceType.Factory;
        knownTypes["AudioReference"]                    = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleReference"]             = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleOrContentReference"]    = () => BundleReferenceType.Factory;
        knownTypes["MasterBundleReferenceString"]       = () => BundleReferenceType.Factory;
        knownTypes["TranslationReference"]              = () => BundleReferenceType.Factory;
        knownTypes["FaceIndex"]                         = () => CharacterCosmeticIndexType.Factory;
        knownTypes["BeardIndex"]                        = () => CharacterCosmeticIndexType.Factory;
        knownTypes["HairIndex"]                         = () => CharacterCosmeticIndexType.Factory;
        knownTypes["LegacyAssetReference"]              = () => LegacyAssetReferenceType.Factory;
        knownTypes["DefaultableLegacyAssetReference"]   = () => LegacyAssetReferenceType.Factory;
        knownTypes[NavIdType.TypeId]                    = () => NavIdType.Instance;
        knownTypes[SpawnpointIdType.TypeId]             = () => SpawnpointIdType.Instance;
        knownTypes[OverlapVolumeIdType.TypeId]          = () => OverlapVolumeIdType.Instance;
        knownTypes[ZombieTableIdType.TypeId]            = () => ZombieTableIdType.Instance;
        knownTypes[ZombieCooldownIdType.TypeId]         = () => ZombieCooldownIdType.Instance;
        knownTypes[FlagIdType.TypeId]                   = () => FlagIdType.Instance;
        knownTypes[NPCAchievementIdType.TypeId]         = () => NPCAchievementIdType.Instance;
        knownTypes[DateTimeType.TypeId]                 = () => DateTimeType.Instance;
        knownTypes[TimeSpanType.TypeId]                 = () => TimeSpanType.Instance;
        knownTypes[DateTimeOffsetType.TypeId]           = () => DateTimeOffsetType.Instance;
        knownTypes[Vector4Type.TypeId]                  = () => Vector4Type.Instance;
        knownTypes[Vector3Type.TypeId]                  = () => Vector3Type.Instance;
        knownTypes[Vector2Type.TypeId]                  = () => Vector2Type.Instance;
        knownTypes[CommaDelimitedStringType.TypeId]     = () => CommaDelimitedStringType.Factory;
        knownTypes[DictionaryType.TypeId]               = () => DictionaryType.Factory;
        knownTypes["MasterBundleName"]                  = () => StringType.Instance;    // todo
        knownTypes["LegacyBundleName"]                  = () => StringType.Instance;    // todo
        knownTypes["AssetBundleVersion"]                = () => Int32Type.Instance;     // todo
        knownTypes["MapName"]                           = () => StringType.Instance;    // todo
        knownTypes["ActionKey"]                         = () => ActionKeyType.Instance;
        knownTypes["LocalizableString"]                 = () => LocalizableStringType.Factory;
        knownTypes["LocalizableTargetString"]           = () => LocalizationKeyType.Factory;
        knownTypes["SkillLevel"]                        = () => SkillLevelType.Factory;
        knownTypes["Skill"]                             = () => SkillType.Factory;
        knownTypes["BlueprintSkill"]                    = () => SkillType.Factory;
        knownTypes["SteamItemDef"]                      = () => SteamItemDefType.Instance;
        knownTypes["CaliberId"]                         = () => UInt16Type.Instance;    // todo
        knownTypes["BladeId"]                           = () => UInt8Type.Instance;     // todo
        knownTypes["PhysicsMaterial"]                   = () => StringType.Instance;    // todo
        knownTypes["PhysicsMaterialLegacy"]             = () => StringType.Instance;    // todo
        knownTypes["TypeReference"]                     = () => TypeReferenceType.Factory;
        knownTypes["IPv4Filter"]                        = () => IPv4FilterType.Instance;
        knownTypes[Steam64IdType.TypeId]                = () => Steam64IdType.Factory;
        knownTypes["Url"]                               = () => StringType.Instance;    // todo
        knownTypes["Path"]                              = () => StringType.Instance;    // todo
        knownTypes[NullType.TypeId]                     = () => NullType.Instance;

        knownTypes[AssetCategory.TypeId]                = () => AssetCategory.Instance;

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

            result = MathMatrix.As<int, TResult>((int)i);
        }
        else if (typeof(TResult) == typeof(uint))
        {
            if (i is < 0 or > uint.MaxValue)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<uint, TResult>((uint)i);
        }
        else if (typeof(TResult) == typeof(long))
        {
            result = MathMatrix.As<long, TResult>(i);
        }
        else if (typeof(TResult) == typeof(ushort))
        {
            if (i is < 0 or > ushort.MaxValue)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<ushort, TResult>((ushort)i);
        }
        else if (typeof(TResult) == typeof(GuidOrId))
        {
            if (i is < 0 or > ushort.MaxValue)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<GuidOrId, TResult>(new GuidOrId((ushort)i));
        }
        else if (typeof(TResult) == typeof(char))
        {
            if (i is < 0 or > 9)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<char, TResult>((char)(i + '0'));
        }
        else if (typeof(TResult) == typeof(ulong))
        {
            if (i < 0)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<ulong, TResult>((ulong)i);
        }
        else if (typeof(TResult) == typeof(short))
        {
            if (i is < short.MinValue or > short.MaxValue)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<short, TResult>((short)i);
        }
        else if (typeof(TResult) == typeof(sbyte))
        {
            if (i is < sbyte.MinValue or > sbyte.MaxValue)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<sbyte, TResult>((sbyte)i);
        }
        else if (typeof(TResult) == typeof(byte))
        {
            if (i is < 0 or > byte.MaxValue)
            {
                result = default;
                return false;
            }

            result = MathMatrix.As<byte, TResult>((byte)i);
        }
        else if (typeof(TResult) == typeof(float))
        {
            result = MathMatrix.As<float, TResult>(i);
        }
        else if (typeof(TResult) == typeof(double))
        {
            result = MathMatrix.As<double, TResult>(i);
        }
        else if (typeof(TResult) == typeof(decimal))
        {
            result = MathMatrix.As<decimal, TResult>(i);
        }
        else if (typeof(TResult) == typeof(string))
        {
            result = MathMatrix.As<string, TResult>(i.ToString(CultureInfo.InvariantCulture));
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
    public static bool TryReadFromJson<TValue>(this IType<TValue> type, in JsonElement element, IAssetSpecDatabase database, IDatSpecificationObject owner, [NotNullWhen(true)] out IValue<TValue>? value)
        where TValue : IEquatable<TValue>

    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (typeof(TValue) == typeof(bool) && Conditions.TryReadComplexOrBasicConditionFromJson(in element, database, owner, out IValue<bool>? condition))
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

    private static readonly Type[] FactoryMethodTypes = [ typeof(SpecificationTypeFactoryArgs).MakeByRefType() ];

    /// <summary>
    /// Attempts to create a type from a built-in C# type, optionally using the factory method.
    /// </summary>
    /// <remarks>The type must be decorated with the <see cref="SpecificationTypeAttribute"/> and implement <see cref="IType"/> for this to succeed.</remarks>
    public static bool TryCreateBuiltInType(
        Type clrType,
        IDatSpecificationReadContext context,
        IDatSpecificationObject owner,
        string typeId,
        [NotNullWhen(true)] out IType? type,
        bool throwExceptions = false,
        bool requireDatType = false)
    {
        type = null;

        if (requireDatType)
        {
            if (!clrType.IsSubclassOf(typeof(DatType)))
            {
                return false;
            }
        }
        else if (!typeof(IType).IsAssignableFrom(clrType))
        {
            return false;
        }

        Attribute? attribute = clrType.GetCustomAttribute(typeof(SpecificationTypeAttribute), inherit: false);
        if (attribute is not SpecificationTypeAttribute specTypeAttribute)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(specTypeAttribute.FactoryMethod))
        {
            try
            {
                MethodInfo? method = clrType.GetMethod(
                    specTypeAttribute.FactoryMethod,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                    null,
                    FactoryMethodTypes,
                    null
                );

                if (method == null)
                {
                    return false;
                }

                SpecificationTypeFactory factory = (SpecificationTypeFactory)method.CreateDelegate(typeof(SpecificationTypeFactory));

                SpecificationTypeFactoryArgs args = new SpecificationTypeFactoryArgs(context, owner, typeId);

                type = factory(in args);
                return type != null;
            }
            catch
            {
                if (throwExceptions)
                    throw;
                
                return false;
            }
        }

        object? newType = null;
        try
        {
            newType = Activator.CreateInstance(clrType, nonPublic: true);
            if (newType is not IType t)
            {
                if (newType is IDisposable disp)
                    disp.Dispose();
                return false;
            }

            type = t;
            return true;
        }
        catch
        {
            if (newType is IDisposable disp)
                disp.Dispose();
            if (throwExceptions)
                throw;

            return false;
        }
    }

    private delegate IType? SpecificationTypeFactory(in SpecificationTypeFactoryArgs args);
}