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

public class NetworkLocalSocketManager(INetworkLocalInterface networkGameInterface)
{
    private readonly Dictionary<int, Socket> _sockets = [];
    private bool _watchingLocal = false;
    private bool _listening = false;

    public void StartWatchingLocal(CancellationToken cancellationToken, TaskCompletionSource started, TaskCompletionSource ended)
    {
        if (_watchingLocal)
        {
            throw new ArgumentException("NetworkLocalSocketManager is already watching local");
        }

        _watchingLocal = true;
        Debug.WriteLine("Started watching local");

        started.SetResult();

        try
        {
            while (true)
            {
                CheckRecieveDataFromLocal(cancellationToken).Wait();
            }
        }
        catch (AggregateException ae)
        {
            ae.Handle(ex =>
            {
                return ex is TaskCanceledException;
            });
        }
        finally
        {
            _watchingLocal = false;
            Debug.WriteLine("Stopped watching local");
        }

        ended.SetResult();
    }

    public void StartWatchingSocket(Socket socket, CancellationToken cancellationToken, TaskCompletionSource started, TaskCompletionSource ended)
    {
        if (_sockets.ContainsValue(socket))
        {
            throw new ArgumentException("NetworkLocalSocketManager is already watching socket.");
        }

        var id = 0;

        while (_sockets.ContainsKey(id)) id++;

        _sockets.Add(id, socket);
        Debug.WriteLine("Started watching socket");

        started.SetResult();

        try
        {
            while (socket.Connected)
            {
                CheckRecieveDataFromSocket(socket, id, cancellationToken).Wait();
            }
        }
        catch (AggregateException ae)
        {
            ae.Handle(ex =>
            {
                return ex is TaskCanceledException;
            });
        }
        finally
        {
            _sockets.Remove(id);
            Debug.WriteLine("Stopped watching socket");
        }

        ended.SetResult();
    }

    public void StartListeningSocket(Socket socket, CancellationToken cancellationToken, TaskCompletionSource started, TaskCompletionSource endedListening, TaskCompletionSource endedSockets)
    {
        if (_listening)
        {
            throw new ArgumentException("NetworkLocalSocketManager is already listeniing to a socket");
        }

        _listening = true;
        Debug.WriteLine("Started listening to socket");

        started.SetResult();

        var tasks = new List<Task>();
        endedSockets.SetFromTask(Task.WhenAll(tasks));

        try
        {
            while (true)
            {
                var startedSocket = new TaskCompletionSource();
                var endedSocket = new TaskCompletionSource();
                CheckNewConnection(socket, cancellationToken, startedSocket, endedSocket).Wait();

                tasks.Add(endedSocket.Task);
            }
        }
        catch (AggregateException ae)
        {
            ae.Handle(ex =>
            {
                return ex is TaskCanceledException;
            });
        }
        finally
        {
            _listening = false;
            Debug.WriteLine("Stopped listening to socket");
        }

        endedListening.SetResult();
    }

    private async Task CheckNewConnection(Socket socket, CancellationToken cancellationToken, TaskCompletionSource startedSocket, TaskCompletionSource endedSocket)
    {
        var newSocket = await socket.AcceptAsync(cancellationToken);

        _ = Task.Run(() =>
        {
            StartWatchingSocket(newSocket, cancellationToken, startedSocket, endedSocket);
        }, cancellationToken);
    }

    private async Task<bool> CheckRecieveDataFromSocket(Socket socket, int id, CancellationToken cancellationToken)
    {
        var buffer = new byte[networkGameInterface.MaxBufferSizeFromNetwork];

        var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

        if (received <= 0) return false;

        Debug.WriteLine("Received data from socket with id: " + id);

        var responseTask = new TaskCompletionSource<byte[]>();
        var (respond, disconnect) = await networkGameInterface.OnRecieveDataAsync(buffer, received, id, responseTask);

        if (respond)
        {
            Debug.WriteLine("Sending response");
            var data = await responseTask.Task;

            await SendDataToSocket(data, socket);
        }
        else
        {
            Debug.WriteLine("Finished reponse chain");
        }

        return disconnect;
    }

    public async Task CheckRecieveDataFromLocal(CancellationToken cancellationToken)
    {
        var (data, id) = await networkGameInterface.GetDataToSendAsync(cancellationToken);

        if (id.HasValue)
        {
            if (!_sockets.TryGetValue(id.Value, out var socket))
            {
                throw new InvalidOperationException("Cannot send data to unregistered socket.");
            }

            Debug.WriteLine("Sending data to socket with id: " + id.Value);
            await SendDataToSocket(data, socket);
        } else
        {
            Debug.WriteLine("Sending data to connected sockets");
            await SendDataToSockets(data, _sockets.Values);
        }
    }

    private async Task SendDataToSockets(byte[] data, IEnumerable<Socket> sockets)
    {
        if (!sockets.Any()) return;

        await Task.WhenAll(sockets.Select(socket => SendDataToSocket(data, socket)));
    }

    private async Task SendDataToSocket(byte[] data, Socket socket)
    {
        await socket.SendAsync(data);
    }
}