using MathNet.Numerics;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NinoChess.Networking;

public class TestNetworking()
{
    private bool _hasServer;
    private Server? _server = null;
    private TestNetworkLocalInterface? _serverInterface = null;

    private bool _hasClient;
    private Client? _client = null;
    private TestNetworkLocalInterface? _clientInterface = null;

    private int _port = 25565;

    public void CreateServer()
    {
        if (_hasServer == true)
        {
            throw new InvalidOperationException("Server has already been created.");
        }

        _hasServer = true;

        _serverInterface = new TestNetworkLocalInterface();
        _server = new Server(_serverInterface);
    }

    public void CreateClient()
    {
        if (_hasClient == true)
        {
            throw new InvalidOperationException("Client has already been created.");
        }

        _hasClient = true;

        _clientInterface = new TestNetworkLocalInterface();
        _client = new Client(_clientInterface);
    }

    public void StartConsoleInterface()
    {
        new Thread(() =>
        {
            CancellationTokenSource? clientCancelationTokenSource = default;

            var line = Console.ReadLine();
            while (!(line?.Equals("quit", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                if (_hasServer && matchline("startserver"))
                {
                    if (!_server.Running && !_server.Starting)
                    {
                        var endpoint = new IPEndPoint(IPAddress.Any, _port);

                        Task.Run(() => _server.Start(endpoint));
                    }
                    else if (_server.Running)
                    {
                        Console.WriteLine("Error. Server is already started.");
                    }
                    else
                    {
                        Console.WriteLine("Error. Server is already starting.");
                    }
                }

                else if (_hasClient && matchline("connect"))
                {
                    if (!_client.Connected && !_client.Connecting)
                    {
                        clientCancelationTokenSource = new CancellationTokenSource();
                        var address = IPAddress.Parse(Console.ReadLine());
                        var endpoint = new IPEndPoint(address, _port);

                        Task.Run(() =>
                        {
                            try
                            {
                                _client.ConnectAsync(endpoint, clientCancelationTokenSource.Token).Wait(clientCancelationTokenSource.Token);
                            }
                            catch (OperationCanceledException e)
                            {
                                
                            }
                            catch (AggregateException ae)
                            {
                                ae.Handle(ex =>
                                {
                                    return ex is OperationCanceledException;
                                });
                            }
                            finally
                            {
                                clientCancelationTokenSource = null;
                            }
                        }, clientCancelationTokenSource.Token);
                    }
                    else if (_client.Connected)
                    {
                        Console.WriteLine("Error. Client is already connected.");
                    }
                    else
                    {
                        Console.WriteLine("Error. Client is already connecting.");
                    }
                }

                else if(_hasServer && matchline("stopserver"))
                {
                    if (_server.Running)
                    {
                        if (!_server.HasClients)
                        {
                            _server.Stop();
                        }
                        else
                        {
                            Console.WriteLine("Error. Server cannot stop while it has connected clients. WILL FIX LATER");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error. Server is not running.");
                    }
                }

                else if(_hasClient && matchline("disconnect"))
                {
                    if (_client.Connected && !_client.Disconnecting)
                    {
                        Task.Run(() =>
                        {
                            _clientInterface.Input.SetResult(BitConverter.GetBytes(-2));

                            do
                            {
                                _clientInterface.DataUpdated.WaitOne();
                                _clientInterface.DataUpdated.Reset();
                            }
                            while (_clientInterface.GetDataAsInt() != -3);

                            _client.Disconnect();
                        });
                    }
                    else if (_client.Connecting)
                    {
                        clientCancelationTokenSource.Cancel();
                    }
                    else
                    {
                        Console.WriteLine("Error. Client is not connected.");
                    }
                }

                else if(_hasServer && matchline("printserver"))
                {
                    Console.WriteLine(_serverInterface.GetDataAsInt());
                }

                else if(_hasClient && matchline("printclient"))
                {
                    Console.WriteLine(_clientInterface.GetDataAsInt());
                }

                else if(_hasServer && matchline("inputserver"))
                {
                    if (_server.Running)
                    {
                        var input = int.TryParse(Console.ReadLine(), out var value) ? value : -1;
                        var data = BitConverter.GetBytes(input);
                        _serverInterface.Input.SetResult(data);
                    }
                    else
                    {
                        Console.WriteLine("Error. Server is not running.");
                    }
                }

                else if(_hasClient && matchline("inputclient"))
                {
                    if (_client.Connected)
                    {
                        var input = int.TryParse(Console.ReadLine(), out var value) ? value : -1;
                        var data = BitConverter.GetBytes(input);
                        _clientInterface.Input.SetResult(data);
                    } else
                    {
                        Console.WriteLine("Error. Client is not connected.");
                    }
                }

                else if(matchline("setport"))
                {
                    _port = int.Parse(Console.ReadLine());
                }

                else
                {
                    Console.WriteLine("Invalid command. Please try again.");
                }

                line = Console.ReadLine();
            }

            if (_hasClient && _client.Connected)
            {
                _client.DisconnectAsync();
            }

            if (_hasServer && _server.Running)
            {
                _server.StopAsync();
            }

            bool matchline(string value) => line?.Equals(value, StringComparison.OrdinalIgnoreCase) ?? false;
        }).Start();
    }
}