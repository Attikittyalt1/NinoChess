using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NinoChess.Networking;

public class Server(INetworkLocalInterface gameInterface)
{
    public bool Running { get; private set; } = false;
    public bool Starting { get; private set; } = false;
    public bool Stopping { get; private set; } = false;
    public bool HasClients => Running && _manager.HasConnectedSockets;

    private Socket? _listener;
    private CancellationTokenSource? _cancellationTokenSource;
    private IPEndPoint? _endPoint;
    private Task? _tasksEnded;
    private NetworkLocalSocketManager? _manager;

    public async Task StartAsync(IPEndPoint endPoint)
    {
        if (Running == true)
        {
            throw new InvalidOperationException("Server is already running.");
        }

        if (Starting == true)
        {
            throw new InvalidOperationException("Server is already starting.");
        }

        Starting = true;

        Console.WriteLine("Starting server.");

        try
        {
            _listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _endPoint = endPoint;
            _listener.Bind(endPoint);
            _listener.Listen();

            _cancellationTokenSource = new CancellationTokenSource();

            _manager = new NetworkLocalSocketManager(gameInterface);

            var startedLocal = new TaskCompletionSource();
            var startedListener = new TaskCompletionSource();
            var endedLocal = new TaskCompletionSource();
            var endedListener = new TaskCompletionSource();
            var endedSockets = new TaskCompletionSource();
            _tasksEnded = Task.WhenAll(endedLocal.Task, startedListener.Task, endedSockets.Task);

            _ = Task.Run(() => _manager.StartWatchingLocal(startedLocal, endedLocal, _cancellationTokenSource.Token));
            _ = Task.Run(() => _manager.StartListeningSocket(_listener, startedListener, endedListener, endedSockets, _cancellationTokenSource.Token));

            await Task.WhenAll(startedLocal.Task, startedListener.Task);

            Running = true;

            Console.WriteLine("Started server.");

        } 
        finally
        {
            Starting = false;
        }
    }

    public void Start(IPEndPoint endPoint)
    {
        StartAsync(endPoint).Wait();
    }

    public async Task StopAsync()
    {
        if (Running == false)
        {
            throw new InvalidOperationException("Server is already stopped.");
        }

        if (Stopping == true)
        {
            throw new InvalidOperationException("Server is already stopping.");
        }

        Stopping = true;

        Console.WriteLine("Stopping server.");

        try
        {
            if (!_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }

            await _tasksEnded;

            _listener.Close();

            _endPoint = null;
            _listener = null;
            _tasksEnded = null;
            _cancellationTokenSource = null;
            _manager = null;

            Running = false;

            Console.WriteLine("Stopped server.");
        } 
        finally
        {
            Stopping = false;
        }
    }

    public void Stop()
    {
        StopAsync().Wait();
    }

    private bool IsSocketHealthy()
    {
        if (!_listener.Connected || !Running)
        {
            return false;
        }

        //Do a ping test to see if the server is reachable
        try
        {
            var pingTest = new Ping();
            var reply = pingTest.Send(_endPoint.Address);
            if (reply.Status != IPStatus.Success) return false;
        }
        catch (PingException)
        {
            return false;
        }

        //See if the tcp state is ok
        if (_listener.Poll(5000, SelectMode.SelectRead) && (_listener.Available == 0))
        {
            return false;
        }

        return true;
    }
}