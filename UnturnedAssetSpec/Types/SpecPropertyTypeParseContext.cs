using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public readonly ref struct SpecPropertyTypeParseContext
{
    public readonly FileEvaluationContext EvaluationContext;

    public required AssetFileValueNode? Node { get; init; }
    public required AssetFileNode? Parent { get; init; }
    public required IAssetSpecDatabase Database { get; init; }
    public AssetFileTree? File { get; init; }
    public string? BaseKey { get; init; }
    public required AssetFileType FileType { get; init; }

    public ICollection<DatDiagnosticMessage>? Diagnostics { get; }

    public bool HasDiagnostics { get; }

    public SpecPropertyTypeParseContext(FileEvaluationContext evalContext)
    {
        EvaluationContext = evalContext;
    }

    public SpecPropertyTypeParseContext(ICollection<DatDiagnosticMessage> diagnostics)
    {
        if (diagnostics is { IsReadOnly: true })
            throw new ArgumentException("Diagnostics collection is readonly.", nameof(diagnostics));

        Diagnostics = diagnostics;
        HasDiagnostics = diagnostics != null;
    }

    public void Log(DatDiagnosticMessage message)
    {
        Diagnostics?.Add(message);
    }
}