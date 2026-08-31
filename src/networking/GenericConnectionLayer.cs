using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using static NinoChess.Networking.CustomPacket;

namespace NinoChess.Networking;

public abstract class GenericConnectionLayer() : INetworkLocalConnectionLayer
{
    public int MaxBufferSizeFromNetwork => 256;
    public int MaxBufferSizeFromLocal => 256;

    public TaskCompletionSource<CustomPacket> Input { get; private set; } = new();

    public async Task<(byte[]? response, bool disconnect)> OnRecieveDataAsync(byte[] data, int byteCount, int id)
    {
        /*for (int i = 0; i < byteCount - 1; i++)
        {
            Console.Write("{0}, ", data[i]);
        }
        Console.WriteLine(data[byteCount - 1]);*/

        var packet = FromBytesRaw(data, byteCount);

        var (response, disconnect) = await HandleIncomingPacket(packet, id);

        return (response?.ToBytesRaw(), disconnect);
    }

    protected virtual int? GetDestinationOfPacket(CustomPacket packet) => null;

    public async Task<(byte[] data, int? id)> GetDataToSendAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Input.TrySetCanceled());

        try
        {
            var packet = await Input.Task;
            var size = packet.GetSize();

            if (size > MaxBufferSizeFromLocal)
            {
                throw new ArgumentException("Packet size is too large to send");
            }

            var data = packet.ToBytesRaw();

            /*for (int i = 0; i < size - 1; i++)
            {
                Console.Write("{0}, ", data[i]);
            }
            Console.WriteLine(data[size - 1]);*/

            return (data, GetDestinationOfPacket(packet));
        }
        catch (OperationCanceledException e)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return ([], null);
        }
        finally
        {
            Input = new();
        }
    }

    protected abstract Task<(CustomPacket? response, bool disconnect)> HandleIncomingPacket(CustomPacket packet, int id);
}