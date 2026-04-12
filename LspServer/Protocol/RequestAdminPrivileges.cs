using MediatR;
using Newtonsoft.Json;
using OmniSharp.Extensions.JsonRpc;

namespace DanielWillett.UnturnedDataFileLspServer.Protocol;

[Parallel, Method("unturnedDataFile/requestAdminPrivileges", Direction.ServerToClient)]
public class RequestAdminPrivilegesParams : INotification
{
    [JsonProperty("message")]
    public required string Message { get; set; }

    [JsonProperty("type")]
    public required ushort Type { get; set; }
}

[Parallel, Method("unturnedDataFile/sendAdminPrivilegesResponse", Direction.ClientToServer)]
public class SendAdminPrivilegesResponseParams : INotification
{
    [JsonProperty("allowed")]
    public bool Allowed { get; set; }

    [JsonProperty("type")]
    public ushort Type { get; set; }
}