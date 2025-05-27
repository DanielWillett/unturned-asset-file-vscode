using MediatR;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;
using System.Text.Json.Serialization;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;

[Parallel, Method("unturnedDataFile/getDocumentContent", Direction.ServerToClient)]
public class GetDocumentContentParams : IRequest<GetDocumentContentResponse>
{
    [JsonPropertyName("document")]
    public required DocumentUri Document { get; set; }
}

public class GetDocumentContentResponse
{
    [JsonPropertyName("text")]
    public required string? Text { get; set; }
}