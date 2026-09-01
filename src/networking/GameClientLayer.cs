using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;
using static NinoChess.Pieces.Scholar;

namespace NinoChess.Networking;

public class GameClientLayer(Client client, TurnManager turnManager)
{
    public interface IClientTurn
    {
        public bool HasBeenMarkedInvalid { get; set; }
        public int TurnOffset { get; }
    }
    public record struct ClientMove(MoveInfo MoveInfo) : IClientTurn
    {
        public bool HasBeenMarkedInvalid { get; set; }
        public readonly int TurnOffset => 1;
    }

    public readonly Queue<IClientTurn> NetworkTurnBuffer = [];
    public int NetworkTurnOffset;

    public readonly Queue<IClientTurn> LocalTurnBuffer = [];
    public int LocalTurnOffset;

    public int? PlayerID = null; // this should have a private getter but i still need to move more things here from the game
    public bool _active = false; // this should be fully private but i still need to move more things here from the game

    public int? _possibleRegistrationID = null;
    public int? _registrationID = null;

    public void Initialize()
    {
        client.SocketManager.OnReceived += (o, e) => Task.Run(async () =>
        {
            await OnRecieved((NetworkLocalSocketManager)o, e.Packet, e.ID);
        });
    }

    private async Task OnRecieved(NetworkLocalSocketManager socketManager, CustomPacket packet, int id, CancellationToken cancellationToken = default)
    {
        switch (packet.Type)
        {
            case PacketType.ResultWithoutData: return;
            case PacketType.AssignID: return;
            case PacketType.Shutdown:
                {
                    if (!_active)
                    {
                        Console.WriteLine("TODO: add failure message A");
                        await SendResult(ReturnCode.FailureA);
                        break;
                    }

                    var code = await socketManager.SendAndRecieveResponseAsync(Disconnect, id);

                    Console.WriteLine("TODO: process return code for shutdown");
                    break;
                }
            case PacketType.MessageInteger:
                {
                    var message = packet.ToMessageInt();

                    Console.WriteLine("Recieved integer from server: {0}", message);
                    await SendResult(ReturnCode.Success);
                    break;
                }
            case PacketType.MessageString:
                {
                    var message = packet.ToMessageString();

                    Console.WriteLine("Recieved string from server: {0}", message);
                    await SendResult(ReturnCode.Success);
                    break;
                }
            case PacketType.MessageObject:
                {
                    throw new NotImplementedException();
                    break;
                }
            case PacketType.DoMove:
                {
                    var (move, turnCount) = packet.ToMove();

                    var theirTurnRelative = turnCount - NetworkTurnOffset;
                    var myTurnRelative = turnManager.Turn - LocalTurnOffset;

                    if (theirTurnRelative != myTurnRelative)
                    {
                        Console.WriteLine("Turn count mismatch: Client ({0}) should be the same as Server ({1})", myTurnRelative, theirTurnRelative);
                        await SendResult(ReturnCode.FailureA);
                        break;
                    }

                    if (LocalTurnBuffer.TryPeek(out var myMove))
                    {
                        if (myMove is ClientMove clientMove && clientMove.MoveInfo == move)
                        {
                            LocalTurnBuffer.Dequeue();
                            LocalTurnOffset -= myMove.TurnOffset;

                            Console.WriteLine("Successfully recieved my turn from server");
                            await SendResult(ReturnCode.Success);
                            break;
                        }

                        myMove.HasBeenMarkedInvalid = true;

                        Console.WriteLine("TODO: add failure message B");
                        await SendResult(ReturnCode.FailureB);
                    }

                    var turn = new ClientMove(move);

                    NetworkTurnBuffer.Enqueue(turn);
                    NetworkTurnOffset += turn.TurnOffset;

                    Console.WriteLine("Successfully recieved another player's turn from server");
                    await SendResult(ReturnCode.Success);
                    break;
                }
            default: throw new ArgumentException(string.Format("Client could not send packet. Result format invalid for client: {0}", Enum.GetName(packet.Type)));
        }

        async Task SendResult(ReturnCode code)
        {
            await socketManager.SendAsync(FromResultWithoutData(packet.Type, code), id, cancellationToken);
        }
    }
}