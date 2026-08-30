using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NinoChess.Networking;

public class TestNetworkLocalInterface() : INetworkLocalInterface
{

    public int MaxBufferSizeFromNetwork => 4;

    public int MaxBufferSizeFromLocal => 4;

    private readonly byte[] data = new byte[4];

    public TaskCompletionSource<byte[]> Input { get; private set; } = new();

    public ManualResetEvent DataUpdated { get; private set; } = new(false);

    public async Task<(bool respond, bool disconnect)> OnRecieveDataAsync(byte[] data, int byteCount, int id, TaskCompletionSource<byte[]> response)
    {
        var update = false;

        for (int i = 0; i < byteCount; i++)
        {
            if (this.data[i] != data[i])
            {
                this.data[i] = data[i];
                update = true;
            }
        }

        DataUpdated.Set();

        var dataAsInt = GetDataAsInt();

        switch (dataAsInt)
        {
            case -1:
                {
                    Debug.WriteLine("Recieved -1");
                    return (false, false);
                }
            case -2:
                {
                    Debug.WriteLine("Recieved -2");
                    response.SetResult(BitConverter.GetBytes(-3));
                    return (true, true);
                }
            case -3:
                {
                    Debug.WriteLine("Recieved -3");
                    return (false, false);
                }
            default:
                {
                    return (false, false);
                }
        }
    }

    public async Task<(byte[] data, int? id)> GetDataToSendAsync(CancellationToken cancellationToken)
    {
        cancellationToken.Register(() => Input.TrySetCanceled());

        try
        {
            var result = await Input.Task;

            return (result, null);
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

    public string GetDataAsString()
    {
        return Encoding.UTF8.GetString(data);
    }
    public int GetDataAsInt()
    {
        return BitConverter.ToInt32(data);
    }
}