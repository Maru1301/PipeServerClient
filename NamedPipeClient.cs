using MenuVisualizer;
using MenuVisualizer.Model;
using MenuVisualizer.Model.Interface;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

class NamedPipeClient
{
    static async Task Main()
    {
        var menu = Intialize();

        var manager = new ConsoleMenuManager();

        manager.Construct(menu);

        await manager.ShowAsync();
    }

    private static Menu Intialize()
    {
        FileStream fileStream;
        string docPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string directoryName = "M4Virus";
        string textFileName = "VpnIps.txt";
        var directoryPath = Path.Combine(docPath, directoryName);
        var combinedPath = Path.Combine(directoryPath, textFileName);
        if (!File.Exists(combinedPath))
        {
            Directory.CreateDirectory(directoryPath);
            fileStream = File.Create(combinedPath);
        }
        else
        {
            fileStream = new FileStream(combinedPath, FileMode.Open, FileAccess.ReadWrite);
        }

        var vpnList = new List<string>();
        const int BufferSize = 128;
        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
        {
            string? line;
            while ((line = streamReader.ReadLine()) != null)
            {
                vpnList.Add(line);
            }
        }

        var afterConMenu = new Menu()
        {
            Name = "Server Command",
            Options =
            [
                new FunctionOption()
                {
                    Name = "Start Server",
                    Func = async (object? obj) => await Start(obj),
                },
                new FunctionOption()
                {
                    Name = "Check Server",
                    Func = async (object? obj) => await Check(obj),
                },
                new FunctionOption()
                {
                    Name = "Stop Server",
                    Func = async (object? obj) => await Stop(obj),
                },
                new SubMenuOption()
                {
                    Name = "Go Back",
                }
            ]
        };

        IOption firstOption = CreateFirstOption(vpnList, combinedPath, afterConMenu);

        var menu = new Menu()
        {
            Name = "==ValhiemServer==",
            Options = 
            [
                firstOption,
                new FunctionOption()
                {
                    Name = "Exit",
                    Func = async (object? o) =>  
                    {
                        await Task.Yield();
                        return (object)OptionDefault.Exit; 
                    }
                }
            ]
        };

        ((SubMenuOption)afterConMenu.Options.First(option => option.Name == "Go Back")).SubMenu = menu;

        return menu;
    }

    private static IOption CreateFirstOption(List<string> vpnList, string combinedPath, Menu afterConMenu)
    {
        IOption option;

        if (vpnList.Count > 0)
        {
            var ops = vpnList.Select(vpn => new FunctionOption()
            {
                Name = vpn,
                Func = async (object? o) => await GetNetworkStream(combinedPath),
                AfterFuncSubMenu = afterConMenu
            }).ToList();

            var vpnIpMenu = new Menu()
            {
                Name = "Choose an Ip",
                Options =
                [
                    ..ops,
                    new FunctionOption()
                    {
                        Name = "Other Ip",
                        Func = async (object? o) => await GetNetworkStream(combinedPath),
                        AfterFuncSubMenu = afterConMenu
                    }
                ]
            };

            option = new SubMenuOption()
            {
                Name = "Start Connection",
                SubMenu = vpnIpMenu
            };
        }
        else
        {
            option = new FunctionOption()
            {
                Name = "Start Connection",
                Func = async (object? o) => await GetNetworkStream(combinedPath),
                AfterFuncSubMenu = afterConMenu
            };
        }

        return option;
    }

    private static async Task<NetworkStream> GetNetworkStream(string combinedPath)
    {
        await Task.Yield();
        Console.Clear();

        bool isConnect = false;

        TcpClient client;
        while (!isConnect)
        {
            try
            {
                Console.WriteLine("Enter Target IP");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;
                var regex = new Regex("^((25[0-5]|(2[0-4]|1\\d|[1-9]|)\\d)\\.?\\b){4}$");

                if (!regex.IsMatch(input))
                {
                    Console.WriteLine("Wrong ip format!");
                }

                // Connect to the server using its VPN IP address and port
                client = new TcpClient(input, 5000); // Server's VPN IP address
                
                NetworkStream stream = client.GetStream();

                isConnect = true;
                using var outputFile = new StreamWriter(combinedPath);
                outputFile.Write(input); // Write input as text

                return stream;
            }
            catch (Exception)
            {
                Console.WriteLine("要嘛你IP打錯，要嘛我程式寫錯。");
            }
        }

        return null;
    }

    private static async Task<NetworkStream> Start(object? obj)
    {
        NetworkStream stream = (NetworkStream)obj!;
        // Send a command to start the app
        var message = "start";
        await SendCommand(stream, message);

        return stream;
    }

    private static async Task<NetworkStream> Check(object? obj)
    {
        NetworkStream stream = (NetworkStream)obj!;
        // Send a command to start the app
        var message = "check";
        await SendCommand(stream, message);

        return stream;
    }

    private static async Task<object?> Stop(object? obj)
    {
        NetworkStream stream = (NetworkStream)obj!;
        // Send a command to start the app
        var message = "stop";
        await SendCommand(stream, message);

        return stream;
    }

    private static async Task SendCommand(NetworkStream stream, string message)
    {
        if (string.IsNullOrEmpty(message)) return;
        byte[] buffer = Encoding.ASCII.GetBytes(message);
        await stream.WriteAsync(buffer);

        // Receive confirmation from the server
        buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
        Console.WriteLine("Server response: " + response);
    }
}
