using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Singleton instances of common type parsers.
/// </summary>
public static class TypeParsers
{
    /// <summary>
    /// The type parser for flag properties, meaning properties that do not require a value.
    /// </summary>
    public static ITypeParser<bool> Flag { get; } = new FlagParser();
    
    /// <summary>
    /// The type parser for <see cref="BooleanOrFlagType"/> properties, meaning properties that do not require a value, but can supply a boolean value.
    /// </summary>
    public static ITypeParser<bool> BooleanOrFlag { get; } = new BooleanOrFlagParser();

    /// <summary>
    /// The type parser for <see cref="bool"/> values.
    /// </summary>
    public static ITypeParser<bool> Boolean { get; } = new TypeConverterParser<bool>(TypeConverters.Boolean);

    /// <summary>
    /// The type parser for <see cref="char"/> values.
    /// </summary>
    public static ITypeParser<char> Character { get; } = new TypeConverterParser<char>(TypeConverters.Character);

    /// <summary>
    /// The type parser for <see cref="string"/> values.
    /// </summary>
    public static ITypeParser<string> String { get; } = new TypeConverterParser<string>(TypeConverters.String);

    /// <summary>
    /// The type parser for <see cref="byte"/> values.
    /// </summary>
    public static ITypeParser<byte> UInt8 { get; } = new TypeConverterParser<byte>(TypeConverters.UInt8);

    /// <summary>
    /// The type parser for <see cref="ushort"/> values.
    /// </summary>
    public static ITypeParser<ushort> UInt16 { get; } = new TypeConverterParser<ushort>(TypeConverters.UInt16);

    /// <summary>
    /// The type parser for <see cref="uint"/> values.
    /// </summary>
    public static ITypeParser<uint> UInt32 { get; } = new TypeConverterParser<uint>(TypeConverters.UInt32);

    /// <summary>
    /// The type parser for <see cref="ulong"/> values.
    /// </summary>
    public static ITypeParser<ulong> UInt64 { get; } = new TypeConverterParser<ulong>(TypeConverters.UInt64);

    /// <summary>
    /// The type parser for <see cref="sbyte"/> values.
    /// </summary>
    public static ITypeParser<sbyte> Int8 { get; } = new TypeConverterParser<sbyte>(TypeConverters.Int8);

    /// <summary>
    /// The type parser for <see cref="short"/> values.
    /// </summary>
    public static ITypeParser<short> Int16 { get; } = new TypeConverterParser<short>(TypeConverters.Int16);

    /// <summary>
    /// The type parser for <see cref="int"/> values.
    /// </summary>
    public static ITypeParser<int> Int32 { get; } = new TypeConverterParser<int>(TypeConverters.Int32);

    /// <summary>
    /// The type parser for <see cref="long"/> values.
    /// </summary>
    public static ITypeParser<long> Int64 { get; } = new TypeConverterParser<long>(TypeConverters.Int64);

    /// <summary>
    /// The type parser for <see cref="float"/> values.
    /// </summary>
    public static ITypeParser<float> Float32 { get; } = new TypeConverterParser<float>(TypeConverters.Float32);

    /// <summary>
    /// The type parser for <see cref="double"/> values.
    /// </summary>
    public static ITypeParser<double> Float64 { get; } = new TypeConverterParser<double>(TypeConverters.Float64);

    /// <summary>
    /// The type parser for <see cref="decimal"/> values.
    /// </summary>
    public static ITypeParser<decimal> Float128 { get; } = new TypeConverterParser<decimal>(TypeConverters.Float128);

    /// <summary>
    /// The type parser for <see cref="System.DateTime"/> values.
    /// </summary>
    public static ITypeParser<DateTime> DateTime { get; } = new TypeConverterParser<DateTime>(TypeConverters.DateTime);

    /// <summary>
    /// The type parser for <see cref="System.DateTimeOffset"/> values.
    /// </summary>
    public static ITypeParser<DateTimeOffset> DateTimeOffset { get; } = new TypeConverterParser<DateTimeOffset>(TypeConverters.DateTimeOffset);

    /// <summary>
    /// The type parser for <see cref="System.TimeSpan"/> values.
    /// </summary>
    public static ITypeParser<TimeSpan> TimeSpan { get; } = new TypeConverterParser<TimeSpan>(TypeConverters.TimeSpan);

    /// <summary>
    /// The type parser for <see cref="System.Guid"/> values.
    /// </summary>
    public static ITypeParser<Guid> Guid { get; } = new TypeConverterParser<Guid>(TypeConverters.Guid);

