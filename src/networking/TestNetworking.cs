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
                    Task.Run(() => _server.Start(_serverEP));
                }

                if (_hasClient && matchline("connect"))
                {
                    if (!_client.Connected)
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
                }

                if (_hasServer && matchline("stopserver"))
                {
                    Task.Run(() => _server.Stop());
                }

                if (_hasClient && matchline("disconnect"))
                {
                    if (_client.Connected)
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
                    if (!_server.Running)
                    {
                        throw new InvalidOperationException("Servier is not running");
                    }

                    var input = int.TryParse(Console.ReadLine(), out var value) ? value : -1;
                    var data = BitConverter.GetBytes(input);
                    _serverInterface.Input.SetResult(data);
                }

                if (_hasClient && matchline("inputclient"))
                {
                    if (!_client.Connected)
                    {
                        throw new InvalidOperationException("Client is not connected");
                    }

                    var input = int.TryParse(Console.ReadLine(), out var value) ? value : -1;
                    var data = BitConverter.GetBytes(input);
                    _clientInterface.Input.SetResult(data);
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