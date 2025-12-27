using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Objects which are used to parse basic values from value properties implement this interface.
/// </summary>
/// <remarks>Most type converters should be case-insensitive.</remarks>
public interface ITypeConverter<T> where T : IEquatable<T>
{
    /// <summary>
    /// Attempt to parse a value from it's <paramref name="text"/> span. This and <see cref="Format"/> should have a round-trip relationship.
    /// </summary>
    /// <param name="text">The text to parse. If <see cref="TypeConverterParseArgs.TextAsString"/> is set, it should have an identical value to this property.</param>
    /// <param name="args">Other arguments passed to all parsers.</param>
    /// <param name="parsedValue">The value, if it was successfully parsed, otherwise <see langword="default"/>.</param>
    /// <returns><see langword="true"/> if the value was parsed successfully, otherwise <see langword="false"/>.</returns>
    bool TryParse(ReadOnlySpan<char> text, ref TypeConverterParseArgs<T> args, [MaybeNullWhen(false)] out T parsedValue);

    /// <summary>
    /// Formats a value back into a string. This and <see cref="TryParse"/> should have a round-trip relationship, although can output different formats (ex. N instead of D for GUIDs).
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="args">Other arguments passed to all parsers.</param>
    string Format(T value, ref TypeConverterFormatArgs args);

    /// <summary>
    /// Formats a value back into a string. This and <see cref="TryParse"/> should have a round-trip relationship, although can output different formats (ex. N instead of D for GUIDs).
    /// </summary>
    /// <remarks>When a format fails, <paramref name="size"/> should be at least the size required to write properly, but potentially higher if it's not possible/reasonable to calculate the required length.</remarks>
    /// <param name="output">Buffer to format values to.</param>
    /// <param name="value">The value to format.</param>
    /// <param name="size">Number of characters required to format <paramref name="value"/>.</param>
    /// <param name="args">Other arguments passed to all parsers.</param>
    /// <returns><see langword="true"/> if the value was able to fit into <paramref name="output"/>, otherwise <see langword="false"/>.</returns>
    bool TryFormat(Span<char> output, T value, out int size, ref TypeConverterFormatArgs args);

    /// <summary>
    /// Converts this value to a value of the specified type.
    /// </summary>
    /// <typeparam name="TTo">The type to convert <paramref name="obj"/> to.</typeparam>
    /// <param name="obj">The value to convert to <typeparamref name="TTo"/>.</param>
    /// <returns>Whether or not <paramref name="obj"/> could be converted to <typeparamref name="TTo"/>.</returns>
    bool TryConvertTo<TTo>(Optional<T> obj, out Optional<TTo> result) where TTo : IEquatable<TTo>;
    
    /// <summary>
    /// Writes a value to a <see cref="Utf8JsonWriter"/>.
    /// </summary>
    void WriteJson(Utf8JsonWriter writer, T value, ref TypeConverterFormatArgs args);

    /// <summary>
    /// Reads a value from a <see cref="JsonElement"/>.
    /// </summary>
    bool TryReadJson(in JsonElement json, out Optional<T> value, ref TypeConverterParseArgs<T> args);
}

/// <summary>
/// Arguments passed to all <see cref="ITypeConverter{T}.TryParse"/> implementations.
/// </summary>
public struct TypeConverterParseArgs<T> : IDiagnosticProvider where T : IEquatable<T>
{
    /// <summary>
    /// Allows parsers which directly interpret strings to return this value instead of allocating a new identical string.
    /// </summary>
    public string? TextAsString;

    /// <summary>
    /// Used to report diagnostics encountered when parsing. Ignored if <see langword="null"/>.
    /// </summary>
    public IDiagnosticSink? DiagnosticSink;

    /// <summary>
    /// Set to <see langword="true"/> when a diagnostic is reported by a parser,
    /// meaning the fallback 'failed to parse' diagnostic shouldn't be emitted.
    /// </summary>
    public bool ShouldIgnoreFailureDiagnostic;

    /// <summary>
    /// The type being parsed.
    /// </summary>
    public required IType<T> Type;

    /// <summary>
    /// Range used to report diagnostics for this value.
    /// </summary>
    public FileRange ValueRange;

    /// <summary>
    /// The property being parsed.
    /// </summary>
    public DatProperty? Property;

    DatProperty? IDiagnosticProvider.Property => Property;


    [SetsRequiredMembers]
    public TypeConverterParseArgs(IType<T> type)
    {
        Type = type;
    }

    [SetsRequiredMembers]
    public TypeConverterParseArgs(IType<T> type, IDiagnosticSink diagnosticSink, FileRange range)
    {
        Type = type;
        ValueRange = range;
        DiagnosticSink = diagnosticSink;
    }

    public readonly string GetString(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty)
            return string.Empty;

        return TextAsString != null && text.Length == TextAsString.Length ? TextAsString : text.ToString();
    }

    public FileRange GetRangeAndRegisterDiagnostic()
    {
        ShouldIgnoreFailureDiagnostic = true;
        return ValueRange;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
    internal readonly ReadOnlySpan<char> StringOrSpan(ReadOnlySpan<char> text) => text;
#else
    internal readonly string StringOrSpan(ReadOnlySpan<char> text) => GetString(text);
#endif
}

/// <summary>
/// Arguments passed to all <see cref="ITypeConverter{T}.Format"/> implementations.
/// </summary>
public struct TypeConverterFormatArgs
{
    /// <summary>
    /// The default value for formatting a value.
    /// </summary>
    // ReSharper disable once UnassignedReadonlyField
    public static readonly TypeConverterFormatArgs Default;

    /// <summary>
    /// Used to re-use ToString conversions between multiple calls to <see cref="ITypeConverter{T}.TryFormat"/>.
    /// </summary>
    internal string? FormatCache;
}