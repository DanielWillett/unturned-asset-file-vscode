using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Properties;
using Microsoft.Extensions.Logging;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// A implementation pattern that provides all services needed to parse files.
/// </summary>
public interface IParsingServices : IServiceProvider
{
    IAssetSpecDatabase Database { get; }

    ILoggerFactory LoggerFactory { get; }

    IWorkspaceEnvironment Workspace { get; }
    
    IFileRelationalModelProvider RelationalModelProvider { get; }
    
    InstallDirUtility GameDirectory { get; }

    InstallationEnvironment Installation { get; }
}