    /// <summary>
    /// The type parser for <see cref="Data.GuidOrId"/> values.
    /// </summary>
    public static ITypeParser<GuidOrId> GuidOrId { get; } = new TypeConverterParser<GuidOrId>(TypeConverters.GuidOrId);

    /// <summary>
    /// The type parser for <see cref="Data.IPv4Filter"/> values.
    /// </summary>
    public static ITypeParser<IPv4Filter> IPv4Filter { get; } = new TypeConverterParser<IPv4Filter>(TypeConverters.IPv4Filter);

    /// <summary>
    /// The type parser for <see cref="Data.QualifiedType"/> values.
    /// </summary>
    public static ITypeParser<QualifiedType> QualifiedType { get; } = new TypeConverterParser<QualifiedType>(TypeConverters.QualifiedType);

    /// <summary>
    /// The type parser for steam item def values.
    /// </summary>
    public static ITypeParser<int> SteamItemDef => SteamItemDefType.Instance;

    /// <summary>
    /// Gets the type parser for the given type.
    /// </summary>
    /// <typeparam name="T">The type to convert.</typeparam>
    /// <exception cref="InvalidOperationException">The requested type does not have a built-in parser.</exception>
    public static ITypeParser<T> Get<T>() where T : IEquatable<T>
    {
        return TypeParserCache<T>.Parser;
    }

    internal static bool TryParseStringValueOnly<TValue>(ref TypeParserArgs<TValue> args, [NotNullWhen(true)] out IValueSourceNode? valueNode)
        where TValue : IEquatable<TValue>
    {
        switch (args.ValueNode)
        {
            case IValueSourceNode v:
                valueNode = v;
                return true;

            case IListSourceNode l:
                args.DiagnosticSink?.UNT2004_ListInsteadOfValue(ref args, l, args.Type);
                break;

            case IDictionarySourceNode d:
                args.DiagnosticSink?.UNT2004_DictionaryInsteadOfValue(ref args, d, args.Type);
                break;

            default:
                args.DiagnosticSink?.UNT2004_NoValue(ref args, args.ParentNode);
                break;
        }

        valueNode = null;
        return false;
    }

    private static class TypeParserCache<T> where T : IEquatable<T>
    {
        public static readonly ITypeParser<T> Parser;

        static TypeParserCache()
        {
            if (typeof(T).IsPrimitive)
            {
                if (typeof(T) == typeof(byte))
                    Parser = (ITypeParser<T>)UInt8;
                else if (typeof(T) == typeof(sbyte))
                    Parser = (ITypeParser<T>)Int8;
                else if (typeof(T) == typeof(ushort))
                    Parser = (ITypeParser<T>)UInt16;
                else if (typeof(T) == typeof(short))
                    Parser = (ITypeParser<T>)Int16;
                else if (typeof(T) == typeof(uint))
                    Parser = (ITypeParser<T>)UInt32;
                else if (typeof(T) == typeof(int))
                    Parser = (ITypeParser<T>)Int32;
                else if (typeof(T) == typeof(ulong))
                    Parser = (ITypeParser<T>)UInt64;
                else if (typeof(T) == typeof(long))
                    Parser = (ITypeParser<T>)Int64;
                else if (typeof(T) == typeof(float))
                    Parser = (ITypeParser<T>)Float32;
                else if (typeof(T) == typeof(double))
                    Parser = (ITypeParser<T>)Float64;
                else if (typeof(T) == typeof(bool))
                    Parser = (ITypeParser<T>)Boolean;
                else if (typeof(T) == typeof(char))
                    Parser = (ITypeParser<T>)Character;
                else goto err;
                return;
            }

            if (typeof(T).IsValueType)
            {
                if (typeof(T) == typeof(decimal))
                    Parser = (ITypeParser<T>)Float128;
                else if (typeof(T) == typeof(Guid))
                    Parser = (ITypeParser<T>)Guid;
                else if (typeof(T) == typeof(DateTime))
                    Parser = (ITypeParser<T>)DateTime;
                else if (typeof(T) == typeof(DateTimeOffset))
                    Parser = (ITypeParser<T>)DateTimeOffset;
                else if (typeof(T) == typeof(TimeSpan))
                    Parser = (ITypeParser<T>)TimeSpan;
                else if (typeof(T) == typeof(IPv4Filter))
                    Parser = (ITypeParser<T>)IPv4Filter;
                else if (typeof(T) == typeof(QualifiedType))
                    Parser = (ITypeParser<T>)QualifiedType;
                else goto err;
                return;
            }

            if (typeof(T) == typeof(string))
                Parser = (ITypeParser<T>)String;
            else goto err;
            return;

        err:
            throw new InvalidOperationException($"Type {typeof(T).FullName} doesn't have an associated default type parser.");
        }
    }
}