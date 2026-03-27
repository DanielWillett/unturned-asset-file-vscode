using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using DanielWillett.UnturnedDataFileLspServer.Data.Project;
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

    IProjectFileProvider ProjectFileProvider { get; }
}

/// <summary>
/// Extensions for <see cref="IParsingServices"/>.
/// </summary>
public static class ParsingServicesExtensions
{
    extension(IParsingServices parsingServices)
    {
        /// <inheritdoc cref="LoggerFactoryExtensions.CreateLogger{T}(ILoggerFactory)"/>
        public ILogger<T> CreateLogger<T>() where T : notnull
        {
            return parsingServices.LoggerFactory.CreateLogger<T>();
        }

        /// <inheritdoc cref="ILoggerFactory.CreateLogger(string)"/>
        public ILogger CreateLogger(string categoryName)
        {
            return parsingServices.LoggerFactory.CreateLogger(categoryName);
        }

        /// <inheritdoc cref="LoggerFactoryExtensions.CreateLogger(ILoggerFactory,Type)"/>
        public ILogger CreateLogger(Type type)
        {
            return parsingServices.LoggerFactory.CreateLogger(type);
        }
    }
}