using System;
using System.Threading;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;

namespace NinoChess.Networking;

public class ClientConnectionLayer() : GenericConnectionLayer
{
    public ManualResetEvent Connected { get; private set; } = new(false);

    public ManualResetEvent Registered { get; private set; } = new(false);

    public ManualResetEvent Unregistered { get; private set; } = new(false);

    public ManualResetEvent Disconnected { get; private set; } = new(false);

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
                        SendResult(ReturnCode.Failure);
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
                        SendResult(ReturnCode.Failure);
                        break;
                    }

                    response = Disconnect;
                    break;
                }
            default: throw new ArgumentException("Client could not send packet. Format invalid for client.");
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

                    Connected.Set();
                    Disconnected.Reset();
                    break;
                }
            case PacketFormat.Disconnect:
                {
                    _active = false;
                    disconnect = true;

                    Disconnected.Set();
                    Connected.Reset();
                    break;
                }
            case PacketFormat.LinkID:
                {
                    _registrationID = _possibleRegistrationID;
                    _possibleRegistrationID = null;

                    Registered.Set();
                    Unregistered.Reset();
                    break;
                }
            case PacketFormat.AbandonID:
                {
                    _registrationID = null;

                    Unregistered.Set();
                    Registered.Reset();
                    break;
                }
            case PacketFormat.MessageInteger:
                {

                    Console.WriteLine("Successfully sent integer,");
                    break;
                }
            case PacketFormat.MessageString:
                {
                    Console.WriteLine("Successfully sent string,");
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
            default: throw new ArgumentException("Client could not send packet. Result format invalid for client.");
        }
    }
}