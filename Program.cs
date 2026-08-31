using NinoChess;
using NinoChess.Networking;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

var game = new MyGame(new IPEndPoint(IPAddress.Parse(Console.ReadLine()), 25565));

var app = new NetworkingCommandLineApp();

_ = Task.Run(() =>
{
    app.StartConsoleInterface();
});


game.Run();