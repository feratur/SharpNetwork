using System;
using System.Net;
using System.Net.Sockets;

namespace SharpNetwork
{
    /// <summary>
    /// Contains information about connected socket's local and remote IP endpoints.
    /// </summary>
    public struct SocketEndpoints
    {
        /// <summary>
        /// The local endpoint of the socket.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; }

        /// <summary>
        /// The remote endpoint of the socket.
        /// </summary>
        public IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SharpNetwork.SocketEndpoints" /> structure.
        /// </summary>
        /// <param name="socket">Connected socket.</param>
        public SocketEndpoints(Socket socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            LocalEndPoint = socket.LocalEndPoint as IPEndPoint;

            RemoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
        }
    }
}