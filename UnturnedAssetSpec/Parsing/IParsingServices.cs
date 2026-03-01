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

public static class ParsingServicesExtensions
{
    extension(IParsingServices parsingServices)
    {
        public ILogger<T> CreateLogger<T>() where T : notnull
        {
            return parsingServices.LoggerFactory.CreateLogger<T>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return parsingServices.LoggerFactory.CreateLogger(categoryName);
        }

        public ILogger CreateLogger(Type type)
        {
            return parsingServices.LoggerFactory.CreateLogger(type);
        }
    }
}