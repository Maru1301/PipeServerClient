namespace PipeServerClient
{
    internal class Program
    {
        static async Task Main()
        {
            var client = new NamedPipeClient();

            await client.Go();
        }
    }
}
