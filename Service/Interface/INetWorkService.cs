using System.Net.Sockets;

namespace PipeServerClient.Service.Interface;

public interface INetworkService
{
    Task<NetworkStream> ConnectAsync(string ip);
    Task SendCommandAsync(string message);
    Task DisposeAsync();
}
