using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NinoChess.Networking;

public class NetworkLocalSocketManager()
{
    public int MaxBufferSizeFromNetwork { get; init; } = 256;

    private readonly Dictionary<int, (Socket socket, CancellationTokenSource stopWatchingCancellationTokenSource)> _sockets = [];
    public IReadOnlyCollection<int> ConnectedSockets => _sockets.Keys;
    public bool IsListeningSocket { get; private set; } = false;
    private CancellationTokenSource? _stopListeningCancellationTokenSource;


    public class PacketArgs : EventArgs
    {
        public required CustomPacket Packet { get; init; }
        public required int ID { get; init; }
        public void Deconstruct(out CustomPacket Packet, out int ID)
        {
            Packet = this.Packet;
            ID = this.ID;
        }

    }

    public EventHandler<PacketArgs>? OnReceived;
    public Task<PacketArgs> Received => _receivedSource.Task;
    public Task ReceivedReset => _receivedResetSource.Task;
    private TaskCompletionSource<PacketArgs> _receivedSource = new();
    private TaskCompletionSource _receivedResetSource = new();

    public async Task<ReturnCode> SendAndRecieveReturnCodeAsync(CustomPacket packet, int id, CancellationToken cancellationToken = default)
    {
        await SendAsync(packet, id, cancellationToken);

        while (true)
        {
            var data = await Received;

            cancellationToken.ThrowIfCancellationRequested();

            if (data.ID == id && data.Packet.Type == PacketType.ResultWithoutData)
            {
                var (type, code) = data.Packet.ToResultWithoutData();

                if (type == packet.Type)
                {
                    return code;
                }
            }

            await ReceivedReset;
        }
    }

    public async Task<CustomPacket> SendAndRecieveSpecificResponsesOrReturnCodeAsync(CustomPacket packet, int id, ICollection<PacketType> targetTypes, CancellationToken cancellationToken = default)
    {
        await SendAsync(packet, id, cancellationToken);

        while (true)
        {
            var data = await Received;

            cancellationToken.ThrowIfCancellationRequested();

            if (data.ID == id)
            {
                if (targetTypes.Contains(data.Packet.Type))
                {
                    return data.Packet;
                }

                if (data.Packet.Type == PacketType.ResultWithoutData)
                {
                    var type = data.Packet.ToResultWithoutData().type;

                    if (type == packet.Type)
                    {
                        return packet;
                    }
                }
            }

            await ReceivedReset;
        }
    }

    public async Task<CustomPacket> SendAndRecieveSpecificResponseOrReturnCodeAsync(CustomPacket packet, int id, PacketType targetType, CancellationToken cancellationToken = default)
    {
        await SendAsync(packet, id, cancellationToken);

        while (true)
        {
            var data = await Received;

            cancellationToken.ThrowIfCancellationRequested();

            if (data.ID == id)
            {

                if (data.Packet.Type == targetType)
                {
                    return data.Packet;
                }

                if (data.Packet.Type == PacketType.ResultWithoutData)
                {
                    var type = data.Packet.ToResultWithoutData().type;

                    if (type == packet.Type)
                    {
                        return packet;
                    }
                }
            }

            await ReceivedReset;
        }
    }

    public async Task<CustomPacket> SendAndRecieveSpecificResponsesAsync(CustomPacket packet, int id, ICollection<PacketType> targetTypes, CancellationToken cancellationToken = default)
    {
        await SendAsync(packet, id, cancellationToken);

        while (true)
        {
            var data = await Received;

            cancellationToken.ThrowIfCancellationRequested();

            if (data.ID == id && targetTypes.Contains(data.Packet.Type))
            {
                return data.Packet;
            }

            await ReceivedReset;
        }
    }

    public async Task<CustomPacket> SendAndRecieveSpecificResponseAsync(CustomPacket packet, int id, PacketType targetType, CancellationToken cancellationToken = default)
    {
        await SendAsync(packet, id, cancellationToken);

        while (true)
        {
            var data = await Received;

            cancellationToken.ThrowIfCancellationRequested();

            if (data.ID == id && data.Packet.Type == targetType)
            {
                return data.Packet;
            }

            await ReceivedReset;
        }
    }

