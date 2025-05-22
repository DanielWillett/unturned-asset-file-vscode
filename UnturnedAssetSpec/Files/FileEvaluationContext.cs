using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

public readonly struct FileEvaluationContext
{
    public readonly SpecProperty Self;
    public readonly ISpecType This;
    public readonly AssetFileTree File;
    public readonly AssetFileType FileType;
    public readonly IWorkspaceEnvironment Workspace;
    public readonly InstallationEnvironment Environment;
    public readonly IAssetSpecDatabase Information;

    public FileEvaluationContext(SpecProperty self, ISpecType @this, AssetFileTree file, IWorkspaceEnvironment workspace, InstallationEnvironment environment, IAssetSpecDatabase information)
    {
        Self = self;
        This = @this;
        File = file;
        FileType = AssetFileType.FromFile(file, information);
        Workspace = workspace;
        Environment = environment;
        Information = information;
    }

    public FileEvaluationContext(in FileEvaluationContext self, SpecProperty newProperty)
    {
        Self = newProperty;
        This = newProperty.Owner;
        File = self.File;
        FileType = self.FileType;
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
    }

    public FileEvaluationContext(in FileEvaluationContext self, AssetFileTree file)
    {
        Self = null!;
        This = null!;
        File = file;
        FileType = AssetFileType.FromFile(file, self.Information);
        Workspace = self.Workspace;
        Environment = self.Environment;
        Information = self.Information;
    }
}