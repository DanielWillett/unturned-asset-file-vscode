using DanielWillett.UnturnedDataFileLspServer.Data.AssetEnvironment;
using DanielWillett.UnturnedDataFileLspServer.Data.Spec;
using DanielWillett.UnturnedDataFileLspServer.Data.Utility;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Files;

/// <summary>
/// Basic implementation of <see cref="IWorkspaceEnvironment"/> that provides <see cref="StaticSourceFile"/> for assets.
/// </summary>
public class StaticSourceFileWorkspaceEnvironment : IWorkspaceEnvironment, IDisposable
{
    private readonly IAssetSpecDatabase _database;
    private readonly SourceNodeTokenizerOptions _defaultSourceOptions;
    private InstallationEnvironment? _installationEnvironment;
    private readonly ConcurrentDictionary<string, StaticSourceFile>? _cache;

    public StaticSourceFileWorkspaceEnvironment(
        bool useCache,
        IAssetSpecDatabase database,
        SourceNodeTokenizerOptions defaultSourceOptions = SourceNodeTokenizerOptions.Lazy,
        InstallationEnvironment? installationEnvironment = null)
    {
        _database = database;
        _defaultSourceOptions = defaultSourceOptions;
        _cache = useCache ? new ConcurrentDictionary<string, StaticSourceFile>(OSPathHelper.PathComparer) : null;

        if (useCache && installationEnvironment != null)
        {
            _installationEnvironment = installationEnvironment;
            installationEnvironment.OnFileUpdated += OnFileUpdated;
            installationEnvironment.OnFileRemoved += OnFileRemoved;
        }
    }

    public void Dispose()
    {
        InstallationEnvironment? env = Interlocked.Exchange(ref _installationEnvironment, null);
        if (env == null)
            return;

        env.OnFileUpdated -= OnFileUpdated;
        env.OnFileRemoved -= OnFileRemoved;
    }

    private void OnFileRemoved(DiscoveredDatFile file)
    {
        _cache?.TryRemove(file.FilePath, out _);
    }

    private void OnFileUpdated(DiscoveredDatFile oldFile, DiscoveredDatFile newFile)
    {
        _cache?.TryRemove(oldFile.FilePath, out _);
    }

    /// <inheritdoc />
    public IWorkspaceFile? TemporarilyGetOrLoadFile(string filePath)
    {
        if (_cache == null)
        {
            return StaticSourceFile.FromAssetFile(filePath, _database, _defaultSourceOptions);
        }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET472_OR_GREATER
        StaticSourceFile file = _cache.GetOrAdd(
            filePath,
            static (filePath, env) => StaticSourceFile.FromAssetFile(filePath, env._database, env._defaultSourceOptions),
            this
        );
#else
        StaticSourceFile file = _cache.GetOrAdd(
            filePath,
            filePath => StaticSourceFile.FromAssetFile(filePath, _database, _defaultSourceOptions)
        );
#endif
        return file;
    }
}