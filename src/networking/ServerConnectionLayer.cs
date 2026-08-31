using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;

namespace NinoChess.Networking;

public class ServerConnectionLayer() : GenericConnectionLayer
{
    private readonly Dictionary<int, int> _idRegistry = [];
    private readonly Dictionary<int, int> _idsBeingAssigned = [];
    private readonly Dictionary<int, int?> _activeClients = [];

    private int GetAvailableID()
    {
        int id = 0;

        while (_idRegistry.ContainsKey(id))
        {
            id++;
        }

        return id;
    }

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

                    HandleSuccess(type);

                    response = null;
                    break;
                }
            case PacketFormat.Connect:
                {
                    if (!_activeClients.TryAdd(id, null))
                    {
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    _idsBeingAssigned.Remove(id);

                    SendResult(ReturnCode.Success);
                    break;
                }
            case PacketFormat.Disconnect:
                {
                    if (!_activeClients.Remove(id))
                    {
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    _idsBeingAssigned.Remove(id);

                    disconnect = true;
                    SendResult(ReturnCode.Success);
                    break;
                }
            case PacketFormat.RequestID:
                {
                    if (!_activeClients.TryGetValue(id, out var previousAssignedID) || previousAssignedID != null)
                    {
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    var newAssignedID = GetAvailableID();

                    if (!_idsBeingAssigned.TryAdd(id, newAssignedID))
                    {
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    response = FromAssignID(newAssignedID);
                    break;
                }
            case PacketFormat.LinkID:
                {
                    if (!_activeClients.TryGetValue(id, out var previousAssignedID) || previousAssignedID != null)
                    {
                        SendResult(0);
                        break;
                    }

                    var assignedID = packet.ToLinkID();

                    if (_idRegistry.ContainsKey(assignedID))
                    {
                        _idRegistry[assignedID] = id;
                        _activeClients[id] = assignedID;

                        SendResult(ReturnCode.Success);
                        break;
                    }

                    if (_idsBeingAssigned.TryGetValue(id, out var listedAssignedID) || listedAssignedID != assignedID)
                    {
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    _idsBeingAssigned.Remove(id);
                    _idRegistry.Add(assignedID, id);
                    _activeClients[id] = assignedID;

                    SendResult(ReturnCode.Success);
                    break;
                }
            case PacketFormat.AbandonID:
                {
                    if (!_activeClients.TryGetValue(id, out var assignedID) || assignedID == null)
                    {
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    _idRegistry.Remove(id);
                    _activeClients[id] = null;

                    SendResult(ReturnCode.Success);
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
            case PacketFormat.Move:
                {

                    throw new NotImplementedException();
                    break;
                }
            default: throw new ArgumentException(string.Format("Server could not send packet. Result format invalid for server: {0}", Enum.GetName(packet.Format)));
        }

        return (response, disconnect);

        void SendResult(ReturnCode code)
        {
            response = FromResultWithoutData(packet.Format, code);
        }
    }
    private void HandleSuccess(PacketFormat type)
    {
        switch (type)
        {
            case PacketFormat.MessageInteger:
                {

                    Console.WriteLine("Successfully sent integer to client.");
                    break;
                }
            case PacketFormat.MessageString:
                {
                    Console.WriteLine("Successfully sent string to client.");
                    break;
                }
            case PacketFormat.MessageObject:
                {

                    throw new NotImplementedException();
                    break;
                }
            default: throw new ArgumentException(string.Format("Server could not send packet. Result format invalid for server: {0}", Enum.GetName(type)));
        }
    }
}