using DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;
using MediatR;
using Newtonsoft.Json;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;

[Parallel, Method("unturnedDataFile/assetBundleAssets", Direction.ClientToServer)]
public interface IDiscoverBundleAssetsHandler : IJsonRpcRequestHandler<DiscoverBundleAssetsParams, Container<BundleAssetInfo>>;

[Parallel, Method("unturnedDataFile/assetBundleAssets", Direction.ClientToServer)]
public class DiscoverBundleAssetsParams : IRequest<Container<BundleAssetInfo>>
{
    [JsonProperty("document")]
    public required DocumentUri Document { get; set; }

    /// <summary>
    /// Path to the property to get children for.
    /// </summary>
    [JsonProperty("path")]
    public string? Path { get; set; }
}