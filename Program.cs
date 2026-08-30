using NinoChess;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using NinoChess.Networking;

//using var game = new NinoChess.MyGame();
//game.Run();

var stream = new MemoryStream();
var writer = new StreamWriter(stream);

var serverInterface = new TestNetworkLocalInterface();
var server = new Server(serverInterface);

var clientInterface = new TestNetworkLocalInterface();
var client = new Client(clientInterface);

var ip = new IPEndPoint(IPAddress.Parse("172.220.68.48"), 25565);
var ipForServer = new IPEndPoint(IPAddress.Any, 25565);

server.Start(ipForServer);
client.Connect(ip);

var line = Console.ReadLine();
while (!(line?.Equals("stop", StringComparison.OrdinalIgnoreCase) ?? false)) {
    if (line?.Equals("stopserver", StringComparison.OrdinalIgnoreCase) ?? false)
    {
        server.Stop();
    }

    if (line?.Equals("disconnect", StringComparison.OrdinalIgnoreCase) ?? false)
    {
        clientInterface.Input.SetResult(BitConverter.GetBytes(-2));

        do
        {
            clientInterface.DataUpdated.WaitOne();
            clientInterface.DataUpdated.Reset();
        }
        while (clientInterface.GetDataAsInt() != -3);

        client.Disconnect();
    }

    if (line?.Equals("printserver", StringComparison.OrdinalIgnoreCase) ?? false)
    {
        Console.WriteLine(serverInterface.GetDataAsInt());
    }

    if (line?.Equals("printclient", StringComparison.OrdinalIgnoreCase) ?? false)
    {
        Console.WriteLine(clientInterface.GetDataAsInt());
    }

    if (line?.Equals("inputclient", StringComparison.OrdinalIgnoreCase) ?? false)
    {
        var input = int.TryParse(Console.ReadLine(), out var value) ? value : -1;
        var data = BitConverter.GetBytes(input);
        clientInterface.Input.SetResult(data);
    }

    if (line?.Equals("inputserver", StringComparison.OrdinalIgnoreCase) ?? false)
    {
        var input = int.TryParse(Console.ReadLine(), out var value) ? value : -1;
        var data = BitConverter.GetBytes(input);
        serverInterface.Input.SetResult(data);
    }

    line = Console.ReadLine();
}

if (client.Connected)
{
    client.Disconnect();
}

if (server.Active)
{
    server.Stop();
}