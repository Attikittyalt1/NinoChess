using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace NinoChess.Networking;

public partial class TestNetworking2
{
    private readonly (Server core, ServerConnectionLayer layer) _server;

    private readonly List<(Client core, ClientConnectionLayer layer)> _clients = [];
    private int _port = 25565;
    private IPAddress? _lastAddress = null;

    public TestNetworking2()
    {
        _server.layer = new();
        _server.core = new(_server.layer);
    }

    public void StartConsoleInterface()
    {
        new Thread(() =>
        {
            var line = Console.ReadLine();
            while (true)
            {
                line = Console.ReadLine();
            }
        }).Start();
    }
}