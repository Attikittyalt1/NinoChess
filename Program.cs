using NinoChess.Networking;

var test = new TestNetworking();

var ip = args[0];
var port = int.Parse(args[1]);

test.CreateServer(port);

test.CreateClient(ip, int.Parse(args[1]));

test.StartConsoleInterface();

//var game = new MyGame();
//game.Run();