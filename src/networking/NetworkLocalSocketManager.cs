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

public class NetworkLocalSocketManager(INetworkLocalConnectionLayer connectionLayer)
{
    public bool HasConnectedSockets => _sockets.Count > 0;

    private readonly Dictionary<int, Socket> _sockets = [];
    private bool _watchingLocal = false;
    private bool _listening = false;

    public void StartWatchingLocal(TaskCompletionSource started, TaskCompletionSource ended, CancellationToken cancellationToken = default)
    {
        if (_watchingLocal)
        {
            throw new ArgumentException("NetworkLocalSocketManager is already watching local");
        }

        _watchingLocal = true;
        Console.WriteLine("Started watching local");

        started.SetResult();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                CheckRecieveDataFromLocal(cancellationToken).Wait();
            }
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
            _watchingLocal = false;
            Console.WriteLine("Stopped watching local");

            ended.SetResult();
        }
    }

    public void StartWatchingSocket(Socket socket, TaskCompletionSource started, TaskCompletionSource ended, CancellationToken cancellationToken = default)
    {
        if (_sockets.ContainsValue(socket))
        {
            throw new ArgumentException("NetworkLocalSocketManager is already watching socket.");
        }

        var id = 0;

        while (_sockets.ContainsKey(id)) id++;

        _sockets.Add(id, socket);
        Console.WriteLine("Started watching socket");

        started.SetResult();

        try
        {
            while (socket.Connected && !cancellationToken.IsCancellationRequested)
            {
                if (CheckRecieveDataFromSocket(socket, id, cancellationToken).Result)
                {
                    break;
                }
            }
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
            _sockets.Remove(id);
            Console.WriteLine("Stopped watching socket");

            ended.SetResult();
        }
    }

    public void StartListeningSocket(Socket socket, TaskCompletionSource started, TaskCompletionSource endedListening, TaskCompletionSource endedSockets, CancellationToken cancellationToken = default)
    {
        if (_listening)
        {
            throw new ArgumentException("NetworkLocalSocketManager is already listeniing to a socket");
        }

        _listening = true;
        Console.WriteLine("Started listening to socket");

        started.SetResult();

        var tasks = new List<Task>();
        endedSockets.SetFromTask(Task.WhenAll(tasks));

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var startedSocket = new TaskCompletionSource();
                var endedSocket = new TaskCompletionSource();
                CheckNewConnection(socket, startedSocket, endedSocket, cancellationToken).Wait();

                tasks.Add(endedSocket.Task);
            }
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
            _listening = false;
            Console.WriteLine("Stopped listening to socket");

            endedListening.SetResult();
        }
    }

    private async Task CheckNewConnection(Socket socket, TaskCompletionSource startedSocket, TaskCompletionSource endedSocket, CancellationToken cancellationToken = default)
    {
        var newSocket = await socket.AcceptAsync(cancellationToken);

        _ = Task.Run(() =>
        {
            StartWatchingSocket(newSocket, startedSocket, endedSocket, cancellationToken);
        }, cancellationToken);
    }

    private async Task<bool> CheckRecieveDataFromSocket(Socket socket, int id, CancellationToken cancellationToken)
    {
        var buffer = new byte[connectionLayer.MaxBufferSizeFromNetwork];

        var received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);

        if (received <= 0) return false;

        Console.WriteLine("Received data from socket with id: " + id);

        var (response, disconnect) = await connectionLayer.OnRecieveDataAsync(buffer, received, id);

        if (response is not null)
        {
            Console.WriteLine("Sending response");

            await SendDataToSocket(response, socket);
        }
        else
        {
            Console.WriteLine("Finished reponse chain");
        }

        return disconnect;
    }

    public async Task CheckRecieveDataFromLocal(CancellationToken cancellationToken)
    {
        var (data, id) = await connectionLayer.GetDataToSendAsync(cancellationToken);

        if (id.HasValue)
        {
            if (!_sockets.TryGetValue(id.Value, out var socket))
            {
                throw new InvalidOperationException("Cannot send data to unregistered socket.");
            }

            Console.WriteLine("Sending data to socket with id: " + id.Value);
            await SendDataToSocket(data, socket);
        } else
        {
            Console.WriteLine("Sending data to connected sockets");
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
        ArgumentOutOfRangeException.ThrowIfGreaterThan(data.Length, connectionLayer.MaxBufferSizeFromLocal);
        await socket.SendAsync(data);
    }
}