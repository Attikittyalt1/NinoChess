using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NinoChess.Networking;

public interface INetworkLocalInterface
{
    public int MaxBufferSizeFromNetwork { get; }
    public int MaxBufferSizeFromLocal { get; }

    public Task<(bool respond, bool disconnect)> UpdateWithDataAsync(byte[] data, int byteCount, TaskCompletionSource<byte[]> response);
    public Task<byte[]> GetLocalDataAsync(CancellationToken cancellationToken);
}