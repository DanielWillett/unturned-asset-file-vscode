using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using System;
using System.Collections.Generic;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Project;

/// <summary>
/// Provides access to all active project files given the context of a file.
/// </summary>
public interface IProjectFileProvider
{
    /// <summary>
    /// Enumerates project files in hierarchical order (with the most precise one first).
    /// </summary>
    IEnumerable<ProjectFile> EnumerateProjectFiles(IWorkspaceFile? fileContext);

    /// <summary>
    /// Gets a merged orderfile that applies to the given file.
    /// </summary>
    IPropertyOrderFile GetScaffoldedOrderfile(IWorkspaceFile? fileContext);

    /// <summary>
    /// Enumerates project files in hierarchical order (with the most precise one first), choosing the first non-null value.
    /// </summary>
    T? AggregateProjectFiles<T>(IWorkspaceFile? fileContext, Func<ProjectFile, T> selector) where T : class;

    /// <summary>
    /// Enumerates project files in hierarchical order (with the most precise one first), choosing the first non-null value.
    /// </summary>
    T? AggregateProjectFiles<T>(IWorkspaceFile? fileContext, Func<ProjectFile, T?> selector) where T : struct;
}

/// <summary>
/// Implementation of <see cref="IProjectFileProvider"/> that has no project files and exclusively uses the global orderfile.
/// </summary>
public sealed class NilProjectFileProvider : IProjectFileProvider
{
    private readonly IAssetSpecDatabase _database;

    public NilProjectFileProvider(IAssetSpecDatabase database)
    {
        _database = database;
    }

    /// <inheritdoc />
    public IEnumerable<ProjectFile> EnumerateProjectFiles(IWorkspaceFile? fileContext) => Array.Empty<ProjectFile>();

    /// <inheritdoc />
    public IPropertyOrderFile GetScaffoldedOrderfile(IWorkspaceFile? fileContext)
    {
        return _database.GlobalOrderFile;
    }

    /// <inheritdoc />
    public T? AggregateProjectFiles<T>(IWorkspaceFile? fileContext, Func<ProjectFile, T> selector)
        where T : class
    {
        return null;
    }

    /// <inheritdoc />
    public T? AggregateProjectFiles<T>(IWorkspaceFile? fileContext, Func<ProjectFile, T?> selector)
        where T : struct
    {
        return null;
    }
}

/// <summary>
/// Extensions for <see cref="IProjectFileProvider"/>.
/// </summary>
public static class ProjectFileProviderExtensions
{
    extension(IProjectFileProvider provider)
    {
        /// <summary>
        /// Gets the current GUID style given all project files.
        /// </summary>
        public GuidStyle GetGuidStyle(IWorkspaceFile? fileContext)
        {
            return provider.AggregateProjectFiles(fileContext, p => p.GuidStyle) ?? GuidStyle.NormalLower;
        }
    }
}