using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System.Diagnostics.CodeAnalysis;
using DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public readonly struct FileEvaluationContext
{
    internal static readonly FileEvaluationContext None = default;

    public readonly DatProperty Self;
    public readonly DatTypeWithProperties This;
    public readonly ISourceFile SourceFile;
    public readonly AssetFileType FileType;
    public readonly IWorkspaceEnvironment Workspace;
    public readonly InstallationEnvironment Environment;
    public readonly IAssetSpecDatabase Information;

    // todo:
    public readonly PropertyResolutionContext PropertyContext;
    public readonly OneOrMore<int> TemplateIndices;

    // todo:
    public readonly INestedObjectContext? CurrentObject;

    public FileEvaluationContext(DatProperty self, ISourceFile file, IWorkspaceEnvironment workspace, InstallationEnvironment environment, IAssetSpecDatabase information, PropertyResolutionContext propertyContext)
        : this(self, self.Owner, file, workspace, environment, information, propertyContext)
    {
        TemplateIndices = OneOrMore<int>.Null;
    }
    public FileEvaluationContext(DatProperty self, DatTypeWithProperties @this, ISourceFile file, IWorkspaceEnvironment workspace, InstallationEnvironment environment, IAssetSpecDatabase information, PropertyResolutionContext propertyContext)
    {
        Self = self;
        This = @this;
        SourceFile = file;
        FileType = AssetFileType.FromFile(file, information);
        Workspace = workspace;
        Environment = environment;
        Information = information;
        PropertyContext = propertyContext;
        TemplateIndices = OneOrMore<int>.Null;
    }

    public FileEvaluationContext(in FileEvaluationContext self, DatProperty newProperty, PropertyResolutionContext propertyContext)
    {
        Self = newProperty;
        PropertyContext = propertyContext;
        This = newProperty.Owner;
        SourceFile = self.SourceFile;
        FileType = self.FileType;
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
        TemplateIndices = OneOrMore<int>.Null;
    }

    public FileEvaluationContext(in FileEvaluationContext self, IWorkspaceFile openedFile)
    {
        PropertyContext = PropertyResolutionContext.Modern;
        Self = null!;
        This = null!;
        SourceFile = openedFile.SourceFile;
        FileType = AssetFileType.FromFile(SourceFile, self.Information);
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
        TemplateIndices = OneOrMore<int>.Null;
    }

    public bool TryGetRelevantMap([NotNullWhen(true)] out RelevantMapInfo? mapInfo)
    {
        // todo
        mapInfo = null;
        return false;
    }
}

public class RelevantMapInfo;