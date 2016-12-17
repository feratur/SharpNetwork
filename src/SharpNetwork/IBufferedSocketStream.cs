using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;

namespace SharpNetwork
{
    /// <summary>
    /// Represents a network stream with buffered read/write operations.
    /// </summary>
    public interface IBufferedSocketStream
    {
        /// <summary>
        /// An amount of time (in milliseconds) that represents a timeout for a read operation.
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        /// An amount of time (in milliseconds) that represents a timeout for a write operation.
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        /// The underlying <see cref="T:System.Net.Sockets.Socket" />.
        /// </summary>
        Socket Client { get; }

        /// <summary>
        /// An array of bytes received from a socket.
        /// </summary>
        byte[] ReceiveBufferArray { get; }

        /// <summary>
        /// A number of bytes received from a socket.
        /// </summary>
        int ReceivedBytes { get; }

        /// <summary>
        /// A <see cref="T:SharpStructures.MemoryBuffer" /> the contents of which will be written to a stream.
        /// </summary>
        MemoryBuffer SendBuffer { get; }

        /// <summary>
        /// Sets the number of received bytes to zero.
        /// </summary>
        void ClearReceiveBuffer();

        /// <summary>
        /// Reads from socket and writes received data to a receive buffer.
        /// </summary>
        /// <param name="count">The number of bytes to receive.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ReadToBufferAsync(int count, CancellationToken token);

        /// <summary>
        /// Writes the contents of the send buffer to a socket stream.
        /// </summary>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task WriteBufferToSocketAsync(CancellationToken token);
    }
}