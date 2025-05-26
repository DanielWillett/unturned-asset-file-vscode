using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Handlers.AssetProperties;


[Parallel, Method("unturnedDataFile/getAddProperty")]
public interface IGetAssetPropertyAddLocationHandler : IJsonRpcRequestHandler<GetAssetPropertyAddLocationParams, GetAssetPropertyAddLocationResponse>;

[Parallel, Method("unturnedDataFile/getAddProperty")]
public class GetAssetPropertyAddLocationParams : IRequest<GetAssetPropertyAddLocationResponse>
{
    [JsonPropertyName("document")]
    public required DocumentUri Document { get; set; }

    [JsonPropertyName("key")]
    public required string Key { get; set; }
}

public class GetAssetPropertyAddLocationResponse
{
    [JsonPropertyName("position")]
    public required Position? Position { get; init; }

    [JsonPropertyName("isFlag")]
    public required bool IsFlag { get; init; }

    [JsonPropertyName("insertLines")]
    public required int InsertLines { get; init; }
}