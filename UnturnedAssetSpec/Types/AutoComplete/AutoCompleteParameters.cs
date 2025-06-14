using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Files;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Types.AutoComplete;

public readonly struct AutoCompleteParameters
{
    public IAssetSpecDatabase Database { get; }
    public AssetFileTree File { get; }
    public FilePosition Position { get; }
    public AssetFileType FileType { get; }
    public SpecProperty Property { get; }
    public IWorkspaceEnvironment Workspace { get; }
    public InstallationEnvironment Environment { get; }

    public AutoCompleteParameters(IAssetSpecDatabase database, AssetFileTree file, FilePosition position, AssetFileType fileType, SpecProperty property, IWorkspaceEnvironment workspace, InstallationEnvironment environment)
    {
        Database = database;
        File = file;
        Position = position;
        FileType = fileType;
        Property = property;
        Workspace = workspace;
        Environment = environment;
    }
}
