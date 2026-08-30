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
    private IPEndPoint? _serverEP;

    private bool _hasClient;
    private Client? _client = null;
    private TestNetworkLocalInterface? _clientInterface = null;
    private IPEndPoint? _clientEP;

    public void CreateServer(int port)
    {
        if (_hasServer == true)
        {
            throw new InvalidOperationException("Server has already been created.");
        }

        _hasServer = true;
        _serverEP = new IPEndPoint(IPAddress.Any, port);

        _serverInterface = new TestNetworkLocalInterface();
        _server = new Server(_serverInterface);
    }

    public void CreateClient(string ip, int port)
    {
        if (_hasClient == true)
        {
            throw new InvalidOperationException("Client has already been created.");
        }

        _hasClient = true;
        _clientEP = new IPEndPoint(IPAddress.Parse(ip), port);

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
                        Task.Run(() => _server.Start(_serverEP));
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

                if (_hasClient && matchline("connect"))
                {
                    if (!_client.Connected && !_client.Connecting)
                    {
                        clientCancelationTokenSource = new CancellationTokenSource();
                        Task.Run(() =>
                        {
                            try
                            {
                                _client.ConnectAsync(_clientEP, clientCancelationTokenSource.Token).Wait(clientCancelationTokenSource.Token);
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

                if (_hasServer && matchline("stopserver"))
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

                if (_hasClient && matchline("disconnect"))
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

                if (_hasServer && matchline("printserver"))
                {
                    Console.WriteLine(_serverInterface.GetDataAsInt());
                }

                if (_hasClient && matchline("printclient"))
                {
                    Console.WriteLine(_clientInterface.GetDataAsInt());
                }

                if (_hasServer && matchline("inputserver"))
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

                if (_hasClient && matchline("inputclient"))
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