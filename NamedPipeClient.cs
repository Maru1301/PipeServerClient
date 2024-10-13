using System.Net.Sockets;
using System.Text;

class NamedPipeClient
{
    static void Main()
    {
        // Connect to the server using its VPN IP address and port
        var client = new TcpClient("26.102.129.179", 5000); // Server's VPN IP address
        NetworkStream stream = client.GetStream();

        // Send a command to start the app
        while (true)
        {
            var message = Console.ReadLine();
            if (string.IsNullOrEmpty(message)) continue;
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);

            // Receive confirmation from the server
            buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("Server response: " + response);
        }
    }
}
