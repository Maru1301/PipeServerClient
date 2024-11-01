using PipeServerClient.Service.Interface;

namespace PipeServerClient.Service
{
    internal class FileService : IFileService
    {
        private readonly string _directoryPath;
        private readonly string _filePath;

        public FileService()
        {
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            const string directoryName = "M4Virus";
            _directoryPath = Path.Combine(docPath, directoryName);
            _filePath = Path.Combine(_directoryPath, "VpnIps.txt");
        }

        public List<string> ReadVpnIps()
        {
            if (!File.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
                File.Create(_filePath).Dispose();
                return [];
            }

            return [.. File.ReadAllLines(_filePath)];
        }

        public void WriteVpnIp(string ip)
        {
            using var writer = new StreamWriter(_filePath, true);
            writer.WriteLine(ip);
        }

        public string GetVpnFilePath() => _filePath;
    }
}
