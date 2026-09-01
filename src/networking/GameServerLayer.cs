using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;

namespace NinoChess.Networking;

public class GameServerLayer(Server server, TurnManager turnManager)
{
    private readonly Dictionary<int, int?> _activeClients = [];
    private readonly Dictionary<int, int> _idsBeingAssigned = [];
    private readonly Dictionary<int, (int id, int? player)> _idRegistry = [];
    private readonly List<int> _players = [];

    public void Initialize()
    {
        server.SocketManager.OnReceived += (o, e) => Task.Run(async () =>
        {
            await OnRecieved((NetworkLocalSocketManager)o, e.Packet, e.ID);
        });
    }

    private async Task OnRecieved(NetworkLocalSocketManager socketManager, CustomPacket packet, int id, CancellationToken cancellationToken = default)
    {
        switch (packet.Type)
        {
            case PacketType.ResultWithoutData: return;
            case PacketType.Connect:
                {
                    if (!_activeClients.TryAdd(id, null))
                    {
                        Console.WriteLine("Could not activate socket. Socket is already active", id);
                        await SendResult(ReturnCode.FailureA);
                        return;
                    }

                    _idsBeingAssigned.Remove(id);

                    Console.WriteLine("Socket {0} is now active", id);
                    await SendResult(ReturnCode.Success);
                    return;
                }
            case PacketType.Disconnect:
                {
                    if (!_activeClients.Remove(id))
                    {
                        Console.WriteLine("Could not deactivate socket. Socket is already not active", id);
                        await SendResult(ReturnCode.FailureA);
                        return;
                    }

                    _idsBeingAssigned.Remove(id);

                    socketManager.StopWatchingSocket(id);
                    Console.WriteLine("Socket {0} is now inactive", id);
                    await SendResult(ReturnCode.Success);
                    return;
                }
            case PacketType.RequestID:
                {
                    if (!_activeClients.TryGetValue(id, out var previousAssignedID) || previousAssignedID != null)
                    {
                        Console.WriteLine("Failed to assign socket {0} with client. Socket is not active or is already linked.", id, previousAssignedID);
                        await SendResult(ReturnCode.FailureA);
                        return;
                    }

                    if (_idsBeingAssigned.ContainsKey(id))
                    {
                        // should probably add an option to unrequest. currently the client needs to disconnect and reconnect
                        Console.WriteLine("Client has already been requested by socket {0}", id);
                        await SendResult(ReturnCode.FailureB);
                        return;
                    }

                    var newAssignedID = GetAvailableID();

                    _idsBeingAssigned.Add(id, newAssignedID);

                    Console.WriteLine("Assigned socket {0} with client {1}", id, newAssignedID);
                    await socketManager.SendAsync(FromAssignID(newAssignedID), id, cancellationToken);
                    return;
                }
            case PacketType.LinkID:
                {
                    var assignedID = packet.ToLinkID();

                    if (!_activeClients.TryGetValue(id, out var previousAssignedID) || previousAssignedID != null)
                    {
                        Console.WriteLine("Failed to link socket {0} with client {1}. Socket is not active or is already linked.", id, assignedID);
                        await SendResult(0);
                        return;
                    }

                    if (_idRegistry.ContainsKey(assignedID))
                    {
                        _idRegistry[assignedID] = _idRegistry[assignedID] with { id = id };
                        _activeClients[id] = assignedID;

                        Console.WriteLine("Relinked socket {0} with client {1}", id, assignedID);
                        await SendResult(ReturnCode.Success);
                        return;
                    }

                    if (!_idsBeingAssigned.TryGetValue(id, out var listedAssignedID) || listedAssignedID != assignedID)
                    {
                        Console.WriteLine("Failed to link socket {0} with new client {1}. Client is not valid.", id, assignedID);
                        await SendResult(ReturnCode.FailureA);
                        return;
                    }

                    _idsBeingAssigned.Remove(id);
                    _idRegistry.Add(assignedID, (id, null));
                    _activeClients[id] = assignedID;

                    Console.WriteLine("Linked socket {0} with new client {1}", id, assignedID);
                    await SendResult(ReturnCode.Success);
                    return;
                }
            case PacketType.AbandonID:
                {
                    if (!_activeClients.TryGetValue(id, out var assignedID) || assignedID == null)
                    {
                        Console.WriteLine("Failed to unlink socket {0}", id);
                        await SendResult(ReturnCode.FailureA);
                        return;
                    }

                    var player = _idRegistry[assignedID.Value].player;

                    if (player is not null)
                    {
                        _players.Remove(player.Value);
                    }

                    _activeClients[id] = null;
                    _idRegistry.Remove(assignedID.Value);

                    Console.WriteLine("Successfully unlinked client {0}", assignedID);
                    await SendResult(ReturnCode.Success);
                    return;
                }
            case PacketType.MessageInteger:
                {
                    var message = packet.ToMessageInt();

                    Console.WriteLine("Recieved integer from socket {0}: {1}", id, message);
                    await SendResult(ReturnCode.Success);
                    return;
                }
            case PacketType.MessageString:
                {
                    var message = packet.ToMessageString();

                    Console.WriteLine("Recieved string from socket {0}: {1}", id, message);
                    await SendResult(ReturnCode.Success);
                    return;
                }
            case PacketType.MessageObject:
                {

                    throw new NotImplementedException();
                    return;
                }
            case PacketType.DoMove:
                {
                    var (move, turnCount) = packet.ToMove();

                    if (!_activeClients.TryGetValue(id, out var assignedID) || assignedID is null)
                    {
                        Console.WriteLine("Server recieved move from inactive or unlinked socket {0}", id);
                        await SendResult(ReturnCode.FailureA);
                        return;
                    }

                    if (turnCount != turnManager.Turn)
                    {
                        Console.WriteLine("Server recieved move from client {0} with invalid turn count. Turn delta: {1}", assignedID, turnCount - turnManager.Turn);
                        await SendResult(ReturnCode.FailureB);
                        return;
                    }

                    var currentPlayer = turnManager.CurrentPlayer;

                    if (!_players.Contains(currentPlayer) && _idRegistry[assignedID.Value].player == null)
                    {
                        _idRegistry[assignedID.Value] = _idRegistry[assignedID.Value] with { player = currentPlayer };
                        _players.Add(currentPlayer);
                        Console.WriteLine("Added client {0} as player {1}", assignedID, currentPlayer);
                    }

                    var supposedPlayer = _idRegistry[assignedID.Value].player;

                    if (supposedPlayer != currentPlayer)
                    {
                        Console.WriteLine("Server recieved move from player {0} when it is not their turn", currentPlayer);
                        await SendResult(ReturnCode.FailureC);
                        return;
                    }

                    if (!turnManager.IsValid(move))
                    {
                        Console.WriteLine("Server recieved invalid move from player {0}", currentPlayer);
                        await SendResult(ReturnCode.FailureD);
                        return;
                    }

                    turnManager.Do(move);

                    Console.WriteLine("Successfully recieved turn from player {0}", currentPlayer);
                    await SendResult(ReturnCode.Success);

                    Console.WriteLine("Sending moves to linked players");
                    await foreach (var socketID in socketManager.ConnectedSockets.ToAsyncEnumerable())
                    {
                        if (!_activeClients.TryGetValue(socketID, out var socketAssignedID) || socketAssignedID is null)
                        {
                            continue;
                        }

                        var returnCode = await socketManager.SendAndRecieveReturnCodeAsync(packet, socketID, cancellationToken);

                        if (IsSuccess(returnCode))
                        {
                            Console.WriteLine("Successfully sent move to socket {0}.", socketID);
                        }
                        else
                        {
                            Console.WriteLine("Failed to send move to socket {0}. Return code: {1}", socketID, returnCode);
                        }
                    }

                    return;
                }
            default: throw new ArgumentException(string.Format("Server recieved bad packet from socket {0}. Result format invalid for server: {1}", id, Enum.GetName(packet.Type)));
        }

        async Task SendResult(ReturnCode code)
        {
            await socketManager.SendAsync(FromResultWithoutData(packet.Type, code), id, cancellationToken);
        }
    }
    private int GetAvailableID()
    {
        int id = 0;

        while (_idRegistry.ContainsKey(id))
        {
            id++;
        }

        return id;
    }

}