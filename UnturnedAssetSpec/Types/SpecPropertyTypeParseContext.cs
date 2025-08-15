using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
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

    public static SpecPropertyTypeParseContext FromFileEvaluationContext(FileEvaluationContext evalContext, SpecProperty? property, AssetFileNode? parentNode, AssetFileValueNode? valueNode, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        return new SpecPropertyTypeParseContext(evalContext, diagnostics)
        {
            Parent = parentNode,
            Node = valueNode,
            Database = evalContext.Information,
            FileType = evalContext.FileType,
            BaseKey = property?.Key,
            File = evalContext.File
        };
    }

    public void Log(DatDiagnosticMessage message)
    {
        Diagnostics?.Add(message);
    }

    public GuidOrId GetThisId()
    {
        if (File == null)
            return GuidOrId.Empty;

        Guid? guid = File.GetGuid();
        if (guid.HasValue && guid.Value != Guid.Empty)
            return new GuidOrId(guid.Value);

        ushort? id = File.GetId();
        if (id is null or 0)
            return GuidOrId.Empty;

        EnumSpecTypeValue category = File.GetCategory(Database);
        return new GuidOrId(id.Value, in category);
    }
}