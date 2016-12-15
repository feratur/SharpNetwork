using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNetwork
{
    public class AsyncSocketStream : Stream
    {
        public Socket Client { get; }

        private readonly Timer _timer;

        private bool _disposed;

        public AsyncSocketStream(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            Client = socket;

            _timer = new Timer(state => ((Socket)state).Close(), Client, -1, -1);
        }

        public override int ReadTimeout { get; set; } = -1;

        public override int WriteTimeout { get; set; } = -1;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override bool CanTimeout => true;

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count).GetAwaiter().GetResult();

        public override void Write(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count).GetAwaiter().GetResult();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count,
            CancellationToken cancellationToken)
        {
            await
                ReadWriteAsync(buffer, offset, count, ReceiveAsync, ReadTimeout, cancellationToken)
                    .ConfigureAwait(false);

            return count;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadWriteAsync(buffer, offset, count, SendAsync, WriteTimeout, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _timer.Dispose();

            _disposed = true;

            base.Dispose(disposing);
        }

        protected async Task ReadWriteAsync(byte[] buffer, int offset, int count,
            Func<byte[], int, int, Task<int>> method, int timeout, CancellationToken token)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncSocketStream));

            try
            {
                SetTimerIfNeeded(timeout);

                using (token.Register(() => Client.Close()))
                    await SendReceiveLoopAsync(buffer, offset, count, method).ConfigureAwait(false);
            }
            finally
            {
                ResetTimer();
            }
        }

        protected Task<int> SendAsync(byte[] buffer, int offset, int count)
        {
            return
                Task.Factory.FromAsync(
                    (callback, state) => Client.BeginSend(buffer, offset, count, SocketFlags.None, callback, state),
                    Client.EndSend, null);
        }

        protected Task<int> ReceiveAsync(byte[] buffer, int offset, int count)
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
    }
}