using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public readonly ref struct SpecPropertyTypeParseContext
{
    public readonly FileEvaluationContext EvaluationContext;

    public required IAnyValueSourceNode? Node { get; init; }
    public required ISourceNode? Parent { get; init; }
    public string? BaseKey { get; init; }

    public IAssetSpecDatabase Database => EvaluationContext.Information;
    public ISourceFile? File => EvaluationContext.SourceFile;
    public AssetFileType FileType => EvaluationContext.FileType;

    public ICollection<DatDiagnosticMessage>? Diagnostics { get; }

    public bool HasDiagnostics { get; }

    public SpecPropertyTypeParseContext WithoutDiagnostics()
    {
        if (!HasDiagnostics)
            return this;

        return new SpecPropertyTypeParseContext(EvaluationContext, null)
        {
            BaseKey = BaseKey,
            Node = Node,
            Parent = Parent
        };
    }

    public SpecPropertyTypeParseContext WithDiagnostics(ICollection<DatDiagnosticMessage> diagnostics)
    {
        return new SpecPropertyTypeParseContext(EvaluationContext, diagnostics)
        {
            BaseKey = BaseKey,
            Node = Node,
            Parent = Parent
        };
    }

    public SpecPropertyTypeParseContext(ICollection<DatDiagnosticMessage> diagnostics) : this(default, diagnostics)
    {

    }

    public SpecPropertyTypeParseContext(FileEvaluationContext evalContext, ICollection<DatDiagnosticMessage>? diagnostics)
    {
        if (diagnostics is { IsReadOnly: true })
            throw new ArgumentException("Diagnostics collection is readonly.", nameof(diagnostics));

        Diagnostics = diagnostics;
        HasDiagnostics = diagnostics != null;
        EvaluationContext = evalContext;
    }

    public static SpecPropertyTypeParseContext FromFileEvaluationContext(FileEvaluationContext evalContext, SpecProperty? property, ISourceNode? parentNode, IAnyValueSourceNode? valueNode, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        return new SpecPropertyTypeParseContext(evalContext, diagnostics)
        {
            Parent = parentNode,
            Node = valueNode,
            BaseKey = property?.Key
        };
    }

    public void Log(DatDiagnosticMessage message)
    {
        if (!HasDiagnostics)
            return;

        try
        {
            Diagnostics!.Add(message);
        }
        catch (Exception ex)
        {
            Database.LogMessage($"Error adding diagnostics message{Environment.NewLine}{ex}");
        }
    }

    public GuidOrId GetThisId()
    {
        if (File is not IAssetSourceFile assetSourceFile)
            return GuidOrId.Empty;

        Guid? guid = assetSourceFile.Guid;
        if (guid.HasValue && guid.Value != Guid.Empty)
            return new GuidOrId(guid.Value);

        ushort? id = assetSourceFile.Id;
        if (id is null or 0)
            return GuidOrId.Empty;

        return new GuidOrId(id.Value, assetSourceFile.Category);
    }
}