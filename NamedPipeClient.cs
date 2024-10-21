using MenuVisualizer;
using MenuVisualizer.Model;
using MenuVisualizer.Model.Interface;
using PipeServerClient;
using System.Text;

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
                new FunctionOption()
                {
                    Name = "Go Back",
                    Func = async (object? obj) => 
                    {
                        await Task.Yield();
                        TcpClientManager tcpClientManager = (TcpClientManager)obj!;
                        tcpClientManager.Disconnect();

                        return null;
                    }
                }
            ]
        };

        afterConMenu.Options.ForEach(option =>
        {
            if (option is FunctionOption functionOption)
            {
                functionOption.AfterFuncSubMenu = afterConMenu;
            }
        });

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

        ((FunctionOption)afterConMenu.Options.First(option => option.Name == "Go Back")).AfterFuncSubMenu = menu;

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
                Func = async (object? o) => await GetNetworkStream(combinedPath, vpn),
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

    private static async Task<TcpClientManager> GetNetworkStream(string combinedPath)
    {
        return await GetNetworkStream(combinedPath, string.Empty);
    }

    private static async Task<TcpClientManager> GetNetworkStream(string combinedPath, string ip)
    {
        await Task.Yield();
        Console.Clear();

        if (!string.IsNullOrEmpty(ip))
        {
            var clientManager = new TcpClientManager(ip);

            if (await clientManager.ConnectAsync())
            {
                return clientManager;
            }
            else
            {
                Console.WriteLine("連線失敗");
            }
        }

        while (true)
        {
            try
            {
                Console.WriteLine("Enter Target IP");
                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input)) continue;
                var regex = RegexRule.IpCheck();

                if (!regex.IsMatch(input))
                {
                    Console.WriteLine("Wrong ip format!");
                }

                // Connect to the server using its VPN IP address and port
                var clientManager = new TcpClientManager(ip);

                if(await clientManager.ConnectAsync())
                {
                    using var outputFile = new StreamWriter(combinedPath);
                    outputFile.Write(input); // Write input as text

                    return clientManager;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("給Maru看的:" + ex.Message);
                Console.WriteLine("連線失敗");
            }
        }
    }

    private static async Task<TcpClientManager> Start(object? obj)
    {
        TcpClientManager tcpClientManager = (TcpClientManager)obj!;
        
        var message = "start";
        DisableKeyboardInput();
        await tcpClientManager.SendCommandAsync(message);
        EnableKeyboardInput();

        return tcpClientManager;
    }

    private static async Task<TcpClientManager> Check(object? obj)
    {
        TcpClientManager tcpClientManager = (TcpClientManager)obj!;
        
        var message = "check";
        DisableKeyboardInput();
        await tcpClientManager.SendCommandAsync(message);
        EnableKeyboardInput();

        return tcpClientManager;
    }

    private static async Task<TcpClientManager> Stop(object? obj)
    {
        TcpClientManager tcpClientManager = (TcpClientManager)obj!;

        var message = "stop";
        DisableKeyboardInput();
        await tcpClientManager.SendCommandAsync(message);
        EnableKeyboardInput();

        return tcpClientManager;
    }

    // Disables the keyboard input (ignores key presses)
    private static void DisableKeyboardInput()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                while (Console.KeyAvailable) Console.ReadKey(true); // Discard all input
                await Task.Delay(100); // Avoid busy waiting
            }
        });
    }

    // Re-enabling keyboard input simply stops the disabling task
    private static void EnableKeyboardInput()
    {
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(intercept: true); // Clear the key press
        // No-op in this example; depends on implementation.
        // If needed, you can use a cancellation token to stop the disabling task above.
    }
}
