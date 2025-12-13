using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Type parsers are used to read values from <see cref="IAnyValueSourceNode"/> nodes.
/// </summary>
/// <typeparam name="T">The type being parsed.</typeparam>
public interface ITypeParser<T> where T : IEquatable<T>
{
    /// <summary>
    /// Attempt to parse this value from a source node.
    /// </summary>
    /// <param name="args">Other arguments passed to all parsers.</param>
    /// <param name="ctx">Workspace context.</param>
    /// <param name="value">The parsed value wrapped in an <see cref="Optional{T}"/> object.</param>
    /// <returns>Whether or not the value could be parsed successfully.</returns>
    bool TryParse(ref TypeParserArgs<T> args, in FileEvaluationContext ctx, out Optional<T> value);
}


/// <summary>
/// Arguments passed to all implementations of <see cref="ITypeParser{T}.TryParse"/>.
/// </summary>
public struct TypeParserArgs<T> : IDiagnosticProvider where T : IEquatable<T>
{
    /// <summary>
    /// The value node being parsed.
    /// </summary>
    public required IAnyValueSourceNode? ValueNode;
    
    /// <summary>
    /// The parent of the node being parsed.
    /// </summary>
    public required IParentSourceNode ParentNode;

    /// <summary>
    /// The type being parsed.
    /// </summary>
    public required IType<T> Type;

    /// <summary>
    /// Used to report diagnostics encountered when parsing. Ignored if <see langword="null"/>.
    /// </summary>
    public IDiagnosticSink? DiagnosticSink;

    /// <summary>
    /// Used to report undefined properties that are being used by this parser.
    /// </summary>
    public IReferencedPropertySink? ReferencedPropertySink;

    /// <summary>
    /// Set to <see langword="true"/> when a diagnostic is reported by a parser,
    /// meaning the fallback 'failed to parse' diagnostic shouldn't be emitted.
    /// </summary>
    public bool ShouldIgnoreFailureDiagnostic;

    /// <summary>
    /// The filter currently active based on the key used.
    /// </summary>
    public LegacyExpansionFilter KeyFilter;

    /// <summary>
    /// Creates <see cref="TypeParserArgs{TElementType}"/> used to parse sub-values, such as the elements in a list.
    /// </summary>
    /// <typeparam name="TElementType"></typeparam>
    /// <param name="args">Arguments to pass to <see cref="ITypeParser{T}.TryParse"/>.</param>
    /// <param name="valueNode">The node of the value being parsed.</param>
    /// <param name="parentNode">The parent node of the value being parsed.</param>
    /// <param name="type">The type of value being parsed.</param>
    public void CreateSubTypeParserArgs<TElementType>(
        out TypeParserArgs<TElementType> args,
        IAnyValueSourceNode? valueNode,
        IParentSourceNode parentNode,
        IType<TElementType> type,
        LegacyExpansionFilter filter)
        where TElementType : IEquatable<TElementType>
    {
        args.ValueNode = valueNode;
        args.ParentNode = parentNode;
        args.Type = type;
        args.ShouldIgnoreFailureDiagnostic = false;
        args.DiagnosticSink = DiagnosticSink;
        args.ReferencedPropertySink = ReferencedPropertySink;
        args.KeyFilter = filter;
    }

    /// <summary>
    /// Creates <see cref="TypeConverterParseArgs{T}"/> to read from <see cref="ValueNode"/>.
    /// </summary>
    /// <param name="parseArgs">Arguments to pass to <see cref="ITypeConverter{T}.TryParse"/>.</param>
    /// <param name="text">The text being read.</param>
    public void CreateTypeConverterParseArgs(out TypeConverterParseArgs<T> parseArgs, string? text = null)
    {
        parseArgs.Type = Type;
        parseArgs.DiagnosticSink = DiagnosticSink;
        parseArgs.ShouldIgnoreFailureDiagnostic = false;
        parseArgs.ValueRange = ValueNode?.Range ?? ParentNode.Range;
        parseArgs.TextAsString = text;
    }

    /// <summary>
    /// Creates <see cref="TypeConverterParseArgs{T}"/> to read from <see cref="ValueNode"/>.
    /// </summary>
    /// <param name="parseArgs">Arguments to pass to <see cref="ITypeConverter{T}.TryParse"/>.</param>
    /// <param name="text">The text being read.</param>
    public void CreateTypeConverterParseArgsWithoutDiagnostics(out TypeConverterParseArgs<T> parseArgs, string? text = null)
    {
        parseArgs.Type = Type;
        parseArgs.DiagnosticSink = null;
        parseArgs.ShouldIgnoreFailureDiagnostic = false;
        parseArgs.ValueRange = ValueNode?.Range ?? ParentNode.Range;
        parseArgs.TextAsString = text;
    }

    /// <summary>
    /// Creates <see cref="TypeConverterParseArgs{TElementType}"/> to read from <see cref="ValueNode"/> as another type.
    /// </summary>
    /// <param name="parseArgs">Arguments to pass to <see cref="ITypeConverter{TElementType}.TryParse"/>.</param>
    /// <param name="text">The text being read.</param>
    public void CreateTypeConverterParseArgs<TElementType>(out TypeConverterParseArgs<TElementType> parseArgs, IType<TElementType> type, string? text = null)
        where TElementType : IEquatable<TElementType>
    {
        parseArgs.Type = type;
        parseArgs.DiagnosticSink = DiagnosticSink;
        parseArgs.ShouldIgnoreFailureDiagnostic = false;
        parseArgs.ValueRange = ValueNode?.Range ?? ParentNode.Range;
        parseArgs.TextAsString = text;
    }

    /// <summary>
    /// Creates <see cref="TypeConverterParseArgs{TElementType}"/> to read from <see cref="ValueNode"/> as another type.
    /// </summary>
    /// <param name="parseArgs">Arguments to pass to <see cref="ITypeConverter{TElementType}.TryParse"/>.</param>
    /// <param name="text">The text being read.</param>
    public void CreateTypeConverterParseArgsWithoutDiagnostics<TElementType>(out TypeConverterParseArgs<TElementType> parseArgs, IType<TElementType> type, string? text = null)
        where TElementType : IEquatable<TElementType>
    {
        parseArgs.Type = type;
        parseArgs.DiagnosticSink = null;
        parseArgs.ShouldIgnoreFailureDiagnostic = false;
        parseArgs.ValueRange = ValueNode?.Range ?? ParentNode.Range;
        parseArgs.TextAsString = text;
    }

    public FileRange GetRangeAndRegisterDiagnostic()
    {
        ShouldIgnoreFailureDiagnostic = true;
        return ValueNode?.Range ?? ParentNode.Range;
    }
}