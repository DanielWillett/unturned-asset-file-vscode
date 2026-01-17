using DanielWillett.UnturnedDataFileLspServer.Data.Diagnostics;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

internal struct LazySource
{
    internal ReadOnlyMemory<char> Segment { get; }
    internal SourceValueType ExpectedType { get; }
    internal object? CachedNode { get; set; }
    internal SourceNodeTokenizerOptions Options { get; set; }
    internal long PositionOffset { get; set; }
    internal long IndexDepthOffset { get; set; }

    internal LazySource(object? cachedNode, SourceValueType expectedType)
    {
        CachedNode = cachedNode;
        ExpectedType = expectedType;
    }

    internal LazySource(ReadOnlyMemory<char> segment, SourceValueType expectedType)
    {
        Segment = segment;
        ExpectedType = expectedType;
    }

    private void GetTokenizer(IDiagnosticSink? diagnosticSink, out SourceNodeTokenizer tokenizer)
    {
        unchecked
        {
            tokenizer = new SourceNodeTokenizer(
                Segment,
                Options,
                new FilePosition((int)(PositionOffset >> 32), (int)PositionOffset),
                (int)(IndexDepthOffset >> 32),
                (int)IndexDepthOffset,
                diagnosticSink
            );
        }
    }

    internal ISourceNode? GetNode(IDiagnosticSink? diagnosticSink)
    {
        if (CachedNode != null)
            return (ISourceNode)CachedNode;

        if (Segment.IsEmpty)
            return null;

        GetTokenizer(diagnosticSink, out SourceNodeTokenizer tokenizer);
        try
        {
            ISourceNode? node = tokenizer.ParseValue(false);
            if (node is not IAnyValueSourceNode av || av.ValueType != ExpectedType)
                return null;

            return node;
        }
        finally
        {
            tokenizer.Dispose();
        }
    }
}
