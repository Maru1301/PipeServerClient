using PipeServerClient.Service.Interface;

namespace PipeServerClient.Service
{
    internal class FileService : IFileService
    {
        private readonly string _directoryPath;
        private readonly string _vpnIpFilePath;
        private readonly string _gameFilePath;

        public FileService()
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            const string directoryName = "M4Virus";
            _directoryPath = Path.Combine(docPath, directoryName);
            _vpnIpFilePath = Path.Combine(_directoryPath, "VpnIps.txt");
            _gameFilePath = Path.Combine(_directoryPath, "Game.txt");
        }

        public List<string> ReadVpnIps()
        {
            if (!File.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
                File.Create(_vpnIpFilePath).Dispose();
                return [];
            }

            return [.. File.ReadAllLines(_vpnIpFilePath)];
        }

        public List<string> ReadGames()
        {
            if (!File.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
                File.Create(_gameFilePath).Dispose();
            }

            return [.. File.ReadAllLines(_gameFilePath)];
        }

        public void WriteVpnIp(string ip)
        {
            using var writer = new StreamWriter(_vpnIpFilePath, true);
            writer.WriteLine(ip);
        }

        public string GetVpnFilePath() => _vpnIpFilePath;
    }
}
