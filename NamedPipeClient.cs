using System.IO.Pipes;

class NamedPipeClient
{
    static void Main()
    {
        using (NamedPipeClientStream pipeClient = new("26.102.129.179", "TestPipe", PipeDirection.InOut))
        {
            Console.WriteLine("Connecting to server...");
            pipeClient.Connect();
            Console.WriteLine("Connected to server.");

            using (StreamReader reader = new StreamReader(pipeClient))
            {
                string message = reader.ReadLine();
                Console.WriteLine("Received: " + message);
            }
        }
    }
}
