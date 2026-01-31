using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Text.Json;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// A type of <see cref="ITypeParser{T}"/> that parses only string values using an <see cref="ITypeConverter{T}"/>.
/// </summary>
/// <typeparam name="T">The type being parsed.</typeparam>
public class TypeConverterParser<T>(ITypeConverter<T> typeConverter)
    : ITypeParser<T>
    where T : IEquatable<T>
{
    private readonly ITypeConverter<T> _typeConverter = typeConverter;

    /// <summary>
    /// The type converter used to parse values.
    /// </summary>
    public ITypeConverter<T> TypeConverter => _typeConverter;

    /// <summary>
    /// Whether or not certain types, such as the comma-delimited string,
    /// can bypass this parser to avoid extra allocations when parsing from a span.
    /// </summary>
    public virtual bool CanUseTypeConverterDirectly => true;

    /// <summary>
    /// Overridable behavior for parsing a string value, allowing for injecting more diagnostics after parsing.
    /// </summary>
    /// <param name="v">The value node being parsed.</param>
    /// <param name="args">Other arguments passed to all parsers.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="value">The parsed value wrapped in an <see cref="Optional{T}"/> object.</param>
    /// <returns>Whether or not the value could be parsed successfully.</returns>
    protected virtual bool TryParseValueNode(IValueSourceNode v, ref TypeParserArgs<T> args, in FileEvaluationContext ctx, out Optional<T> value)
    {
        args.CreateTypeConverterParseArgs(out TypeConverterParseArgs<T> parseArgs, v.Value);

        if (_typeConverter.TryParse(v.Value.AsSpan(), ref parseArgs, out T? parsedValue))
        {
            if (parseArgs.ShouldIgnoreFailureDiagnostic)
                args.ShouldIgnoreFailureDiagnostic = true;
            value = parsedValue;
            return true;
        }

        if (parseArgs.ShouldIgnoreFailureDiagnostic)
            args.ShouldIgnoreFailureDiagnostic = true;

        value = Optional<T>.Null;
        return false;
    }

    /// <inheritdoc />
    public bool TryParse(ref TypeParserArgs<T> args, in FileEvaluationContext ctx, out Optional<T> value)
    {
        if (!TypeParsers.TryParseStringValueOnly(ref args, out IValueSourceNode? v))
        {
            value = Optional<T>.Null;
            return false;
        }

        if (TryParseValueNode(v, ref args, in ctx, out value))
        {
            return true;
        }

        if (!args.ShouldIgnoreFailureDiagnostic)
        {
            args.DiagnosticSink?.UNT2004_Generic(ref args, v.Value, args.Type);
            args.ShouldIgnoreFailureDiagnostic = true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool TryReadValueFromJson(in JsonElement json, out Optional<T> value, IType<T> valueType)
    {
        TypeConverterParseArgs<T> parseArgs = new TypeConverterParseArgs<T>(valueType);
        return _typeConverter.TryReadJson(in json, out value, ref parseArgs);
    }

    /// <inheritdoc />
    public void WriteValueToJson(Utf8JsonWriter writer, T value, IType<T> valueType, JsonSerializerOptions options)
    {
        TypeConverterFormatArgs args = default;
        _typeConverter.WriteJson(writer, value, ref args, options);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is TypeConverterParser<T> p && _typeConverter.Equals(p._typeConverter);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _typeConverter.GetHashCode() * 397;
    }
}