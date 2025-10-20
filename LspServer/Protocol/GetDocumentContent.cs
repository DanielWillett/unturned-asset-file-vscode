using MediatR;
using Newtonsoft.Json;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;

[Parallel, Method("unturnedDataFile/getDocumentContent", Direction.ServerToClient)]
public class GetDocumentContentParams : IRequest<GetDocumentContentResponse>
{
    [JsonProperty("document")]
    public required DocumentUri Document { get; set; }
}

public class GetDocumentContentResponse
{
    [JsonProperty("text")]
    public required string? Text { get; set; }

    [JsonProperty("version")]
    public int? Version { get; set; }
}