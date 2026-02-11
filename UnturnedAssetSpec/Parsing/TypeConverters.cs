using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Singleton instances of common type converters.
/// </summary>
public static class TypeConverters
{
    /// <summary>
    /// The type converter for <see cref="bool"/> values.
    /// </summary>
    public static ITypeConverter<bool> Boolean { get; } = new BooleanTypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="char"/> values.
    /// </summary>
    public static ITypeConverter<char> Character { get; } = new CharacterTypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="string"/> values.
    /// </summary>
    public static ITypeConverter<string> String { get; } = new StringTypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="byte"/> values.
    /// </summary>
    public static ITypeConverter<byte> UInt8 { get; } = new UInt8TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="ushort"/> values.
    /// </summary>
    public static ITypeConverter<ushort> UInt16 { get; } = new UInt16TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="uint"/> values.
    /// </summary>
    public static ITypeConverter<uint> UInt32 { get; } = new UInt32TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="ulong"/> values.
    /// </summary>
    public static ITypeConverter<ulong> UInt64 { get; } = new UInt64TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="sbyte"/> values.
    /// </summary>
    public static ITypeConverter<sbyte> Int8 { get; } = new Int8TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="short"/> values.
    /// </summary>
    public static ITypeConverter<short> Int16 { get; } = new Int16TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="int"/> values.
    /// </summary>
    public static ITypeConverter<int> Int32 { get; } = new Int32TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="long"/> values.
    /// </summary>
    public static ITypeConverter<long> Int64 { get; } = new Int64TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="float"/> values.
    /// </summary>
    public static ITypeConverter<float> Float32 { get; } = new Float32TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="double"/> values.
    /// </summary>
    public static ITypeConverter<double> Float64 { get; } = new Float64TypeConverter();
    
    /// <summary>
    /// The type converter for <see cref="decimal"/> values.
    /// </summary>
    public static ITypeConverter<decimal> Float128 { get; } = new Float128TypeConverter();

    /// <summary>
    /// The type converter for <see cref="System.DateTime"/> values.
    /// </summary>
    public static ITypeConverter<DateTime> DateTime { get; } = new DateTimeTypeConverter();

    /// <summary>
    /// The type converter for <see cref="System.DateTimeOffset"/> values.
    /// </summary>
    public static ITypeConverter<DateTimeOffset> DateTimeOffset { get; } = new DateTimeOffsetTypeConverter();

    /// <summary>
    /// The type converter for <see cref="System.TimeSpan"/> values.
    /// </summary>
    public static ITypeConverter<TimeSpan> TimeSpan { get; } = new TimeSpanTypeConverter();

    /// <summary>
    /// The type converter for <see cref="System.Guid"/> values.
    /// </summary>
    public static ITypeConverter<Guid> Guid { get; } = new GuidTypeConverter();

    /// <summary>
    /// The type converter for <see cref="System.GuidOrId"/> values.
    /// </summary>
    public static ITypeConverter<GuidOrId> GuidOrId { get; } = new GuidOrIdTypeConverter();

    /// <summary>
    /// The type converter for <see cref="Data.Types.IPv4Filter"/> values.
    /// </summary>
    public static ITypeConverter<IPv4Filter> IPv4Filter { get; } = new IPv4FilterTypeConverter();

    /// <summary>
    /// The type converter for <see cref="Data.QualifiedType"/> values.
    /// </summary>
    public static ITypeConverter<QualifiedType> QualifiedType { get; } = new QualifiedTypeTypeConverter();

    /// <summary>
    /// The type converter for <see cref="Data.BundleReference"/> values.
    /// </summary>
    public static ITypeConverter<BundleReference> BundleReference { get; } = new BundleReferenceTypeConverter();

    /// <summary>
    /// The type converter for steam item def values.
    /// </summary>
    public static ITypeConverter<int> SteamItemDef => SteamItemDefType.Instance;

    /// <summary>
    /// Gets the type converter for the given type.
    /// </summary>
    /// <typeparam name="T">The type to convert.</typeparam>
    /// <exception cref="InvalidOperationException">The requested type does not have a built-in converter.</exception>
    public static ITypeConverter<T> Get<T>() where T : IEquatable<T>
    {
        return TypeConverterCache<T>.Converter
               ?? throw new InvalidOperationException($"Type {typeof(T).FullName} doesn't have an associated default type converter.");
    }

