using System;
using System.IO;
using System.Text;

namespace NinoChess.Networking;

public class CustomPacket
{
    public enum PacketType : byte
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
        DoMove = 12,
        Undo = 13,
        Redo = 13,
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

    public PacketType Type { get; private init; } = PacketType.Invalid;
    private object? Data { get; init; }

    public int GetSize() => Type switch
    {
        PacketType.Connect => 1,
        PacketType.ResultWithoutData => 3,
        PacketType.Disconnect => 1,
        PacketType.RequestID => 1,
        PacketType.AssignID => 5,
        PacketType.LinkID => 5,
        PacketType.AbandonID => 1,
        PacketType.Shutdown => 1,
        PacketType.MessageInteger => 5,
        PacketType.MessageString => 5 + Encoding.UTF8.GetByteCount((string) Data),
        PacketType.MessageObject => throw new NotImplementedException(),
        PacketType.DoMove => 21,
        _ => throw new InvalidDataException("Invalid packet format")
    };
    
    public byte[] ToBytesRaw()
    {
        return [(byte)Type, .. Type switch
        {
            PacketType.ResultWithoutData => ResultWithoutDataToBytes(),
            PacketType.Connect => [],
            PacketType.Disconnect => [],
            PacketType.RequestID => [],
            PacketType.AssignID => BitConverter.GetBytes((int)Data),
            PacketType.LinkID => BitConverter.GetBytes((int)Data),
            PacketType.AbandonID => [],
            PacketType.Shutdown => [],
            PacketType.MessageInteger => [..BitConverter.GetBytes((int)Data)],
            PacketType.MessageString => [..BitConverter.GetBytes((int)Data), ..Encoding.UTF8.GetBytes((string)Data)],
            PacketType.MessageObject => throw new NotImplementedException(),
            PacketType.DoMove => MoveToBytes(),
        _ => throw new InvalidDataException("Invalid packet format")
        }];

        byte[] ResultWithoutDataToBytes()
        {
            (PacketType type, ReturnCode code) = ((PacketType type, ReturnCode code))Data;

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

        return ((PacketType) data[0] switch
        {
            PacketType.ResultWithoutData => FromResultWithoutData((PacketType)data[1], (ReturnCode)data[2]),
            PacketType.Connect => Connect,
            PacketType.Disconnect => Disconnect,
            PacketType.RequestID => RequestID,
            PacketType.AssignID => FromAssignID(BitConverter.ToInt32(data, 1)),
            PacketType.LinkID => FromLinkID(BitConverter.ToInt32(data, 1)),
            PacketType.AbandonID => UnassignID,
            PacketType.Shutdown => Shutdown,
            PacketType.MessageInteger => FromMessage(BitConverter.ToInt32(data, 1)),
            PacketType.MessageString => FromMessage(Encoding.UTF8.GetString(data, 5, BitConverter.ToInt32(data, 1))),
            PacketType.MessageObject => throw new NotImplementedException(),
            PacketType.DoMove => BytesToMove(),
            _ => throw new InvalidDataException("Invalid packet format")
        });

        CustomPacket BytesToMove()
        {
            var turn = BitConverter.ToInt32(data, 1);
            var origin = new Position(BitConverter.ToInt32(data, 5), BitConverter.ToInt32(data, 9));
            var target = new Position(BitConverter.ToInt32(data, 13), BitConverter.ToInt32(data, 17));

            return FromMove(new(origin, target), turn);
        }
    }

    public static void ValidateDataSize(byte[] data, int size)
    {
        if (data.Length < 1)
        {
            throw new ArgumentException("Data is empty.");
        }

        var type = (PacketType)data[0];

        var minimumHeaderSize = type switch
        {
            PacketType.ResultWithoutData => 3,
            PacketType.Connect => 1,
            PacketType.Disconnect => 1,
            PacketType.RequestID => 1,
            PacketType.AssignID => 5,
            PacketType.LinkID => 5,
            PacketType.AbandonID => 1,
            PacketType.Shutdown => 1,
            PacketType.MessageInteger => 5,
            PacketType.MessageString => 5,
            PacketType.MessageObject => throw new NotImplementedException(),
            PacketType.DoMove => 21,
            _ => throw new InvalidDataException("Invalid packet format")
        };

        if (size < minimumHeaderSize)
        {
            throw new ArgumentException("Data size under minimum header size.");
        }

        var minimumTotalSize = type switch
        {
            PacketType.MessageString => 9 + BitConverter.ToInt32(data, 5),
            PacketType.MessageObject => 9 + BitConverter.ToInt32(data, 5),
            _ => minimumHeaderSize
        };

        if (size < minimumTotalSize)
        {
            throw new ArgumentException("Data size under minimum total size.");
        }
    }

    public static CustomPacket Invalid => new() { Type = PacketType.Invalid };
    public static CustomPacket FromResultWithoutData(PacketType type, ReturnCode code) => new() { Type = PacketType.ResultWithoutData, Data = (type, code) };
    public static CustomPacket Connect => new() { Type = PacketType.Connect };
    public static CustomPacket Disconnect => new() { Type = PacketType.Disconnect };
    public static CustomPacket RequestID => new() { Type = PacketType.RequestID };
    public static CustomPacket FromAssignID(int id) => new() { Type = PacketType.AssignID, Data = id };
    public static CustomPacket FromLinkID(int id) => new() { Type = PacketType.LinkID, Data = id };
    public static CustomPacket UnassignID => new() { Type = PacketType.AbandonID };
    public static CustomPacket Shutdown => new() { Type = PacketType.Shutdown };
    public static CustomPacket FromMessage(int message) => new() { Type = PacketType.MessageInteger, Data = message };
    public static CustomPacket FromMessage(string message) => new() { Type = PacketType.MessageString, Data = message };
    public static CustomPacket FromMessage(object o) => new() { Type = PacketType.MessageObject, Data = o };
    public static CustomPacket FromMove(MoveInfo move, int turnCount) => new() { Type = PacketType.DoMove, Data = (move, turnCount) };

    public (PacketType type, ReturnCode code) ToResultWithoutData() => ((PacketType type, ReturnCode code))ThrowIfNotType(PacketType.ResultWithoutData).Data;
    public int ToAssignID() => (int)ThrowIfNotType(PacketType.AssignID).Data;
    public int ToLinkID() => (int)ThrowIfNotType(PacketType.LinkID).Data;
    public int ToMessageInt() => (int)ThrowIfNotType(PacketType.MessageInteger).Data;
    public string ToMessageString() => (string)ThrowIfNotType(PacketType.MessageString).Data;
    public object ToMessageObject() => ThrowIfNotType(PacketType.MessageObject).Data;
    public (MoveInfo info, int turnCount) ToMove() => ((MoveInfo info, int turnCount))ThrowIfNotType(PacketType.DoMove).Data;

    private CustomPacket ThrowIfNotType(PacketType type)
    {
        if (Type != type)
        {
            throw new ArgumentException(string.Format("Packet format must be of type: {0}", Enum.GetName(type)));
        }

        return this;
    }

    public static bool IsSuccess(ReturnCode code)
    {
        return code == ReturnCode.Success;
    }
}