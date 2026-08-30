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

public class Client(INetworkLocalInterface gameInterface)
{
    public bool Connected { get; private set; } = false;
    public bool Connecting { get; private set; } = false;
    public bool Disconnecting { get; private set; } = false;

    private Socket _socket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _tasksEnded;

    public async Task ConnectAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {
        if (Connected == true)
        {
            throw new InvalidOperationException("Client is already connected.");
        }

        if (Connecting == true)
        {
            throw new InvalidOperationException("Client is already connecting.");
        }

        Connecting = true;

        Console.WriteLine("Connecting client.");

        try
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await _socket.ConnectAsync(endPoint, cancellationToken);

            _cancellationTokenSource = new CancellationTokenSource();

            var manager = new NetworkLocalSocketManager(gameInterface);

            var startedLocal = new TaskCompletionSource();
            var startedSocket = new TaskCompletionSource();
            var endedLocal = new TaskCompletionSource();
            var endedSocket = new TaskCompletionSource();
            _tasksEnded = Task.WhenAll(endedLocal.Task, endedSocket.Task);

            _ = Task.Run(() => manager.StartWatchingLocal(startedLocal, endedLocal, _cancellationTokenSource.Token), _cancellationTokenSource.Token);
            _ = Task.Run(() => manager.StartWatchingSocket(_socket, startedSocket, endedSocket, _cancellationTokenSource.Token), _cancellationTokenSource.Token);

            await Task.WhenAll(startedLocal.Task, startedSocket.Task);

            Connected = true;

            Console.WriteLine("Connected client.");
        } 
        finally
        {
            Connecting = false;
        }
    }

    public void Connect(IPEndPoint endPoint)
    {
        ConnectAsync(endPoint).Wait();
    }

    public async Task DisconnectAsync()
    {
        if (Connected == false)
        {
            throw new InvalidOperationException("Client is already disconnected.");
        }

        if (Disconnecting == true)
        {
            throw new InvalidOperationException("Client is already disconnecting.");
        }

        Disconnecting = true;

        Console.WriteLine("Disconecting client.");

        try
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            await _tasksEnded;

            await _socket.DisconnectAsync(false);

            _socket = null;
            _cancellationTokenSource = null;
            _tasksEnded = null;
            Connected = false;

            Console.WriteLine("Disconnected client.");
        }
        finally
        {
            Disconnecting = false;
        }
    }

    public void Disconnect()
    {
        DisconnectAsync().Wait();
    }
}