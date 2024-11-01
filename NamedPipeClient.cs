using MenuVisualizer;
using MenuVisualizer.Model;
using MenuVisualizer.Model.Interface;
using PipeServerClient;
using PipeServerClient.Service;
using PipeServerClient.Service.Interface;
using System.Net.Sockets;

class NamedPipeClient
{
    private readonly INetworkService _networkService = new NetworkService();
    private readonly IFileService _fileService = new FileService();
    public async Task Go()
    {
        var menu = Intialize();

        var manager = new ConsoleMenuManager();

        manager.Construct(menu);

        await manager.ShowAsync();
    }

    private Menu Intialize()
    {
        var vpnList = _fileService.ReadVpnIps();

        var serverCommandMenu = new Menu()
        {
            Name = "Server Command",
            Options =
            [
                new FunctionOption()
                {
                    Name = "Start Server",
                    Func = async (object? obj) => 
                    {
                        await Start();
                        return null;
                    }
                },
                new FunctionOption()
                {
                    Name = "Check Server",
                    Func = async (object? obj) => 
                    {
                        await Check();
                        return null;
                    }
                },
                new FunctionOption()
                {
                    Name = "Stop Server",
                    Func = async (object? obj) =>
                    {
                        await Stop();
                        return null;
                    }
                },
                new FunctionOption()
                {
                    Name = "Go Back",
                    Func = async (object? obj) => 
                    {
                        await _networkService.DisposeAsync();
                        return null;
                    }
                }
            ]
        };

        serverCommandMenu.Options.ForEach(option =>
        {
            if (option is FunctionOption functionOption)
            {
                functionOption.AfterFuncSubMenu = serverCommandMenu;
            }
        });

        var gameServerNamelist = _fileService.ReadGames();
        var chooseGameServerMenu = new Menu()
        {
            Name = "Choose the game",
            Options = [..gameServerNamelist.Select(name =>
            {
                return new FunctionOption()
                {
                    Name = name,
                    Func = async (object? o) => 
                    {
                        await ChooseGameServer(name);
                        return null;
                    },
                    AfterFuncSubMenu = serverCommandMenu
                };
            }).ToList()]
        };

        IOption firstOption = CreateFirstOption(vpnList, chooseGameServerMenu);

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

        ((FunctionOption)serverCommandMenu.Options.First(option => option.Name == "Go Back")).AfterFuncSubMenu = menu;

        return menu;
    }

    private IOption CreateFirstOption(List<string> vpnList,Menu nextMenu)
    {
        IOption option;

        if (vpnList.Count > 0)
        {
            var ops = vpnList.Select(vpn => new FunctionOption()
            {
                Name = vpn,
                Func = async (object? o) => await ConnectServer(vpn),
                AfterFuncSubMenu = nextMenu
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
                        Func = async (object? o) => await ConnectServer(),
                        AfterFuncSubMenu = nextMenu
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
                Func = async (object? o) => await ConnectServer(),
                AfterFuncSubMenu = nextMenu
            };
        }

        return option;
    }

    private async Task<NetworkStream> ConnectServer()
    {
        return await ConnectServer(string.Empty);
    }

    private async Task<NetworkStream> ConnectServer(string ip)
    {
        await Task.Yield();
        Console.Clear();

        try
        {
            if (!string.IsNullOrEmpty(ip))
            {
                return await _networkService.ConnectAsync(ip);
            }
        }
        catch (Exception)
        {
            Console.WriteLine("連線失敗");
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
                if (!string.IsNullOrEmpty(input))
                {
                    var networkStream = await _networkService.ConnectAsync(input);
                    _fileService.WriteVpnIp(input);

                    return networkStream;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("For Maru:" + ex.Message);
                Console.WriteLine("連線失敗");
            }
        }
    }

    private async Task ChooseGameServer(string gameServerName)
    {
        var message = $"set {gameServerName}";
        await HandleSendCommandAsync(message);
    }

    private async Task Start()
    {
        var message = "start";
        await HandleSendCommandAsync(message);
    }

    private async Task Check()
    {
        var message = "check";
        await HandleSendCommandAsync(message);
    }

    private async Task Stop()
    {
        var message = "stop";
        await HandleSendCommandAsync(message);
    }

    private async Task HandleSendCommandAsync(string message)
    {
        DisableKeyboardInput();
        await _networkService.SendCommandAsync(message);
        EnableKeyboardInput();
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
