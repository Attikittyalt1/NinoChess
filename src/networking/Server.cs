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

    private readonly Socket _listener = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private CancellationTokenSource? _tokenSource;
    private IPEndPoint? _endPoint;
    private Task? _tasksEnded;

    public void Start(IPEndPoint endPoint)
    {
        if (Running == true)
        {
            throw new InvalidOperationException("Cannot start server that is already running.");
        }

        Running = true;

        Debug.WriteLine("Starting server.");

        _endPoint = endPoint;
        _listener.Bind(endPoint);
        _listener.Listen();

        _tokenSource = new CancellationTokenSource();

        var manager = new NetworkLocalSocketManager(gameInterface);

        var startedLocal = new TaskCompletionSource();
        var startedListener = new TaskCompletionSource();
        var endedLocal = new TaskCompletionSource();
        var endedListener = new TaskCompletionSource();
        var endedSockets = new TaskCompletionSource();
        _tasksEnded = Task.WhenAll(endedLocal.Task, startedListener.Task, endedSockets.Task);

        _ = Task.Run(() => manager.StartWatchingLocal(_tokenSource.Token, startedLocal, endedLocal));
        _ = Task.Run(() => manager.StartListeningSocket(_listener, _tokenSource.Token, startedListener, endedListener, endedSockets));

        Task.WaitAll(startedLocal.Task, startedListener.Task);

        Debug.WriteLine("Started server.");
    }

    public void Stop()
    {
        if (Running == false)
        {
            throw new InvalidOperationException("Cannot stop server that is not running.");
        }

        Running = false;

        Debug.WriteLine("Stopping server.");

        _endPoint = null;

        _tokenSource.Cancel();

        _tasksEnded.Wait();

        _listener.Close();

        Debug.WriteLine("Stopped server.");
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