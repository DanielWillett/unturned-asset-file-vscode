using System;
using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public readonly struct FileEvaluationContext
{
    public readonly SpecProperty Self;
    public readonly ISpecType This;
    public readonly ISourceFile SourceFile;
    public readonly AssetFileType FileType;
    public readonly IWorkspaceEnvironment Workspace;
    public readonly InstallationEnvironment Environment;
    public readonly IAssetSpecDatabase Information;

    public FileEvaluationContext(SpecProperty self, ISpecType @this, ISourceFile file, IWorkspaceEnvironment workspace, InstallationEnvironment environment, IAssetSpecDatabase information)
    {
        Self = self;
        This = @this;
        SourceFile = file;
        FileType = AssetFileType.FromFile(file, information);
        Workspace = workspace;
        Environment = environment;
        Information = information;
    }

    public FileEvaluationContext(in FileEvaluationContext self, SpecProperty newProperty)
    {
        Self = newProperty;
        This = newProperty.Owner;
        SourceFile = self.SourceFile;
        FileType = self.FileType;
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
    }

    public FileEvaluationContext(in FileEvaluationContext self, IWorkspaceFile openedFile)
    {
        Self = null!;
        This = null!;
        SourceFile = openedFile.SourceFile;
        FileType = AssetFileType.FromFile(SourceFile, self.Information);
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
    }

    public bool TryGetValue(out ISpecDynamicValue? value, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        return TryGetValue(out value, out _, diagnostics);
    }

    public bool TryGetValue([MaybeNullWhen(false)] out ISpecDynamicValue value, out IPropertySourceNode? property, ICollection<DatDiagnosticMessage>? diagnostics = null)
    {
        if (diagnostics is { IsReadOnly: true })
            throw new ArgumentException("Diagnostics collection is readonly.", nameof(diagnostics));

        if (!SourceFile.TryResolveProperty(Self, out property))
        {
            value = Self.DefaultValue!;
            return value != null;
        }

        SpecPropertyTypeParseContext parse = SpecPropertyTypeParseContext.FromFileEvaluationContext(this, Self, property, property.Value, diagnostics);

        if (Self.Type.TryParseValue(in parse, out value))
        {
            return true;
        }

        value = Self.IncludedDefaultValue ?? Self.DefaultValue!;
        return value != null;
    }
}