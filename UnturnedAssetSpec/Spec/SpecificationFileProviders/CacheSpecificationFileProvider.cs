using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

/// <summary>
/// Pulls specification files from a <see cref="ISpecDatabaseCache"/>.
/// </summary>
public sealed class CacheSpecificationFileProvider : ISpecificationFileProvider
{
    private readonly ISpecDatabaseCache? _cache;
    private readonly string? _latestCommit;

    public int Priority => 2;
    public bool IsEnabled => _cache != null && _latestCommit != null && _cache.IsUpToDateCache(_latestCommit);

    public CacheSpecificationFileProvider(ISpecDatabaseCache? cache, string? latestCommit)
    {
        _cache = cache;
        _latestCommit = latestCommit;
    }

    public Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token)
    {
        return _cache?.ReadAssetAsync(type, state, action, token) ?? Task.FromResult(false);
    }

    public Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default)
    {
        return _cache?.ReadKnownFileAsync(file, state, action, token) ?? Task.FromResult(false);
    }
}