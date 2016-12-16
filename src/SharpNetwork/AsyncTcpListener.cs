using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNetwork
{
    public class AsyncTcpListener
    {
        public Action<Exception> OnException { get; set; }

        public Action<OperationCanceledException> OnCancelled { get; set; }

        public Action<Socket> OnDisconnected { get; set; }

        public Action<Socket> OnConnected { get; set; }

        public async Task ListenAsync(int port, Func<Socket, CancellationToken, Task> clientInteraction,
            CancellationToken token)
        {
            if (port < 0 || port > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(port));

            if (clientInteraction == null)
                throw new ArgumentNullException(nameof(clientInteraction));

            var listener = TcpListener.Create(port);

            try
            {
                listener.Start();

                using (token.Register(() => listener.Stop()))
                {
                    while (!token.IsCancellationRequested)
                    {
                        Socket client;

                        try
                        {
                            client = await listener.AcceptSocketAsync().ConfigureAwait(false);
                        }
                        catch (ObjectDisposedException)
                        {
                            break;
                        }

                        InteractWithClient(client, clientInteraction, token);
                    }
                }
            }
            finally
            {
                listener.Stop();
            }
        }

        private async void InteractWithClient(Socket client,
            Func<Socket, CancellationToken, Task> clientInteraction, CancellationToken token)
        {
            try
            {
                using (client)
                {
                    OnConnected?.Invoke(client);

                    await clientInteraction(client, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException ex)
            {
                OnCancelled?.Invoke(ex);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);
            }
            finally
            {
                OnDisconnected?.Invoke(client);
            }
        }
    }
}