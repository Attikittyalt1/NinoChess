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

public interface INetworkLocalConnectionLayer
{
    public int MaxBufferSizeFromNetwork { get; }
    public int MaxBufferSizeFromLocal { get; }

    public Task<(byte[]? response, bool disconnect)> OnRecieveDataAsync(byte[] data, int byteCount, int id);
    public Task<(byte[] data, int? id)> GetDataToSendAsync(CancellationToken cancellationToken);
}