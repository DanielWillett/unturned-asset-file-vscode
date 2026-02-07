using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using Microsoft.Extensions.Logging;
using System;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Parsing;

/// <summary>
/// Default implementation of <see cref="IParsingServices"/>.
/// </summary>
public class ParsingServiceProvider : IParsingServices, IDisposable
{
    /// <summary>
    /// The service provider given in <see cref="ParsingServiceProvider(IServiceProvider)"/>, or <see langword="null"/> if one was not provided.
    /// </summary>
    protected readonly IServiceProvider? ServiceProvider;

    /// <inheritdoc />
    public IAssetSpecDatabase Database { get; }

    /// <inheritdoc />
    public ILoggerFactory LoggerFactory { get; }

    /// <inheritdoc />
    public IWorkspaceEnvironment Workspace { get; }

    /// <inheritdoc />
    public InstallDirUtility GameDirectory { get; }

    /// <inheritdoc />
    public InstallationEnvironment Installation { get; }

    /// <summary>
    /// Create an <see cref="IParsingServices"/> from the given services.
    /// </summary>
    /// <exception cref="ArgumentNullException"/>
    public ParsingServiceProvider(
        IAssetSpecDatabase database,
        ILoggerFactory loggerFactory,
        IWorkspaceEnvironment workspace,
        InstallDirUtility installDirUtility,
        InstallationEnvironment installation)
    {
        Database      = database          ?? throw new ArgumentNullException(nameof(database));
        LoggerFactory = loggerFactory     ?? throw new ArgumentNullException(nameof(loggerFactory));
        Workspace     = workspace         ?? throw new ArgumentNullException(nameof(workspace));
        GameDirectory = installDirUtility ?? throw new ArgumentNullException(nameof(installDirUtility));
        Installation  = installation      ?? throw new ArgumentNullException(nameof(installation));
    }

    /// <summary>
    /// Create an <see cref="IParsingServices"/> from an <see cref="IServiceProvider"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Service not found.</exception>
    /// <exception cref="ArgumentNullException"/>
    public ParsingServiceProvider(IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
            throw new ArgumentNullException(nameof(serviceProvider));

        Database =
            (IAssetSpecDatabase?)serviceProvider.GetService(typeof(IAssetSpecDatabase))
                ?? throw new InvalidOperationException($"Service not found: {nameof(IAssetSpecDatabase)}.");
        LoggerFactory =
            (ILoggerFactory?)serviceProvider.GetService(typeof(ILoggerFactory))
                ?? throw new InvalidOperationException($"Service not found: {nameof(ILoggerFactory)}.");
        Workspace =
            (IWorkspaceEnvironment?)serviceProvider.GetService(typeof(IWorkspaceEnvironment))
                ?? throw new InvalidOperationException($"Service not found: {nameof(IWorkspaceEnvironment)}.");
        GameDirectory =
            (InstallDirUtility?)serviceProvider.GetService(typeof(InstallDirUtility))
                ?? throw new InvalidOperationException($"Service not found: {nameof(InstallDirUtility)}.");
        Installation =
            (InstallationEnvironment?)serviceProvider.GetService(typeof(InstallationEnvironment))
                ?? throw new InvalidOperationException($"Service not found: {nameof(InstallationEnvironment)}.");

        ServiceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public virtual object? GetService(Type serviceType)
    {
        if (ServiceProvider == null)
            return GetServiceByType(serviceType);
        
        if (serviceType == typeof(IParsingServices) || serviceType == typeof(IServiceProvider) || serviceType == typeof(ParsingServiceProvider))
            return this;

        return ServiceProvider.GetService(serviceType);
    }

    /// <summary>
    /// Fallback if service provider is not provided, usually by calling <see cref="ParsingServiceProvider(IAssetSpecDatabase,ILoggerFactory,IWorkspaceEnvironment,InstallDirUtility,InstallationEnvironment)"/>.
    /// </summary>
    /// <inheritdoc cref="IServiceProvider.GetService"/>
    protected virtual object? GetServiceByType(Type serviceType)
    {
        if (typeof(IAssetSpecDatabase) == serviceType)
            return Database;

        if (typeof(ILoggerFactory) == serviceType)
            return LoggerFactory;

        if (typeof(IWorkspaceEnvironment) == serviceType)
            return Workspace;

        if (typeof(InstallDirUtility) == serviceType)
            return GameDirectory;

        if (typeof(InstallationEnvironment) == serviceType)
            return Installation;

        if (serviceType == typeof(IParsingServices) || serviceType == typeof(IServiceProvider) || serviceType == typeof(ParsingServiceProvider))
            return this;

        if (serviceType is { IsConstructedGenericType: true, IsInterface: true } && serviceType.GetGenericTypeDefinition() == typeof(ILogger<>))
        {
            Type loggerType = serviceType.GetGenericArguments()[0];
            return LoggerFactory.CreateLogger(loggerType);
        }

        return null;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (ServiceProvider != null)
            return;

        (Database as IDisposable)?.Dispose();
        LoggerFactory.Dispose();
        (Workspace as IDisposable)?.Dispose();
        (GameDirectory as IDisposable)?.Dispose();
        Installation.Dispose();
    }
}
