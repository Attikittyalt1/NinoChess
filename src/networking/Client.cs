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

    private readonly Socket _socket = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private CancellationTokenSource? _tokenSource;
    private Task? _tasksEnded;

    public async Task ConnectAsync(IPEndPoint endPoint)
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

        Debug.WriteLine("Connecting client.");

        await _socket.ConnectAsync(endPoint);

        _tokenSource = new CancellationTokenSource();

        var manager = new NetworkLocalSocketManager(gameInterface);

        var startedLocal = new TaskCompletionSource();
        var startedSocket = new TaskCompletionSource();
        var endedLocal = new TaskCompletionSource();
        var endedSocket = new TaskCompletionSource();
        _tasksEnded = Task.WhenAll(endedLocal.Task, endedSocket.Task);

        _ = Task.Run(() => manager.StartWatchingLocal(_tokenSource.Token, startedLocal, endedLocal));
        _ = Task.Run(() => manager.StartWatchingSocket(_socket, _tokenSource.Token, startedSocket, endedSocket));

        await Task.WhenAll(startedLocal.Task, startedSocket.Task);

        Connected = true;
        Connecting = false;

        Debug.WriteLine("Connected client.");
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

        Debug.WriteLine("Disconecting client.");

        _tokenSource.Cancel();

        await _tasksEnded;

        await _socket.DisconnectAsync(false);

        Connected = false;
        Disconnecting = false;

        Debug.WriteLine("Disconnected client.");
    }

    public void Disconnect()
    {
        DisconnectAsync().Wait();
    }
}