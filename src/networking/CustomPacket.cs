using System;
using System.Linq;
using System.Text;

namespace NinoChess.Networking;

class CustomPacket
{
    public enum PacketFormat : byte
    {
        Invalid = 0,
        Connect = 1,
        Disconnect = 2,
        RequestID = 3,
        AssignID = 4,
        UnregisterID = 5,
        Shutdown = 6,
        Maintain = 7,
        MessageInteger = 8,
        MessageString = 9,
        MessageObject = 10,
        Move = 11,
    }

    public PacketFormat Format { get; private init; } = PacketFormat.Invalid;
    private object? Data { get; init; }
    public byte SenderID { get; private set; } = 0;

    public int GetSize() => Format switch
    {
        PacketFormat.Connect => 2,
        PacketFormat.Disconnect => 2,
        PacketFormat.Maintain => 2,
        PacketFormat.MessageInteger => 6,
        PacketFormat.MessageString => 6 + Encoding.UTF8.GetByteCount((string) Data),
        PacketFormat.MessageObject => throw new NotImplementedException(),
        PacketFormat.Move => throw new NotImplementedException(),
        _ => throw new FormatException("Invalid packet format")
    };

    public static byte[] ToBytesRaw(CustomPacket packet)
    {
        return [(byte)packet.Format, packet.SenderID, .. packet.Format switch
        {
            PacketFormat.Connect => [],
            PacketFormat.Disconnect => [],
            PacketFormat.Maintain => [],
            PacketFormat.MessageInteger => BitConverter.GetBytes((int)packet.Data),
            PacketFormat.MessageString => BitConverter.GetBytes((int)packet.Data).Concat(Encoding.UTF8.GetBytes((string)packet.Data)),
            PacketFormat.MessageObject => throw new NotImplementedException(),
            PacketFormat.Move => throw new NotImplementedException(),
            _ => throw new FormatException("Invalid packet format")
        }];
    }

    public static CustomPacket FromBytesRaw(byte[] data, int size)
    {
        if (size <= 0)
        {
            throw new ArgumentException("Must have at least one byte to process");
        }

        return ((PacketFormat) data[0] switch
        {
            PacketFormat.Connect => Connect,
            PacketFormat.Disconnect => Disconnect,
            PacketFormat.Maintain => Maintain,
            PacketFormat.MessageInteger => FromMessage(BitConverter.ToInt32(data, 2)),
            PacketFormat.MessageString => FromMessage(Encoding.UTF8.GetString(data, 6, BitConverter.ToInt32(data, 2))),
            PacketFormat.MessageObject => throw new NotImplementedException(),
            PacketFormat.Move => throw new NotImplementedException(),
            _ => throw new FormatException("Invalid packet format")
        }).WithSenderID(data[1]);
    }

    public CustomPacket WithSenderID(byte id)
    {
        SenderID = id;
        return this;
    }

    public static CustomPacket Invalid => new() { Format = PacketFormat.Invalid };
    public static CustomPacket Connect => new() { Format = PacketFormat.Connect };
    public static CustomPacket Disconnect => new() { Format = PacketFormat.Disconnect };
    public static CustomPacket Maintain => new() { Format = PacketFormat.Maintain };
    public static CustomPacket FromMessage(int message) => new() { Format = PacketFormat.MessageInteger, Data = message };
    public static CustomPacket FromMessage(string message) => new() { Format = PacketFormat.MessageString, Data = message };
    public static CustomPacket FromMessage(object o) => new() { Format = PacketFormat.MessageObject, Data = o };
}