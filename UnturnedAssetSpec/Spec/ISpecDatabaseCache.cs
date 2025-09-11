using System.Threading;
using System.Threading.Tasks;
using DanielWillett.UnturnedDataFileLspServer.Data.Types;

namespace DanielWillett.UnturnedDataFileLspServer.Data.Spec;

public interface ISpecDatabaseCache
{
    bool IsUpToDateCache(string latestCommit);
    Task CacheNewFilesAsync(IAssetSpecDatabase database, CancellationToken token = default);

    Task<AssetInformation?> GetCachedInformationAsync(CancellationToken token = default);
    Task<AssetSpecType?> GetCachedTypeAsync(QualifiedType type, CancellationToken token = default);
}
