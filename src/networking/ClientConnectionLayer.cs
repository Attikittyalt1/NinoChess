using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;

namespace NinoChess.Networking;

public class ClientConnectionLayer(TurnManager turnManager) : GenericConnectionLayer
{
    public event EventHandler? Connected;

    public event EventHandler? Registered;

    public event EventHandler? Unregistered;

    public event EventHandler? Disconnected;

    public readonly Queue<MoveInfo> NetworkMoveBuffer = [];
    public readonly Queue<MoveInfo> LocalMoveBuffer = [];
    public int UndoBuffer = 0;

    private int? _registrationID = null;
    private int? _possibleRegistrationID = null;
    private bool _active;

    protected override async Task<(CustomPacket? response, bool disconnect)> HandleIncomingPacket(CustomPacket packet, int id)
    {
        CustomPacket? response;
        bool disconnect = false;

        switch (packet.Format)
        {
            case PacketFormat.ResultWithoutData:
                {
                    var (type, code) = packet.ToResultWithoutData();

                    if (!IsSuccess(code))
                    {
                        throw new Exception(string.Format("Packet result failed with type {0} and error code {1}.", type, Enum.GetName(code)));
                    }

                    HandleSuccess(type, ref disconnect);

                    response = null;
                    break;
                }
            case PacketFormat.AssignID:
                {
                    if (!_active || _registrationID != null)
                    {
                        SendResult(ReturnCode.FailureA);
                        break;
                    }

                    _possibleRegistrationID = packet.ToAssignID();

                    response = FromLinkID(_possibleRegistrationID.Value);
                    break;
                }
            case PacketFormat.Shutdown:
                {
                    if (!_active)
                    {
                        SendResult(ReturnCode.FailureA);
                        break;
                    }

                    response = Disconnect;
                    break;
                }
            case PacketFormat.MessageInteger:
                {
                    var message = packet.ToMessageInt();

                    Console.WriteLine("Recieved Integer: {0}", message);
                    SendResult(ReturnCode.Success);
                    break;
                }
            case PacketFormat.MessageString:
                {
                    var message = packet.ToMessageInt();

                    Console.WriteLine("Recieved String: {0}", message);
                    SendResult(ReturnCode.Success);
                    break;
                }
            case PacketFormat.MessageObject:
                {

                    throw new NotImplementedException();
                    break;
                }
            case PacketFormat.Turn:
                {
                    var (move, turnCount) = packet.ToTurn();

                    var theirTurnRelative = turnCount - NetworkMoveBuffer.Count;
                    var myTurnRelative = turnManager.Turn - LocalMoveBuffer.Count;

                    if (theirTurnRelative != myTurnRelative)
                    {
                        Console.WriteLine("Turn count mismatch: Client ({0}) should be the same as Server ({1})", myTurnRelative, theirTurnRelative);
                        SendResult(ReturnCode.FailureA);
                        break;
                    }

                    if (LocalMoveBuffer.TryDequeue(out var myMove))
                    {
                        if (myMove == move)
                        {
                            SendResult(ReturnCode.Success);
                            break;
                        }

                        UndoBuffer += LocalMoveBuffer.Count + 1;

                        LocalMoveBuffer.Clear();
                    }

                    NetworkMoveBuffer.Enqueue(move);

                    SendResult(ReturnCode.Success);
                    break;
                }
            default: throw new ArgumentException(string.Format("Client could not send packet. Result format invalid for client: {0}", Enum.GetName(packet.Format)));
        }

        return (response, disconnect);

        void SendResult(ReturnCode code)
        {
            response = FromResultWithoutData(packet.Format, code);
        }
    }
    private void HandleSuccess(PacketFormat type, ref bool disconnect)
    {
        switch (type)
        {
            case PacketFormat.Connect:
                {
                    _active = true;

                    Console.WriteLine("Successfully connected to server.");

                    Connected?.Invoke(this, EventArgs.Empty);
                    break;
                }
            case PacketFormat.Disconnect:
                {
                    _active = false;
                    disconnect = true;

                    Console.WriteLine("Successfully disconnected from server.");

                    Disconnected?.Invoke(this, EventArgs.Empty);
                    break;
                }
            case PacketFormat.LinkID:
                {
                    _registrationID = _possibleRegistrationID;
                    _possibleRegistrationID = null;


                    Console.WriteLine("Successfully linked id: {0}", _registrationID);

                    Registered?.Invoke(this, EventArgs.Empty);
                    break;
                }
            case PacketFormat.AbandonID:
                {
                    _registrationID = null;


                    Console.WriteLine("Successfully abandoned id.");

                    Unregistered?.Invoke(this, EventArgs.Empty);
                    break;
                }
            case PacketFormat.MessageInteger:
                {

                    Console.WriteLine("Successfully sent integer to server.");
                    break;
                }
            case PacketFormat.MessageString:
                {
                    Console.WriteLine("Successfully sent string to server.");
                    break;
                }
            case PacketFormat.MessageObject:
                {

                    throw new NotImplementedException();
                    break;
                }
            case PacketFormat.Turn:
                {
                    Console.WriteLine("Successfully sent move to server.");
                    break;
                }
            default: throw new ArgumentException(string.Format("Client could not send packet. Result format invalid for client: {0}", Enum.GetName(type)));
        }
    }
}