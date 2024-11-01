namespace PipeServerClient.Service.Interface;
public interface IFileService
{
    List<string> ReadVpnIps();
    void WriteVpnIp(string ip);
    string GetVpnFilePath();
}
