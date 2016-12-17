using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNetwork
{
    /// <summary>
    /// Listens for connections from TCP network clients and processes connections asynchronously.
    /// </summary>
    public class AsyncTcpListener
    {
        /// <summary>
        /// Fired when an exception other than <see cref="T:System.OperationCanceledException" /> is caught.
        /// </summary>
        public Action<SocketEndpoints, Exception> OnException { get; set; }

        /// <summary>
        /// Fired when an <see cref="T:System.OperationCanceledException" /> is caught.
        /// </summary>
        public Action<SocketEndpoints, OperationCanceledException> OnCancelled { get; set; }

        /// <summary>
        /// Fired after a client disconnects.
        /// </summary>
        public Action<SocketEndpoints> OnDisconnected { get; set; }

        /// <summary>
        /// Fired when a connection is established.
        /// </summary>
        public Action<Socket> OnConnected { get; set; }

        /// <summary>
        /// Listens for incoming TCP connections on a specified port.
        /// </summary>
        /// <param name="port">The port on which to listen for incoming connection attempts.</param>
        /// <param name="clientInteraction">An asynchronous action that is performed on each connected socket.</param>
        /// <param name="token">The token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        #region Private methods

        private async void InteractWithClient(Socket client,
            Func<Socket, CancellationToken, Task> clientInteraction, CancellationToken token)
        {
            var socketInfo = new SocketEndpoints();

            try
            {
                using (client)
                {
                    socketInfo = new SocketEndpoints(client);

                    OnConnected?.Invoke(client);

                    await clientInteraction(client, token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException ex)
            {
                OnCancelled?.Invoke(socketInfo, ex);
            }
            catch (Exception ex)
            {
                OnException?.Invoke(socketInfo, ex);
            }
            finally
            {
                OnDisconnected?.Invoke(socketInfo);
            }
        }

        #endregion
    }
}