using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;


[Parallel, Method("unturnedDataFile/getAddProperty", Direction.ClientToServer)]
public interface IGetAssetPropertyAddLocationHandler : IJsonRpcRequestHandler<GetAssetPropertyAddLocationParams, GetAssetPropertyAddLocationResponse>;

[Parallel, Method("unturnedDataFile/getAddProperty", Direction.ClientToServer)]
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