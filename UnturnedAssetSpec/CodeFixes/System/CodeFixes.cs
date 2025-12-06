using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DanielWillett.UnturnedDataFileLspServer.Data.CodeFixes;

public sealed class GlobalCodeFixes
{
    // private readonly IFilePropertyVirtualizer _virtualizer;
    // private readonly IAssetSpecDatabase _database;
    // private readonly InstallationEnvironment _installEnv;
    // private readonly IWorkspaceEnvironment _workspaceEnv;

    internal ICodeFix[] AllArray { get; }

    public IReadOnlyList<ICodeFix> All { get; }

    public GlobalCodeFixes(
        IFilePropertyVirtualizer virtualizer,
        IAssetSpecDatabase database,
        InstallationEnvironment installEnv,
        IWorkspaceEnvironment workspaceEnv)
    {
        // _virtualizer = virtualizer;
        // _database = database;
        // _installEnv = installEnv;
        // _workspaceEnv = workspaceEnv;

        AllArray =
        [
            new BlueprintUseThisKeyword(virtualizer, database, installEnv, workspaceEnv),
            new GenerateNewGuid(virtualizer, database, installEnv, workspaceEnv),
            new UnknownProperty(virtualizer, database, installEnv, workspaceEnv),
        ];

        for (int i = 0; i < AllArray.Length; ++i)
        {
            AllArray[i].Index = i;
        }

        All = new ReadOnlyCollection<ICodeFix>(AllArray);
    }
}
