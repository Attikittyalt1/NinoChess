using System;
using System.IO;
using System.Text;

namespace NinoChess.Networking;

public class CustomPacket
{
    public enum PacketFormat : byte
    {
        Invalid = 0,
        ResultWithoutData = 1,
        Connect = 2,
        Disconnect = 3,
        RequestID = 4,
        AssignID = 5,
        LinkID = 6,
        AbandonID = 7,
        Shutdown = 8,
        MessageInteger = 9,
        MessageString = 10,
        MessageObject = 11,
        Turn = 12,
    }

    public enum ReturnCode : byte
    {
        Failure = 0,
        Success = 1,
        FailureA = 2,
        FailureB = 3,
        FailureC = 4,
        FailureD = 5,
        FailureE = 6
    }

    public PacketFormat Format { get; private init; } = PacketFormat.Invalid;
    private object? Data { get; init; }

    public int GetSize() => Format switch
    {
        PacketFormat.Connect => 1,
        PacketFormat.ResultWithoutData => 3,
        PacketFormat.Disconnect => 1,
        PacketFormat.RequestID => 1,
        PacketFormat.AssignID => 5,
        PacketFormat.LinkID => 5,
        PacketFormat.AbandonID => 1,
        PacketFormat.Shutdown => 1,
        PacketFormat.MessageInteger => 5,
        PacketFormat.MessageString => 5 + Encoding.UTF8.GetByteCount((string) Data),
        PacketFormat.MessageObject => throw new NotImplementedException(),
        PacketFormat.Turn => 21,
        _ => throw new InvalidDataException("Invalid packet format")
    };
    
    public byte[] ToBytesRaw()
    {
        return [(byte)Format, .. Format switch
        {
            PacketFormat.ResultWithoutData => ResultWithoutDataToBytes(),
            PacketFormat.Connect => [],
            PacketFormat.Disconnect => [],
            PacketFormat.RequestID => [],
            PacketFormat.AssignID => BitConverter.GetBytes((int)Data),
            PacketFormat.LinkID => BitConverter.GetBytes((int)Data),
            PacketFormat.AbandonID => [],
            PacketFormat.Shutdown => [],
            PacketFormat.MessageInteger => [..BitConverter.GetBytes((int)Data)],
            PacketFormat.MessageString => [..BitConverter.GetBytes((int)Data), ..Encoding.UTF8.GetBytes((string)Data)],
            PacketFormat.MessageObject => throw new NotImplementedException(),
            PacketFormat.Turn => MoveToBytes(),
        _ => throw new InvalidDataException("Invalid packet format")
        }];

        byte[] ResultWithoutDataToBytes()
        {
            (PacketFormat type, ReturnCode code) = ((PacketFormat type, ReturnCode code))Data;

            return [(byte) type, (byte) code];
        }

        byte[] MoveToBytes()
        {
            (MoveInfo move, int turn) = ((MoveInfo, int))Data;

            return 
                [
                ..BitConverter.GetBytes(turn),
                ..BitConverter.GetBytes(move.Origin.X),
                ..BitConverter.GetBytes(move.Origin.Y),
                ..BitConverter.GetBytes(move.Target.X),
                ..BitConverter.GetBytes(move.Target.Y)
                ];
        }
    }

    public static byte[] ToBytesRaw(CustomPacket packet) => packet.ToBytesRaw();

    public static CustomPacket FromBytesRaw(byte[] data, int size)
    {
        ValidateDataSize(data, size);

        return ((PacketFormat) data[0] switch
        {
            PacketFormat.ResultWithoutData => FromResultWithoutData((PacketFormat)data[1], (ReturnCode)data[2]),
            PacketFormat.Connect => Connect,
            PacketFormat.Disconnect => Disconnect,
            PacketFormat.RequestID => RequestID,
            PacketFormat.AssignID => FromAssignID(BitConverter.ToInt32(data, 1)),
            PacketFormat.LinkID => FromLinkID(BitConverter.ToInt32(data, 1)),
            PacketFormat.AbandonID => AbandonID,
            PacketFormat.Shutdown => Shutdown,
            PacketFormat.MessageInteger => FromMessage(BitConverter.ToInt32(data, 1)),
            PacketFormat.MessageString => FromMessage(Encoding.UTF8.GetString(data, 5, BitConverter.ToInt32(data, 1))),
            PacketFormat.MessageObject => throw new NotImplementedException(),
            PacketFormat.Turn => BytesToMove(),
            _ => throw new InvalidDataException("Invalid packet format")
        });

        CustomPacket BytesToMove()
        {
            var turn = BitConverter.ToInt32(data, 1);
            var origin = new Position(BitConverter.ToInt32(data, 5), BitConverter.ToInt32(data, 9));
            var target = new Position(BitConverter.ToInt32(data, 13), BitConverter.ToInt32(data, 17));

            return FromTurn(new(origin, target), turn);
        }
    }

