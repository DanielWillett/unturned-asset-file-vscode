using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types;

public readonly ref struct SpecPropertyTypeParseContext
{
    public AssetFileValueNode? Node { get; }
    public AssetFileNode? Parent { get; }
    public AssetSpecDatabase Database { get; }
    public string? BaseKey { get; }
    public AssetFileType FileType { get; }

    public ICollection<DatDiagnosticMessage>? Diagnostics { get; }

    public bool HasDiagnostics { get; }

    public SpecPropertyTypeParseContext(
        AssetFileValueNode? node,
        AssetFileNode? parent,
        AssetSpecDatabase database,
        ICollection<DatDiagnosticMessage>? diagnostics,
        string? baseKey,
        AssetFileType fileType)
    {
        if (diagnostics != null && diagnostics.IsReadOnly)
            throw new ArgumentException("Diagnostics collection is readonly.", nameof(diagnostics));

        Node = node;
        Parent = parent;
        Database = database;
        Diagnostics = diagnostics;
        BaseKey = baseKey;
        FileType = fileType;
        HasDiagnostics = diagnostics != null;
    }

    public void Log(DatDiagnosticMessage message)
    {
        Diagnostics?.Add(message);
    }
}