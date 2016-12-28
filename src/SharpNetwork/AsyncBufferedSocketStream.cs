using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;

namespace SharpNetwork
{
    /// <summary>
    /// An implementation of the <see cref="T:SharpNetwork.IBufferedSocketStream" /> interface.
    /// </summary>
    public class AsyncBufferedSocketStream : AsyncSocketStream, IBufferedSocketStream
    {
        #region Private members

        private readonly MemoryBuffer _receiveBuffer;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.AsyncBufferedSocketStream" /> class.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        public AsyncBufferedSocketStream(Socket socket) : this(socket, CancellationToken.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.AsyncBufferedSocketStream" /> class.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        public AsyncBufferedSocketStream(Socket socket, CancellationToken cancellationToken) : base(socket, cancellationToken)
        {
            _receiveBuffer = new MemoryBuffer();

            ReceiveBuffer = new ReadOnlyList<byte>(_receiveBuffer);
        }

        /// <summary>
        /// A read-only collection of bytes received from a stream.
        /// </summary>
        public ReadOnlyList<byte> ReceiveBuffer { get; }

        /// <summary>
        /// A <see cref="T:SharpStructures.MemoryBuffer" /> the contents of which will be written to a stream.
        /// </summary>
        public MemoryBuffer SendBuffer { get; } = new MemoryBuffer();

        /// <summary>
        /// Sets the number of received bytes to zero.
        /// </summary>
        public void ClearReceiveBuffer() => _receiveBuffer.SetPosition(0);

        /// <summary>
        /// Reads from socket and writes received data to a receive buffer.
        /// </summary>
        /// <param name="count">The number of bytes to receive.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task ReadToBufferAsync(int count, CancellationToken token)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return;

            var offset = _receiveBuffer.Position;

            _receiveBuffer.AllocateSpace(count);

            await ReadAsync(_receiveBuffer.Array, offset, count, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the contents of the send buffer to a socket stream.
        /// </summary>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task WriteBufferToSocketAsync(CancellationToken token)
        {
            if (SendBuffer.Position > 0)
                await WriteAsync(SendBuffer.Array, 0, SendBuffer.Position, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Tries to read all bytes available to the socket; all exceptions within the method are swallowed.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. Contains true if the operation is successful; contains false if an exception occurs.</returns>
        public async Task<bool> TryReadAvailableBytesAsync()
        {
            try
            {
                var availableBytes = Client.Available;

                await ReadToBufferAsync(availableBytes, CancellationToken.None).ConfigureAwait(false);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}