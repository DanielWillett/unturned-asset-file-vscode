using DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;
using MediatR;
using Newtonsoft.Json;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;

[Parallel, Method("unturnedDataFile/assetProperties", Direction.ClientToServer)]
public interface IDiscoverAssetPropertiesHandler : IJsonRpcRequestHandler<DiscoverAssetPropertiesParams, Container<AssetProperty>>;

[Parallel, Method("unturnedDataFile/assetProperties", Direction.ClientToServer)]
public class DiscoverAssetPropertiesParams : IRequest<Container<AssetProperty>>
{
    [JsonProperty("document")]
    public required DocumentUri Document { get; set; }
}