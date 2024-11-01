namespace PipeServerClient.Service.Interface;
public interface IFileService
{
    List<string> ReadVpnIps();
    List<string> ReadGames();
    void WriteVpnIp(string ip);
    string GetVpnFilePath();
}
