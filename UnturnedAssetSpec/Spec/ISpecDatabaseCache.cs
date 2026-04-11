using System.Threading;
using System.Threading.Tasks;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public interface ISpecDatabaseCache
{
    string? RootDirectory { get; }

    Task CacheNewFilesAsync(IAssetSpecDatabase database, CancellationToken token = default);
}