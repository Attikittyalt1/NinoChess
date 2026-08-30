using NinoChess;
using NinoChess.Networking;

var test = new TestNetworking();

test.CreateServer(25565);

test.CreateClient("172.220.68.48", 25565);

test.StartConsoleInterface();

//var game = new MyGame();
//game.Run();