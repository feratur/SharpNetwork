using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;

namespace SharpNetwork
{
    public interface IBufferedSocketStream
    {
        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }

        Socket Client { get; }

        byte[] ReceiveBufferArray { get; }

        int ReceivedBytes { get; }

        MemoryBuffer SendBuffer { get; }

        void ClearReceiveBuffer();

        Task ReadToBufferAsync(int count, CancellationToken token);

        Task WriteBufferToSocketAsync(CancellationToken token);
    }
}