using PipeServerClient.Service.Interface;
using System.Net.Sockets;
using System.Text;


namespace PipeServerClient.Service
{
    public class NetworkService : INetworkService
    {
        private NetworkStream? networkStream = null;
        public async Task<NetworkStream> ConnectAsync(string ip)
        {
            await Task.Yield();

            var client = new TcpClient(ip, 5000);
            networkStream = client.GetStream();

            return client.GetStream();
        }

        public async Task SendCommandAsync(string message)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            if(networkStream != null)
            {
                await networkStream.WriteAsync(buffer);
            }
        }

        public async Task DisposeAsync()
        {
            if (networkStream != null)
            {
                await networkStream.DisposeAsync();
            }
        }
    }
}