    public static void ValidateDataSize(byte[] data, int size)
    {
        if (data.Length < 1)
        {
            throw new ArgumentException("Data is empty.");
        }

        var format = (PacketFormat)data[0];

        var minimumHeaderSize = format switch
        {
            PacketFormat.ResultWithoutData => 3,
            PacketFormat.Connect => 1,
            PacketFormat.Disconnect => 1,
            PacketFormat.RequestID => 1,
            PacketFormat.AssignID => 5,
            PacketFormat.LinkID => 5,
            PacketFormat.AbandonID => 1,
            PacketFormat.Shutdown => 1,
            PacketFormat.MessageInteger => 5,
            PacketFormat.MessageString => 5,
            PacketFormat.MessageObject => throw new NotImplementedException(),
            PacketFormat.Turn => 21,
            _ => throw new InvalidDataException("Invalid packet format")
        };

        if (size < minimumHeaderSize)
        {
            throw new ArgumentException("Data size under minimum header size.");
        }

        var minimumTotalSize = format switch
        {
            PacketFormat.MessageString => 9 + BitConverter.ToInt32(data, 5),
            PacketFormat.MessageObject => 9 + BitConverter.ToInt32(data, 5),
            _ => minimumHeaderSize
        };

        if (size < minimumTotalSize)
        {
            throw new ArgumentException("Data size under minimum total size.");
        }
    }

    public static CustomPacket Invalid => new() { Format = PacketFormat.Invalid };
    public static CustomPacket FromResultWithoutData(PacketFormat type, ReturnCode code) => new() { Format = PacketFormat.ResultWithoutData, Data = (type, code) };
    public static CustomPacket Connect => new() { Format = PacketFormat.Connect };
    public static CustomPacket Disconnect => new() { Format = PacketFormat.Disconnect };
    public static CustomPacket RequestID => new() { Format = PacketFormat.RequestID };
    public static CustomPacket FromAssignID(int id) => new() { Format = PacketFormat.AssignID, Data = id };
    public static CustomPacket FromLinkID(int id) => new() { Format = PacketFormat.LinkID, Data = id };
    public static CustomPacket AbandonID => new() { Format = PacketFormat.AbandonID };
    public static CustomPacket Shutdown => new() { Format = PacketFormat.Shutdown };
    public static CustomPacket FromMessage(int message) => new() { Format = PacketFormat.MessageInteger, Data = message };
    public static CustomPacket FromMessage(string message) => new() { Format = PacketFormat.MessageString, Data = message };
    public static CustomPacket FromMessage(object o) => new() { Format = PacketFormat.MessageObject, Data = o };
    public static CustomPacket FromTurn(MoveInfo move, int turnCount) => new() { Format = PacketFormat.Turn, Data = (move, turnCount) };

    public (PacketFormat type, ReturnCode code) ToResultWithoutData() => ((PacketFormat type, ReturnCode code))ThrowIfNotFormat(PacketFormat.ResultWithoutData).Data;
    public int ToAssignID() => (int)ThrowIfNotFormat(PacketFormat.AssignID).Data;
    public int ToLinkID() => (int)ThrowIfNotFormat(PacketFormat.LinkID).Data;
    public int ToMessageInt() => (int)ThrowIfNotFormat(PacketFormat.MessageInteger).Data;
    public string ToMessageString() => (string)ThrowIfNotFormat(PacketFormat.MessageString).Data;
    public object ToMessageObject() => ThrowIfNotFormat(PacketFormat.MessageObject).Data;
    public (MoveInfo info, int turnCount) ToTurn() => ((MoveInfo info, int turnCount))ThrowIfNotFormat(PacketFormat.Turn).Data;

    private CustomPacket ThrowIfNotFormat(PacketFormat format)
    {
        if (Format != format)
        {
            throw new ArgumentException(string.Format("Packet format must be of type: {0}", Enum.GetName(format)));
        }

        return this;
    }

    public static bool IsSuccess(ReturnCode code)
    {
        return code == ReturnCode.Success;
    }
}