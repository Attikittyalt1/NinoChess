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

public class Client()
{
    public bool Connected { get; private set; } = false;
    public bool Connecting { get; private set; } = false;
    public bool Disconnecting { get; private set; } = false;

    public readonly NetworkLocalSocketManager SocketManager = new ();

    private Socket? _socket;
    private IPEndPoint? _endPoint;

    private (int id, Task stopWatching)? _socketInfo;

    public async Task ConnectAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default)
    {

        if (Connecting) throw new InvalidOperationException("Client is already connecting.");

        if (Disconnecting) throw new InvalidOperationException("Client is still disconnecting.");

        if (Connected) throw new InvalidOperationException("Client is already connected.");

        Connecting = true;

        Console.WriteLine("Connecting client.");

        try
        {
            _socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _endPoint = endPoint;

            cancellationToken.Register(() =>
            {
                Console.WriteLine("Stopped connecting.");
            });

            await _socket.ConnectAsync(endPoint, cancellationToken);

            _socketInfo = SocketManager.StartWatchingSocket(_socket, cancellationToken);

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

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (Disconnecting) throw new InvalidOperationException("Client is already disconnecting.");

        if (Connecting) throw new InvalidOperationException("Client is still connecting.");

        if (!Connected) throw new InvalidOperationException("Client is already disconnected.");

        Disconnecting = true;

        Console.WriteLine("Disconnecting client.");

        try
        {
            SocketManager.StopWatchingSocket(_socketInfo.Value.id);
            await _socketInfo.Value.stopWatching;

            await _socket.DisconnectAsync(false, cancellationToken);

            _socket = null;
            Connected = false;
            _endPoint = null;

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