    public async Task<CustomPacket> SendAndRecieveResponseAsync(CustomPacket packet, int id, CancellationToken cancellationToken = default)
    {
        await SendAsync(packet, id, cancellationToken);

        while (true)
        {
            var data = await Received;

            cancellationToken.ThrowIfCancellationRequested();

            if (data.ID == id)
            {
                return data.Packet;
            }

            await ReceivedReset;
        }
    }

    public async Task SendAsync(CustomPacket packet, int id, CancellationToken cancellationToken = default)
    {
        var data = packet.ToBytesRaw();

        var socket = _sockets[id].socket;

        Debug.WriteLine("Sending data to socket with id: {0}, LISTENING = {1}", id, IsListeningSocket);
        await SendDataToSocketAsync(data, socket, cancellationToken);
    }

    public void StopWatchingSocket(int id)
    {
        if (!_sockets.TryGetValue(id, out var data))
        {
            throw new InvalidOperationException("Socket is already not being watched.");
        }

        data.stopWatchingCancellationTokenSource.Cancel();
    }

    public void StopListeningSocket()
    {
        if (!IsListeningSocket)
        {
            throw new InvalidOperationException("Socket is already not being listened to.");
        }

        _stopListeningCancellationTokenSource.Cancel();
    }

    public (int, Task) StartWatchingSocket(Socket socket, CancellationToken cancellationToken = default)
    {
        // TODO: should probably check that the socket isn't already being watched
        
        var id = 0;

        while (_sockets.ContainsKey(id)) id++;

        var cancellationTokenSource = new CancellationTokenSource();

        _sockets.Add(id, (socket, cancellationTokenSource));
        Console.WriteLine("Started watching socket");

        var task = Task.Run(async () =>
        {
            try
            {
                while (socket.Connected && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await CheckRecieveDataFromSocketAsync(socket, id, cancellationToken);
                }
            }
            finally
            {
                _sockets.Remove(id);
                Console.WriteLine("Stopped watching socket");
            }
        }, cancellationToken);

        return (id, task);
    }

    public Task StartListeningSocket(Socket socket, CancellationToken cancellationToken = default)
    {
        if (IsListeningSocket)
        {
            throw new ArgumentException("NetworkLocalSocketManager is already listeniing to a socket");
        }

        IsListeningSocket = true;
        Console.WriteLine("Started listening to socket");

        _stopListeningCancellationTokenSource = new();

        var tasks = new List<Task>();

        var task = Task.Run(async () =>
        {
            try
            {
                while (!_stopListeningCancellationTokenSource.Token.IsCancellationRequested)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var task = await CheckNewConnectionAsync(socket, cancellationToken);

                    tasks.Add(task);
                }
            }
            finally
            {
                IsListeningSocket = false;
                _stopListeningCancellationTokenSource = null;
                Console.WriteLine("Stopped listening to socket");
            }

            await Task.WhenAll(tasks);
        }, cancellationToken);

        return task;
    }

    private async Task<Task> CheckNewConnectionAsync(Socket socket, CancellationToken cancellationToken = default)
    {
        var newSocket = await socket.AcceptAsync(cancellationToken);

        var task = Task.Run(() =>
        {
            StartWatchingSocket(newSocket, cancellationToken);
        }, cancellationToken);

        return task;
    }

    private async Task CheckRecieveDataFromSocketAsync(Socket socket, int id, CancellationToken cancellationToken)
    {
        var buffer = new byte[MaxBufferSizeFromNetwork];

        var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

        if (received <= 0) return;

        Debug.WriteLine("Received data from socket with id: {0}, LISTENING = {1}", id, IsListeningSocket);

        var packet = FromBytesRaw(buffer, received);

        var args = new PacketArgs { Packet = packet, ID = id };

        _receivedSource.SetResult(args);
        _receivedSource = new();
        _receivedResetSource.SetResult();
        _receivedResetSource = new();
        OnReceived?.Invoke(this, args);
    }

    private async Task SendDataToSocketAsync(byte[] data, Socket socket, CancellationToken cancellationToken = default)
    {
        //await socket.SendAsync(data, cancellationToken);
        socket.Send(data);
    }
}