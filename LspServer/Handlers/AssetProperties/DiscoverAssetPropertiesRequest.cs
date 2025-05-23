using System.Text.Json.Serialization;
using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;

[Parallel, Method("unturnedDataFile/assetProperties")]
public interface IDiscoverAssetPropertiesHandler : IJsonRpcRequestHandler<DiscoverAssetPropertiesParams, Container<AssetProperty>>;

[Parallel, Method("unturnedDataFile/assetProperties")]
public class DiscoverAssetPropertiesParams : IRequest<Container<AssetProperty>>
{
    [JsonPropertyName("document")]
    public required DocumentUri Document { get; set; }
}