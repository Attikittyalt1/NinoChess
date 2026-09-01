using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NinoChess.Networking;

public class NetworkingCommandLineApp
{
    private readonly (Server core, GameServerLayer layer) _server;

    private readonly Dictionary<string, (Client core, GameClientLayer layer, CancellationTokenSource? stopConnecting)> _clients = [];
    private int _port = 25565;
    private IPAddress? _lastAddress = null;

    public NetworkingCommandLineApp()
    {
        var grid = new Grid(8);
        var boardState = new BoardStateData(grid);
        var eventService = new EventService();
        var mutationService = new MutationService();

        var turnManager = new TurnManager(boardState, mutationService, eventService);

        turnManager.SetupBoard();

        _server.core = new();
        _server.layer = new(_server.core, turnManager);
        _server.layer.Initialize();
    }

    public void StartConsoleInterface()
    {
        bool stop = false;

        #region server

        #region server start
        var serverStart = new Command("start");
        serverStart.SetAction(parseResult =>
        {
            if (_server.core.Starting)
            {
                Console.WriteLine("Server is already starting.");
                return;
            }

            if (_server.core.Stopping)
            {
                Console.WriteLine("Server is still stopping.");
                return;
            }

            if (_server.core.Running)
            {
                Console.WriteLine("Server is already running.");
                return;
            }

            var ep = new IPEndPoint(IPAddress.Any, _port);
            Task.Run(() =>
            {
                _server.core.Start(ep);
            });
        });
        #endregion server start

        #region server stop
        var serverStop = new Command("stop");
        serverStop.SetAction(parseResult =>
        {
            if (_server.core.Stopping)
            {
                Console.WriteLine("Server is already stopping.");
                return;
            }

            if (_server.core.Starting)
            {
                Console.WriteLine("Server is still starting.");
                return;
            }

            if (!_server.core.Running)
            {
                Console.WriteLine("Server is already stopped.");
                return;
            }

            Task.Run(() =>
            {
                _server.core.Stop();
            });
        });
        #endregion server stop

        #region server send
        var serverSend = new Command("send")
        {
            new Argument<int>("message")
        };
        serverSend.SetAction(parseResult =>
        {
            var message = parseResult.GetRequiredValue<int>("message");

            if (_server.core.Starting)
            {
                Console.WriteLine("Server is still starting.");
                return;
            }

            if (_server.core.Stopping)
            {
                Console.WriteLine("Server is still stopping.");
                return;
            }

            if (!_server.core.Running)
            {
                Console.WriteLine("Server is not running.");
                return;
            }

            Task.Run(async () =>
            {
                var manager = _server.core.SocketManager;

                await foreach (var socketID in manager.ConnectedSockets.ToAsyncEnumerable())
                {
                    await manager.SendAndRecieveReturnCodeAsync(CustomPacket.FromMessage(message), socketID);
                    Console.WriteLine("Successfully sent message to client {0}", socketID);
                }
            });
        });
        #endregion server send

        var serverRoot = new Command("server")
        {
            serverStop,
            serverStart,
            serverSend
        };

        #endregion server

        #region client

        var clientName = new Argument<string>("name");

        #region client create

        var clientCreate = new Command("create")
        {
            clientName
        };
        clientCreate.SetAction(parseResult =>
        {
            var name = parseResult.GetRequiredValue(clientName);

            var grid = new Grid(8);
            var boardState = new BoardStateData(grid);
            var eventService = new EventService();
            var mutationService = new MutationService();

            var turnManager = new TurnManager(boardState, mutationService, eventService);

            turnManager.SetupBoard();

            var core = new Client();
            var layer = new GameClientLayer(core, turnManager);
            layer.Initialize();
            
            if (!_clients.TryAdd(name, (core, layer, null)))
            {
                Console.WriteLine("Client already exists.");
                return;
            }
            
            Console.WriteLine("Created client with name: {0}", name);
        });

        #endregion client create

        #region client connect

        var clientConnect = new Command("connect")
        {
            clientName,
            new Argument<string>("ipaddress")
            {
                Arity = ArgumentArity.ZeroOrOne
            }
        };
        clientConnect.SetAction(parseResult =>
        {
            var name = parseResult.GetRequiredValue(clientName);
            var iparg = parseResult.GetValue<string>("ipaddress");
            var ipaddress = _lastAddress;

            if (iparg is not null)
            {
                if (IPAddress.TryParse(iparg, out var newAddress))
                {
                    ipaddress = newAddress;
                } else
                {
                    Console.WriteLine("Invalid address.");
                    return;
                }
            } else if (_lastAddress is not null)
            {
                Console.WriteLine("No ip address provided. Using previous address {0}", _lastAddress.ToString());
                ipaddress = _lastAddress;
            }
            else
            {
                Console.WriteLine("No ip address provided.");
                return;
            }

            if (!_clients.TryGetValue(name, out var data))
            {
                Console.WriteLine("Could not find client with the provided name.");
                return;
            }

            if (data.core.Connecting)
            {
                Console.WriteLine("Client is already connecting.");
                return;
            }

            if (data.core.Disconnecting)
            {
                Console.WriteLine("Client is still disconnecting.");
                return;
            }

            if (data.core.Connected)
            {
                Console.WriteLine("Client is already connected.");
                return;
            }

            CancellationTokenSource cancellationTokenSource = new();
            _clients[name] = data with { stopConnecting = cancellationTokenSource };
            _lastAddress = ipaddress;

            Task.Run(async () =>
            {
                await data.core.ConnectAsync(new(ipaddress, _port), cancellationTokenSource.Token);

                var connectReturnCode = await data.core.SocketManager.SendAndRecieveReturnCodeAsync(CustomPacket.Connect, 0, cancellationTokenSource.Token);

                if (!CustomPacket.IsSuccess(connectReturnCode))
                {
                    Console.WriteLine("Failed to activate id.");
                }

                var requestPacket = await data.core.SocketManager.SendAndRecieveSpecificResponseOrReturnCodeAsync(CustomPacket.RequestID, 0, CustomPacket.PacketType.AssignID, cancellationTokenSource.Token);

                if (requestPacket.Type != CustomPacket.PacketType.AssignID)
                {
                    Console.WriteLine("Failed to recieve assigned id.");
                }

                var linkReturnCode = await data.core.SocketManager.SendAndRecieveReturnCodeAsync(CustomPacket.FromLinkID(requestPacket.ToAssignID()), 0, cancellationTokenSource.Token);

                if (CustomPacket.IsSuccess(linkReturnCode))
                {
                    Console.WriteLine("Failed to link id.");
                }

            }, cancellationTokenSource.Token);
        });

        #endregion client connect

        #region client disconnect

        var clientDisconnect = new Command("disconnect")
        {
            clientName
        };
        clientDisconnect.SetAction(parseResult =>
        {
            var name = parseResult.GetRequiredValue(clientName);

            if (!_clients.TryGetValue(name, out var data))
            {
                Console.WriteLine("Could not find client with the provided name.");
                return;
            }

            if (data.core.Disconnecting)
            {
                Console.WriteLine("Client is already disconnecting.");
                return;
            }

            if (data.core.Connecting)
            {
                data.stopConnecting.Cancel();
                return;
            }

            if (!data.core.Connected)
            {
                Console.WriteLine("Client is already disconnected.");
                return;
            }

            Task.Run(async () =>
            {
                var unassignReturnCode = await data.core.SocketManager.SendAndRecieveReturnCodeAsync(CustomPacket.UnassignID, 0);

                if (!CustomPacket.IsSuccess(unassignReturnCode))
                {
                    Console.WriteLine("Failed to unassign id.");
                }

                var disconnectReturnCode = await data.core.SocketManager.SendAndRecieveReturnCodeAsync(CustomPacket.Disconnect, 0);

                if (!CustomPacket.IsSuccess(disconnectReturnCode))
                {
                    Console.WriteLine("Failed to deactivate id.");
                }

                await data.core.DisconnectAsync();
            });
        });

        #endregion client disconnect

        #region client send
        var clientSend = new Command("send")
        {
            clientName,
            new Argument<int>("message")
        };
        clientSend.SetAction(parseResult =>
        {
            var name = parseResult.GetRequiredValue(clientName);
            var message = parseResult.GetRequiredValue<int>("message");

            if (!_clients.TryGetValue(name, out var data))
            {
                Console.WriteLine("Could not find client with the provided name.");
                return;
            }

            if (data.core.Connecting)
            {
                Console.WriteLine("Client is still connecting.");
                return;
            }

            if (data.core.Disconnecting)
            {
                Console.WriteLine("Client is still disconnecting.");
                return;
            }

            if (!data.core.Connected)
            {
                Console.WriteLine("Client is not connected.");
                return;
            }

            Task.Run(async () =>
            {
                await data.core.SocketManager.SendAndRecieveReturnCodeAsync(CustomPacket.FromMessage(message), 0);
                Console.WriteLine("Successfully sent message to server.");
            });
        });
        #endregion client send

        var clientRoot = new Command("client")
        {
            clientCreate,
            clientConnect,
            clientDisconnect,
            clientSend
        };

        #endregion client

        #region exit

        var exit = new Command("exit");
        exit.Aliases.Add("quit");

        exit.SetAction(parseResult =>
        {
            stop = true;
            return 0;
        });

        #endregion exit

        #region setport

        var setport = new Command("setport")
        {
            new Argument<int>("port")
        };

        setport.SetAction(parseResult =>
        {
            var port = parseResult.GetRequiredValue<int>("port");

            _port = port;
            return 0;
        });

        #endregion setport

        var rootCommand = new RootCommand("Basic networking playground for NinoChess")
        {
            serverRoot,
            clientRoot,
            exit,
            setport
        };

        string input;
        while (!stop)
        {
            input = Console.ReadLine();

            var result = rootCommand.Parse(input);
            result.Invoke();
        }
    }
}