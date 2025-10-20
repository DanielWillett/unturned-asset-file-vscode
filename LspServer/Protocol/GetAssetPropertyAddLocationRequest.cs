using MediatR;
using Newtonsoft.Json;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;


[Parallel, Method("unturnedDataFile/getAddProperty", Direction.ClientToServer)]
public interface IGetAssetPropertyAddLocationHandler : IJsonRpcRequestHandler<GetAssetPropertyAddLocationParams, GetAssetPropertyAddLocationResponse>;

[Parallel, Method("unturnedDataFile/getAddProperty", Direction.ClientToServer)]
public class GetAssetPropertyAddLocationParams : IRequest<GetAssetPropertyAddLocationResponse>
{
    [JsonProperty("document")]
    public required DocumentUri Document { get; set; }

    [JsonProperty("key")]
    public required string Key { get; set; }
}

public class GetAssetPropertyAddLocationResponse
{
    [JsonProperty("position")]
    public required Position? Position { get; init; }

    [JsonProperty("isFlag")]
    public required bool IsFlag { get; init; }

    [JsonProperty("insertLines")]
    public required int InsertLines { get; init; }
}