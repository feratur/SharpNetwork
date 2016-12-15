using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SharpStructures;

namespace SharpNetwork
{
    public class AsyncBufferedSocketStream : AsyncSocketStream, IBufferedSocketStream
    {
        private readonly MemoryBuffer _receiveBuffer = new MemoryBuffer();

        public AsyncBufferedSocketStream(Socket socket) : base(socket)
        {
        }

        public byte[] ReceiveBufferArray => _receiveBuffer.Array;

        public int ReceivedBytes => _receiveBuffer.Position;

        public MemoryBuffer SendBuffer { get; } = new MemoryBuffer();

        public void ClearReceiveBuffer() => _receiveBuffer.SetPosition(0);

        public async Task ReadToBufferAsync(int count, CancellationToken token)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (count == 0)
                return;

            var offset = _receiveBuffer.Position;

            _receiveBuffer.AllocateSpace(count);

            await
                ReadWriteAsync(_receiveBuffer.Array, offset, count, ReceiveAsync, ReadTimeout, token)
                    .ConfigureAwait(false);
        }

        public async Task WriteBufferToSocketAsync(CancellationToken token)
        {
            if (SendBuffer.Position > 0)
                await
                    ReadWriteAsync(SendBuffer.Array, 0, SendBuffer.Position, SendAsync, WriteTimeout, token)
                        .ConfigureAwait(false);
        }
    }
}