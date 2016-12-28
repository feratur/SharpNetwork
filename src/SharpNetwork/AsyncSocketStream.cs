using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNetwork
{
    /// <summary>
    /// An implementation of <see cref="T:System.IO.Stream" /> that supports async read/write operations and cancellation.
    /// </summary>
    public class AsyncSocketStream : Stream
    {
        /// <summary>
        /// The underlying <see cref="T:System.Net.Sockets.Socket" />.
        /// </summary>
        public Socket Client { get; }

        #region Private members

        private readonly Timer _timer;
        private readonly Tuple<CancellationToken, CancellationTokenRegistration> _tokenInfo;

        private bool _disposed;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.AsyncSocketStream" /> class.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        public AsyncSocketStream(Socket socket) : this(socket, CancellationToken.None)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.AsyncSocketStream" /> class.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        public AsyncSocketStream(Socket socket, CancellationToken cancellationToken)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            Client = socket;

            if (cancellationToken.CanBeCanceled)
                _tokenInfo = Tuple.Create(cancellationToken, cancellationToken.Register(() => Client.Close()));

            _timer = new Timer(state => ((Socket)state).Close(), Client, -1, -1);
        }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out.
        /// </summary>
        public override int ReadTimeout { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out.
        /// </summary>
        public override int WriteTimeout { get; set; } = -1;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => true;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => false;

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => true;

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        public override bool CanTimeout => true;

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count).GetAwaiter().GetResult();

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous read operation. Contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.</returns>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            await
                ReadWriteAsync(buffer, offset, count, ReceiveAsync, ReadTimeout, cancellationToken)
                    .ConfigureAwait(false);

            return count;
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>A task that represents the asynchronous write operation.</returns>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadWriteAsync(buffer, offset, count, SendAsync, WriteTimeout, cancellationToken);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _timer.Dispose();

            _tokenInfo?.Item2.Dispose();

            _disposed = true;

            base.Dispose(disposing);
        }

        #region Private methods

        private async Task ReadWriteAsync(byte[] buffer, int offset, int count,
            Func<byte[], int, int, Task<int>> method, int timeout, CancellationToken token)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncSocketStream));

            try
            {
                SetTimerIfNeeded(timeout);

                if (token.CanBeCanceled && (_tokenInfo == null || !_tokenInfo.Item1.Equals(token)))
                {
                    using (token.Register(() => Client.Close()))
                        await SendReceiveLoopAsync(buffer, offset, count, method).ConfigureAwait(false);
                }
                else
                    await SendReceiveLoopAsync(buffer, offset, count, method).ConfigureAwait(false);
            }
            finally
            {
                ResetTimer();
            }
        }

        private Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            return
                Task.Factory.FromAsync(
                    (callback, state) => Client.BeginSend(buffer, offset, count, SocketFlags.None, callback, state),
                    Client.EndSend, null);
        }

        private Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
        {
            return
                Task.Factory.FromAsync(
                    (callback, state) => Client.BeginReceive(buffer, offset, count, SocketFlags.None, callback, state),
                    Client.EndReceive, null);
        }

        private void SetTimerIfNeeded(int timeout)
        {
            if (timeout < 0)
                return;

            _timer.Change(timeout, -1);
        }

        private void ResetTimer() => _timer.Change(-1, -1);

        private static async Task SendReceiveLoopAsync(byte[] buffer, int offset, int count,
            Func<byte[], int, int, Task<int>> method)
        {
            for (int startPosition = offset, endPosition = offset + count, bytesProcessed;
                startPosition < endPosition;
                startPosition += bytesProcessed)
            {
                try
                {
                    bytesProcessed =
                        await method(buffer, startPosition, endPosition - startPosition).ConfigureAwait(false);

                    if (bytesProcessed == 0)
                        throw new SocketStreamException(SocketStreamError.Disconnected);
                }
                catch (SocketException ex)
                {
                    throw new SocketStreamException(SocketStreamError.SocketException, ex);
                }
                catch (ObjectDisposedException)
                {
                    throw new SocketStreamException(SocketStreamError.SocketClosedOrTimedOut);
                }
            }
        }

        #endregion
    }
}