using System;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Collections.Generic;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public readonly struct FileEvaluationContext
{
    public readonly SpecProperty Self;
    public readonly ISpecType This;
    public readonly AssetFileTree File;
    public readonly AssetFileType FileType;
    public readonly IWorkspaceFile OpenedFile;
    public readonly IWorkspaceEnvironment Workspace;
    public readonly InstallationEnvironment Environment;
    public readonly IAssetSpecDatabase Information;

    public FileEvaluationContext(SpecProperty self, ISpecType @this, AssetFileTree file, IWorkspaceEnvironment workspace, InstallationEnvironment environment, IAssetSpecDatabase information, IWorkspaceFile openedFile)
    {
        Self = self;
        This = @this;
        File = file;
        FileType = AssetFileType.FromFile(file, information);
        Workspace = workspace;
        Environment = environment;
        Information = information;
        OpenedFile = openedFile;
    }

    public FileEvaluationContext(in FileEvaluationContext self, SpecProperty newProperty)
    {
        Self = newProperty;
        OpenedFile = self.OpenedFile;
        This = newProperty.Owner;
        File = self.File;
        FileType = self.FileType;
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
    }

    public FileEvaluationContext(in FileEvaluationContext self, AssetFileTree file, IWorkspaceFile openedFile)
    {
        Self = null!;
        This = null!;
        File = file;
        OpenedFile = openedFile;
        FileType = AssetFileType.FromFile(file, self.Information);
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
    }

    public bool TryGetValue(out ISpecDynamicValue? value, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        return TryGetValue(out value, out _, diagnostics);
    }

    public bool TryGetValue(out ISpecDynamicValue value, out AssetFileKeyValuePairNode? node, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        if (diagnostics is { IsReadOnly: true })
            throw new ArgumentException("Diagnostics collection is readonly.", nameof(diagnostics));

        if (!File.TryGetProperty(Self, out node))
        {
            value = Self.DefaultValue!;
            return value != null;
        }

        SpecPropertyTypeParseContext parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(this, Self, node, node.Value, diagnostics);
            
        if (Self.Type.TryParseValue(in parse, out value))
        {
            return true;
        }

        value = Self.IncludedDefaultValue ?? Self.DefaultValue!;
        return value != null;
    }
}