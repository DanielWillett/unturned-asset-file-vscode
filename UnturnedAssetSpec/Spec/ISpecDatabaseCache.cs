using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public interface ISpecDatabaseCache
{
    bool IsUpToDateCache(string latestCommit);
    Task CacheNewFilesAsync(IAssetSpecDatabase database, CancellationToken token = default);

    Task<bool> ReadAssetAsync<TState>(QualifiedType type, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);
    Task<bool> ReadKnownFileAsync<TState>(KnownConfigurationFile file, TState state, Func<Stream, TState, CancellationToken, Task> action, CancellationToken token = default);
}