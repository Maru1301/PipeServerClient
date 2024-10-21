using System.Net.Sockets;
using System.Text;

namespace PipeServerClient;

public class TcpClientManager(string serverIp, int serverPort = 5000)
{
    private readonly string _serverIp = serverIp;
    private readonly int _serverPort = serverPort;
    private readonly TcpClient _client = new();
    private NetworkStream? _networkStream;

    // Connects to the server and returns the network stream
    public async Task<bool> ConnectAsync()
    {
        try
        {
            // Connect to the server using the specified IP and port
            await _client.ConnectAsync(_serverIp, _serverPort);
            _networkStream = _client.GetStream();
            Console.WriteLine("Connected to server.");
            return true;
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Failed to connect: {ex.Message}");
            return false;
        }
    }

    // Sends a command to the server and reads the response
    public async Task<string?> SendCommandAsync(string command)
    {
        if (_networkStream == null || !_client.Connected)
        {
            Console.WriteLine("Not connected to server.");
            return null;
        }

        try
        {
            // Send command to the server
            byte[] buffer = Encoding.ASCII.GetBytes(command);
            await _networkStream.WriteAsync(buffer);
            Console.WriteLine($"Sent command: {command}");

            // Read response from the server
            buffer = new byte[1024];
            int bytesRead = await _networkStream.ReadAsync(buffer);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Received response: {response}");

            return response;
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Network error: {ex.Message}");
            return null;
        }
    }

    // Disconnects from the server
    public void Disconnect()
    {
        if (_client != null && _client.Connected)
        {
            _client.Close();
            _networkStream?.Close();
            Console.WriteLine("Disconnected from server.");
        }
    }
}
