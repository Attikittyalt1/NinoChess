using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NinoChess.Networking;

public class Server()
{
    public bool Running { get; private set; } = false;
    public bool Starting { get; private set; } = false;
    public bool Stopping { get; private set; } = false;
    public bool HasClients => Running && SocketManager.ConnectedSockets.Count > 0;

    public readonly NetworkLocalSocketManager SocketManager = new();

    private Socket? _listener;
    private IPEndPoint? _endPoint;
    private Task? _stoppedListening;

    public async Task StartAsync(IPEndPoint endPoint, CancellationToken cancellationToken = default)
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

            _stoppedListening = SocketManager.StartListeningSocket(_listener, cancellationToken);

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
            SocketManager.StopListeningSocket();
            await _stoppedListening;

            _listener.Close();

            _endPoint = null;
            _listener = null;
            _stoppedListening = null;

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