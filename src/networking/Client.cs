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

    private readonly Socket _socket = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private CancellationTokenSource? _tokenSource;
    private Task? _tasksEnded;

    public void Connect(IPEndPoint endPoint)
    {
        if (Connected == true)
        {
            throw new InvalidOperationException("Cannot connect client that is already connected.");
        }

        Connected = true;

        Debug.WriteLine("Connecting client.");

        _socket.Connect(endPoint);

        _tokenSource = new CancellationTokenSource();

        var manager = new NetworkLocalSocketManager(gameInterface);

        var startedLocal = new TaskCompletionSource();
        var startedSocket = new TaskCompletionSource();
        var endedLocal = new TaskCompletionSource();
        var endedSocket = new TaskCompletionSource();
        _tasksEnded = Task.WhenAll(endedLocal.Task, endedSocket.Task);

        _ = Task.Run(() => manager.StartWatchingLocal(_tokenSource.Token, startedLocal, endedLocal));
        _ = Task.Run(() => manager.StartWatchingSocket(_socket, _tokenSource.Token, startedSocket, endedSocket));

        Task.WaitAll(startedLocal.Task, startedSocket.Task);

        Debug.WriteLine("Connected client.");
    }

    public void Disconnect()
    {
        if (Connected == false)
        {
            throw new InvalidOperationException("Cannot disconnect client that is not connected.");
        }

        Connected = false;

        Debug.WriteLine("Disconecting client.");

        _tokenSource.Cancel();

        _tasksEnded.Wait();

        _socket.Disconnect(false);

        Debug.WriteLine("Disconnected client.");
    }
}