    /// <summary>
    /// Gets the type converter for the given type.
    /// </summary>
    /// <typeparam name="T">The type to convert.</typeparam>
    public static ITypeConverter<T>? TryGet<T>() where T : IEquatable<T>
    {
        return TypeConverterCache<T>.Converter;
    }

    /// <summary>
    /// Checks whether or not <typeparamref name="TTo"/> can be converted as a number.
    /// </summary>
    /// <remarks>This includes all floating point types, all integer types including native integers, <see cref="bool"/>, <see cref="char"/>, and <see cref="GuidOrId"/>.</remarks>
    public static bool IsNumericConvertible<TTo>() where TTo : IEquatable<TTo>
    {
        return typeof(TTo) == typeof(float) || typeof(TTo) == typeof(double) || typeof(TTo) == typeof(decimal)
               || typeof(TTo) == typeof(bool) || typeof(TTo) == typeof(char)
               || typeof(TTo) == typeof(sbyte) || typeof(TTo) == typeof(short) || typeof(TTo) == typeof(int) || typeof(TTo) == typeof(long) || typeof(TTo) == typeof(nint)
               || typeof(TTo) == typeof(byte) || typeof(TTo) == typeof(ushort) || typeof(TTo) == typeof(uint) || typeof(TTo) == typeof(ulong) || typeof(TTo) == typeof(nuint)
               || typeof(TTo) == typeof(GuidOrId);
    }

    private static class TypeConverterCache<T> where T : IEquatable<T>
    {
        public static readonly ITypeConverter<T>? Converter;

        static TypeConverterCache()
        {
            if (typeof(T).IsPrimitive)
            {
                if (typeof(T) == typeof(byte))
                    Converter = (ITypeConverter<T>)UInt8;
                else if (typeof(T) == typeof(sbyte))
                    Converter = (ITypeConverter<T>)Int8;
                else if (typeof(T) == typeof(ushort))
                    Converter = (ITypeConverter<T>)UInt16;
                else if (typeof(T) == typeof(short))
                    Converter = (ITypeConverter<T>)Int16;
                else if (typeof(T) == typeof(uint))
                    Converter = (ITypeConverter<T>)UInt32;
                else if (typeof(T) == typeof(int))
                    Converter = (ITypeConverter<T>)Int32;
                else if (typeof(T) == typeof(ulong))
                    Converter = (ITypeConverter<T>)UInt64;
                else if (typeof(T) == typeof(long))
                    Converter = (ITypeConverter<T>)Int64;
                else if (typeof(T) == typeof(float))
                    Converter = (ITypeConverter<T>)Float32;
                else if (typeof(T) == typeof(double))
                    Converter = (ITypeConverter<T>)Float64;
                else if (typeof(T) == typeof(bool))
                    Converter = (ITypeConverter<T>)Boolean;
                else if (typeof(T) == typeof(char))
                    Converter = (ITypeConverter<T>)Character;
                return;
            }

            if (typeof(T).IsValueType)
            {
                if (typeof(T) == typeof(decimal))
                    Converter = (ITypeConverter<T>)Float128;
                else if (typeof(T) == typeof(Guid))
                    Converter = (ITypeConverter<T>)Guid;
                else if (typeof(T) == typeof(GuidOrId))
                    Converter = (ITypeConverter<T>)GuidOrId;
                else if (typeof(T) == typeof(DateTime))
                    Converter = (ITypeConverter<T>)DateTime;
                else if (typeof(T) == typeof(DateTimeOffset))
                    Converter = (ITypeConverter<T>)DateTimeOffset;
                else if (typeof(T) == typeof(TimeSpan))
                    Converter = (ITypeConverter<T>)TimeSpan;
                else if (typeof(T) == typeof(IPv4Filter))
                    Converter = (ITypeConverter<T>)IPv4Filter;
                else if (typeof(T) == typeof(QualifiedType))
                    Converter = (ITypeConverter<T>)QualifiedType;
                else if (typeof(T) == typeof(BundleReference))
                    Converter = (ITypeConverter<T>)BundleReference;
                return;
            }

            if (typeof(T) == typeof(string))
            {
                Converter = (ITypeConverter<T>)String;
                return;
            }

            if (VectorTypes.TryGetProvider<T>() is { } vectorProvider)
            {
                Converter = vectorProvider.Converter;
            }
        }
    }
}