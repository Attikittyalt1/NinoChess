using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NinoChess.Networking;

public class Client(INetworkLocalConnectionLayer connectionLayer)
{
    public bool Running { get; private set; } = false;
    public bool Starting { get; private set; } = false;
    public bool Stopping { get; private set; } = false;
    public bool Connected { get; private set; } = false;
    public bool Connecting { get; private set; } = false;
    public bool Disconnecting { get; private set; } = false;

    private readonly NetworkLocalSocketManager _manager = new (connectionLayer);

    private Socket? _socket;
    private CancellationTokenSource? _cancelLocal;
    private Task? _endedLocal;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (Starting) throw new InvalidOperationException("Client is already starting.");

        if (Stopping) throw new InvalidOperationException("Client is still stopping.");

        if (Running) throw new InvalidOperationException("Client is already running.");

        Starting = true;

        Console.WriteLine("Starting client.");

        try
        {
            _cancelLocal = new CancellationTokenSource();

            var startedLocal = new TaskCompletionSource();
            var endedLocal = new TaskCompletionSource();

            _endedLocal = endedLocal.Task;

            _ = Task.Run(() => _manager.StartWatchingLocal(startedLocal, endedLocal, _cancelLocal.Token), _cancelLocal.Token);

            await startedLocal.Task;

            Running = true;

            Console.WriteLine("Started client.");
        } 
        finally
        {
            Starting = false;
        }
    }

    public async Task ConnectAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {

        if (Connecting) throw new InvalidOperationException("Client is already connecting.");

        if (Disconnecting) throw new InvalidOperationException("Client is still disconnecting.");

        if (Connected) throw new InvalidOperationException("Client is already connected.");

        if (Starting) throw new InvalidOperationException("Client is still starting.");

        if (Stopping) throw new InvalidOperationException("Client is still stopping.");

        if (!Running) throw new InvalidOperationException("Client is not running.");

        Connecting = true;

        Console.WriteLine("Connecting client.");

        try
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            cancellationToken.Register(() =>
            {
                Console.WriteLine("Stopped connecting.");
            });

            await _socket.ConnectAsync(endPoint, cancellationToken);

            var startedSocket = new TaskCompletionSource();
            var endedSocket = new TaskCompletionSource();

            _ = Task.Run(() => _manager.StartWatchingSocket(_socket, startedSocket, endedSocket));

            await startedSocket.Task;

            Connected = true;

            Console.WriteLine("Connected client.");
        }
        finally
        {
            Connecting = false;
        }
    }

    public void Start()
    {
        StartAsync().Wait();
    }

    public void Connect(IPEndPoint endPoint)
    {
        ConnectAsync(endPoint).Wait();
    }

    public async Task StopAsync()
    {
        if (Stopping) throw new InvalidOperationException("Client is already stopping.");

        if (Starting) throw new InvalidOperationException("Client is still starting.");

        if (!Running) throw new InvalidOperationException("Client is already stopped.");

        if (Disconnecting) throw new InvalidOperationException("Client is still disconnecting.");

        if (Connecting) throw new InvalidOperationException("Client is still connecting.");

        if (Connected) throw new InvalidOperationException("Client is still connected.");

        Disconnecting = true;

        Console.WriteLine("Disconecting client.");

        try
        {
            if (!_cancelLocal.IsCancellationRequested)
            {
                _cancelLocal.Cancel();
            }

            await _endedLocal;

            _cancelLocal = null;
            _endedLocal = null;
            Running = false;

            Console.WriteLine("Disconnected client.");
        }
        finally
        {
            Stopping = false;
        }
    }

    public async Task DisconnectAsync()
    {
        if (Disconnecting) throw new InvalidOperationException("Client is already disconnecting.");

        if (Connecting) throw new InvalidOperationException("Client is still connecting.");

        if (!Connected) throw new InvalidOperationException("Client is already disconnected.");

        Disconnecting = true;

        Console.WriteLine("Disconnecting client.");

        try
        {
            await _socket.DisconnectAsync(false);

            _socket = null;
            Connected = false;

            Console.WriteLine("Disconnected client.");
        }
        finally
        {
            Disconnecting = false;
        }
    }

    public void Stop()
    {
        StopAsync().Wait();
    }

    public void Disconnect()
    {
        DisconnectAsync().Wait();
    }
}